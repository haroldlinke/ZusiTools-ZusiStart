using log4net;
using Sovoma;
using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace ZusiKlassenLib2.Common
{
    [Serializable]
    public class Datei : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Dateiname",
            "NurInfo"
        };
#pragma warning restore IDE0052
        #endregion

        private static readonly ILog _log = LogManager.GetLogger(typeof(Datei));

        private string _dateiname;
        private bool _nurInfo;
        private string _fullPath;
        private DataPathType _dataPath;

        public string Dateiname
        {
            get => _dateiname;
            set => _dateiname = value;
        }

        public DataPathType DataPath => _dataPath;

        public bool Exists => GetExists();

        public string FullPath => GetFullPath();

        public bool IsEmpty => string.IsNullOrEmpty(_dateiname) && !_nurInfo;

        public bool IsRelative => PathHelper.IsPathRelative(_dateiname);

        //---------------------------------------------------------------------
        public static Datei CreateNew(IZusiObjectParent parent, string dateiname, bool nurInfo)
        {
            Datei datei = CreateNew<Datei>(parent, "Datei");
            datei._dateiname = dateiname;
            datei._nurInfo = nurInfo;
            return datei;
        }

        //---------------------------------------------------------------------
        public Datei()
        { }

        //---------------------------------------------------------------------
        public Datei(IZusiObjectParent parent, XElement x)
            : this(parent, x, x.Name.LocalName)
        { }

        //---------------------------------------------------------------------
        public Datei(IZusiObjectParent parent, XElement x, string nodeName)
            : base(parent, x, nodeName)
        {
            _dateiname = x.GetAttrValue("Dateiname", "");
            _nurInfo = x.GetAttrValue("NurInfo", false);
        }

        //---------------------------------------------------------------------
        public Datei(IZusiObjectParent parent, Datei source)
            : base(parent, source)
        {
            _dateiname = source._dateiname;
            _nurInfo = source._nurInfo;
        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            if (_nurInfo)
            {
                return string.Format("\"{0}\" ({1})", _dateiname, _nurInfo);
            }
            else
            {
                return string.Format("\"{0}\"", _dateiname);
            }
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeStringIfNotEmpty("Dateiname", _dateiname);
            writer.WriteAttributeIf(_nurInfo, "NurInfo", 1);
        }

        //---------------------------------------------------------------------
        private bool GetExists()
        {
            string fp = GetFullPath();
            return !string.IsNullOrEmpty(fp) && File.Exists(fp);
        }

        //---------------------------------------------------------------------
        private string GetFullPath()
        {
            if (_fullPath == null && !string.IsNullOrEmpty(_dateiname))
            {
                if (string.IsNullOrEmpty(Path.GetDirectoryName(_dateiname)))
                {
                    ZusiDocumentBase document = GetDocument();
                    if (document != null)
                    {
                        System.Diagnostics.Debug.Assert(PathHelper.IsPathFullyQualified(document.Path));
                        _fullPath = PathHelper.BuildPath(document.Path, _dateiname);
                    }
                }
                else
                {
                    _fullPath = Zusi.GetAbsolutePathOf(_dateiname, ref _dataPath);
                }
            }

            if (string.IsNullOrEmpty(_fullPath))
            {
                _log.Warn($"Cannot resolve full path of {_dateiname}");
                _fullPath = string.Empty;
            }

            return _fullPath;
        }
    }
}
