using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class Koppelung : ZusiGenericObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "NameGekoppelterSchalter",
            "Koppelart",
            "RastenNummer"
        };
#pragma warning restore IDE0052

        public Koppelung(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
