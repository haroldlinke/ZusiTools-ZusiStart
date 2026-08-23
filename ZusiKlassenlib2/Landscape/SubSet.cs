//#define CONVERT

using Sovoma;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Landscape
{
    public enum TypLs3
    {
        None,
        Grundplatte,
        Bahndamm,
        Stuetzmauer,
        Gleisbett,
        Schulter,
        Randweg,
        Gleiszwischenraum,
        Schiene,
        Radlenker,
        Bahnsteig,
        Strasse,
        Wasser,
        Tunnel,
        Fahrleitung,
        Wald,
        Dummy,
        Spitzenlicht_vorne,
        Schlusslicht_vorne,
        Spitzenlicht_hinten,
        Schlusslicht_hinten,
        Schiene_2D,
        Radlenker_2D
    }

    public enum TypGF
    {
        Ignore,
        Standard,
        Tunnel,
        Oberbau,
        Waldrand,
        Waldflaeche_GF,
        Grundplatte_GF,
        Hintergrund_GF
    }

    [Flags]
    public enum Beleuchtungstyp
    {
        None = 0,
        Schattenwurf = (1 << 0),
        Schattenempfang = (1 << 1),
        Lichtempfang = (1 << 2)
    }

    //=========================================================================
    public class SubSet : ZusiObject//, I3DModel
    {
        //private static readonly ILog _log = LogManager.GetLogger(typeof(SubSet));

        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Ca", "CA",
            "Cd", "C",
            "Ce", "E",
            "ls3Typ",   // veraltet
            "TypLs3",
            "GFTyp",    // veraltet
            "TypGF",
            "BeleuchtungTyp",
            "GruppenName",
            "MeterProTex",
            "MeterProTex2",
            "zBias",
            "MeshV",
            "MeshI",
            "DoppeltRendern",
            "Zwangshelligkeit",
            "Nachtumschaltung",
            "NachtEinstellung",
            "zZoom"
        };

        private static readonly string[] _knownElems =
        {
            "RenderFlags",
            "Textur",
            "Vertex",
            "Face"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly string _ca;
        private readonly string _cd;
        private readonly string _ce;
        private readonly TypLs3 _typLs3;
        private readonly TypGF _typGF;
        private readonly Beleuchtungstyp _beleuchtungTyp;
        private readonly string _gruppenName;
        private readonly string _meterProTex;
        private readonly string _meterProTex2;
        private readonly int _zBias;
        private int _meshV; // count of vertices
        private int _meshI; // count of indices
        private readonly string _doppeltRendern;
        private readonly string _zwangshelligkeit;
        private readonly string _nachtEinstellung;
        private readonly string _nachtumschaltung;
        private readonly string _zZoom;
        private readonly RenderFlags _renderFlags;
        private readonly List<Textur> _textures = new();
        private readonly List<Face> _faces = new();
        private readonly List<Vertex> _vertices = new();
        private Mesh _mesh;
        private MeshAnimation _animation;
        private readonly Queue<MatrixTransform3D> _transform = new();
        private int _renderWeightning;

        public string DiffuseColor => _cd;
        public string EmissiveColor => _ce;
        public Mesh Mesh => _mesh;
        public int MeshI => _meshI;
        public int MeshV => _meshV;
        public TypLs3 TypeLs3 => _typLs3;
        public TypGF TypeGF => _typGF;
        public List<Face> Faces => _faces;
        public List<Vertex> Vertices => _vertices;
        public int ZBias => _zBias;
        public MeshAnimation Animation
        {
            get { return _animation; }
            set { _animation = value; }
        }
        public Queue<MatrixTransform3D> Transforms { get { return _transform; } }
        public int RenderWeightning
        {
            get { return _renderWeightning; }
        }
        public List<Textur> Textures { get { return _textures; } }

        //---------------------------------------------------------------------
        public SubSet(IZusiObjectParent parent, XElement x, BinaryReader lsbReader)
            : base(parent, x)
        {
            _ca = GetAttrStringValueFromList(x, "Ca", "CA");
            _cd = GetAttrStringValueFromList(x, "Cd", "C");
            _ce = GetAttrStringValueFromList(x, "Ce", "E");
            _typLs3 = GetAttrEnumFromList(x, TypLs3.None, "TypLs3", "ls3Typ");
            _typGF = GetAttrEnumFromList(x, TypGF.Ignore, "TypGF", "GFType");
            _beleuchtungTyp = x.GetAttrEnum<Beleuchtungstyp>("BeleuchtungTyp", Beleuchtungstyp.None);
            _gruppenName = x.GetAttrValue("GruppenName", "");
            _meterProTex = x.GetAttrValue("MeterProTex", "");
            _meterProTex2 = x.GetAttrValue("MeterProTex2", "");
            _zBias = x.GetAttrValue("zBias", 0);
            _meshV = x.GetAttrValue("MeshV", 0);
            _meshI = x.GetAttrValue("MeshI", 0);
            _doppeltRendern = x.GetAttrValue("DoppeltRendern", "");
            _zwangshelligkeit = x.GetAttrValue("Zwangshelligkeit", "");
            _nachtEinstellung = x.GetAttrValue("NachtEinstellung", "");
            _nachtumschaltung = x.GetAttrValue("Nachtumschaltung", "");
            _zZoom = x.GetAttrValue("zZoom", "");

            _renderFlags = new RenderFlags(this, x.Element("RenderFlags"));

            foreach (XElement xt in x.Elements("Textur"))
            {
                _textures.Add(new Textur(this, xt));
            }

            foreach (XElement xv in x.Elements("Vertex"))
            {
                _vertices.Add(new Vertex(this, xv));
            }

            foreach (XElement xf in x.Elements("Face"))
            {
                _faces.Add(new Face(this, xf));
            }

            if (_meshI > 0 || _meshV > 0)
            {
            }

            if (lsbReader != null)
            {
                _mesh = new Mesh(lsbReader, _meshV, _meshI);
            }
            else
            {
                _mesh = new Mesh(_vertices, _faces);
            }

            CalculateRenderWeightning();








#if false
            ZusiDocumentBase doc = GetDocument();
            if (_textures.Count == 0)
            {
                if (ColorEx.TryParse(_ce, out Color ce))
                {
                    if (!ce.IsBlack())
                    {
                        if (!string.IsNullOrEmpty(_cd))
                        {
                            _log.Debug($"{doc?.Filename}: diffuse color ({_cd}) and emissive color ({_ce})");
                        }
                        else
                        {
                            _log.Debug($"{doc?.Filename}: emissive color ({_ce}) without texture");
                        }
                    }
                }
            }
#endif
        }

        //---------------------------------------------------------------------
        public SubSet(IZusiObjectParent parent, SubSet source)
            : base(parent, source)
        {
            _ca = source._ca;
            _cd = source._cd;
            _ce = source._ce;
            _typLs3 = source._typLs3;
            _typGF = source._typGF;
            _beleuchtungTyp = source._beleuchtungTyp;
            _gruppenName = source._gruppenName;
            _meterProTex = source._meterProTex;
            _meterProTex2 = source._meterProTex2;
            _zBias = source._zBias;
            _meshV = source._meshV;
            _meshI = source._meshI;
            _doppeltRendern = source._doppeltRendern;
            _zwangshelligkeit = source._zwangshelligkeit;
            _nachtEinstellung = source._nachtEinstellung;
            _nachtumschaltung = source._nachtumschaltung;
            _zZoom = source._zZoom;

            _renderFlags = new RenderFlags(this, source._renderFlags);

            foreach (Textur t in source._textures)
            {
                _textures.Add(new Textur(this, t));
            }

            _mesh = new Mesh(_mesh);
        }

        //---------------------------------------------------------------------
        public void CollectRelatedFiles(List<string> files)
        {
            _textures.ForEach(t => t.CollectRelatedFiles(files));
        }

        //---------------------------------------------------------------------
        public Model3D CreateModel(params AnimationInfo[] infos)
        {
            // Material
            MaterialGroup material = new();

            if (_textures.Count > 0)
            {
                Material m = _textures[0].ToMaterial(_cd, _ce);
                material.Children.Add(m);
            }
            else
            {
                DiffuseMaterial m = null;

                bool withTransparency = false;
                switch ((RenderFlagsType)_renderFlags)
                {
                    case RenderFlagsType.Transparency:
                    case RenderFlagsType.HalfTransparency1:
                    case RenderFlagsType.SignalLamp:
                    case RenderFlagsType.HalfTransparency2:
                        withTransparency = true;
                        break;
                }

#if OLDWAY
                if (ZusiColor.TryParse(_cd, withTransparency, out Color cd))
                {
                    m = new DiffuseMaterial(new SolidColorBrush(cd));
#if false
                    if (ColorEx.TryParse(_ca, true, out Color ca))
                    {
                        m.AmbientColor = ca;
                    };
#endif
                    material.Children.Add(m);
                }

#if true
                if (ZusiColor.TryParse(_ce, out Color ce) && !ce.IsBlack())
                {
                    //EmissiveMaterial em = new(new SolidColorBrush(ce));
                    EmissiveMaterial em = new() { Color = ce };
                    material.Children.Add(em);
                }
#endif
#else
                if (ZusiColor.TryParse(_cd, withTransparency, out Color cd))
                {
                    if (ZusiColor.TryParse(_ce, out Color ce))
                    {
                        m = new DiffuseMaterial(new SolidColorBrush(ZusiKlassenLib2.Common.ZusiColor.Coalescence(cd, ce)));
                    }
                    else
                    {
                        m = new DiffuseMaterial(new SolidColorBrush(cd));
                    }
                    material.Children.Add(m);
                }
#endif
            }

            // create model
            GeometryModel3D model = new(_mesh.CreateModel(), material);

            // apply animation
            if (_animation != null)
            {
                AnimationType at = _animation.AnimationType;
                AnimationInfo ai = infos.FirstOrDefault(i => i.Type == AnimationType.All || i.Type == at);
                if (ai != null)
                {
                    ai.Animations.Add(_animation);
                    _animation.AnimateTo(ai.Time);
                }
                model.Transform = _animation.Transform;
                //model.Transform = _animation.GetTransform3D(ai != null ? ai.AnimationTime : 0);
            }

            return model;
        }

        //---------------------------------------------------------------------
        public MeshInfo GetMeshInfo()
        {
            MeshInfo mi = _mesh.GetMeshInfo();
            foreach (Textur t in _textures)
            {
                mi.AddTextureSize(t.TextureSize);
            }
            return mi;
        }

        //---------------------------------------------------------------------
        public void MoveBy(double xOfs, double yOfs)
        {
            if (_mesh != null)
            {
                _mesh.MoveBy(xOfs, yOfs);
            }
        }

        //---------------------------------------------------------------------
        public void Pack(Huellkurve hk, double xOfs, double yOfs)
        {
            if (_mesh == null)
                return;

            ZPoint2D[] triangle = new ZPoint2D[3];

            for (int i = 0; i < _mesh.Indizes.Length; i += 3)
            {
                for (int j = 0, k = i; j < 3; j++, k++)
                {
                    MeshVertex mv = _mesh.Vertices[_mesh.Indizes[k]];
                    triangle[j] = new ZPoint2D(mv.Position.X + xOfs, mv.Position.Y + yOfs);
                }

                int n = 0;
                foreach (ZPoint2D p in triangle)
                {
                    if (hk.IsPointInside(p)) n++;
                }
                if (n == 0)
                {
                    for (int j = 0, k = i; j < 3; j++, k++)
                    {
                        _mesh.Indizes[k] = 65535;
                    }
                }
            }

            bool rebuild = false;
            int ii = 0;
            while (ii < _meshI)
            {
                if (_mesh.Indizes[ii] == 65535)
                {
                    Array.Copy(_mesh.Indizes, ii + 3, _mesh.Indizes, ii, _mesh.Indizes.Length - ii - 3);
                    _meshI -= 3;
                    System.Diagnostics.Debug.Assert(_meshI >= 0);
                    rebuild = true;
                }
                else
                {
                    ii += 3;
                }
            }
            if (_meshI > 0)
            {
                if (rebuild)
                    _mesh.RebuildIndizes(_meshI);
            }
            else
            {
                _mesh = null;
                _meshI = _meshV = 0;
            }
        }

        //---------------------------------------------------------------------
        public bool IsRail()
        {
            return (from t in _textures
                    where t.IsRail()
                    select t).FirstOrDefault() != null;
        }

        //---------------------------------------------------------------------
        public void SaveMesh(Stream stream)
        {
            _mesh?.Save(stream);
        }

        //---------------------------------------------------------------------
        public void Dump(TextWriter w)
        {
            _mesh?.Dump(w);
        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            return string.Format("Mesh ({0})", _renderWeightning);
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(System.Xml.XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeStringIfNotEmpty("Cd", _cd);
            writer.WriteAttributeStringIfNotEmpty("Ca", _ca);
            writer.WriteAttributeStringIfNotEmpty("Ce", _ce);
            writer.WriteAttributeIf(_typLs3 != TypLs3.None, "TypLs3", (int)_typLs3);
            writer.WriteAttributeIf(_typGF != TypGF.Ignore, "TypGF", (int)_typGF);
            writer.WriteAttributeStringIfNotEmpty("GruppenName", _gruppenName);
            writer.WriteAttributeIf(_beleuchtungTyp != Beleuchtungstyp.None, "BeleuchtungTyp", _beleuchtungTyp);
            writer.WriteAttributeStringIfNotEmpty("Zwangshelligkeit", _zwangshelligkeit);
            // Blink ?
            writer.WriteAttributeStringIfNotEmpty("MeterProTex", _meterProTex);
            writer.WriteAttributeStringIfNotEmpty("MeterProTex2", _meterProTex2);
            writer.WriteAttributeIf(_zBias != 0, "zBias", _zBias);
            writer.WriteAttributeStringIfNotEmpty("zZoom", _zZoom);
            writer.WriteAttributeStringIfNotEmpty("DoppeltRendern", _doppeltRendern);
            writer.WriteAttributeStringIfNotEmpty("Nachtumschaltung", _nachtumschaltung);
            writer.WriteAttributeStringIfNotEmpty("NachtEinstellung", _nachtEinstellung);
            writer.WriteAttributeIf(_meshV > 0, "MeshV", _meshV);
            writer.WriteAttributeIf(_meshI > 0, "MeshI", _meshI);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(System.Xml.XmlWriter writer)
        {
            base.SaveElements(writer);

            _renderFlags.Save(writer);

            foreach (Textur t in _textures)
            {
                t.Save(writer);
            }
        }

        //---------------------------------------------------------------------
        private void CalculateRenderWeightning()
        {
            // zBias
            int w = _zBias * -1000;
            if (_renderFlags != null && _renderFlags.RenderSemiTransparent)
            {
                w += 100;
            }
            _renderWeightning = w;
        }

        //---------------------------------------------------------------------
        private static T GetAttrEnumFromList<T>(XElement element, T defaultValue, params string[] valueNames) where T : struct, IComparable
        {
            if (valueNames != null)
            {
                foreach (string valueName in valueNames)
                {
                    XAttribute xa = element.Attribute(valueName);
                    if (xa != null)
                    {
                        if (Enum.TryParse(xa.Value, true, out T t))
                        {
                            return t;
                        }
                        else if (int.TryParse(xa.Value, out int v))
                        {
                            return (T)(object)v;
                        }
                    }
                }
            }

            return defaultValue;
        }

        //---------------------------------------------------------------------
        private static string GetAttrStringValueFromList(XElement element, params string[] valueNames)
        {
            if (valueNames != null)
            {
                foreach (string valueName in valueNames)
                {
                    XAttribute xa = element.Attribute(valueName);
                    if (xa != null)
                    {
                        return xa.Value;
                    }
                }
            }

            return string.Empty;
        }

        public void ExportTexCoords(TextWriter writer)
        {
            _mesh?.ExportTexCoords(writer);
        }

        public void Hack()
        {
            _mesh.Hack();
        }
    }
}
