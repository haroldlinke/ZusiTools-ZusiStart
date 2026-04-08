using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class Raste : ZusiGenericObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "SperreRunter",
            "SperreRauf",
            "SperrZeit"
        };

        private static readonly string[] _knownElems =
        {
            "Belegung",
            "Koppelung"
        };
#pragma warning restore IDE0052

        public List<Belegung> Belegungen { get { return Objects<Belegung>(); } }

        public Raste(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
