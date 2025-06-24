using System.Diagnostics;
using System.Reflection;
using static CorpseLib.Test.UnitTest;

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

            public string ToDebugString() => $"time: {m_ElapsedTime}ms, real: {m_ProcessorRealTime}ms, user: {m_ProcessorUserTime}ms, memory: {m_BytesUsed}bytes";
        }
        private readonly string m_Name;

        public string Name => m_Name;

        protected TestCase(string name) => m_Name = name;

        internal Result StartTest()
        {
            Process currentProcess = Process.GetCurrentProcess();
            Stopwatch watch = Stopwatch.StartNew();
            long lastBytesUsed = currentProcess.VirtualMemorySize64;
            long lastProcessorRealTime = (long)currentProcess.TotalProcessorTime.TotalMilliseconds;
            long lastProcessorUserTime = (long)currentProcess.UserProcessorTime.TotalMilliseconds;
            bool testResult = RunTest();
            watch.Stop();
            return new Result(m_Name, currentProcess.VirtualMemorySize64 - lastBytesUsed, watch.ElapsedMilliseconds,
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
                    m_TestCase = () => (bool)method.Invoke(instance, null)!;
                }
                else if (method.ReturnType == typeof(void))
                {
                    m_TestCase = () =>
                    {
                        method.Invoke(instance, null);
                        return true;
                    };
                }
                else if (method.ReturnType == typeof(OperationResult))
                {
                    m_TestCase = () =>
                    {
                        OperationResult result = (OperationResult)method.Invoke(instance, null)!;
                        if (result)
                            return true;
                        else
                        {
                            Console.WriteLine(result);
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

        protected override bool RunTest()
        {
            try
            {
                return m_TestCase();
            }
            catch (TestFailedException testFailed)
            {
                Console.WriteLine(testFailed.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}
