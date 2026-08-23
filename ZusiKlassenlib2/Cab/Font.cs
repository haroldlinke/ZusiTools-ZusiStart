using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class Font : Datei
    {
        public Font(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
