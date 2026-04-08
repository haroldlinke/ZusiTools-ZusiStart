using System.Collections.Generic;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
#if false
    public class DriversCabFile : ZusiDocument<DriversCab>// ZusiHauptDatei
    {
        private string _name;
        private DriversCab _driversCab;

        public List<Zeigerinstrument> BrokenInstruments { get { return _driversCab.BrokenInstruments; } }

        public bool HasBrokenInstruments { get { return _driversCab.HasBrokenInstruments; } }

        public string Name { get { return _name; } }

        public DriversCabFile(string path, XElement x)
            : base(path, x, null, new string[] { "Fuehrerstand" })
        {
            _name = System.IO.Path.GetFileName(path);
            _driversCab = new DriversCab(x.Element("Fuehrerstand"));
        }

        //---------------------------------------------------------------------
        public void AddMyAuthorEntry()
        {
            _info.AddMyAuthorEntry();
        }

        //---------------------------------------------------------------------
        protected override bool GetIsDirty()
        {
            if (base.GetIsDirty())
                return true;

            return _driversCab.IsDirty;
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);
            _driversCab.Save(writer);
        }
    }
#else
    public class DriversCabFile : ZusiDocument<DriversCab>
    {
        public List<Zeigerinstrument> BrokenInstruments { get { return Root.BrokenInstruments; } }

        public bool HasBrokenInstruments { get { return Root.HasBrokenInstruments; } }

        public DriversCabFile(string path)
            : base(null, path, null, "Fuehrerstand")
        { }
    }
#endif
}
