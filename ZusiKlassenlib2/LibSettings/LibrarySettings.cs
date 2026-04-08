namespace ZusiKlassenLib2
{
    public static class LibrarySettings
    {
        private static bool _checkAttributes =
#if DEBUG
            true;
#else
            false;
#endif

        private static bool _checkElements =
#if DEBUG
            true;
#else
            false;
#endif

        public static bool CheckAttributes
        {
            get => _checkAttributes;
            set => _checkAttributes = value;
        }
        public static bool CheckElements
        {
            get => _checkElements;
            set => _checkElements = value;
        }
    }
}
