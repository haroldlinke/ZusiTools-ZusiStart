//#define LOG

using System.Collections;
using System.IO;
using System.Windows;

namespace ZusiKlassenLib2.Landscape
{
    //=========================================================================
    public class TexCoord
    {
#if LOG
        private static readonly ILog Log = LogManager.GetLogger(typeof(TexCoord));
#endif
        private Point _coord;

        private readonly float _orgU;
        private readonly float _orgV;

        public float OrgU { get { return _orgU; } }
        public float OrgV { get { return _orgV; } }

        public double U { get { return _coord.X; } }
        public double V { get { return _coord.Y; } }

        //---------------------------------------------------------------------
        public TexCoord(BinaryReader lsbReader)
            : this(lsbReader.ReadSingle(), lsbReader.ReadSingle())
        { }

        //---------------------------------------------------------------------
        public TexCoord(float u, float v)
        {
            _orgU = u;
            _orgV = v;
            _coord = new Point(_orgU, _orgV);
        }

        //---------------------------------------------------------------------
        public void Offset(double uOfs, double vOfs)
        {
            _coord.Offset(uOfs, vOfs);
        }

        //---------------------------------------------------------------------
        public void Save(BinaryWriter bw)
        {
            float u = (float)_coord.X;
            float v = (float)_coord.Y;
            bw.Write(u);
            bw.Write(v);
        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            return string.Format("u:{0:N3} v:{1:N3}", _coord.X, _coord.Y);
        }

        //---------------------------------------------------------------------
        public static implicit operator Point(TexCoord t)
        {
            return t._coord;
        }
    }

    //=========================================================================
    public class TexCoords : IEnumerable
    {
        private readonly TexCoord[] _coords = new TexCoord[2];

        public TexCoord this[int index] { get { return index >= 0 && index < 2 ? _coords[index] : null; } }

        //---------------------------------------------------------------------
        public TexCoords(BinaryReader lsbReader)
            : this(new TexCoord(lsbReader), new TexCoord(lsbReader))
        {
        }

        //---------------------------------------------------------------------
        public TexCoords(TexCoord tx1, TexCoord tx2)
        {
            _coords[0] = tx1;
            _coords[1] = tx2;
        }

        //---------------------------------------------------------------------
        public TexCoords(TexCoord tx)
            : this(tx, null)
        { }

        //---------------------------------------------------------------------
        public IEnumerator GetEnumerator()
        {
            return _coords.GetEnumerator();
        }
    }
}
