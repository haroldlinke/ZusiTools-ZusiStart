using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class SoundTB0 : Sound
    {
        public SoundTB0(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
