using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace ZusiKlassenLib2.Common
{
    [Serializable]
    public abstract class ZusiGenericObject : ZusiObject
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ZusiGenericObject));

        private static int _emptyObjects = 0;
        public static int EmptyObjects => _emptyObjects;

        private string[] __knownElems = null;

        protected Dictionary<string, string> _attributes = new();
        protected List<ZusiObject> _objects = new();

        //---------------------------------------------------------------------
        public ZusiGenericObject(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            foreach (XAttribute xa in x.Attributes())
            {
                _attributes[xa.Name.LocalName] = xa.Value;
            }

            foreach (XElement xc in x.Elements())
            {
                ZusiObject obj = Fabric(this, xc);
                if (obj != null)
                {
                    _objects.Add(obj);
                }
            }
        }

        //---------------------------------------------------------------------
        public bool BoolAttribute(string name, bool defaultValue)
        {
            return _attributes.ContainsKey(name) ? int.Parse(_attributes[name]) != 0 : defaultValue;
        }

        //---------------------------------------------------------------------
        public string Attribute(string name)
        {
            return _attributes.ContainsKey(name) ? _attributes[name] : null;
        }

        //---------------------------------------------------------------------
        public T Object<T>() where T : ZusiObject
        {
            return _objects.FirstOrDefault(o => o is T) as T;
        }

        //---------------------------------------------------------------------
        public T Object<T>(string key) where T : ZusiObject
        {
            return _objects.FirstOrDefault(o => o.NodeName == key) as T;
        }

        //---------------------------------------------------------------------
        public List<T> Objects<T>() where T : ZusiObject
        {
            return _objects.Where(o => o is T).Select(s => (T)s).ToList();
        }

        //---------------------------------------------------------------------
        public List<T> Objects<T>(string key) where T : ZusiObject
        {
            return _objects.Where(o => o.NodeName == key).Select(s => (T)s).ToList();
        }

        //---------------------------------------------------------------------
        private ZusiGenericObject()
        { }

        //---------------------------------------------------------------------
        protected override bool GetDirty()
        {
            foreach (ZusiObject o in _objects)
            {
                if (o.IsDirty)
                    return true;
            }

            return base.GetDirty();
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            foreach (KeyValuePair<string, string> kvp in _attributes)
            {
                writer.WriteAttributeString(kvp.Key, kvp.Value);
            }
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            _objects.ForEach(o => o.Save(writer));
        }

        //---------------------------------------------------------------------
        private ZusiObject Fabric(IZusiObjectParent parent, XElement x)
        {
            if (LibrarySettings.CheckElements)
            {
                if (__knownElems == null)
                {
                    __knownElems = EnumerateKnownElements(GetType(), null);
                }
                if (__knownElems != null)
                {
                    string n = __knownElems.FirstOrDefault(s => string.Compare(s, x.Name.LocalName) == 0);
                    if (!string.IsNullOrEmpty(n))
                    {
                        IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Name == n);
                        if (types == null || !types.Any())
                        {
                            Log.FatalFormat("Type not found: {0}", n);
                        }
                        else if (types.Count() > 1)
                        {
                            Log.FatalFormat("Ambiguous type found: {0}", n);
                        }
                        else
                        {
                            return Activator.CreateInstance(types.ElementAt(0), parent, x) as ZusiObject;
                        }
                    }
                }

                Log.FatalFormat("{0}: element '{1}' isn't a known child of '{2}'", parent.GetType().ToString(), x.Name.LocalName, NodeName);
                _emptyObjects++;
            }
            else
            {
                string n = x.Name.LocalName;
                IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Name == n);
                if (types == null || !types.Any())
                {
                    Log.FatalFormat("Type not found: {0}", n);
                }
                else if (types.Count() > 1)
                {
                    Log.FatalFormat("Ambiguous type found: {0}", n);
                }
                else
                {
                    return Activator.CreateInstance(types.ElementAt(0), parent, x) as ZusiObject;
                }
            }

            return null;
        }
    }
}
