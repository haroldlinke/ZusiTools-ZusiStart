using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class Ueberdeckend : ZusiGenericObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "InstrName"
        };
#pragma warning restore IDE0052

        public Ueberdeckend(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
