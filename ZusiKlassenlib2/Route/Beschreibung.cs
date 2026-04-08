using Sovoma;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Route
{
    public class Beschreibung : ZusiGenericObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Beschreibung"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly string _beschreibung;

        //---------------------------------------------------------------------
        public Beschreibung(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _beschreibung = x.GetAttrValue("Beschreibung", string.Empty);
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeStringIfNotEmpty("Beschreibung", _beschreibung);
        }
    }
}
