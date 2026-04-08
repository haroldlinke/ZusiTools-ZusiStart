using System;
using System.Xml.Linq;

namespace ZusiKlassenLib2.Common
{
    [Serializable]
    public class Abhaengigkeit : ZusiGenericObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "PhysikGroesse",
            "LautstaerkeAbh",
            "SoundOperator",
            "TriggerGrenze",
            "Trigger",
            "SoundGeschwAbh"
        };

        private static readonly string[] _knownElems =
        {
            "Kennfeld"
        };
#pragma warning restore IDE0052

        public Abhaengigkeit(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
