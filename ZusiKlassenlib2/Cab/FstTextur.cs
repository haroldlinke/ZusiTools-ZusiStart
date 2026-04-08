using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class FstTextur : ZusiGenericObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "ALPHATESTENABLE",
            "SRCBLEND",
            "DESTBLEND"
        };

        private static readonly string[] _knownElems =
        {
            "EinzelTextur",
            "ALPHATESTENABLE",
            "SRCBLEND",
            "DESTBLEND"
        };
#pragma warning restore IDE0052

        public FstTextur(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
