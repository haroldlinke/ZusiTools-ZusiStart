using Sovoma;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2
{
    //=========================================================================
    public class ZPoint2D : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "X", "Y"
        };
#pragma warning restore IDE0052
        #endregion

        private double _x;
        private double _y;

        public double X { get { return _x; } }
        public double Y { get { return _y; } }

        //---------------------------------------------------------------------
        public static ZPoint2D FromZPoint3D(ZPoint3D p3d) => new(p3d.X, p3d.Y);

        //---------------------------------------------------------------------
        public static ZPoint2D FromZPoint3DWithOffset(ZPoint3D p3d, double xOfs = 0.0, double yOfs = 0.0) => new(p3d.X + xOfs, p3d.Y + yOfs);

        //---------------------------------------------------------------------
        public ZPoint2D()
        {
            _x = 0;
            _y = 0;
        }

        //---------------------------------------------------------------------
        public ZPoint2D(double x, double y)
        {
            _x = x;
            _y = y;
        }

        //---------------------------------------------------------------------
        public ZPoint2D(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _x = GetAttrValueDouble(x, "X", 0.0);
            _y = GetAttrValueDouble(x, "Y", 0.0);
        }

        //---------------------------------------------------------------------
        public void MoveBy(double xOfs, double yOfs)
        {
            _x += xOfs;
            _y += yOfs;
        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            return string.Format("{0:N4}; {1:N4}", _x, _y);
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeDoubleIf(_x != 0.0, "X", _x, 4);
            writer.WriteAttributeDoubleIf(_y != 0.0, "Y", _y, 4);
        }
    }

    //=========================================================================
    public class ZRect
    {
        private readonly ZPoint2D _p;
        private readonly double _width;
        private readonly double _height;

        public ZPoint2D TopLeft { get { return _p; } }
        public double Width { get { return _width; } }
        public double Height { get { return _height; } }
        public double Left { get { return _p.X; } }
        public double Top { get { return _p.Y; } }
        public double Right { get { return _p.X + _width; } }
        public double Bottom { get { return _p.Y + _height; } }

        public ZRect(double left, double top, double width, double height)
        {
            _p = new ZPoint2D(left, top);
            _width = width;
            _height = height;
        }

        public ZRect(ZPoint2D topLeft, double width, double height)
        {
            _p = topLeft;
            _width = width;
            _height = height;
        }

        public ZRect(ZPoint3D topLeft, double width, double height)
        {
            _p = ZPoint2D.FromZPoint3D(topLeft);
            _width = width;
            _height = height;
        }

        public bool PtInRect(ZPoint2D p)
        {
            return (p.X >= _p.X && p.X < Right) && (p.Y >= _p.Y && p.Y < Bottom);
        }

        public bool PtInRect(ZPoint3D p)
        {
            return PtInRect(ZPoint2D.FromZPoint3D(p));
        }
    }
}
