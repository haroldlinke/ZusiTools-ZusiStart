using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    public enum ZugdatenType
    {
        None,
        IndusiAnalog,
        IndusiRechner,
        PZ80,
        LZB80
    }

    [Serializable]
    public abstract class Indusi : Schalter
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "LMZugartM",
            "LMZugartO",
            "LMZugartU",

            "LM500Hz",
            "LM1000Hz",

            "SchalterWachsam",
            "SchalterFrei",
            "SchalterBefehl",

            "Zugarten",

            "IndusiGrenzdruckHL"
        };

        private static readonly string[] _knownElems =
        {
            "SoundIndusiHupe"
        };
#pragma warning restore IDE0052

        public abstract ZugdatenType ZugdatenType { get; }

        public Indusi(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
        }
    }

    [Serializable]
    public class IndusiI54 : Indusi
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "LMZugart"
        };
#pragma warning restore IDE0052

        public override ZugdatenType ZugdatenType => ZugdatenType.None;

        public IndusiI54(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
        }
    }

    [Serializable]
    public class IndusiI60 : Indusi
    {
        public override ZugdatenType ZugdatenType => ZugdatenType.IndusiAnalog;

        public IndusiI60(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
        }
    }

    [Serializable]
    public class IndusiI60DR : Indusi
    {
        public override ZugdatenType ZugdatenType => ZugdatenType.IndusiAnalog;

        public IndusiI60DR(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class IndusiI60M : Indusi
    {
        public override ZugdatenType ZugdatenType => ZugdatenType.IndusiAnalog;

        public IndusiI60M(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
        }
    }

    [Serializable]
    public class PZ80 : Indusi
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "LM60Gelb",
            "LM40Gelb",
            "LMLoeschen",
            "LMPZBEin"
        };
#pragma warning restore IDE0052

        public override ZugdatenType ZugdatenType => ZugdatenType.PZ80;

        public PZ80(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public abstract class PZB_LZB : Indusi
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "LMBefehl"
        };

        private static readonly string[] _knownElems =
        {
            "IndusiGrunddaten",
            "IndusiErsatzzugdaten"
        };
#pragma warning restore IDE0052

        public PZB_LZB(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
        }
    }

    [Serializable]
    public class IndusiI60R : PZB_LZB
    {
        public override ZugdatenType ZugdatenType => ZugdatenType.IndusiRechner;

        public IndusiI60R(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
        }
    }

    [Serializable]
    public class ZUB : Schalter
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "LMGNT",
            "LMGNTUe",
            "LMGNTG",
            "LMGNTS"
        };

        private static readonly string[] _knownElems =
        {
            "SoundGNT"
        };
#pragma warning restore IDE0052

        public ZUB(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public abstract class PZB : PZB_LZB
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownElems =
        {
            "ZUB"
        };
#pragma warning restore IDE0052

        public override ZugdatenType ZugdatenType => ZugdatenType.IndusiRechner;

        public PZB(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class LZB : PZB_LZB
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "LMLZBEnde",
            "LMLZBH",
            "LMLZBG",
            "LMLZBE40",
            "LMLZBB",
            "LMLZBEL",
            "LMLZBV40",
            "LMLZBS",
            "LMLZBUe",
            "LMLZBPruefen",
            "InstrLZBZielGeschw",
            "InstrLZBAFBSollGeschw",
            "InstrLZBSollGeschw",
            "InstrLZBZielweg",
            "BalkenLZBZielweg",
            "Sprachausgabe"
        };

        private static readonly string[] _knownElems =
        {
            "PZB",
            "SoundIndusiSchnarre",
            "SoundIndusiZB"
        };
#pragma warning restore IDE0052

        public override ZugdatenType ZugdatenType => ZugdatenType.LZB80;

        public LZB(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class LZB80_PZB90_20 : LZB
    {
        public LZB80_PZB90_20(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class LZB80_PZB90_20_ZUB262 : LZB
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownElems =
        {
            "ZUB",
        };
#pragma warning restore IDE0052

        public LZB80_PZB90_20_ZUB262(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class LZB80_CE_PZB90_20 : LZB
    {
        public LZB80_CE_PZB90_20(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class LZB80_I80 : LZB
    {
        public LZB80_I80(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class LZB80_CE_I80 : LZB
    {
        public LZB80_CE_I80(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class PZB90I60R_V20 : PZB
    {
        public PZB90I60R_V20(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class PZB90I60R_V20_ZUB262 : PZB
    {
        public PZB90I60R_V20_ZUB262(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class PZB90ER24_V20 : PZB
    {
        public PZB90ER24_V20(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class PZ80R : Indusi
    {
        public override ZugdatenType ZugdatenType => ZugdatenType.PZ80;

        public PZ80R(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
