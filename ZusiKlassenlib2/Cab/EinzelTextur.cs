using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class EinzelTextur : ZusiGenericObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Uol",
            "Vol",
            "Uur",
            "Vur",
            "TransparentFarbe",
            "TransparentModus"
        };

        private static readonly string[] _knownElems =
        {
            "Datei"
        };
#pragma warning restore IDE0052

        public EinzelTextur(IZusiObjectParent parent, XElement x)                    
            : base(parent, x)
        { }
    }
}
