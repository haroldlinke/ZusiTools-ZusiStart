using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ZusiKlassenLib2.Landscape
{
    //=========================================================================
    static class Helper
    {
        //---------------------------------------------------------------------
        private static void AddSubElements<T>(this Transform3DGroup target, Transform3D source) where T : Transform3D
        {
            if (source is Transform3DGroup group)
            {
                foreach (var e in group.Children.Where(c => c is T))
                {
                    target.Children.Add(e);
                }
            }
            else if (source is T)
            {
                target.Children.Add(source);
            }
        }

        //---------------------------------------------------------------------
        public static Transform3D Merge(this Transform3D first, Transform3D second)
        {
            Transform3DGroup res = new();

            res.AddSubElements<RotateTransform3D>(first);
            res.AddSubElements<RotateTransform3D>(second);
            res.AddSubElements<TranslateTransform3D>(first);
            res.AddSubElements<TranslateTransform3D>(second);

            return res;
        }
    }

    //=========================================================================
    class XColor
    {
        private Color _c;
        private readonly double _a;
        private readonly double _b;
        private readonly double _g;
        private readonly double _r;

        public Color Color { get { return _c; } }
        public double A { get { return _a; } }
        public double B { get { return _b; } }
        public double G { get { return _g; } }
        public double R { get { return _r; } }

        public XColor(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                _c = Colors.White;
                _a = _b = _g = _r = 1;
            }
            else
            {
                _c = (Color)ColorConverter.ConvertFromString("#" + s);
                _a = _c.A / 255.0;
                _b = _c.B / 255.0;
                _g = _c.G / 255.0;
                _r = _c.R / 255.0;
            }
        }
    }
}
