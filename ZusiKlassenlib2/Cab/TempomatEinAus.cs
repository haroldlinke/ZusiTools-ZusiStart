using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class TempomatEinAus : Schalter
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "TempomatTyp"
        };
#pragma warning restore IDE0052

        public TempomatEinAus(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
