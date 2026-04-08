using Sovoma;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;
using System.Linq;

namespace ZusiKlassenLib2.Route
{
    //=========================================================================
    public class FahrstrItem : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Ref"
        };

        private static readonly string[] _knownElems =
        {
            "Datei"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly int _ref;

        private readonly Datei _datei;

        public Datei Datei => _datei;
        public int Ref => _ref;

        //---------------------------------------------------------------------
        public FahrstrItem(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _ref = x.GetAttrValue("Ref", 0);

            _datei = new Datei(this, x.Element("Datei"));
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeIf(_ref != 0, "Ref", _ref);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _datei.Save(writer);
        }
    }

    //=========================================================================
    public class FahrstrSignal : FahrstrItem
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "FahrstrSignalZeile",
            "FahrstrSignalErsatzsignal"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly string _ersatzSignal;
        private readonly string _zeile;

        public string ErsatzSignal => _ersatzSignal;
        public string Zeile => _zeile;

        //---------------------------------------------------------------------
        public FahrstrSignal(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _ersatzSignal = x.GetAttrValue("FahrstrSignalErsatzsignal", "");
            _zeile = x.GetAttrValue("FahrstrSignalZeile", "");
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            writer.WriteAttributeStringIfNotEmpty("FahrstrSignalZeile", _zeile);
            writer.WriteAttributeStringIfNotEmpty("FahrstrSignalErsatzsignal", _ersatzSignal);
        }
    }

    //=========================================================================
    public class FahrstrVSignal : FahrstrItem
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "FahrstrSignalSpalte"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly string _spalte;

        public string Spalte => _spalte;

        //---------------------------------------------------------------------
        public FahrstrVSignal(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _spalte = x.GetAttrValue("FahrstrSignalSpalte", "");
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            writer.WriteAttributeStringIfNotEmpty("FahrstrSignalSpalte", _spalte);
        }
    }

    //=========================================================================
    public class FahrstrWeiche : FahrstrItem
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "FahrstrWeichenlage"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly string _weichenlage;

        public string Weichenlage => _weichenlage;

        //---------------------------------------------------------------------
        public FahrstrWeiche(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _weichenlage = x.GetAttrValue("FahrstrWeichenlage", "");
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            writer.WriteAttributeStringIfNotEmpty("FahrstrWeichenlage", _weichenlage);
        }
    }

    //=========================================================================
    public class Fahrstrasse : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "FahrstrName",
            "FahrstrTyp",
            "FahrstrStrecke",
            "RglGgl",
            "Laenge",
            "ZufallsWert"
        };

        private static readonly string[] _knownElems =
        {
            "FahrstrStart",
            "FahrstrZiel",
            "FahrstrRegister",
            "FahrstrWeiche",
            "FahrstrSignal",
            "FahrstrVSignal",
            "FahrstrSigHaltfall",
            "FahrstrAufloesung",
            "FahrstrTeilaufloesung"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly string _name;
        private readonly string _typ;
        private readonly string _strecke;
        private readonly string _laenge;
        private readonly string _rglggl;
        private readonly string _zufallswert;

        private readonly FahrstrItem _start;
        private readonly FahrstrItem _ziel;
        private readonly List<FahrstrItem> _register = new();
        private readonly List<FahrstrWeiche> _weiche = new();
        private readonly List<FahrstrSignal> _signal = new();
        private readonly List<FahrstrVSignal> _vSignal = new();
        private readonly List<FahrstrItem> _sigHaltfall = new();
        private readonly List<FahrstrItem> _aufloesung = new();
        private readonly List<FahrstrItem> _teilaufloesung = new();

        public string Name => _name;
        public string Typ => _typ;
        public string Strecke => _strecke;
        public string Laenge => _laenge;
        public string Rglggl => _rglggl;
        public string Zufallswert => _zufallswert;

        public FahrstrItem Start => _start;
        public FahrstrItem Ziel => _ziel;
        public List<FahrstrItem> Register => _register;
        public List<FahrstrWeiche> Weiche => _weiche;
        public List<FahrstrSignal> Signal => _signal;
        public List<FahrstrVSignal> VSignal => _vSignal;
        public List<FahrstrItem> SigHaltfall => _sigHaltfall;
        public List<FahrstrItem> Aufloesung => _aufloesung;
        public List<FahrstrItem> Teilaufloesung => _teilaufloesung;

        //---------------------------------------------------------------------
        public Fahrstrasse(IZusiObjectParent parent, XElement x)
                : base(parent, x)
        {
            _name = x.GetAttrValue("FahrstrName", "");
            _typ = x.GetAttrValue("FahrstrTyp", "");
            _strecke = x.GetAttrValue("FahrstrStrecke", "");
            _rglggl = x.GetAttrValue("RglGgl", "");
            _laenge = x.GetAttrValue("Laenge", "");
            _zufallswert = x.GetAttrValue("ZufallsWert", "");

            _start = GetOptionalObject<FahrstrItem>(this, x.Element("FahrstrStart"));
            _ziel = GetOptionalObject<FahrstrItem>(this, x.Element("FahrstrZiel"));
            _register.AddRange(from XElement xi in x.Elements("FahrstrRegister")
                               select new FahrstrItem(this, xi));
            _weiche.AddRange(from XElement xi in x.Elements("FahrstrWeiche")
                             select new FahrstrWeiche(this, xi));
            _signal.AddRange(from XElement xi in x.Elements("FahrstrSignal")
                             select new FahrstrSignal(this, xi));
            _vSignal.AddRange(from XElement xi in x.Elements("FahrstrVSignal")
                              select new FahrstrVSignal(this, xi));
            _sigHaltfall.AddRange(from XElement xi in x.Elements("FahrstrSigHaltfall")
                                  select new FahrstrItem(this, xi));
            _aufloesung.AddRange(from XElement xi in x.Elements("FahrstrAufloesung")
                                 select new FahrstrItem(this, xi));
            _teilaufloesung.AddRange(from XElement xi in x.Elements("FahrstrTeilaufloesung")
                                     select new FahrstrItem(this, xi));
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeStringIfNotEmpty("FahrstrName", _name);
            writer.WriteAttributeStringIfNotEmpty("FahrstrTyp", _typ);
            writer.WriteAttributeStringIfNotEmpty("FahrstrStrecke", _strecke);
            writer.WriteAttributeStringIfNotEmpty("RglGgl", _rglggl);
            writer.WriteAttributeStringIfNotEmpty("Laenge", _laenge);
            writer.WriteAttributeStringIfNotEmpty("ZufallsWert", _zufallswert);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _start?.Save(writer);
            _ziel?.Save(writer);

            foreach (FahrstrItem item in _register)
            {
                item.Save(writer);
            }

            foreach (FahrstrItem item in _weiche)
            {
                item.Save(writer);
            }

            foreach (FahrstrItem item in _signal)
            {
                item.Save(writer);
            }

            foreach (FahrstrItem item in _vSignal)
            {
                item.Save(writer);
            }

            foreach (FahrstrItem item in _sigHaltfall)
            {
                item.Save(writer);
            }

            foreach (FahrstrItem item in _aufloesung)
            {
                item.Save(writer);
            }

            foreach (FahrstrItem item in _teilaufloesung)
            {
                item.Save(writer);
            }
        }
    }
}
