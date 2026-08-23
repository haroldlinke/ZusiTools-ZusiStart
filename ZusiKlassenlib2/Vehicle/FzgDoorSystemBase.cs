using System;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Vehicle
{
    public enum DoorSystem
    {
        None,
        SAT,
        TB0,
        TB5,
        SST,
        TAV,
        UIC_WTB,
        SBahn
    }

    [Serializable]
    public class FzgTuerSystemBasis : ZusiGenericObject
    {
        private readonly DoorSystem _doorSystem;

        public virtual bool IsTB0 => false;

        //---------------------------------------------------------------------
        public DoorSystem DoorSystem => _doorSystem;

        //---------------------------------------------------------------------
        public FzgTuerSystemBasis(IZusiObjectParent parent, XElement x, DoorSystem doorSystem)
            : base(parent, x)
        {
            _doorSystem = doorSystem;
        }
    }
}
