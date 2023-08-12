namespace CorpseLib.Test
{
    public class Configuration
    {
        public class Header
        {
            public int Size;
            public char Filler;

            internal void Print(string str)
            {
                int headerFirstFillerHalf = (Size - str.Length - 2) / 2;
                int headerSecondFillerHalf = Size - (headerFirstFillerHalf + str.Length + 2);
                Console.WriteLine(string.Format("{0} {1} {2}", new string(Filler, headerFirstFillerHalf), str, new string(Filler, headerSecondFillerHalf)));
            }
        }

        public Header TestCaseHeader = new() { Size = 50, Filler = '-' };
        public Header UnitTestHeader = new() { Size = 50, Filler = '=' };
        public bool ContinueTestOnFail = true;
    }
}
