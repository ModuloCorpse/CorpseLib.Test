using System.Diagnostics;

namespace CorpseLib.Test
{
    public class Debugger
    {
        private readonly Stopwatch m_Watch = new();
        private long m_LastBytesUsed;
        private long m_LastProcessorRealTime;
        private long m_LastProcessorUserTime;

        private long m_BytesUsed = 0;
        private long m_ElapsedTime = 0;
        private long m_ProcessorRealTime = 0;
        private long m_ProcessorUserTime = 0;

        public void Start()
        {
            Process currentProcess = Process.GetCurrentProcess();
            m_Watch.Restart();
            m_LastBytesUsed = currentProcess.WorkingSet64;
            m_LastProcessorRealTime = (long)currentProcess.TotalProcessorTime.TotalMilliseconds;
            m_LastProcessorUserTime = (long)currentProcess.UserProcessorTime.TotalMilliseconds;
        }

        public void Stop()
        {
            m_Watch.Stop();
            Process currentProcess = Process.GetCurrentProcess();
            m_BytesUsed = currentProcess.WorkingSet64 - m_LastBytesUsed;
            m_ElapsedTime = m_Watch.ElapsedMilliseconds;
            m_ProcessorRealTime = (long)currentProcess.TotalProcessorTime.TotalMilliseconds - m_LastProcessorRealTime;
            m_ProcessorUserTime = (long)currentProcess.UserProcessorTime.TotalMilliseconds - m_LastProcessorUserTime;
        }

        public override string ToString() => $"time: {m_ElapsedTime}ms, real: {m_ProcessorRealTime}ms, user: {m_ProcessorUserTime}ms, memory: {m_BytesUsed}bytes";
    }
}
