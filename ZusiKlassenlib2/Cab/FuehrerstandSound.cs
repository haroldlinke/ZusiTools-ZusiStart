using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class FuehrerstandSound : ZusiGenericObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownElems =
        {
            "Abhaengigkeit",
            "Sound"
        };
#pragma warning restore IDE0052

        public FuehrerstandSound(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
