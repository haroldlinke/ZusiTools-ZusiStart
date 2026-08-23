using Sovoma;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Route
{
    [Flags]
    public enum StrElementFuncs
    {
        None = 0,
        Tunnel = 1 << 0,
        NoTrack = 1 << 1,
        JunctionSet = 1 << 2,
        NoShoulderRight = 1 << 3,
        NoShoulderLeft = 1 << 4
    }

    public class StrElement : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Nr",
            "Ueberh",
            "kr",
            "spTrass",
            "Anschluss",
            "Fkt",
            "Oberbau",
            "Volt",
            "Drahthoehe",
            "Zwangshelligkeit"
        };

        private static readonly string[] _knownElems =
        {
            "g",
            "b",
            "InfoNormRichtung",
            "InfoGegenRichtung",
            "NachNorm",
            "NachNormModul",
            "NachGegen",
            "NachGegenModul"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly int _nr;
        private readonly StrElementFuncs _fkt;
        private readonly double _kr;
        private readonly string _ueberh;
        private readonly string _spTrass;
        private readonly string _anschluss;
        private readonly string _oberbau;
        private readonly int _volt;
        private readonly string _drahtHoehe;
        private readonly string _zwangshelligkeit;

        private ZPoint3D _green;
        private ZPoint3D _blue;
        private readonly InfoRichtung _infoNormRichtung;
        private readonly InfoRichtung _infoGegenRichtung;
        private readonly List<Nach> _nachNorm = new();
        private readonly List<NachModul> _nachNormModul = new();
        private readonly List<Nach> _nachGegen = new();
        private readonly List<NachModul> _nachGegenModul = new();

        //---------------------------------------------------------------------
        public ZPoint3D Blue
        {
            get => _blue;
            private set
            {
                _blue = value;
                _blue.NodeName = "b";
            }
        }

        public double Curvature => _kr;

        public double Direction => _blue.Direction(_green);

        public StrElementFuncs Fkt => _fkt;

        public ZPoint3D Green
        {
            get => _green;
            private set
            {
                _green = value;
                _green.NodeName = "g";
            }
        }

        public double InvDirection => _blue.InvDirection(_green);

        public InfoRichtung InfoNormRichtung => _infoNormRichtung;
        public InfoRichtung InfoGegenRichtung => _infoGegenRichtung;

        public double Length => GetLength();

        public int Nr => _nr;

        public Signal Signal => GetSignal();

        public int Volt => _volt;

        //---------------------------------------------------------------------
        public StrElement(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _nr = x.GetAttrValue("Nr", 0);
            _ueberh = x.GetAttrValue("Ueberh", "");
            _kr = GetAttrValueDouble(x,"kr", 0.0);
            _spTrass = x.GetAttrValue("spTrass", "");
            _anschluss = x.GetAttrValue("Anschluss", "");
            _fkt = (StrElementFuncs)x.GetAttrValue("Fkt", 0);
            _oberbau = x.GetAttrValue("Oberbau", "");
            _volt = x.GetAttrValue("Volt", -1);
            _drahtHoehe = x.GetAttrValue("Drahthoehe", "");
            _zwangshelligkeit = x.GetAttrValue("Zwangshelligkeit", "");

            _green = new ZPoint3D(this, x.Element("g"));
            _blue = new ZPoint3D(this, x.Element("b"));

            _infoNormRichtung = x.GetOptionalElement(this, "InfoNormRichtung", (p, c) => { return new InfoRichtung(p, c); });
            _infoGegenRichtung = x.GetOptionalElement(this, "InfoGegenRichtung", (p, c) => { return new InfoRichtung(p, c); });

            foreach (XElement nn in x.Elements("NachNorm"))
            {
                _nachNorm.Add(new Nach(this, nn));
            }

            foreach (XElement ng in x.Elements("NachGegen"))
            {
                _nachGegen.Add(new Nach(this, ng));
            }

            foreach (XElement nnm in x.Elements("NachNormModul"))
            {
                _nachNormModul.Add(new NachModul(this, nnm));
            }

            foreach (XElement ngm in x.Elements("NachGegenModul"))
            {
                _nachGegenModul.Add(new NachModul(this, ngm));
            }
        }

        //---------------------------------------------------------------------
        public StrElement(IZusiObjectParent parent, StrElement source)
            : base(parent, source)
        {
            _nr = source._nr;
            _ueberh = source._ueberh;
            _kr = source._kr;
            _spTrass = source._spTrass;
            _anschluss = source._anschluss;
            _fkt = source._fkt;
            _oberbau = source._oberbau;
            _volt = source._volt;
            _drahtHoehe = source._drahtHoehe;
            _zwangshelligkeit = source._zwangshelligkeit;

            _green = new ZPoint3D(this, source._green);
            _blue = new ZPoint3D(this, source._blue);

            if (source._infoNormRichtung != null)
            {
                _infoNormRichtung = new InfoRichtung(this, source._infoNormRichtung);
            }
            if (source._infoGegenRichtung != null)
            {
                _infoGegenRichtung = new InfoRichtung(this, source._infoGegenRichtung);
            }

            foreach (Nach nn in source._nachNorm)
            {
                _nachNorm.Add(new Nach(this, nn));
            }

            foreach (Nach ng in source._nachGegen)
            {
                _nachGegen.Add(new Nach(this, ng));
            }

            foreach (NachModul nnm in source._nachNormModul)
            {
                _nachNormModul.Add(new NachModul(this, nnm));
            }

            foreach (NachModul ngm in source._nachGegenModul)
            {
                _nachGegenModul.Add(new NachModul(this, ngm));
            }

        }

#if false
        //---------------------------------------------------------------------
        public void AdjustFiles()
        {
            if (_infoNormRichtung != null)
            {
                _infoNormRichtung.AdjustFiles();
            }
            if (_infoGegenRichtung != null)
            {
                _infoGegenRichtung.AdjustFiles();
            }
        }
#endif

        //---------------------------------------------------------------------
        public void MoveBlueTo(double x, double y, double z)
        {
            double dx = _green.X - _blue.X;
            double dy = _green.Y - _blue.Y;
            double dz = _green.Z - _blue.Z;
            _blue.MoveTo(x, y, z);
            _green.MoveTo(_blue.X + dx, _blue.Y + dy, _blue.Z + dz);
        }

        //---------------------------------------------------------------------
        public void MoveBlueTo(ZPoint3D p)
        {
            MoveBlueTo(p.X, p.Y, p.Z);
        }

        //---------------------------------------------------------------------
        public void MoveBy(double xOfs, double yOfs, double zOfs)
        {
            _blue.MoveBy(xOfs, yOfs, zOfs);
            _green.MoveBy(xOfs, yOfs, zOfs);
            if (_infoNormRichtung != null && _infoNormRichtung.Signal != null)
            {
                _infoNormRichtung.Signal.MoveBy(xOfs, yOfs, zOfs);
            }
            if (_infoGegenRichtung != null && _infoGegenRichtung.Signal != null)
            {
                _infoGegenRichtung.Signal.MoveBy(xOfs, yOfs, zOfs);
            }
        }

        //---------------------------------------------------------------------
        public void MoveGreenTo(double x, double y, double z)
        {
            double dx = _blue.X - _green.X;
            double dy = _blue.Y - _green.Y;
            double dz = _blue.Z - _green.Z;
            _green.MoveTo(x, y, z);
            _blue.MoveTo(_green.X + dx, _green.Y + dy, _green.Z + dz);
        }

        //---------------------------------------------------------------------
        public void MoveGreenTo(ZPoint3D p)
        {
            MoveGreenTo(p.X, p.Y, p.Z);
        }

        //---------------------------------------------------------------------
        public void MoveXY(double distance, double arc)
        {
            Green = _green.MoveXY(distance, arc);
            Blue = _blue.MoveXY(distance, arc);
        }

        //---------------------------------------------------------------------
        public void RotateZ(ZPoint3D center, double arc)
        {
            Green = _green.RotateZ(center, arc);
            Blue = _blue.RotateZ(center, arc);
        }

        //---------------------------------------------------------------------
        public int NextGegen(int index)
        {
            return index >= 0 && index < _nachGegen.Count ? _nachGegen[index].Nr : 0;
        }

        //---------------------------------------------------------------------
        public int NextNorm(int index)
        {
            return index >= 0 && index < _nachNorm.Count ? _nachNorm[index].Nr : 0;
        }

#if false
        public double GetKm(StrDirection dir)
        {
            return dir == StrDirection.Norm ? (_infoNormRichtung != null ? _infoNormRichtung.Km : 0.0) : (_infoGegenRichtung != null ? _infoGegenRichtung.Km : 0.0);
        }
#endif

#if false
        public double SetKm(double km, bool ascending)
        {
            double res;
            double len = GetLength() / 1000.0;

            if (ascending)
            {
                if (_infoNormRichtung == null)
                {
                    _infoNormRichtung = new InfoNormRichtung();
                }
                _infoNormRichtung.Km = km;
                _infoNormRichtung.Pos = 1;

                if (_infoGegenRichtung == null)
                {
                    _infoGegenRichtung = new InfoGegenRichtung();
                }
                _infoGegenRichtung.Km = (km + len);
                _infoGegenRichtung.Pos = 0;
                res = _infoGegenRichtung.Km;
            }
            else
            {
                if (_infoNormRichtung == null)
                {
                    _infoNormRichtung = new InfoNormRichtung();
                }
                _infoNormRichtung.Km = (km - len);
                _infoNormRichtung.Pos = 0;
                res = _infoNormRichtung.Km;

                if (_infoGegenRichtung == null)
                {
                    _infoGegenRichtung = new InfoGegenRichtung();
                }
                _infoGegenRichtung.Km = km;
                _infoGegenRichtung.Pos = 1;
            }

            return res;
        }
#endif

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttribute("Nr", _nr);
            writer.WriteAttributeDoubleIf(_kr != 0, "kr", _kr, 4);
            writer.WriteAttributeStringIfNotEmpty("Anschluss", _anschluss);
            writer.WriteAttributeIf(_fkt != StrElementFuncs.None, "Fkt", (int)_fkt);
            writer.WriteAttributeStringIfNotEmpty("Drahthoehe", _drahtHoehe);
            writer.WriteAttributeIf(_volt > -1, "Volt", _volt);
            writer.WriteAttributeStringIfNotEmpty("Zwangshelligkeit", _zwangshelligkeit);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _green.Save(writer);
            _blue.Save(writer);
            if (_infoNormRichtung != null)
            {
                _infoNormRichtung.Save(writer);
            }
            if (_infoGegenRichtung != null)
            {
                _infoGegenRichtung.Save(writer);
            }
            foreach (Nach nn in _nachNorm)
            {
                nn.Save(writer);
            }
            foreach (Nach nnm in _nachNormModul)
            {
                nnm.Save(writer);
            }
            foreach (Nach ng in _nachGegen)
            {
                ng.Save(writer);
            }
            foreach (Nach ngm in _nachGegenModul)
            {
                ngm.Save(writer);
            }
        }

        //---------------------------------------------------------------------
        private double GetLength()
        {
            double dx = _blue.X - _green.X;
            double dy = _blue.Y - _green.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        //---------------------------------------------------------------------
        private Signal GetSignal()
        {
            if (_infoNormRichtung != null)
            {
                return _infoNormRichtung.Signal;
            }

            return null;
        }
    }
}
