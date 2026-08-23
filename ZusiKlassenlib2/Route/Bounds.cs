namespace ZusiKlassenLib2.Route
{
    public class Bounds
    {
        private ZPoint2D _center = null;

        public ZPoint2D Center => _center;

        public double West { get; private set; }
        public double North { get; private set; }
        public double East { get; private set; }
        public double South { get; private set; }

        public double Width { get { return East - West; } }
        public double Height { get { return North - South; } }

        //---------------------------------------------------------------------
        public static Bounds CreateFrom(Strecke s)
        {
            Bounds b = new();

            foreach (StrElement se in s.StrElements)
            {
                if (se.Blue.X < b.West) b.West = se.Blue.X;
                if (se.Blue.X > b.East) b.East = se.Blue.X;
                if (se.Blue.Y < b.South) b.South = se.Blue.Y;
                if (se.Blue.Y > b.North) b.North = se.Blue.Y;
                if (se.Green.X < b.West) b.West = se.Green.X;
                if (se.Green.X > b.East) b.East = se.Green.X;
                if (se.Green.Y < b.South) b.South = se.Green.Y;
                if (se.Green.Y > b.North) b.North = se.Green.Y;
            }

            b._center = new ZPoint2D(b.West + b.Width * 0.5, b.South + b.Height * 0.5);

            return b;
        }

        //---------------------------------------------------------------------
        private Bounds()
        {
            Reset();
        }

        //---------------------------------------------------------------------
        public void Reset()
        {
            West = South = double.PositiveInfinity;
            North = East = double.NegativeInfinity;
        }
    }
}
