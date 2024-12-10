using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZusiKlassenLib.Vehicle;

namespace ZusiStart.Data
{
    public class VehicleGroup
    {
        [NonSerialized]
        private static readonly Random _random = new Random();

        private readonly IEnumerable<FahrzeugVariante> _allVariants;
        private readonly string _vclass;
        private readonly Fahrzeug _vehicle;
        [NonSerialized]
        private readonly FahrzeugVariante _variant;

        public IEnumerable<FahrzeugVariante> AllVariants { get => _allVariants; }
        public string VClass { get => _vclass; }
        public Fahrzeug Vehicle { get => _vehicle; }
        public FahrzeugVariante Variant { get => _variant; }

        public VehicleGroup(string vclass, IEnumerable<FahrzeugVariante> variants)
        {
            _allVariants = variants;
            _vclass = vclass;
            ClassFamily cf = ClassFamilies.Family(vclass);
            bool isRailCar = cf != null && cf.IsRailCar;
            if (isRailCar)
            {
                Class c = cf[_random.Next(cf.Count)];
                if (string.IsNullOrEmpty(c.Wagen))
                {
                    _variant = variants.First(v =>
                    {
                        Fahrzeug f = v.FindParent<Fahrzeug>();
                        return c.Compare(v.BR) == 0;
                    });
                }
                else
                {
                    _variant = variants.First(v =>
                    {
                        Fahrzeug f = v.FindParent<Fahrzeug>();
                        return c.Compare(v.BR) == 0 && f.Wagen == c.Wagen;
                    });
                }
            }
            else
            {
                _variant = variants.ElementAt(_random.Next(variants.Count()));
            }
            _vehicle = _variant.FindParent<Fahrzeug>();
        }
    }
}
