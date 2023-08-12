using System.Diagnostics;
using System.Reflection;

namespace CorpseLib.Test
{
    public abstract class TestCase
    {
        public class Result
        {
            private readonly string m_Name;
            private readonly long m_BytesUsed;
            private readonly long m_ElapsedTime;
            private readonly long m_ProcessorRealTime;
            private readonly long m_ProcessorUserTime;
            private readonly bool m_TestCaseResult;

            public string Name => m_Name;
            public bool TestCaseResult => m_TestCaseResult;

            public Result(string name, long bytesUsed, long elapsedTime, long processorRealTime, long processorUserTime, bool testCaseResult)
            {
                m_Name = name;
                m_BytesUsed = bytesUsed;
                m_ElapsedTime = elapsedTime;
                m_ProcessorRealTime = processorRealTime;
                m_ProcessorUserTime = processorUserTime;
                m_TestCaseResult = testCaseResult;
            }

            public void Print() => Console.WriteLine("     * {0} : {1} ({2})", m_Name, (m_TestCaseResult) ? "SUCCESS" : "FAILURE", ToDebugString());

            public string ToDebugString() => string.Format("time: {0}ms, real: {1}ms, user: {2}ms, memory: {3}bytes", m_ElapsedTime, m_ProcessorRealTime, m_ProcessorUserTime, m_BytesUsed);
        }
        private readonly string m_Name;

        public string Name => m_Name;

        protected TestCase(string name) => m_Name = name;

        internal Result StartTest()
        {
            Process currentProcess = Process.GetCurrentProcess();
            Stopwatch watch = Stopwatch.StartNew();
            long lastBytesUsed = currentProcess.WorkingSet64;
            long lastProcessorRealTime = (long)currentProcess.TotalProcessorTime.TotalMilliseconds;
            long lastProcessorUserTime = (long)currentProcess.UserProcessorTime.TotalMilliseconds;
            bool testResult = RunTest();
            watch.Stop();
            return new Result(m_Name, currentProcess.WorkingSet64 - lastBytesUsed, watch.ElapsedMilliseconds,
                (long)currentProcess.TotalProcessorTime.TotalMilliseconds - lastProcessorRealTime,
                (long)currentProcess.UserProcessorTime.TotalMilliseconds - lastProcessorUserTime, testResult);
        }

        protected abstract bool RunTest();
    }

    public class ReflectionTestCase : TestCase
    {
        private delegate bool TestCaseDelegate();
        private readonly TestCaseDelegate m_TestCase;

        public ReflectionTestCase(string name, object instance, MethodInfo method): base(name)
        {
            if (method.GetParameters().Length == 0)
            {
                if (method.ReturnType == typeof(bool))
                {
                    m_TestCase = () =>
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
                    m_TestCase = () =>
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
                    m_TestCase = () =>
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
                else
                    throw new ArgumentException("Given method isn't a valid test case method");
            }
            else
                throw new ArgumentException("Given method isn't a valid test case method");
        }

        protected override bool RunTest() => m_TestCase();
    }
}
