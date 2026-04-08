using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Route
{
    public class ModulDateien : ZusiObject
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
        public ModulDateien(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _datei = new Datei(this, x.Element("Datei"));
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _datei.Save(writer);
        }
    }
}
