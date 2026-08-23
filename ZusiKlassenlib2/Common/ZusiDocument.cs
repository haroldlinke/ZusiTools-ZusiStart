using Sovoma;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ZusiKlassenLib2.Common
{
    [Serializable]
    public class ZusiDocument<T> : ZusiDocumentBase where T : IZusiObject
    {
        //private static readonly ILog _log = LogManager.GetLogger(typeof(ZusiDocument<T>));

        private T _root;
        private readonly string[] _knownAttributes;
        private readonly string _docElementTag;

        public string DocElementTag => _docElementTag;

        public T Root => _root;

        //---------------------------------------------------------------------
        public static bool IsDocumentValid(string filename, string docElementTag)
        {
            bool valid = false;

            try
            {
                using FileStream fs = new(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                XDocument doc = XDocument.Load(XmlReader.Create(fs));
                XElement xroot = doc.Root;
                XElement xdoc = xroot.Element(docElementTag);
                if (xdoc != null)
                {
                    valid = true;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("{0}{1}   File: {2}", ex.ToString(), Environment.NewLine, filename);
            }

            return valid;
        }

        //---------------------------------------------------------------------
        public ZusiDocument(IZusiObjectParent parent, string filename)
            : this(parent, filename, Encoding.UTF8)
        { }

        //---------------------------------------------------------------------
        public ZusiDocument(IZusiObjectParent parent, string filename, Encoding encoding)
            : base(parent, filename, encoding)
        { }

        //---------------------------------------------------------------------
        public ZusiDocument(IZusiObjectParent parent, string filename, string[] knownAttributes, string docElementTag)
            : this(parent, filename, knownAttributes, docElementTag, Encoding.UTF8)
        { }

        //---------------------------------------------------------------------
        public ZusiDocument(IZusiObjectParent parent, string filename, string[] knownAttributes, string docElementTag, Encoding encoding)
            : base(parent, filename, encoding)
        {
            _knownAttributes = knownAttributes;
            _docElementTag = docElementTag;
        }

        //---------------------------------------------------------------------
        public ZusiDocument(string filename, WriteFlags flags)
            : base(filename, flags)
        { }

        //---------------------------------------------------------------------
        public ZusiDocument(string filename, WriteFlags flags, string[] knownAttributes, string docElementTag)
            : base(filename, flags)
        {
            _knownAttributes = knownAttributes;
            _docElementTag = docElementTag;
        }

        //---------------------------------------------------------------------
        public ZusiDocument(string filename, T root, string docElementTag, bool changeParent)
            : this(filename, root, docElementTag, changeParent, Encoding.UTF8)
        { }

        //---------------------------------------------------------------------
        public ZusiDocument(string filename, T root, string docElementTag, bool changeParent, Encoding encoding)
            : base(null, filename, encoding)
        {
            _root = root;
            _docElementTag = docElementTag;
            if (changeParent)
            {
                _root.ChangeParent(this);
            }
        }

        //---------------------------------------------------------------------
        public ZusiDocument(ZusiDocument<T> source, T root)
            : base(source)
        {
            _info = source?._info;
            _root = root;
            _root.ChangeParent(this);
        }

        //---------------------------------------------------------------------
        public override void AddMyAuthorEntry()
        {
            if (_info != null)
            {
                _info.AddMyAuthorEntry();
            }
        }

        //---------------------------------------------------------------------
        protected virtual T CreateDocElement(XElement xdoc)
        {
            return (T)Activator.CreateInstance(typeof(T), this, xdoc);
        }

        //---------------------------------------------------------------------
        protected override bool GetDirty()
        {
            return _info.IsDirty || _root.IsDirty || base.GetDirty();
        }

        //---------------------------------------------------------------------
        protected override void ParseDocument(XDocument doc)
        {
            XElement xroot = doc.Root;

            xroot.CheckAttributes((x, a) =>
            {
                _log.WarnFormat("File {0}:\n    Attribute '{1}' of node '{2}' is unknown", Filename, a.Name, x.Name);
                return true;
            }, _knownAttributes);
            xroot.CheckElements((n, s) =>
            {
                _log.WarnFormat("File {0}:\n    Element '{1}' isn't a known child of '{2}'", Filename, s.Name, n.Name.LocalName);
                return true;
            }, new string[] { "Info", _docElementTag });

            ReadRootAttributes(xroot);

            XElement xinfo = xroot.Element("Info");
            if (xinfo != null)
            {
                _info = new Info(this, xinfo);
            }

            XElement xdoc = xroot.Element(_docElementTag);
            if (xdoc == null)
            {
                throw new ArgumentException(string.Format("Specified doc element ({0}) doesn't exists", _docElementTag));
            }
            _root = CreateDocElement(xdoc);
        }

        //---------------------------------------------------------------------
        protected virtual void ReadRootAttributes(XElement xroot)
        { }

        //---------------------------------------------------------------------
        protected override void SaveDocument(XmlWriter writer)
        {
            _info?.Save(writer);
            _root.Save(writer);
        }
    }
}