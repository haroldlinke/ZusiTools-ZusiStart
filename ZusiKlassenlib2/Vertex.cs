using Sovoma;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2
{
    public class Vertex : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "U",
            "U2",
            "V",
            "V2"
        };

        private static readonly string[] _knownElems =
        {
            "p",
            "n"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly float _u;
        private readonly float _u2;
        private readonly float _v;
        private readonly float _v2;
        private readonly ZPoint3D _n;
        private readonly ZPoint3D _p;

        public Vector3D Normale => _n.ToVector3D();
        public ZPoint3D P => _p;
        public float U => _u;
        public float U2 => _u2;
        public float V => _v;
        public float V2 => _v2;

        //---------------------------------------------------------------------
        public Vertex(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _u = x.GetAttrValue("U", 0f);
            _u2 = x.GetAttrValue("U2", 0f);
            _v = x.GetAttrValue("V", 0f);
            _v2 = x.GetAttrValue("V2", 0f);
            _p = new ZPoint3D(this, x.Element("p"));
            _n = new ZPoint3D(this, x.Element("n"));
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeDoubleIf(_u != 0, "U", _u, 4);
            writer.WriteAttributeDoubleIf(_u2 != 0, "U2", _u2, 4);
            writer.WriteAttributeDoubleIf(_v != 0, "V", _v, 4);
            writer.WriteAttributeDoubleIf(_v2 != 0, "V2", _v2, 4);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _p.Save(writer);
            _n.Save(writer);
        }
    }
}
