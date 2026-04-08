using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class NotbremsSystemBasis : Schalter
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "NameLMNotbremsung",
            "Sprachausgabe"
        };

        private static readonly string[] _knownElems =
        {
            "Sound"
        };
#pragma warning restore IDE0052

        public NotbremsSystemBasis(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class NotbremssystemUICNBUe : NotbremsSystemBasis
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "NameLMBereit",
            "NameLMNotbremsung"
        };
#pragma warning restore IDE0052

        public NotbremssystemUICNBUe(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class NotbremsSystemNBUe2004 : NotbremsSystemBasis
    {
        public NotbremsSystemNBUe2004(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class NotbremsSystemSBahn : NotbremssystemUICNBUe
    {
        public NotbremsSystemSBahn(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
