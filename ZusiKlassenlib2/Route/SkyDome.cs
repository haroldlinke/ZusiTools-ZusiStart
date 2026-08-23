using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Route
{
    public class SkyDome : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownElems =
        {
            "HimmelTex",
            "NebelTex",
            "SonneTex",
            "SonneHorizontTex",
            "MondTex",
            "SternTex"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly Datei _himmelTex;
        private readonly Datei _nebelTex;
        private readonly Datei _sonneTex;
        private readonly Datei _sonneHorizontTex;
        private readonly Datei _mondTex;
        private readonly Datei _sternTex;

        public Datei HimmelTex => _himmelTex;
        public Datei NebelTex => _nebelTex;
        public Datei SonneTex => _sonneTex;
        public Datei SonneHorizontTex => _sonneHorizontTex;
        public Datei MondTex => _mondTex;
        public Datei SternTex => _sternTex;

        //---------------------------------------------------------------------
        public SkyDome(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _himmelTex = GetOptionalObject<Datei>(this, x.Element("HimmelTex"));
            _nebelTex = GetOptionalObject<Datei>(this, x.Element("NebelTex"));
            _sonneTex = GetOptionalObject<Datei>(this, x.Element("SonneTex"));
            _sonneHorizontTex = GetOptionalObject<Datei>(this, x.Element("SonneHorizontTex"));
            _mondTex = GetOptionalObject<Datei>(this, x.Element("MondTex"));
            _sternTex = GetOptionalObject<Datei>(this, x.Element("SternTex"));
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _himmelTex?.Save(writer);
            _nebelTex?.Save(writer);
            _sonneTex?.Save(writer);
            _sonneHorizontTex?.Save(writer);
            _mondTex?.Save(writer);
            _sternTex?.Save(writer);
        }
    }
}
