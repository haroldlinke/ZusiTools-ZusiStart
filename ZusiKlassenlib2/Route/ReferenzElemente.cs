using Sovoma;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Route
{
    public class ReferenzElemente : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "ReferenzNr",
            "StrElement",
            "StrNorm",
            "RefTyp",
            "Info"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly int _referenzNr;
        private readonly int _strElement;
        private readonly int _strNorm;
        private readonly int _refTyp;
        private readonly string _info;

        public int ReferenzNr => _referenzNr;
        public int StrElement => _strElement;
        public int SstrNorm => _strNorm;
        public int RefTyp => _refTyp;
        public string Info => _info;

        //---------------------------------------------------------------------
        public ReferenzElemente(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _referenzNr = x.GetAttrValue("ReferenzNr", 0);
            _strElement = x.GetAttrValue("StrElement", 0);
            _strNorm = x.GetAttrValue("StrNorm", 0);
            _refTyp = x.GetAttrValue("RefTyp", 0);
            _info = x.GetAttrValue("Info", "");
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeIf(_referenzNr != 0, "ReferenzNr", _referenzNr);
            writer.WriteAttributeIf(_strElement != 0, "StrElement", _strElement);
            writer.WriteAttributeIf(_strNorm != 0, "StrNorm", _strNorm);
            writer.WriteAttributeIf(_refTyp != 0, "RefTyp", _refTyp);
            writer.WriteAttributeStringIfNotEmpty("Info", _info);
        }
    }
}
