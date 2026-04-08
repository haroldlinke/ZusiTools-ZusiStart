using Sovoma;
using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class Belegung : ZusiObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "FunktionStr",
            "Parameter"
        };
#pragma warning restore IDE0052

        private readonly string _funktionStr;
        private readonly float _parameter;

        public string FunktionStr { get { return _funktionStr; } }
        public float Parameter { get { return _parameter; } }

        public Belegung(IZusiObjectParent parent, XElement x) : base(parent, x)
        {
            _funktionStr = x.GetAttrValue("FunktionStr", "");
            _parameter = x.GetAttrValue("Parameter", 0f);
        }
    }
}
