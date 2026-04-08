using Sovoma;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Route
{
    public class Nach : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Nr"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly int _nr;

        public int Nr => _nr;

        //---------------------------------------------------------------------
        public Nach(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _nr = x.GetAttrValue("Nr", 0);
        }

        //---------------------------------------------------------------------
        public Nach(IZusiObjectParent parent, Nach source)
            : base(parent, source)
        {
            _nr = source._nr;
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeString("Nr", _nr.ToString());
        }
    }

    public class NachModul : Nach
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownElems =
        {
            "Datei"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly Datei _datei;

        public Datei Datei => _datei;

        //---------------------------------------------------------------------
        public NachModul(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _datei = new Datei(this, x.Element("Datei"));
        }

        //---------------------------------------------------------------------
        public NachModul(IZusiObjectParent parent, NachModul source)
            : base(parent, source)
        {
            _datei = new Datei(this, source._datei);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _datei.Save(writer);
        }
    }
}
