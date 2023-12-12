namespace CorpseLib.Test
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class UnitTestClass(string name, bool continueTestOnFail = false) : Attribute
    {
        private readonly string m_Name = name;
        private readonly bool m_ContinueTestOnFail = continueTestOnFail;
        public string Name => m_Name;
        public bool ContinueOnFail => m_ContinueTestOnFail;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TestCaseMethod(string name) : Attribute
    {
        private readonly string m_Name = name;
        public string Name => m_Name;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TestInitMethod : Attribute {}

    [AttributeUsage(AttributeTargets.Method)]
    public class TestCleanupMethod : Attribute { }
}
