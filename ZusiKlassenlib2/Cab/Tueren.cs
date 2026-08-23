using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class TuerenTB0 : Schalter
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Sprachausgabe"
        };

        private static readonly string[] _knownElems =
        {
            "SoundTB0",
        };
#pragma warning restore IDE0052

        public TuerenTB0(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class Tueren : Schalter
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "NameDrehSchalter",
            "LMLi",
            "LMRe",
            "LMinvers",
            "LMZwangsschliessen",
            "SchalterTyp",
            "OhneFreigabe",
            "HaltebremseImmer",
            "Sprachausgabe"
        };

        private static readonly string[] _knownElems =
        {
            "SoundErinnerung",
            "SoundSchliessvorgang"
        };
#pragma warning restore IDE0052

        public Tueren(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class TuerenTAV : Tueren
    {
        public TuerenTAV(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class TuerenSAT : Tueren
    {
        public TuerenSAT(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class TuerenSST : Tueren
    {
        public TuerenSST(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class TuerenUICWTB : Tueren
    {
        public TuerenUICWTB(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
