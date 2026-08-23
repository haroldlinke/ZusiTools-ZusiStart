using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class Pkt : ZusiGenericObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "PktX",
            "PktY"
        };
#pragma warning restore IDE0052

        public Pkt(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class Stufe : ZusiGenericObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "StufenWert"
        };
#pragma warning disable IDE0052

        public Stufe(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class Kennfeld : ZusiGenericObject
    {
        private static readonly string[] _knownAttribs =
        {
            "Beschreibung",
            "xText",
            "yText"
        };

        private static readonly string[] _knownElems =
        {
            "Pkt",
            "Stufe"
        };

        public Kennfeld(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
