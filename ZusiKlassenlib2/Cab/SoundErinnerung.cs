using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class SoundErinnerung : Sound
    {
        public SoundErinnerung(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
