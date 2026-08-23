using log4net;
using Sovoma;
using Sovoma.WPF;
using System;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2
{
  [Serializable]
  public class ZusiObject : DirtyObjectBase, IZusiObject, IZusiObjectParent
  {
    private static readonly ILog _log = LogManager.GetLogger(typeof(ZusiObject));

    #region private fields

    private ulong _id;

    private string _nodeName;
    private IZusiObjectParent _parent;

    #endregion

    #region public attributes

    public ulong ID { get { return _id; } }

    public string NodeName
    {
      get { return _nodeName; }
      set { _nodeName = value; }
    }

    public IZusiObjectParent Parent
    {
      get { return _parent; }
    }

    #endregion

    #region public static method's

    //---------------------------------------------------------------------
    public static T Clone<T>(IZusiObjectParent parent, T source) where T : class
    {
      return source is T ? (T)Activator.CreateInstance(typeof(T), parent, source) : null;
    }

    //---------------------------------------------------------------------
    public static T CreateNew<T>(IZusiObjectParent parent, string nodeName) where T : ZusiObject
    {
      T t = Activator.CreateInstance<T>();
      t.AssignID();
      t._nodeName = nodeName;
      t._parent = parent;
      return t;
    }

    //---------------------------------------------------------------------
    public static string[] EnumerateKnownAttributes(Type type, string[] a)
    {
      FieldInfo fi = type.GetField("_knownAttribs", BindingFlags.NonPublic | BindingFlags.Static);
      if (fi != null)
      {
        string[] k = (string[])fi.GetValue(null);
        if (k != null)
        {
          if (a == null)
          {
            a = k;
          }
          else
          {
            a = ArrayEx.SafeConcat(a, k);
          }
        }
      }

      if (type.BaseType == null || type.BaseType == typeof(ZusiGenericObject) || type.BaseType == typeof(ZusiObject))
      {
        return a;
      }

      return EnumerateKnownAttributes(type.BaseType, a);
    }

    //---------------------------------------------------------------------
    public static string[] EnumerateKnownElements(Type type, string[] a)
    {
      FieldInfo fi = type.GetField("_knownElems", BindingFlags.NonPublic | BindingFlags.Static);
      if (fi != null)
      {
        string[] k = (string[])fi.GetValue(null);
        if (k != null)
        {
          if (a == null)
          {
            a = k;
          }
          else
          {
            a = ArrayEx.SafeConcat(a, k);
          }
        }
      }

      if (type.BaseType == null || type.BaseType == typeof(ZusiGenericObject) || type.BaseType == typeof(ZusiObject))
      {
        return a;
      }

      return EnumerateKnownElements(type.BaseType, a);
    }

    #endregion

    #region ctor

    //---------------------------------------------------------------------
    public ZusiObject(IZusiObjectParent parent, XElement x)
        : this(parent, x, x?.Name.LocalName)
    { }

    //---------------------------------------------------------------------
    public ZusiObject(IZusiObjectParent parent, XElement x, string nodeName)
    {
      AssignID();
      if (x != null)
      {

        _nodeName = nodeName;
        _parent = parent;
        ZusiDocumentBase doc = null;

        if (LibrarySettings.CheckAttributes)
        {
          doc = GetDocument();
          string[] ka = EnumerateKnownAttributes(GetType(), null);
          x.CheckAttributes((xx, a) =>
          {
            _log.WarnFormat("File {0}:\n    Attribute '{1}' of node '{2}' is unknown", doc?.Filename ?? "<could not be obtained>", a.Name, x.Name);
            return true;
          }, ka);
        }
        if (LibrarySettings.CheckElements)
        {
          doc ??= GetDocument();
          string[] ke = EnumerateKnownElements(GetType(), null);
          x.CheckElements((n, s) =>
          {
            _log.WarnFormat("File {0}:\n    Element '{1}' isn't a known child of '{2}'", doc?.Filename ?? "<could not be obtained>", n.Name.LocalName, s.Name);
            return true;
          }, ke);
        }
      }
    }

    //---------------------------------------------------------------------
    public ZusiObject(IZusiObjectParent parent, ZusiObject source)
    {
      AssignID();

      _nodeName = source._nodeName;
      _parent = parent;
    }

    //---------------------------------------------------------------------
    protected ZusiObject()
    { }

    #endregion

    #region public method's

    //---------------------------------------------------------------------
    public void ChangeParent(IZusiObjectParent parent)
    {
      _parent = parent;
    }

    //---------------------------------------------------------------------
    public T FindParent<T>() where T : IZusiObjectParent
    {
      if (_parent is T t)
      {
        return t;
      }

      if (_parent != null)
      {
        return _parent.FindParent<T>();
      }

      return default;
    }

    //---------------------------------------------------------------------
    public ZusiDocumentBase GetDocument()
    {
      return FindParent<ZusiDocumentBase>();
    }

    //---------------------------------------------------------------------
    public virtual void Save(XmlWriter writer)
    {
      writer.WriteStartElement(_nodeName);
      SaveAttributes(writer);
      SaveElements(writer);
      writer.WriteEndElement();
      IsDirty = false;
    }

    #endregion

    #region protected method's

    protected double GetAttrValueDouble(XElement element, XName name, double defaultValue)
    {
      double res = defaultValue;

      try
      {
        res = element.GetAttrValue(name, defaultValue);
      }
      catch (Exception ex)
      {
        ZusiDocumentBase doc = GetDocument();
        _log.Error($"{doc.Filename}: {ex.Message}");
      }

      return res;
    }

    protected float GetAttrValueFloat(XElement element, XName name, float defaultValue)
    {
      float res = defaultValue;

      try
      {
        res = element.GetAttrValue(name, defaultValue);
      }
      catch (Exception ex)
      {
        ZusiDocumentBase doc = GetDocument();
        _log.Error($"{doc.Filename}: {ex.Message}");
      }

      return res;
    }

    //---------------------------------------------------------------------
    protected static T GetOptionalObject<T>(IZusiObjectParent parent, XElement x) where T : class
    {
      return x != null ? (T)Activator.CreateInstance(typeof(T), parent, x) : null;
    }

    //---------------------------------------------------------------------
    protected static T GetOptionalObject<T>(IZusiObjectParent parent, XElement x, string nodeName) where T : class
    {
      return x != null ? (T)Activator.CreateInstance(typeof(T), parent, x, nodeName) : null;
    }

    //---------------------------------------------------------------------
    protected virtual void SaveAttributes(XmlWriter writer)
    { }

    //---------------------------------------------------------------------
    protected virtual void SaveElements(XmlWriter writer)
    { }

    #endregion

    #region private method's

    //---------------------------------------------------------------------
    private void AssignID()
    {
      if (_id == 0)
      {
        _id = IDManager.GetNextID();
      }
    }

    #endregion
  }
}
