using Sovoma;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Route
{
    public class StreckenStandort : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "StrInfo"
        };

        private static readonly string[] _knownElems =
        {
            "p",
            "lookat",
            "up"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly string _info;

        private readonly ZPoint3D _p;
        private readonly ZPoint3D _lookAt;
        private readonly ZPoint3D _up;

        public ZPoint3D P => _p;
        public ZPoint3D LookAt => _lookAt;
        public ZPoint3D Up => _up;

        //---------------------------------------------------------------------
        public StreckenStandort(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _info = x.GetAttrValue("StrInfo", "");

            _p = new ZPoint3D(this, x.Element("p"));
            _lookAt = new ZPoint3D(this, x.Element("lookat"));
            _up = new ZPoint3D(this, x.Element("up"));
        }

        //---------------------------------------------------------------------
        public StreckenStandort(IZusiObjectParent parent, StreckenStandort source)
            : base(parent, source)
        {
            _info = source._info;

            _p = new ZPoint3D(this, source._p);
            _lookAt = new ZPoint3D(this, source._lookAt);
            _up = new ZPoint3D(this, source._up);
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeStringIfNotEmpty("StrInfo", _info);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(System.Xml.XmlWriter writer)
        {
            base.SaveElements(writer);

            _p.Save(writer);
            _lookAt.Save(writer);
            _up.Save(writer);
        }
    }
}
