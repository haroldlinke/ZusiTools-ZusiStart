using Sovoma;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Route
{
    public class InfoRichtung : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "vMax",
            "km",
            "pos",
            "Reg",
            "KoppelWeicheNorm",
            "KoppelWeicheNr"
        };

        private static readonly string[] _knownElems =
        {
            "Signal",
            "Ereignis"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly double _vmax;
        private readonly double _km;
        private int _pos;
        private readonly string _reg;
        private readonly Signal _signal;
        private readonly string _koppelWeicheNorm;
        private readonly string _koppelWeicheNr;
        private readonly List<Ereignis> _ereignisse = new();

        public List<Ereignis> Ereignisse => _ereignisse;

        public double Km => _km;

        public int Pos
        {
            get => _pos;
            set => _pos = value;
        }

        public Signal Signal => _signal;

        //---------------------------------------------------------------------
        public InfoRichtung(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _vmax = GetAttrValueDouble(x, "vMax", 0.0);
            _km = GetAttrValueDouble(x, "km", 0.0);
            _pos = x.GetAttrValue("pos", 0);
            _reg = x.GetAttrValue("Reg", "");
            _koppelWeicheNorm = x.GetAttrValue("KoppelWeicheNorm", "");
            _koppelWeicheNr = x.GetAttrValue("KoppelWeicheNr", "");

            _signal = x.GetOptionalElement(this, "Signal", (p, c) => new Signal(p, c));

            foreach (XElement xe in x.Elements("Ereignis"))
            {
                _ereignisse.Add(new Ereignis(this, xe));
            }
        }

        //---------------------------------------------------------------------
        public InfoRichtung(IZusiObjectParent parent, InfoRichtung source)
            : base(parent, source)
        {
            _vmax = source._vmax;
            _km = source._km;
            _pos = source._pos;
            _reg = source._reg;
            _koppelWeicheNorm = source._koppelWeicheNorm;
            _koppelWeicheNr = source._koppelWeicheNr;

            if (source._signal != null)
            {
                _signal = new Signal(this, source._signal);
            }

            foreach (Ereignis xe in source._ereignisse)
            {
                _ereignisse.Add(new Ereignis(this, xe));
            }
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeDoubleIf(_vmax != 0, "vMax", _vmax, 4);
            writer.WriteAttributeDoubleIf(_km != 0, "km", _km, 4);
            writer.WriteAttributeIf(_pos != 0, "pos", _pos);
            writer.WriteAttributeStringIfNotEmpty("Reg", _reg);
            writer.WriteAttributeStringIfNotEmpty("KoppelWeicheNorm", _koppelWeicheNorm);
            writer.WriteAttributeStringIfNotEmpty("KoppelWeicheNr", _koppelWeicheNr);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            if (_signal != null)
            {
                _signal.Save(writer);
            }

            foreach (Ereignis e in _ereignisse)
            {
                e.Save(writer);
            }
        }
    }
}
