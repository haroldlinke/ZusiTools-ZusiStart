using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2
{
    public static class XElementEx
    {
        public delegate T ObjectCreator<T>(IZusiObjectParent parent, XElement x);

        //---------------------------------------------------------------------
        public static T GetOptionalElement<T>(this XElement x, IZusiObjectParent parent, XName elemName, ObjectCreator<T> creator) where T : ZusiObject
        {
            XElement e = x.Element(elemName);
            return e == null ? default : creator(parent, e);
        }

        //---------------------------------------------------------------------
        public static T GetOptionalElement<T>(this XElement x, IZusiObjectParent parent, XName elemName, ObjectCreator<T> creator, T defaultValue) where T : ZusiObject
        {
            XElement e = x.Element(elemName);
            return e == null ? defaultValue : creator(parent, e);
        }
    }
}
