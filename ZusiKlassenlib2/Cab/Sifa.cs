using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class Sifa : Schalter
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownElems =
        {
            "SoundSifa"
        };
#pragma warning restore IDE0052

        public Sifa(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class Sifa_ZeitWeg : Sifa
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Taster"
        };
#pragma warning restore IDE0052

        public Sifa_ZeitWeg(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class Sifa_ZeitZeit : Sifa
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Sprachausgabe"
        };

        private static readonly string[] _knownElems =
        {
            "SoundSifaZB"
        };
#pragma warning restore IDE0052

        public Sifa_ZeitZeit(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class Sifa86 : Sifa
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "PruefWeg"
        };
#pragma warning restore IDE0052

        public Sifa86(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
