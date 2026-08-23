using Sovoma;
using System;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Vehicle
{
    //---------------------------------------------------------------------
    [Serializable]
    public class FahrzeugBeladung : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Beschreibung",
            "Masse",
            "Gefahrgut"
        };

        //---------------------------------------------------------------------
        private static readonly string[] _knownElems =
        {
            "Datei",
            "p",
            "phi"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly string _description;
        private readonly float _mass;
        private readonly Datei _datei;
        private readonly p _p;
        private readonly phi _phi;
        private readonly string _gefahrgut;

        public string Description => _description;
        public double Mass => _mass;
        public Datei Datei => _datei;
        public string Gefahrgut => _gefahrgut;
        public p P => _p;
        public phi Phi => _phi;

        //---------------------------------------------------------------------
        public FahrzeugBeladung(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _description = x.GetAttrValue("Beschreibung", string.Empty);
            _mass = x.GetAttrValue("Masse", 0.0f);
            _gefahrgut = x.GetAttrValue("Gefahrgut", string.Empty);

            _datei = new Datei(this, x.Element("Datei"));
            _p = new p(this, x.Element("p"));
            _phi = new phi(this, x.Element("phi"));
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeStringIfNotEmpty("Beschreibung", _description);
            writer.WriteAttributeIf(_mass > 0, "Masse", _mass);
            writer.WriteAttributeStringIfNotEmpty("Gefahrgut", _gefahrgut);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _datei.Save(writer);
            _p.Save(writer);
            _phi.Save(writer);
        }
    }
}
