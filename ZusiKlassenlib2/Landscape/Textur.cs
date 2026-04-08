using log4net;
using Sovoma;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Texture;

namespace ZusiKlassenLib2.Landscape
{
    public class Textur : ZusiObject
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Textur));

        /* TransparentModus
         * 0: Keiner
         * 1: Farbwert vorgeben
         * 2: Farbwert unten links (nur bei .bmp-Texturen)
         */

        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "TransparentFarbe",
            "TransparentModus",
            "AnzahlMipMapLevel"
        };

        private static readonly string[] _knownElems =
        {
            "Datei"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly int _anzahlMipMapLevel;
        private readonly string _transparentFarbe;
        private readonly int _transparentModus;
        private ImageBrush _image;
        private TextureSize _textureSize;

        private Brush Image
        {
            get
            {
                if (_image == null)
                {
                    string path = _datei.FullPath;
                    if (!string.IsNullOrEmpty(path))
                    {
                        _image = TextureImageCache.GetElement(path, out _textureSize);
                    }
                    if (_image == null)
                    {
                        _log.Warn($"image '{_datei.Dateiname}' doesn't exists");
                    }
                    else
                    {
                        if (_textureSize.Width == 0 || _textureSize.Height == 0)
                        {
                            _log.Warn($"{_datei.Dateiname}: texture is empty");
                        }
                    }
                }
                return _image;
            }
        }

        private readonly Datei _datei;

        public string Filename { get { return _datei.FullPath; } }

        public TextureSize TextureSize
        {
            get
            {
                if (_image == null)
                {
                    _image = TextureImageCache.GetElement(_datei.FullPath, out _textureSize);
                    if (_image == null)
                    {
                        _log.Warn($"image '{_datei.Dateiname}' doesn't exists");
                    }
                    else
                    {
                        if (_textureSize.Width == 0 || _textureSize.Height == 0)
                        {
                            _log.WarnFormat("{0}: texture is empty", _datei.Dateiname);
                        }
                    }
                }
                return _textureSize;
            }
        }

        //---------------------------------------------------------------------
        public Textur(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _transparentFarbe = x.GetAttrValue("TransparentFarbe", "");
            _transparentModus = x.GetAttrValue("TransparentModus", 0);
            _anzahlMipMapLevel = x.GetAttrValue("AnzahlMipMapLevel", 0);

#if false
            if (!string.IsNullOrEmpty(_transparentFarbe) || _transparentModus > 0)
            {
                ZusiDocumentBase doc = GetDocument();
                if (!string.IsNullOrEmpty(_transparentFarbe))
                {
                    _log.Debug($"{doc?.Filename}: transparent color: {_transparentFarbe}");
                }
                if (_transparentModus > 0)
                {
                    _log.Debug($"{doc?.Filename}: transparent mode: {_transparentModus}");
                }
            }
#endif

            _datei = new Datei(this, x.Element("Datei"));
        }

        //---------------------------------------------------------------------
        public Textur(IZusiObjectParent parent, Textur source)
            : base(parent, source)
        {
            _transparentFarbe = source._transparentFarbe;
            _transparentModus = source._transparentModus;
            _anzahlMipMapLevel = source._anzahlMipMapLevel;

            _datei = new Datei(this, _datei);
        }

        //---------------------------------------------------------------------
        public void CollectRelatedFiles(List<string> files)
        {
            files.Add(_datei.FullPath);
        }

        //---------------------------------------------------------------------
        public bool IsRail()
        {
            return _datei != null && _datei.Dateiname.Contains("rail.dds");
        }

        //---------------------------------------------------------------------
        public Material ToMaterial(string diffuseColor, string emissiveColor)
        {
            Material material = null;

            if (ZusiColor.TryParse(emissiveColor, out Color ce))
            {
                if (ce.IsBlack())
                {
                    DiffuseMaterial m = new(Image ?? Brushes.Transparent);
                    if (ZusiColor.TryParse(diffuseColor, true, out Color cd))
                    {
                        Color c = cd.Coalescence(ce);
                        if (!c.IsWhite())
                        {
                            m.Color = c;
                        }
                    }
                    material = m;
                }
                else
                {
                    EmissiveMaterial m = new(Image ?? Brushes.Transparent);
                    m.Color = ce;
                    material = m;
                }
            }
            else
            {
                DiffuseMaterial m = new(Image ?? Brushes.Transparent);
#if false
            if (!string.IsNullOrEmpty(_transparentFarbe))
            {
                m.Color = _transparentFarbe.ToColor();
            }
#endif

                if (ZusiColor.TryParse(diffuseColor, true, out Color filter) && !filter.IsWhite())
                {
                    m.Color = filter;
                }

                material = m;
            }

            return material;
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeStringIfNotEmpty("TransparentFarbe", _transparentFarbe);
            writer.WriteAttributeIf(_transparentModus > 0, "TransparentModus", _transparentModus);
            writer.WriteAttributeIf(_anzahlMipMapLevel > 0, "AnzahlMipMapLevel", _anzahlMipMapLevel);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _datei.Save(writer);
        }
    }
}
