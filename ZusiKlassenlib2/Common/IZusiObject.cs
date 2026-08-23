using System.Xml;

namespace ZusiKlassenLib2.Common
{
    public interface IZusiObject
    {
        bool IsDirty { get; set; }
        string NodeName { get; set; }

        void ChangeParent(IZusiObjectParent parent);
        void Save(XmlWriter writer);
    }
}
