using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class Schleuderschutzbremse : Schalter
    {
        public Schleuderschutzbremse(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    [Serializable]
    public class SchleuderschutzDrosselung : Schalter
    {
        public SchleuderschutzDrosselung(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
