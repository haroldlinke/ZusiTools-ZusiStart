using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class SoundSifa : Sound
    {
        public SoundSifa(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
