using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Landscape
{
    public class LandschaftsDatei : ZusiDocument<Landschaft>
    {
        public Landschaft Landschaft { get { return Root; } }

        //---------------------------------------------------------------------
        public LandschaftsDatei(string path)
            : base(null, path, null, "Landschaft")
        { }

#if false
        //---------------------------------------------------------------------
        private LandschaftsDatei(LandschaftsDatei source, Landschaft landschaft)
            : base(source, landschaft)
        { }
#endif

        //---------------------------------------------------------------------
        public void DumpLSB(string filename)
        {
            Root.DumpLSB(filename);
        }

        //---------------------------------------------------------------------
        protected override Landschaft CreateDocElement(XElement xsub)
        {
            return new Landschaft(this, Path, xsub);
        }
    }
}
