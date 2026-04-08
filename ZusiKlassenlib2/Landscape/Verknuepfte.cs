using log4net;
using Sovoma;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Graphic;

namespace ZusiKlassenLib2.Landscape
{
    [Flags]
    public enum LODBit
    {
        LOD3 = (1 << 0),
        LOD2 = (1 << 1),
        LOD1 = (1 << 2),
        LOD0 = (1 << 3),
        All = LOD0 | LOD1 | LOD2 | LOD3
    }

    [Flags]
    public enum LoadFlagsType
    {
        Unknown_1 = 1 << 0,         //   1
        Unknown_2 = 1 << 1,         //   2
        TileFile = 1 << 2,          //   4 Kacheldatei
        Unknown_8 = 1 << 3,         //   8
        WriteProtect = 1 << 4,      //  16 Schreibschutz
        TileDetailFile = 1 << 5,    //  32 Kacheldatei
        Unknown_64 = 1 << 6,        //  64
        Unknown_128 = 1 << 7,       // 128
        Unknown_256 = 1 << 8        // 256
    }

    public class Verknuepfte : ZusiObject, I3DModel
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Verknuepfte));

#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Flags",
            "BoundingR",
            "SichtbarAb",
            "SichtbarBis",
            "Vorlade",
            "GruppenName",
            "LODbit",
            "Wichtig",
            "Helligkeit"
        };

        private static readonly string[] _knownElems =
        {
            "Datei",
            "p",
            "phi",
            "sk"
        };
#pragma warning restore IDE0052

        private MeshAnimation _linkedAnimation;
        private Landschaft _linkedLandscape;
        private readonly LoadFlagsType _flags;
        private readonly string _boundingR;
        private readonly float _sichtbarAb;
        private readonly float _sichtbarBis;
        private readonly string _vorlade;
        private readonly string _gruppenName;
        private readonly LODBit _lodBit;
        private readonly Datei _datei;
        private readonly ZPoint3D _p;
        private readonly ZPoint3D _phi;
        private readonly ZPoint3D _sk;
        private readonly string _wichtig;
        private readonly string _helligkeit;

        public MeshAnimation LinkedAnimation
        {
            get { return _linkedAnimation; }
            set { _linkedAnimation = value; }
        }
        public Landschaft LinkedLandscape { get { return GetLinkedLandscape(); } }
        public LoadFlagsType Flags { get { return _flags; } }
        public Datei Datei { get { return _datei; } }
        public LODBit LODBit { get { return _lodBit; } }
        public ZPoint3D P { get { return _p; } }
        public ZRect TileRect { get { return GetTileRect(); } }

        public Transform3D Transform { get { return GetTransform(); } }

        //---------------------------------------------------------------------
        public Verknuepfte(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _flags = (LoadFlagsType)x.GetAttrValue("Flags", 0);
            _boundingR = x.GetAttrValue("BoundingR", "");
            _sichtbarAb = x.GetAttrValue("SichtbarAb", 0f);
            _sichtbarBis = x.GetAttrValue("SichtbarBis", float.PositiveInfinity);
            _vorlade = x.GetAttrValue("Vorlade", "");
            _gruppenName = x.GetAttrValue("GruppenName", "");
            _lodBit = x.GetAttrValue("LODbit", LODBit.All);
            _wichtig = x.GetAttrValue("Wichtig", "");
            _helligkeit = x.GetAttrValue("Helligkeit", "");

            _datei = new Datei(this, x.Element("Datei"));
            _p = x.GetOptionalElement(this, "p", (p, c) => new ZPoint3D(p, c), ZPoint3D.Null);
            _phi = x.GetOptionalElement(this, "phi", (p, c) => new ZPoint3D(p, c), ZPoint3D.Null);
            _sk = x.GetOptionalElement(this, "sk", (p, c) => new ZPoint3D(p, c, 1.0, 1.0, 1.0));

            /* Flags
             *   1  unbekannt
             *   2  unbekannt
             *   4  Kacheldatei
             *   8
             *  16  3D-Objekt
             *  32  Kacheldetaildatei
             *  64
             * 128
             * 256
             */
            if (_flags.HasFlag(LoadFlagsType.TileFile))
            {
                string xn = _p.NodeName;
                _p = new ZPoint3D(Math.Floor(_p.X + 0.5), Math.Floor(_p.Y + 0.5), _p.Z)
                {
                    NodeName = xn
                };
            }
#if false
            else if (_flags == 16)
            {
                // 3D objects
            }
            else if (_flags == 32)
            {
            }
            else if (_flags != 0)
            {
                ZusiDocumentBase doc = GetDocument();
                Log.WarnFormat("Verknuepfte {0}: unrecognized flags = {1}", doc?.Filename, _flags);
            }
#endif
        }

        //---------------------------------------------------------------------
        public Verknuepfte(IZusiObjectParent parent, Verknuepfte source)
            : base(parent, source)
        {
            _flags = source._flags;
            _boundingR = source._boundingR;
            _sichtbarBis = source._sichtbarBis;
            _vorlade = source._vorlade;
            _gruppenName = source._gruppenName;
            _lodBit = source._lodBit;
            _wichtig = source._wichtig;
            _helligkeit = source._helligkeit;

            _datei = new Datei(this, source._datei);
            _p = new ZPoint3D(this, source._p);
            _phi = new ZPoint3D(this, source._phi);
            _sk = new ZPoint3D(this, source._sk);
        }

        //---------------------------------------------------------------------
        public List<LoDInfo> GetLodInfo()
        {
            List<LoDInfo> lodInfos = new();
            for (int i = 1, lod = 3; lod >= 0; i <<= 1, lod--)
            {
                if (_lodBit.HasFlag((LODBit)i))
                {
                    LoDInfo lodInfo = new(lod, _sichtbarAb, _sichtbarBis);
                    Landschaft ls = LinkedLandscape;
                    if (ls != null)
                    {
                        ls.GetLoDInfo(lodInfo);
                    }
                    lodInfos.Add(lodInfo);
                }
            }
            return lodInfos;
        }

        //---------------------------------------------------------------------
        public bool IsVisibleAt(float distance)
        {
            return distance >= _sichtbarAb && distance <= _sichtbarBis;
        }

        //---------------------------------------------------------------------
        public void CollectRelatedFiles(List<string> files)
        {
            files.Add(_datei.FullPath);
        }

        //---------------------------------------------------------------------
        public Model3D CreateModel(float distance, params AnimationInfo[] infos)
        {
            Model3D model = LinkedLandscape?.CreateModel(distance, infos);
            if (model != null)
            {
                Transform3D t = GetTransform();

                if (_linkedAnimation != null)
                {
                    Transform3D ta = _linkedAnimation.Transform;
                    AnimationType at = _linkedAnimation.AnimationType;
                    AnimationInfo ai = infos.FirstOrDefault(i => i.Type == AnimationType.All || i.Type == at);
                    if (ai != null)
                    {
                        ai.Animations.Add(_linkedAnimation);
                        _linkedAnimation.AnimateTo(ai.Time);
                    }

                    model.Transform = t.Merge(ta);
                }
                else
                {
                    model.Transform = t;
                }
            }

            return model;
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(System.Xml.XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeIf(_flags != 0, "Flags", _flags);
            writer.WriteAttributeStringIfNotEmpty("BoundingR", _boundingR);
            writer.WriteAttributeFloatIf(_sichtbarAb > 0, "SichtbarAb", _sichtbarAb, 0);
            writer.WriteAttributeFloatIf(_sichtbarBis > 0, "SichtbarBis", _sichtbarBis, 0);
            writer.WriteAttributeStringIfNotEmpty("Vorlade", _vorlade);
            writer.WriteAttributeStringIfNotEmpty("GruppenName", _gruppenName);
            writer.WriteAttribute("LODbit", (int)_lodBit);
            writer.WriteAttributeStringIfNotEmpty("Wichtig", _wichtig);
            writer.WriteAttributeStringIfNotEmpty("Helligkeit", _helligkeit);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(System.Xml.XmlWriter writer)
        {
            base.SaveElements(writer);

            _datei.Save(writer);
            _p.Save(writer);
            _phi.Save(writer);
            _sk.Save(writer);
        }

        private ZRect _tileRect = null;

        //---------------------------------------------------------------------
        private ZRect GetTileRect()
        {
            if (_tileRect == null)
            {
                _tileRect = new ZRect(_p.X - 500.0, _p.Y - 500.0, 1000.0, 1000.0);
            }
            return _tileRect;
        }

        //---------------------------------------------------------------------
        private Landschaft GetLinkedLandscape()
        {
            if (_linkedLandscape == null)
            {
                try
                {
                    LandschaftsDatei ld = new(_datei.FullPath);
                    ld.Parse(true);
                    _linkedLandscape = ld.Root;
                }
                catch (Exception ex)
                {
                    ZusiDocumentBase doc = GetDocument();
                    _log.Error($"{doc?.Filename}: linked landscape: {ex.Message}");
                }
            }

            return _linkedLandscape;
        }

        //---------------------------------------------------------------------
        /*          Zusi        WPF
         * roll:    x-axis      x-axis      Längsachse
         * pitch:   y-axis      z-axis      Querachse
         * yaw:     z-axis      y-axis      Hochachse
         * 
         * In case, the object should be rotated by more than one axis the 
         * order of the rotations are important. Accordingly to the Zusi
         * manual the order is
         *  1. yaw
         *  2. pitch
         *  3. roll
         */
        private Transform3D GetTransform()
        {
            Transform3DGroup result = new();

            if (_sk != null && !_sk.IsNullPoint)
            {
                result.Children.Add(new ScaleTransform3D(_sk.X, _sk.Y, _sk.Z));
            }
            if (_phi != null && !_phi.IsNullPoint)
            {
                Quaternion qr = Zusi3D.CreateFromYawPitchRoll(_phi);
                result.Children.Add(new RotateTransform3D(new QuaternionRotation3D(qr)));
            }
            if (_p != null && !_p.IsNullPoint)
            {
                result.Children.Add(new TranslateTransform3D(_p.ToVector3D()));
            }

            return result;
        }
    }
}
