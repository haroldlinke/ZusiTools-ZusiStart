using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class Schalter : ZusiGenericObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Taster",
            "FktName",
            "Tastaturzuordnung",
            "NameSchalter",
            "NameLM"
        };

        private static readonly string[] _knownElems =
        {
            "RastSound"
        };
#pragma warning restore IDE0052

        private readonly int _tastaturZuordnung;

        public string FktName => Attribute("FktName");

        public int TastaturZuordnung => _tastaturZuordnung;

        public Schalter(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            string tz = Attribute("Tastaturzuordnung");
            _tastaturZuordnung = string.IsNullOrEmpty(tz) ? 0 : int.Parse(tz);
        }
    }
}
