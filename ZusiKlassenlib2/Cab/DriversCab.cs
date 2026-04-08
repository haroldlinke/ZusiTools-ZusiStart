using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class DriversCab : ZusiGenericObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownElems =
        {
            "Funktionalitaeten",
            "Grafik"
        };
#pragma warning restore IDE0052

        private readonly List<Zeigerinstrument> _brokenInstruments = new();

        public List<Zeigerinstrument> BrokenInstruments => _brokenInstruments;

        public Funktionalitaeten Items => Object<Funktionalitaeten>();

        public bool HasBrokenInstruments => _brokenInstruments.Count > 0;

        //---------------------------------------------------------------------
        public DriversCab(ZusiDocumentBase parent, XElement x)
            : base(parent, x)
        {
            List<ZusiObject> temp = _objects.Where(o => (o is Grafik) && (o as Grafik).HasBrokenInstruments).ToList();
            temp.ForEach(g =>
            {
                _brokenInstruments.AddRange((g as Grafik).BrokenInstruments);
            });
        }
    }
}
