using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class Grafik : ZusiGenericObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownElems =
        {
            "RenderFlags",
            "Grunddaten",
            "Melder",
            "Zeigerinstrument"
        };
#pragma warning restore IDE0052

        private readonly List<Zeigerinstrument> _brokenInstruments = new();

        public List<Zeigerinstrument> BrokenInstruments => _brokenInstruments;

        public bool HasBrokenInstruments => _brokenInstruments.Count > 0;

        public Grafik(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            List<ZusiObject> temp = _objects.Where(o => o is Zeigerinstrument).ToList();
            temp.ForEach(o =>
            {
                Zeigerinstrument zi = (Zeigerinstrument)o;
                if (zi.IsAscendingArcSequenceBroken)
                {
                    _brokenInstruments.Add(zi);
                }
            });
        }
    }
}
