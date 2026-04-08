using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class GraphicElement : ZusiGenericObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "InstrName",
            "Faktor",
            "DunkelFarbe",
            "PhysikGroesse",
            "Nachkomma"
        };

        private static readonly string[] _knownElems =
        {
            "FstTextur",
            "InstrPkt1",
            "Ueberdeckend"
        };
#pragma warning restore IDE0052

        public string InstrumentName
        {
            get
            {
                return _attributes.ContainsKey("InstrName") ? _attributes["InstrName"] : null;
            }
        }

        public GraphicElement(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class Melder : GraphicElement
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "MaxBildNr",
            "Vorkomma",
            "Farbe",
            "Infotext",
            "MinWert",
            "MaxWert",
            "Texturfilter",
            "TCPTyp",
            "TCPEbene4",
            "MittelNr",
            "KleinerGrenzwertDunkel",
            "KleinerGrenzwert",
            "KleinerGrenzwertDunkel",
            "Runden",
            "ZusiDisplayAuswahl",
            "ZusiDisplayRahmen",
            "ZusiDisplayMaster"
        };

        private static readonly string[] _knownElems =
        {
            "InstrPkt2",
            "InstrPkt3",
            "InstrPkt4",
            "Font"
        };
#pragma warning restore IDE0052

        public Melder(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
