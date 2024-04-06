namespace CorpseLib.Test
{
    public delegate bool TestCaseDelegate();

    public class UnitTest
    {
        public class TestFailedException(string message) : Exception(message) { }

        public static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new TestFailedException(message);
        }

        public static void DumpAndCompare(string source, string target, string message)
        {
            Console.WriteLine(source);
            if (source != target)
                throw new TestFailedException(message);
        }

        internal class Result(string name)
        {
            private readonly List<TestCase.Result> m_Results = [];
            private TestCase.Result? m_InitResult = null;
            private readonly string m_Name = name;
            private int m_SuccessCount = 0;
            private int m_FailureCount = 0;

            public string Name => m_Name;
            public int SuccessCount => m_SuccessCount;
            public int FailureCount => m_FailureCount;
            public bool InitSuccess => m_InitResult == null || m_InitResult.TestCaseResult;

            public void AddResult(TestCase.Result result)
            {
                m_Results.Add(result);
                if (result.TestCaseResult)
                    ++m_SuccessCount;
                else
                    ++m_FailureCount;
            }

            public void SetInitResult(TestCase.Result result) => m_InitResult = result;

            public void Print()
            {
                if (m_InitResult != null && !m_InitResult.TestCaseResult)
                    Console.WriteLine(" - {0} : Initialization failed ({1})", m_Name, m_InitResult.ToDebugString());
                else
                    Console.WriteLine(" - {0}", m_Name);
                foreach (TestCase.Result result in m_Results)
                    result.Print();
            }
        }

        private TestCase? m_Init = null;
        private TestCaseDelegate? m_Cleanup = null;
        private readonly List<TestCase> m_Tests = [];
        private readonly string m_Name;
        private readonly bool m_ContinueTestOnFail;

        public int TestCount => m_Tests.Count;

        internal UnitTest(UnitTestClass unitTestAttribute)
        {
            m_Name = unitTestAttribute.Name;
            m_ContinueTestOnFail = unitTestAttribute.ContinueOnFail;
        }

        public UnitTest(string name, bool continueTestOnFail)
        {
            m_Name = name;
            m_ContinueTestOnFail = continueTestOnFail;
        }

        public void SetInit(TestCase init) => m_Init = init;
        public void SetCleanUp(TestCaseDelegate cleanup) => m_Cleanup = cleanup;
        public void AddTestCase(TestCase testCase) => m_Tests.Add(testCase);

        internal void RunTestCases(Configuration configuration, ref Result result)
        {
            foreach (TestCase test in m_Tests)
            {
                configuration.TestCaseHeader.Print(test.Name);
                TestCase.Result testResult = test.StartTest();
                result.AddResult(testResult);
                if (!testResult.TestCaseResult && (!configuration.ContinueTestOnFail || !m_ContinueTestOnFail))
                    return;
            }
        }

        internal Result Run(Configuration configuration)
        {
            Result result = new(m_Name);
            configuration.UnitTestHeader.Print(m_Name);
            if (m_Init != null)
            {
                TestCase.Result testResult = m_Init.StartTest();
                result.SetInitResult(testResult);
                if (!testResult.TestCaseResult && (!configuration.ContinueTestOnFail || !m_ContinueTestOnFail))
                    return result;
            }
            RunTestCases(configuration, ref result);
            m_Cleanup?.Invoke();
            return result;
        }
    }
}
