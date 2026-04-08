using Sovoma;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2
{
    //=========================================================================
    [Flags]
    public enum SignalFlags
    {
        None = 0,
        SF1 = 1 << 0,   // Fahrwegsignal wirkt auf beide Fahrtrichtungen
        SF2 = 1 << 1,   // Fahrwegsignal ist Weichenanimation
        SF3 = 1 << 2,   // Rangiersignal bei Zugfahrstrasse umstellen
        SF4 = 1 << 3,   // Signal enthält Bahnübergangssteuerung
        SF5 = 1 << 4,   // Kennlichtschaltung mit Nachfolgesignal
        SF6 = 1 << 5,   // Kennlichtschaltung mit Vorgängersignal
        SF7 = 1 << 6,   // Reisendendarstellung
        SF8 = 1 << 7    // Hochsignalisierung
    }

    public enum SignalType
    {
        Unknown,
        Plate,      //  1 Tafel
        Junction,   //  2 Weiche
        ST3,       //  3 Gleissperre
        ST4,       //  4 Bahnübergang
        ST5,       //  5 Rangiersignal
        ST6,       //  6 Vorsignal
        ST7,       //  7 Einfahrsignal
        ST8,       //  8 Zwischensignal
        ST9,       //  9 Ausfahrsignal
        ST10,      // 10 Blocksignal
        ST11,      // 11 Deckungssignal
        ST12,      // 12 Teilblock
        ST13,      // 13 Hilfshauptsignal
        Other,     // 14 sonstiges
    }

    public enum HandweichenBauart
    {
        Undefined,          //  0
        HandRechts,         //  1
        HandLinks,          //  2
        EOWRechts,          //  3
        EOWLinks,           //  4
        HandDKWLinks,       //  5
        HandDKWRechts,      //  6
        EOWDKWLinks,        //  7
        EOWDKWRechts,       //  8
        HandGleissperre,    //  9
        EOWGleissperre,     // 10
        HET,                // 11
        UT,                 // 12
        ZLB                 // 13
    }

    public enum HandweichenGrundstellung
    {
        Undefined,
        None,
        RightWhite,
        LeftWhite,
        RightYellow,
        LeftYellow
    }

    //=========================================================================
    public class SignalFrame : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "WeichenbaugruppeNr",
            "WeichenbaugruppeIndex",
            "WeichenbaugruppeBeschreibung",
            "WeichenbaugruppePos0",
            "WeichenbaugruppePos1"
        };

        private static readonly string[] _knownElems =
        {
            "p",
            "phi",
            "sk",
            "Datei"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly int _wbgNr;
        private readonly int _wbgIndex;
        private readonly string _wbgDescr;
        private readonly int _wbgPos0;
        private readonly int _wbgPos1;
        private readonly p _pivot;
        private readonly phi _phi;
        private readonly sk _sk;

        private readonly Datei _datei;

        public Datei Datei { get { return _datei; } }

        //---------------------------------------------------------------------
        public SignalFrame(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _wbgIndex = x.GetAttrValue("WeichenbaugruppeIndex", -1);
            _wbgNr = x.GetAttrValue("WeichenbaugruppeNr", -1);
            _wbgDescr = x.GetAttrValue("WeichenbaugruppeBeschreibung", (string)null);
            _wbgPos0 = x.GetAttrValue("WeichenbaugruppePos0", -1);
            _wbgPos1 = x.GetAttrValue("WeichenbaugruppePos1", -1);

            _pivot = new p(this, x.Element("p"));
            _phi = new phi(this, x.Element("phi"));
            _sk = x.GetOptionalElement(this, "sk", (p, c) => new sk(p, c));

            _datei = x.GetOptionalElement(this, "Datei", (p, c) => new Datei(p, c));
        }

        //---------------------------------------------------------------------
        public SignalFrame(IZusiObjectParent parent, SignalFrame source)
            : base(parent, source)
        {
            _wbgIndex = source._wbgIndex;
            _wbgNr = source._wbgNr;
            _wbgDescr = source._wbgDescr; ;
            _wbgPos0 = source._wbgPos0;
            _wbgPos1 = source._wbgPos1;

            _pivot = new p(this, source._pivot);
            _phi = new phi(this, source._phi);
            _sk = new sk(this, source._sk);

            if (source._datei != null)
            {
                _datei = new Datei(this, source._datei);
            }
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeIf(_wbgIndex > -1, "WeichenbaugruppeIndex", _wbgIndex);
            writer.WriteAttributeIf(_wbgNr > -1, "WeichenbaugruppeNr", _wbgNr);
            writer.WriteAttributeStringIfNotEmpty("WeichenbaugruppeBeschreibung", _wbgDescr);
            writer.WriteAttributeIf(_wbgPos0 > -1, "WeichenbaugruppePos0", _wbgPos0);
            writer.WriteAttributeIf(_wbgPos1 > -1, "WeichenbaugruppePos1", _wbgPos1);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _pivot.Save(writer);
            _phi.Save(writer);
            _sk?.Save(writer);
            _datei?.Save(writer);
        }
    }

    //=========================================================================
    public class KoppelSignal : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "ReferenzNr"
        };

        private static readonly string[] _knownElems =
        {
            "Datei"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly string _refNr;

        private readonly Datei _datei;

        public bool IsEmpty
        {
            get
            {
                return (_datei == null || _datei.IsEmpty) && string.IsNullOrEmpty(_refNr);
            }
        }

        //---------------------------------------------------------------------
        public KoppelSignal(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _refNr = x.GetAttrValue("ReferenzNr", "");

            _datei = x.GetOptionalElement<Datei>(this, "Datei", (p, c) => { return new Datei(p, c); });
        }

        //---------------------------------------------------------------------
        public KoppelSignal(IZusiObjectParent parent, KoppelSignal source)
            : base(parent, source)
        {
            _refNr = source._refNr;

            if (source._datei != null)
            {
                _datei = new Datei(this, source._datei);
            }
        }

        //---------------------------------------------------------------------
        public override void Save(XmlWriter writer)
        {
            if (!IsEmpty)
            {
                base.Save(writer);
            }
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            if (!string.IsNullOrEmpty(_refNr))
                writer.WriteAttributeString("ReferenzNr", _refNr);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            if (_datei != null)
                _datei.Save(writer);
        }
    }

    //=========================================================================
    public class HsigBegriff : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "FahrstrTyp",
            "HsigGeschw"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly int _fahrstrTyp;
        private readonly double _hsigGeschw;

        //---------------------------------------------------------------------
        public HsigBegriff(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _fahrstrTyp = x.GetAttrValue("FahrstrTyp", 0);
            _hsigGeschw = x.GetAttrValue("HsigGeschw", double.NaN);
        }

        //---------------------------------------------------------------------
        public HsigBegriff(IZusiObjectParent parent, HsigBegriff source)
            : base(parent, source)
        {
            _fahrstrTyp = source._fahrstrTyp;
            _hsigGeschw = source._hsigGeschw;
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeIf(_fahrstrTyp != 0, "FahrstrTyp", _fahrstrTyp.ToString());
            writer.WriteAttributeDoubleIfNotNaN("HsigGeschw", _hsigGeschw, 4);
        }
    }

    //=========================================================================
    public class VsigBegriff : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "VsigGeschw"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly double _vsigGeschw;

        //---------------------------------------------------------------------
        public VsigBegriff(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _vsigGeschw = x.GetAttrValue("VsigGeschw", double.NaN);
        }

        //---------------------------------------------------------------------
        public VsigBegriff(IZusiObjectParent parent, VsigBegriff source)
            : base(parent, source)
        {
            _vsigGeschw = source._vsigGeschw;
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeDoubleIfNotNaN("VsigGeschw", _vsigGeschw, 4);
        }
    }

    //=========================================================================
    public class MatrixEintrag : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Signalbild",
            "SignalID",
            "MatrixGeschw"
        };

        private static readonly string[] _knownElems =
        {
            "Ereignis"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly long _signalBild;
        private readonly string _signalID;
        private readonly double _matrixGeschw;

        private readonly List<Ereignis> _ereignisse = new();

        //---------------------------------------------------------------------
        public MatrixEintrag(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _signalBild = x.GetAttrValue("Signalbild", 0L);
            _signalID = x.GetAttrValue("SignalID", "");
            _matrixGeschw = x.GetAttrValue("MatrixGeschw", double.NaN);

            foreach (XElement xe in x.Elements("Ereignis"))
            {
                _ereignisse.Add(new Ereignis(this, xe));
            }
        }

        //---------------------------------------------------------------------
        public MatrixEintrag(IZusiObjectParent parent, MatrixEintrag source)
            : base(parent, source)
        {
            _signalBild = source._signalBild;
            _signalID = source._signalID;
            _matrixGeschw = source._matrixGeschw;

            foreach (Ereignis xe in source._ereignisse)
            {
                _ereignisse.Add(new Ereignis(this, xe));
            }
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeIf(_signalBild != 0, "Signalbild", _signalBild);
            writer.WriteAttributeStringIfNotEmpty("SignalID", _signalID);
            writer.WriteAttributeDoubleIfNotNaN("MatrixGeschw", _matrixGeschw, 4);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            foreach (Ereignis e in _ereignisse)
            {
                e.Save(writer);
            }
        }
    }

    //=========================================================================
    public class Ersatzsignal : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "ErsatzsigID", "ErsatzsigBezeichnung"
        };

        private static readonly string[] _knownElems =
        {
            "MatrixEintrag"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly string _id;
        private readonly string _bezeichnung;

        private readonly MatrixEintrag _matrix;

        //---------------------------------------------------------------------
        public Ersatzsignal(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _id = x.GetAttrValue("ErsatzsigID", "");
            _bezeichnung = x.GetAttrValue("ErsatzsigBezeichnung", "");

            _matrix = new MatrixEintrag(this, x.Element("MatrixEintrag"));
        }

        //---------------------------------------------------------------------
        public Ersatzsignal(IZusiObjectParent parent, Ersatzsignal source)
            : base(parent, source)
        {
            _id = source._id;
            _bezeichnung = source._bezeichnung;

            _matrix = new MatrixEintrag(this, source._matrix);
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            if (!string.IsNullOrEmpty(_id))
                writer.WriteAttributeString("ErsatzsigID", _id);
            if (!string.IsNullOrEmpty(_bezeichnung))
                writer.WriteAttributeString("ErsatzsigBezeichnung", _bezeichnung);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _matrix.Save(writer);
        }
    }

    //=========================================================================
    public class Signal : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "SignalTyp",
            "SignalFlags",
            "BoundingR",
            "Signalname",
            "Stellwerk",
            "NameBetriebsstelle",
            "ZufallsWert",
            "Weichenbauart",
            "WeichenGrundstellung",
            "Zwangshelligkeit",
            "WeicheStumpfIgnorieren"
        };

        private static readonly string[] _knownElems =
        {
            "p",
            "phi",
            "KoppelSignal",
            "SignalFrame",
            "HsigBegriff",
            "VsigBegriff",
            "MatrixEintrag",
            "Ersatzsignal"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly SignalType _typ;
        private readonly SignalFlags _flags;
        private readonly int _boundingR;
        private readonly string _signalName;
        private readonly string _stellwerk;
        private readonly string _nameBetrSt;
        private readonly string _zufallswert;
        private readonly HandweichenBauart _weichenBauart;
        private readonly HandweichenGrundstellung _weichenGrundstellung;
        private readonly bool _weicheStumpfIgnorieren;
        private readonly string _zwangshelligkeit;
        private ZPoint3D _pivot;
        private ZPoint3D _phi;
        private readonly KoppelSignal _koppelSignal;
        private readonly List<SignalFrame> _frames = new();
        private readonly List<HsigBegriff> _hsigBegriffe = new();
        private readonly List<VsigBegriff> _vsigBegriffe = new();
        private readonly List<MatrixEintrag> _matrixEintraege = new();
        private readonly List<Ersatzsignal> _ersatzSignale = new();

        public SignalFlags Flags => _flags;

        public ZPoint3D Pivot
        {
            get { return _pivot; }
            set
            {
                _pivot = value;
                _pivot.NodeName = "p";
            }
        }

        public ZPoint3D Phi
        {
            get { return _phi; }
            set
            {
                _phi = value;
                _phi.NodeName = "phi";
            }
        }

        public SignalType SignalType => _typ;

        public HandweichenBauart WeichenBauart => _weichenBauart;

        public HandweichenGrundstellung WeichenGrundstellung => _weichenGrundstellung;

        //---------------------------------------------------------------------
        public Signal(IZusiObjectParent parent, XElement xsignal)
            : base(parent, xsignal)
        {
            _typ = (SignalType)xsignal.GetAttrValue("SignalTyp", 0);
            _flags = (SignalFlags)xsignal.GetAttrValue("SignalFlags", 0);
            _boundingR = xsignal.GetAttrValue("BoundingR", 0);
            _signalName = xsignal.GetAttrValue("Signalname", "");
            _stellwerk = xsignal.GetAttrValue("Stellwerk", "");
            _nameBetrSt = xsignal.GetAttrValue("NameBetriebsstelle", "");
            _zufallswert = xsignal.GetAttrValue("ZufallsWert", "");
            _weichenBauart = xsignal.GetAttrEnum<HandweichenBauart>("Weichenbauart", HandweichenBauart.Undefined);
            _weichenGrundstellung = xsignal.GetAttrEnum<HandweichenGrundstellung>("WeichenGrundstellung", HandweichenGrundstellung.Undefined);
            _weicheStumpfIgnorieren = xsignal.GetAttrValue("WeicheStumpfIgnorieren", false);
            _zwangshelligkeit = xsignal.GetAttrValue("Zwangshelligkeit", "");

            _pivot = new ZPoint3D(this, xsignal.Element("p"));
            _phi = new ZPoint3D(this, xsignal.Element("phi"));

            XElement xks = xsignal.Element("KoppelSignal");
            if (xks != null)
            {
                _koppelSignal = new KoppelSignal(this, xks);
            }

            foreach (XElement xf in xsignal.Elements("SignalFrame"))
            {
                _frames.Add(new SignalFrame(this, xf));
            }

            foreach (XElement xh in xsignal.Elements("HsigBegriff"))
            {
                _hsigBegriffe.Add(new HsigBegriff(this, xh));
            }

            foreach (XElement xv in xsignal.Elements("VsigBegriff"))
            {
                _vsigBegriffe.Add(new VsigBegriff(this, xv));
            }

            foreach (XElement xm in xsignal.Elements("MatrixEintrag"))
            {
                _matrixEintraege.Add(new MatrixEintrag(this, xm));
            }

            foreach (XElement xes in xsignal.Elements("Ersatzsignal"))
            {
                _ersatzSignale.Add(new Ersatzsignal(this, xes));
            }
        }

        //---------------------------------------------------------------------
        public Signal(IZusiObjectParent parent, Signal source)
            : base(parent, source)
        {
            _typ = source._typ;
            _flags = source._flags;
            _boundingR = source._boundingR;
            _signalName = source._signalName;
            _stellwerk = source._stellwerk;
            _nameBetrSt = source._nameBetrSt;
            _zufallswert = source._zufallswert;
            _weichenBauart = source._weichenBauart;
            _weichenGrundstellung = source._weichenGrundstellung;
            _weicheStumpfIgnorieren = source._weicheStumpfIgnorieren;
            _zwangshelligkeit = source._zwangshelligkeit;

            _pivot = new ZPoint3D(this, source._pivot);
            _phi = new ZPoint3D(this, source._phi);

            if (source._koppelSignal != null)
            {
                _koppelSignal = new KoppelSignal(this, source._koppelSignal);
            }

            foreach (SignalFrame xf in source._frames)
            {
                _frames.Add(new SignalFrame(this, xf));
            }

            foreach (HsigBegriff xh in source._hsigBegriffe)
            {
                _hsigBegriffe.Add(new HsigBegriff(this, xh));
            }

            foreach (VsigBegriff xv in source._vsigBegriffe)
            {
                _vsigBegriffe.Add(new VsigBegriff(this, xv));
            }

            foreach (MatrixEintrag xm in source._matrixEintraege)
            {
                _matrixEintraege.Add(new MatrixEintrag(this, xm));
            }

            foreach (Ersatzsignal es in source._ersatzSignale)
            {
                _ersatzSignale.Add(new Ersatzsignal(this, es));
            }
        }

        //---------------------------------------------------------------------
        public void MoveBy(double xOfs, double yOfs, double zOfs)
        {
            _pivot.MoveBy(xOfs, yOfs, zOfs);
        }

#if false
        //---------------------------------------------------------------------
        public void AdjustFiles()
        {
            foreach (SignalFrame f in _frames)
            {
                f.AdjustFile();
            }
        }
#endif

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeIf(_typ != SignalType.Unknown, "SignalTyp", (int)_typ);
            writer.WriteAttributeIf(_flags != SignalFlags.None, "SignalFlags", (int)_flags);
            writer.WriteAttributeIf(_boundingR > 0, "BoundingR", _boundingR);
            writer.WriteAttributeStringIfNotEmpty("Signalname", _signalName);
            writer.WriteAttributeStringIfNotEmpty("Stellwerk", _stellwerk);
            writer.WriteAttributeStringIfNotEmpty("NameBetriebsstelle", _nameBetrSt);
            writer.WriteAttributeStringIfNotEmpty("ZufallsWert", _zufallswert);
            writer.WriteAttributeIf(_weichenBauart != HandweichenBauart.Undefined, "Weichenbauart", (int)_weichenBauart);
            writer.WriteAttributeIf(_weichenGrundstellung != HandweichenGrundstellung.Undefined, "WeichenGrundstellung", (int)_weichenGrundstellung);
            writer.WriteAttributeIf(_weicheStumpfIgnorieren, "WeicheStumpfIgnorieren", _weicheStumpfIgnorieren ? 1 : 0);
            writer.WriteAttributeStringIfNotEmpty("Zwangshelligkeit", _zwangshelligkeit);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _pivot.Save(writer);
            _phi.Save(writer);
            if (_koppelSignal != null)
            {
                _koppelSignal.Save(writer);
            }

            foreach (SignalFrame sf in _frames)
            {
                sf.Save(writer);
            }

            foreach (HsigBegriff hs in _hsigBegriffe)
            {
                hs.Save(writer);
            }

            foreach (VsigBegriff vs in _vsigBegriffe)
            {
                vs.Save(writer);
            }

            foreach (MatrixEintrag me in _matrixEintraege)
            {
                me.Save(writer);
            }

            foreach (Ersatzsignal es in _ersatzSignale)
            {
                es.Save(writer);
            }
        }
    }
}
