using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Route
{
    public class StreckenDatei : ZusiDocument<Strecke>
    {
        public Strecke Strecke { get { return Root; } }

        //---------------------------------------------------------------------
        public StreckenDatei(string path)
            : base(null, path, null, "Strecke")
        { }

        //---------------------------------------------------------------------
        public void SaveAs(string path, string landschaftsDatei)
        {
            Path = path;
            Root.Datei.Dateiname = landschaftsDatei;
            Save();
        }
    }
}
