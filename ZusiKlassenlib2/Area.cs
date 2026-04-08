using System;

namespace ZusiKlassenLib2
{
    public class Area
    {
        public double West { get; set; }
        public double North { get; set; }
        public double East { get; set; }
        public double South { get; set; }

        public double EastWest { get { return East - West; } }
        public double NorthSouth { get { return North - South; } }

        public Area()
        {
            Reset();
        }

        public ZPoint2D CalcMidPoint()
        {
            double w = Math.Floor(West / 1000.0) * 1000.0;
            double s = Math.Floor(South / 1000.0) * 1000.0;
            double e = Math.Ceiling(East / 1000.0) * 1000.0;
            double n = Math.Ceiling(North / 1000.0) * 1000.0;
            double x = Math.Floor((e - w) / 2000.0) * 1000.0;
            double y = Math.Floor((n - s) / 2000.0) * 1000.0;
            return new ZPoint2D(-x - w, -y - s);
        }

        public void Reset()
        {
            West = double.PositiveInfinity;
            North = double.NegativeInfinity;
            East = double.NegativeInfinity;
            South = double.PositiveInfinity;
        }
    }
}
