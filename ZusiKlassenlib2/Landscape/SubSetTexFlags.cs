using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Landscape
{
    public class SubSetTexFlags : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "ALPHAARG0",
            "ALPHAARG1",
            "ALPHAARG2",
            "RESULTARG",
            "COLOROP",
            "ALPHAOP",
            "MINFILTER",
            "MAGFILTER",
            "COLORARG0",
            "COLORARG1",
            "COLORARG2"
        };
#pragma warning restore IDE0052
        #endregion

        //---------------------------------------------------------------------
        public SubSetTexFlags(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            // TODO not implemented yet
        }

        //---------------------------------------------------------------------
        public SubSetTexFlags(IZusiObjectParent parent, SubSetTexFlags source)
            : base(parent, source)
        {
            // TODO not implemented yet
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            // TODO not implemented yet
        }
    }
}
