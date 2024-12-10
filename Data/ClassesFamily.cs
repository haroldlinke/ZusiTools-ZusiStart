using Sovoma;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ZusiStart.Data
{
    public class Car
    {
        private readonly int _idMajor;
        private readonly int _idMinor;
        private readonly string _name;
        private readonly bool _rotation;
        private readonly string _wagen;

        public int IDMajor { get => _idMajor; }
        public int IDMinor { get => _idMinor; }
        public string Name { get => _name; }
        public string Wagen { get => _wagen; }
        public bool Rotation { get => _rotation; }

        public Car(XElement xc)
        {
            _name = xc.GetAttrValue("name", "");
            _wagen = xc.GetAttrValue("wagen", "");
            _idMajor = xc.GetAttrValue("idmajor", 0);
            _idMinor = xc.GetAttrValue("idminor", 0);
            _rotation = xc.GetAttrValue("rotation", false);
        }

        internal Car(string name)
        {
            _name = name;
        }

        public int Compare(string vclass)
        {
            return string.Compare(_name, vclass.ToKey());
        }
    }

    public class Class : Car
    {
        private readonly List<Car> _cars = new List<Car>();

        public List<Car> Cars { get => _cars; }

        public Class(XElement xc)
            : base(xc)
        {
            foreach (XElement x in xc.Elements("Car"))
            {
                _cars.Add(new Car(x));
            }
        }

        internal Class(string name)
            : base(name)
        { }
    }

    public class ClassFamily
    {
        class ClassComparer : IEqualityComparer<Class>
        {
            public bool Equals(Class x, Class y)
            {
                return GetHashCode(x) == GetHashCode(y);
            }

            public int GetHashCode(Class obj)
            {
                return obj == null ? 0 : obj.Name.GetHashCode();
            }
        }

        private static readonly ClassComparer _cc = new ClassComparer();

        private readonly bool _isRailCar;
        private readonly string _name;
        private readonly List<Class> _members = new List<Class>();

        public int Count { get => _members.Count; }
        public bool IsRailCar { get => _isRailCar; }
        public string Name { get => _name; }

        public Class this[string index]
        {
            get
            {
                string key = index.ToKey();
                return _members.FirstOrDefault(vc => string.Compare(key, vc.Name) == 0);
            }
        }

        public Class this[int index]
        {
            get
            {
                return _members[index];
            }
        }

        public ClassFamily(XElement xcf)
        {
            _name = xcf.GetAttrValue("name", "");
            _isRailCar = xcf.GetAttrValue("israilcar", false);

            foreach (XElement x in xcf.Elements("Class"))
            {
                _members.Add(new Class(x));
            }
        }

        public bool HasMember(string vclass)
        {
            Class tmp = new Class(vclass.ToKey());
            return _members.Contains(tmp, _cc);
        }
    }

    public class ClassFamilies : Singleton<ClassFamilies>, ISingletonBase
    {
        private readonly List<ClassFamily> _families = new List<ClassFamily>();

        internal List<ClassFamily> Families { get => _families; }

        public void Initialize()
        {
            using MemoryStream ms = new MemoryStream(Properties.Resources.classes);
            ParseDocument(XDocument.Load(XmlReader.Create(ms)));
        }

        private ClassFamily Family_impl(string famname)
        {
            return _families.FirstOrDefault(cf => string.Compare(famname, cf.Name) == 0);
        }

        private ClassFamily First_impl(string vclass)
        {
            return _families.FirstOrDefault(cf => cf.HasMember(vclass));
        }

        private void ForEach_impl(Action<ClassFamily> a)
        {
            _families.ForEach(a);
        }

        private void ParseDocument(XDocument doc)
        {
            XElement xfamilies = doc.Root;
            foreach (XElement x in xfamilies.Elements("Family"))
            {
                _families.Add(new ClassFamily(x));
            }
        }

        public static ClassFamily Family(string famname)
        {
            return Instance.Family_impl(famname);
        }

        public static ClassFamily First(string vclass)
        {
            return Instance.First_impl(vclass);
        }

        public static void ForEach(Action<ClassFamily> a)
        {
            Instance.ForEach_impl(a);
        }
    }

    static class ClassFamiliesHelper
    {
        internal static string ToKey(this string self)
        {
            return self.ToLower().Replace(" ", "").Trim();
        }
    }
}
