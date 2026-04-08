namespace ZusiKlassenLib2.Common
{
    public enum ZusiTriState
    {
        Undefined,
        Off,
        On
    }

    public static class TriStateConverter
    {
        public static ZusiTriState Convert(string s)
        {
            if (s == "1")
                return ZusiTriState.Off;
            else if (s == "2")
                return ZusiTriState.On;
            else
                return ZusiTriState.Undefined;
        }
    }
}
