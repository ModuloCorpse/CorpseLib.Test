namespace CorpseLib.Test
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class UnitTestClass : Attribute
    {
        private readonly string m_Name;
        private readonly bool m_ContinueTestOnFail;
        public string Name => m_Name;
        public bool ContinueOnFail => m_ContinueTestOnFail;
        public UnitTestClass(string name, bool continueTestOnFail = false)
        {
            m_Name = name;
            m_ContinueTestOnFail = continueTestOnFail;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TestCaseMethod : Attribute
    {
        private readonly string m_Name;
        public string Name => m_Name;
        public TestCaseMethod(string name) => m_Name = name;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TestInitMethod : Attribute {}

    [AttributeUsage(AttributeTargets.Method)]
    public class TestCleanupMethod : Attribute { }
}
