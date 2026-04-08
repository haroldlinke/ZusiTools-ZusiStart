using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class Pfeife : Schalter
    {
        #region
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "TexturPosition2"
        };

        private static readonly string[] _knownElems =
        {
            "SoundTief",
            "SoundHoch"
        };
#pragma warning restore IDE0052
        #endregion

        public Pfeife(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
