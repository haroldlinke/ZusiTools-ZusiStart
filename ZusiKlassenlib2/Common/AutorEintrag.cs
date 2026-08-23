using Sovoma;
using System;
using System.Xml;
using System.Xml.Linq;

namespace ZusiKlassenLib2.Common
{
    [Serializable]
    public class AutorEintrag : ZusiObject
    {
        private static readonly AutorEintrag _myAuthorEntry = new()
        {
            NodeName = "AutorEintrag",
            _id = 80,
            _name = "Holger Maaß",
            _email = "service@zusi-tools.org"
        };
        public static AutorEintrag MyAuthorEntry => _myAuthorEntry;

        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "AutorID",
            "AutorName",
            "AutorEmail",
            "AutorBeschreibung",
            "AutorLizenz",
            "AutorAufwand",
            "AutorAufwandStunden"
        };
#pragma warning restore IDE0052
        #endregion

        private int _id;
        private string _name;
        private string _email;
        private readonly string _beschreibung;
        private readonly string _lizenz;
        private readonly string _aufwand;
        private readonly string _aufwandStunden;

        public int Id { get { return _id; } }
        public string Aufwand { get { return _aufwand; } }
        public string AufwandStunden { get { return _aufwandStunden; } }
        public string Beschreibung { get { return _beschreibung; } }
        public string EMail { get { return _email; } }
        public string Lizenz { get { return _lizenz; } }
        public string Name { get { return _name; } }

        //---------------------------------------------------------------------
        public AutorEintrag(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _name = x.GetAttrValue("AutorName", "");
            _aufwand = x.GetAttrValue("AutorAufwand", "");
            _aufwandStunden = x.GetAttrValue("AutorAufwandStunden", "");

            if (string.Compare(_name, MyAuthorEntry._name, true) == 0)
            {
                int id = x.GetAttrValue("AutorID", 0);
                if (id != MyAuthorEntry._id)
                {
                    _id = MyAuthorEntry._id;
                    IsDirty = true;
                }

                string email = x.GetAttrValue("AutorEmail", "");
                if (string.Compare(email, MyAuthorEntry._email, true) != 0)
                {
                    _email = MyAuthorEntry._email;
                    IsDirty = true;
                }
            }
            else
            {
                _id = x.GetAttrValue("AutorID", 0);
                _email = x.GetAttrValue("AutorEmail", "");
                _beschreibung = x.GetAttrValue("AutorBeschreibung", "");
                _lizenz = x.GetAttrValue("AutorLizenz", "");
            }
        }

        //---------------------------------------------------------------------
        private AutorEintrag()
        { }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeIf(_id != 0, "AutorID", _id);
            writer.WriteAttributeStringIfNotEmpty("AutorName", _name);
            writer.WriteAttributeStringIfNotEmpty("AutorEmail", _email);
            writer.WriteAttributeStringIfNotEmpty("AutorBeschreibung", _beschreibung);
            writer.WriteAttributeStringIfNotEmpty("AutorLizenz", _lizenz);
            writer.WriteAttributeStringIfNotEmpty("AutorAufwand", _aufwand);
            writer.WriteAttributeStringIfNotEmpty("AutorAufwandStunden", _aufwandStunden);
        }
    }
}
