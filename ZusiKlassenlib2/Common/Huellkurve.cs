using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Landscape;
using ZusiKlassenLib2.Route;

namespace ZusiKlassenLib2.Common
{
    //=========================================================================
    public class Huellkurve : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownElems =
        {
            "PunktXYZ"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly List<ZPoint3D> _points = new();
        private Rect _outline;

        public Rect Outline => _outline;

        public List<ZPoint3D> Points { get { return _points; } }

        //---------------------------------------------------------------------
        public Huellkurve(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            foreach (XElement xp in x.Elements("PunktXYZ"))
            {
                ZPoint3D p = new(this, xp);
                if (p.X < minX)
                {
                    minX = p.X;
                }
                if (p.Y < minY)
                {
                    minY = p.Y;
                }
                if (p.X > maxX)
                {
                    maxX = p.X;
                }
                if (p.Y > maxY)
                {
                    maxY = p.Y;
                }
                _points.Add(p);
            }

            if (_points.Count > 0)
            {
                minX = Math.Floor(minX);
                minY = Math.Floor(minY);
                maxX = Math.Ceiling(maxX);
                maxY = Math.Ceiling(maxY);

                _outline = new(new Point(minX, minY), new Size(maxX - minX, maxY - minY));
            }
            else
            {
                _outline = new();
            }
        }

        //---------------------------------------------------------------------
        public Huellkurve(IZusiObjectParent parent, Huellkurve source)
            : base(parent, source)
        {
            _points.AddRange(source.Points);
        }

        //---------------------------------------------------------------------
        public bool IsPointInside(ZPoint2D p)
        {
            bool result = false;
            int corners = _points.Count;

            for (int i = 0, j = corners - 1; i < corners; j = i++)
            {
                if (((_points[i].Y > p.Y) != (_points[j].Y > p.Y)) &&
                    (p.X < (_points[j].X - _points[i].X) * (p.Y - _points[i].Y) / (_points[j].Y - _points[i].Y) + _points[i].X))
                {
                    result = !result;
                }
            }

            return result;
        }

        //---------------------------------------------------------------------
        public bool IsPointInside(ZPoint3D p)
        {
            return IsPointInside(ZPoint2D.FromZPoint3D(p));
        }

        //---------------------------------------------------------------------
        public bool IsStrElementInside(StrElement s)
        {
            return IsPointInside(s.Green) || IsPointInside(s.Blue);
        }

        //---------------------------------------------------------------------
        public bool IsSubSetInside(SubSet s, double xOfs, double yOfs, bool complete)
        {
            if (s.Mesh != null)
            {
                ZPoint2D[] triangle = new ZPoint2D[3];

                for (int i = 0; i < s.Mesh.Indizes.Length; i += 3)
                {
                    for (int j = 0, k = i; j < 3; j++, k++)
                    {
                        MeshVertex mv = s.Mesh.Vertices[s.Mesh.Indizes[k]];
                        triangle[j] = new ZPoint2D(mv.Position.X + xOfs, mv.Position.Y + yOfs);
                    }

                    foreach (MeshVertex mv in s.Mesh.Vertices)
                    {
                        ZPoint2D p = new(mv.Position.X + xOfs, mv.Position.Y + yOfs);
                        if (complete)
                        {
                            if (!IsPointInside(p))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (IsPointInside(p))
                            {
                                return true;
                            }
                        }
                    }
                }

                if (complete)
                {
                    return true;
                }
            }
            else
            {
                Console.WriteLine("SubSet has no mesh");
            }
            return false;
        }

        //---------------------------------------------------------------------
        public bool IsTileInside(Verknuepfte tile, double xOfs, double yOfs)
        {
            if (!(tile.Flags.HasFlag(LoadFlagsType.TileFile | LoadFlagsType.TileDetailFile)))
                throw new Exception("Invalid tile");

            ZPoint2D[] corners = new ZPoint2D[4];
            if (tile.Flags.HasFlag(LoadFlagsType.TileFile))
            {
                corners[0] = ZPoint2D.FromZPoint3DWithOffset(tile.P, -500.0, -500.0);
                corners[1] = new ZPoint2D(corners[0].X, corners[0].Y + 1000.0);
                corners[2] = new ZPoint2D(corners[1].X + 1000.0, corners[1].Y);
                corners[3] = new ZPoint2D(corners[2].X, corners[0].Y);
            }
            else
            {
                corners[0] = ZPoint2D.FromZPoint3DWithOffset(tile.P, xOfs - 125.0, yOfs - 125.0);
                corners[1] = new ZPoint2D(corners[0].X, corners[0].Y + 250.0);
                corners[2] = new ZPoint2D(corners[1].X + 250.0, corners[1].Y);
                corners[3] = new ZPoint2D(corners[2].X, corners[0].Y);
            }

            foreach (ZPoint2D p in corners)
            {
                if (IsPointInside(p))
                    return true;
            }

            return false;
        }

        //---------------------------------------------------------------------
        public void MoveBy(ZPoint2D p)
        {
            MoveBy(p.X, p.Y);
        }

        //---------------------------------------------------------------------
        public void MoveBy(double xOfs, double yOfs)
        {
            foreach (ZPoint3D p in _points)
            {
                p.MoveBy(xOfs, yOfs, 0);
            }
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            foreach (ZPoint3D p in _points)
            {
                p.Save(writer);
            }
        }
    }
}
