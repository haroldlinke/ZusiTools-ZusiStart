using System;
using System.Xml.Linq;

namespace ZusiKlassenLib2.Common
{
    [Serializable]
    public class Sound : ZusiGenericObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Autostart",
            "Loop",
            "Lautstaerke",
            "PosAnlauf",
            "PosAuslauf",
            "GeschwAendern",
            "dreiD",
            "MinRadius",
            "MaxRadius"
        };

        private static readonly string[] _knownElems =
        {
             "Datei"
        };
#pragma warning restore IDE0052

        public Sound(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
