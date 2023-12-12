using System.Diagnostics;
using System.Reflection;

namespace CorpseLib.Test
{
    public abstract class TestCase
    {
        public class Result(string name, long bytesUsed, long elapsedTime, long processorRealTime, long processorUserTime, bool testCaseResult)
        {
            private readonly string m_Name = name;
            private readonly long m_BytesUsed = bytesUsed;
            private readonly long m_ElapsedTime = elapsedTime;
            private readonly long m_ProcessorRealTime = processorRealTime;
            private readonly long m_ProcessorUserTime = processorUserTime;
            private readonly bool m_TestCaseResult = testCaseResult;

            public string Name => m_Name;
            public bool TestCaseResult => m_TestCaseResult;

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
