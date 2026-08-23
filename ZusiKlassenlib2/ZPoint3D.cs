using Sovoma;
using System;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;
using System.Windows.Media.Media3D;

namespace ZusiKlassenLib2
{
    [Serializable]
    public class ZPoint3D : ZusiObject, IEquatable<ZPoint3D>
    {
        private static readonly ZPoint3D _null = new(0, 0, 0);
        public static ZPoint3D Null => _null;

#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "X",
            "Y",
            "Z"
        };
#pragma warning restore IDE0052

        private double _x;
        private double _y;
        private double _z;

        public bool IsNullPoint
        {
            get { return _x == 0 && _y == 0 && _z == 0; }
        }

        public double X => _x;
        public double Y => _y;
        public double Z => _z;

        //---------------------------------------------------------------------
        public static ZPoint3D FromPoint2D(ZPoint2D source)
        {
            return new(source.X, source.Y, 0);
        }

        //---------------------------------------------------------------------
        public ZPoint3D(double x, double y, double z)
            : base()
        {
            _x = x;
            _y = y;
            _z = z;
        }

        //---------------------------------------------------------------------
        public ZPoint3D(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _x = GetAttrValueDouble(x, "X", 0.0);
            _y = GetAttrValueDouble(x, "Y", 0.0);
            _z = GetAttrValueDouble(x, "Z", 0.0);
        }

        //---------------------------------------------------------------------
        public ZPoint3D(IZusiObjectParent parent, XElement x, double defaultX, double defaultY, double defaultZ)
            : base(parent, x)
        {
            _x = x.GetAttrValue("X", defaultX);
            _y = x.GetAttrValue("Y", defaultY);
            _z = x.GetAttrValue("Z", defaultZ);
        }

        //---------------------------------------------------------------------
        public ZPoint3D(IZusiObjectParent parent, ZPoint3D source)
            : base(parent, source)
        {
            _x = source._x;
            _y = source._y;
            _z = source._z;
        }

        //---------------------------------------------------------------------
        public void MoveBy(double xOfs, double yOfs, double zOfs)
        {
            _x += xOfs;
            _y += yOfs;
            _z += zOfs;
        }

        //---------------------------------------------------------------------
        public void MoveTo(double x, double y, double z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        //---------------------------------------------------------------------
        public ZPoint3D MoveXY(double distance, double arc)
        {
            double dy = Math.Sin(arc) * distance;
            double dx = Math.Cos(arc) * distance;
            return new ZPoint3D(_x + dx, _y + dy, _z);
        }

        //---------------------------------------------------------------------
        public ZPoint3D RotateZ(ZPoint3D center, double arc)
        {
            double sin = Math.Sin(arc);
            double cos = Math.Cos(arc);
            double x1 = center.X + (_x - center.X) * cos - (_y - center.Y) * sin;
            double y1 = center.Y + (_x - center.X) * sin + (_y - center.Y) * cos;
            return new ZPoint3D(x1, y1, _z);
        }

        //---------------------------------------------------------------------
        public double Direction(ZPoint3D p) => Math.Atan2(_y - p.Y, _x - p.X);

        //---------------------------------------------------------------------
        public double InvDirection(ZPoint3D p) => Math.Atan2(p.Y - _y, p.X - _x);

        //---------------------------------------------------------------------
        public Point3D ToPoint3D() => new(_x, _y, _z);

        //---------------------------------------------------------------------
        public override string ToString() => string.Format("[{0:N4}; {1:N4}; {2:N4}]", _x, _y, _z);

        //---------------------------------------------------------------------
        public Vector3D ToVector3D() => new(_x, _y, _z);

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeDoubleIf(_x != 0, "X", _x, 4);
            writer.WriteAttributeDoubleIf(_y != 0, "Y", _y, 4);
            writer.WriteAttributeDoubleIf(_z != 0, "Z", _z, 4);
        }

        //---------------------------------------------------------------------
        public override bool Equals(object obj) => obj is ZPoint3D p && Equals(p);

        //---------------------------------------------------------------------
        public override int GetHashCode() => base.GetHashCode();

        //---------------------------------------------------------------------
        public bool Equals(ZPoint3D other) => (other as object) != null && this == other;

        //---------------------------------------------------------------------
        public static Vector3D Interpolate(ZPoint3D from, ZPoint3D to, double t) => new(
                from.X + (to.X - from.X) * t,
                from.Y + (to.Y - from.Y) * t,
                from.Z + (to.Z - from.Z) * t);

        //---------------------------------------------------------------------
        public static bool operator ==(ZPoint3D a, ZPoint3D b) => (a as object) == null ? (b as object) == null : (b as object) != null && a.X == b.X && a.Y == b.Y && a.Z == b.Z;

        //---------------------------------------------------------------------
        public static bool operator !=(ZPoint3D a, ZPoint3D b) => (a as object) == null ? (b as object) != null : ((b as object) == null) || a.X != b.X || a.Y != b.Y || a.Z != b.Z;

        //---------------------------------------------------------------------
        public static implicit operator Point3D(ZPoint3D p) => new(p._x, p._y, p._z);

        //---------------------------------------------------------------------
        public static explicit operator System.Windows.Point(ZPoint3D p) => new(p._x, p._y);
    }
}
