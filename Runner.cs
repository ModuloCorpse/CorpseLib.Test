using System.Reflection;

namespace CorpseLib.Test
{
    public class Runner
    {
        private readonly Configuration m_Configuration;
        private readonly List<UnitTest> m_Tests = [];

        public static int Main()
        {
            Log.Start();
            return (new Runner(new()).Run()) ? 0 : -1;
        }

        public static int Main(Configuration configuration)
        {
            Log.Start();
            return (new Runner(configuration).Run()) ? 0 : -1;
        }

        private static TestCaseDelegate? ConvertMethod(object instance, MethodInfo method)
        {
            if (method.GetParameters().Length == 0)
            {
                if (method.ReturnType == typeof(bool))
                {
                    return () =>
                    {
                        try
                        {
                            return (bool)method.Invoke(instance, null)!;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            return false;
                        }
                    };
                }
                else if (method.ReturnType == typeof(void))
                {
                    return () =>
                    {
                        try
                        {
                            method.Invoke(instance, null);
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            return false;
                        }
                    };
                }
                else if (method.ReturnType == typeof(OperationResult))
                {
                    return () =>
                    {
                        try
                        {
                            OperationResult result = (OperationResult)method.Invoke(instance, null)!;
                            if (result)
                                return true;
                            else
                            {
                                Console.WriteLine(result);
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            return false;
                        }
                    };
                }
            }
            return null;
        }

        public Runner(Configuration configuration)
        {
            m_Configuration = configuration;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    object[] unitTestAttributes = type.GetCustomAttributes(typeof(UnitTestClass), true);
                    if (unitTestAttributes.Length == 1 && unitTestAttributes[0] is UnitTestClass unitTestInfo)
                    {
                        object? unitTestInstance = Activator.CreateInstance(type);
                        if (unitTestInstance != null)
                        {
                            UnitTest test = new(unitTestInfo);
                            MethodInfo[] methods = type.GetMethods();
                            foreach (MethodInfo method in methods)
                            {
                                if (method.GetCustomAttributes(typeof(TestInitMethod), true).Length == 1)
                                    test.SetInit(new ReflectionTestCase("Init", unitTestInstance, method));

                                if (method.GetCustomAttributes(typeof(TestCleanupMethod), true).Length == 1)
                                {
                                    TestCaseDelegate? cleanupDelegate = ConvertMethod(unitTestInstance, method);
                                    if (cleanupDelegate != null)
                                        test.SetCleanUp(cleanupDelegate);
                                }

                                object[] testCaseAttributes = method.GetCustomAttributes(typeof(TestCaseMethod), true);
                                if (testCaseAttributes.Length == 1 && testCaseAttributes[0] is TestCaseMethod testCaseInfo)
                                    test.AddTestCase(new ReflectionTestCase(testCaseInfo.Name, unitTestInstance, method));
                            }
                            m_Tests.Add(test);
                        }
                    }
                }
            }
        }

        public void AddTest(UnitTest test) => m_Tests.Add(test);

        private class Result
        {
            private readonly List<UnitTest.Result> m_Results = [];
            private int m_TestSuccessCount = 0;
            private int m_TestFailureCount = 0;
            private int m_SuccessTotalCount = 0;
            private int m_FailureTotalCount = 0;
            private int m_TestCaseCount = 0;
            private int m_ExecutedTestCaseCount = 0;

            public int TestFailureCount => m_TestFailureCount;
            public int FailureTotalCount => m_FailureTotalCount;
            public int TestCaseCount => m_TestCaseCount;
            public int ExecutedTestCaseCount => m_ExecutedTestCaseCount;

            public void AddCaseCount(int caseCount) => m_TestCaseCount += caseCount;

            public void Consume(UnitTest.Result testResult)
            {
                m_Results.Add(testResult);
                m_SuccessTotalCount += testResult.SuccessCount;
                m_FailureTotalCount += testResult.FailureCount;
                m_ExecutedTestCaseCount += testResult.SuccessCount + testResult.FailureCount;
                if (testResult.FailureCount != 0)
                    ++m_TestFailureCount;
                else
                    ++m_TestSuccessCount;
            }

            public void Print(int unitTestCount)
            {
                foreach (UnitTest.Result result in m_Results)
                    result.Print();
                if (m_TestCaseCount != m_ExecutedTestCaseCount)
                    Console.WriteLine("Not all test were executed because {0} test case failed within {1} unit tests", m_FailureTotalCount, m_TestFailureCount);
                else
                {
                    if (m_FailureTotalCount != 0)
                        Console.WriteLine("All test executed but {0} test case failed within {1} unit tests", m_FailureTotalCount, m_TestFailureCount);
                    else
                        Console.WriteLine("All test executed with success ({0} test cases executed within {1} unit tests)", m_TestCaseCount, unitTestCount);
                }
            }
        }

        private Result InternalRun()
        {
            Result result = new();
            foreach (UnitTest test in m_Tests)
            {
                result.AddCaseCount(test.TestCount);
                UnitTest.Result testResult = test.Run(m_Configuration);
                result.Consume(testResult);
                if (testResult.FailureCount != 0 && !m_Configuration.ContinueTestOnFail)
                    return result;
            }
            return result;
        }

        public bool Run()
        {
            Result result = InternalRun();
            Console.WriteLine("////////////////////////////////////////////// Result \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\");
            result.Print(m_Tests.Count);
            return result.TestFailureCount == 0;
        }
    }
}