using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class Grunddaten : ZusiGenericObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "GrafikName",
            "FtdHauptrichtung",
            "FtdLfdNr",
            "FtdHauptrichtung",
            "FtdQualitaet"
        };

        private static readonly string[] _knownElems =
        {
            "p",
            "lookat",
            "up"
        };
#pragma warning restore IDE0052

        public Grunddaten(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
