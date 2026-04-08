using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class Funktionalitaeten : ZusiGenericObject
    {
        private static readonly string[] _knownElems =
        {
            "Sifa_ZeitZeit",
            "Sifa_ZeitWeg",
            "Sifa86",

            "LokbremseEntlueften",
            "AFBEinAus",

            "Schleuderschutzbremse",
            "SchleuderschutzElektr",
            "SchleuderschutzDrosselung",

            // 8 .. 22 Beachte GetIndusis
            "IndusiI54",
            "IndusiI60",
            "IndusiI60DR",
            "IndusiI60M",
            "IndusiI60R",
            "PZB90I60R_V20",
            "PZB90I60R_V20_ZUB262",
            "PZB90ER24_V20",
            "PZ80",
            "PZ80R",

            "LZB80_I80",
            "LZB80_CE_I80",
            "LZB80_PZB90_20",
            "LZB80_PZB90_20_ZUB262",
            "LZB80_CE_PZB90_20",
            //--

            "TuerenTB0",
            "TuerenTAV",
            "TuerenSAT",
            "TuerenSST",
            "TuerenUICWTB",

            "Notaus",
            "Kombischalter",
            "Angleicher",
            "Sander",
            "Pfeife",
            "Luefter",
            "FuehrerstandSound",
            "LuftpresserAus",
            "Abhaengigkeit",
            "DynBremseLSSAus",
            "MgBremseManuell",
            "Bremsprobe",

            "NotbremssystemUICNBUe",
            "NotbremsSystemNBUe2004",
            "NotbremsSystemSBahn",

            "TempomatEinAus",
            "FahrschalterLSSAus",

            "FtdIndividuell"
        };

        public List<Indusi> Indusis => GetIndusis();

        //---------------------------------------------------------------------
        public Funktionalitaeten(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }

        //---------------------------------------------------------------------
        private List<Indusi> GetIndusis()
        {
            List<Indusi> res = new();

            for (int i = 8; i <= 22; i++)
            {
                Indusi indusi = Object<Indusi>(_knownElems[i]);
                if (indusi != null)
                {
                    res.Add(indusi);
                }
            }

            return res;
        }
    }
}
