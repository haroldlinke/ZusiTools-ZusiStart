using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class AFBEinAus : Schalter
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "BremsenHL",
            "Sprachausgabe"
        };

        private static readonly string[] _knownElems =
        {
            "Sound"
        };
#pragma warning restore IDE0052

        public AFBEinAus(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
