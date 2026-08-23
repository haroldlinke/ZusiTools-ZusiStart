using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    [Serializable]
    public class InstrPkt : ZusiGenericObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "X",
            "Y"
        };
#pragma warning restore IDE0052

        public InstrPkt(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    [Serializable]
    public class InstrPkt1 : InstrPkt
    {
        public InstrPkt1(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    [Serializable]
    public class InstrPkt2 : InstrPkt
    {
        public InstrPkt2(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    [Serializable]
    public class InstrPkt3 : InstrPkt
    {
        public InstrPkt3(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    [Serializable]
    public class InstrPkt4 : InstrPkt
    {
        public InstrPkt4(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
