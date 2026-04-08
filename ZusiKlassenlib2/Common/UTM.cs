using Sovoma;
using Sovoma.WPF.MathEx;
using System;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace ZusiKlassenLib2.Common
{
    [Serializable]
    public class UTM : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "UTM_WE",
            "UTM_NS",
            "UTM_Zone",
            "UTM_Zone2"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly int _utmX;
        private readonly int _utmY;
        private readonly int _zone;
        private readonly char _zoneField;
        private readonly string _zone2;

        public bool IsNorth => _zoneField >= 'N';
        public bool IsEast => _zone >= 31;
        public bool IsSouth => _zoneField <= 'M';
        public bool IsWest => _zone <= 30;

        public int WE => _utmX;
        public int NS => _utmY;
        public int Zone => _zone;
        public string Zone2 => _zone2;

        //---------------------------------------------------------------------
        public UTM(IZusiObjectParent parent, UTM source, int weOfs, int nsOfs)
            : base(parent, source)
        {
            _utmX = source._utmX + weOfs;
            _utmY = source._utmY + nsOfs;
            _zone = source._zone;
            _zone2 = source._zone2;
        }

        //---------------------------------------------------------------------
        public UTM(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _utmX = x.GetAttrValue("UTM_WE", 0);
            _utmY = x.GetAttrValue("UTM_NS", 0);
            _zone = x.GetAttrValue("UTM_Zone", 0);
            _zone2 = x.GetAttrValue("UTM_Zone2", "");
            _zoneField = string.IsNullOrEmpty(_zone2) ? '\0' : _zone2[0];
        }

        //---------------------------------------------------------------------
        //public void ToLatLon(double utmX, double utmY, int zone, bool isNorthHemisphere, out double longitude, out double latitude)
        public Point ToLatLon()
        {
            double diflat = -0.00066286966871111111111111111111111111;
            double diflon = -0.0003868060578;

            double c_sa = 6378137.0;
            double c_sb = 6356752.314245;
            double e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            double e2cuadrada = Math.Pow(e2, 2);
            double c = Math.Pow(c_sa, 2) / c_sb;
            double x = _utmX * 1000 - 500000;
            // TODO: check calculation for southern hemisphere. May be it's 1000000 - _utmY
            double y = IsNorth ? _utmY * 1000 : _utmY * 1000 - 10000000;

            double s = (_zone * 6.0) - 183.0;
            double lat = y / (c_sa * 0.9996);
            double v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            double a = x / v;
            double a1 = Math.Sin(2 * lat);
            double a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            double j2 = lat + (a1 / 2.0);
            double j4 = ((3 * j2) + a2) * 0.25;
            double j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            double alfa = 0.75 * e2cuadrada;
            double beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            double gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            double bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            double b = (y - bm) / v;
            double epsi = ((e2cuadrada * Math.Pow(a, 2)) * 0.5) * Math.Pow((Math.Cos(lat)), 2);
            double eps = a * (1 - (epsi / 3.0));
            double nab = (b * (1 - epsi)) + lat;
            double senoheps = (Math.Exp(eps) - Math.Exp(-eps)) * 0.5;
            double delt = Math.Atan(senoheps / (Math.Cos(nab)));
            double tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            return new(
                // longitude 
                Math2D.Degrees(delt) + s + diflon,
                // latitude
                Math2D.Degrees(lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - 1.5 * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) + diflat);
        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            return $"{_zone}{_zone2} {_utmX} {_utmY}";
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttribute("UTM_WE", _utmX);
            writer.WriteAttribute("UTM_NS", _utmY);
            writer.WriteAttribute("UTM_Zone", _zone);
            writer.WriteAttributeString("UTM_Zone2", _zone2);
        }
    }
}
