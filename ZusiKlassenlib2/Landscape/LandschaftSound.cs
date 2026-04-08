using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Landscape
{
    public class LandschaftSound : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownElems =
        {
            "Sound",
            "Abhaengigkeit"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly Abhaengigkeit _abhaengigkeit;
        private readonly Sound _sound;

        //---------------------------------------------------------------------
        public LandschaftSound(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _sound = new Sound(this, x.Element("Sound"));
            _abhaengigkeit = GetOptionalObject<Abhaengigkeit>(this, x.Element("Abhaengigkeit"));
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _sound.Save(writer);
            _abhaengigkeit?.Save(writer);
        }
    }
}
