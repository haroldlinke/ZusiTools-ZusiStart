using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace ZusiKlassenLib2.Landscape
{
    /* Format of lsb entry
        // position
        public float X;
        public float Y;
        public float Z;
        // vector normal
        public float NX;
        public float NY;
        public float NZ;
        // text coords
        public float U1;
        public float V1;
        public float U2;
        public float V2;
     */

    //=========================================================================
    public class MeshVertex
    {
        // position
        private Point3D _pos;
        // vector normal
        private Vector3D _normal;
        // text coords
        private readonly TexCoords _texCoords;

        public Vector3D Normal => _normal;
        public Point3D Position => _pos;
        public TexCoords TexCoords => _texCoords;

        //---------------------------------------------------------------------
        public MeshVertex(BinaryReader lsbReader)
        {
            _pos = new System.Windows.Media.Media3D.Point3D(lsbReader.ReadSingle(), lsbReader.ReadSingle(), lsbReader.ReadSingle());
            _normal = new Vector3D(lsbReader.ReadSingle(), lsbReader.ReadSingle(), lsbReader.ReadSingle());
            _texCoords = new TexCoords(lsbReader);
        }

        //---------------------------------------------------------------------
        public MeshVertex(Vertex vertex)
        {
            _pos = vertex.P.ToPoint3D();
            _normal = vertex.Normale;
            _texCoords = new TexCoords(new TexCoord(vertex.U, vertex.V));
        }

        //---------------------------------------------------------------------
        public void MoveBy(float xOfs, float yOfs) => _pos.Offset(xOfs, yOfs, 0);

        //---------------------------------------------------------------------
        public void AddToMeshGeometry3D(MeshGeometry3D m3d, int texIndex)
        {
            m3d.Positions.Add(_pos);
            m3d.Normals.Add(_normal);
            m3d.TextureCoordinates.Add(_texCoords[texIndex]);
        }

        //---------------------------------------------------------------------
        public void Dump(TextWriter writer)
        {
            writer.Write(string.Format("[{0:N4}, {1:N4}, {2:N4}] [{3:N4}, {4:N4}, {5:N4}] [{6:N4}, {7:N4}] [{8:N4}, {9:N4}]",
                _pos.X, _pos.Z, -_pos.Y,
                _normal.X, _normal.Z, -Normal.Y,
                _texCoords[0].U, _texCoords[0].V,
                _texCoords[1].U, _texCoords[1].V));
        }

        //---------------------------------------------------------------------
        public void Save(BinaryWriter bw)
        {
            float x = (float)_pos.X;
            float y = (float)_pos.Y;
            float z = (float)_pos.Z;
            bw.Write(x);
            bw.Write(z);
            bw.Write(-y);
            x = (float)_normal.X;
            y = (float)_normal.Y;
            z = (float)_normal.Z;
            bw.Write(x);
            bw.Write(z);
            bw.Write(-y);
            foreach (TexCoord t in _texCoords)
            {
                t.Save(bw);
            }
        }

        //---------------------------------------------------------------------
        // Breite des Meshes auf jeder Seite um 0.1 mm schmaler machen
        public void Hack() => _pos.Offset(0, _pos.Y < 0 ? 0.0001 : -0.0001, 0);
    }

    //=========================================================================
    public class Mesh
    {
        //private static readonly ILog Log = LogManager.GetLogger(typeof(Mesh));

        private readonly MeshVertex[] _vertices;
        private ushort[] _indizes;
        private MeshGeometry3D _cachedModel;

        public ushort[] Indizes { get { return _indizes; } }

        public MeshVertex[] Vertices { get { return _vertices; } }

        //---------------------------------------------------------------------
        public Mesh(BinaryReader lsbReader, int numVertices, int numIndizes)
        {
            _vertices = new MeshVertex[numVertices];
            _indizes = new ushort[numIndizes];

            // WPF doesn't like negative texture coordinates
            double uMin = 0;
            double vMin = 0;
            for (int i = 0; i < numVertices; i++)
            {
                MeshVertex mv = new(lsbReader);
                uMin = Math.Min(mv.TexCoords[0].U, uMin);
                vMin = Math.Min(mv.TexCoords[0].V, vMin);
                _vertices[i] = mv;
            }
            uMin = Math.Floor(uMin);
            vMin = Math.Floor(vMin);
            if (uMin != 0 || vMin != 0)
            {
                foreach (MeshVertex mv in _vertices)
                {
                    mv.TexCoords[0].Offset(-uMin, -vMin);
                }
            }

            byte[] temp = new byte[numIndizes * 2];
            lsbReader.Read(temp, 0, temp.Length);
            Buffer.BlockCopy(temp, 0, _indizes, 0, temp.Length);
        }

        //---------------------------------------------------------------------
        public Mesh(List<Vertex> vertices, List<Face> faces)
        {
            _vertices = new MeshVertex[vertices.Count];
            int n = 0;
            // WPF doesn't like negative texture coordinates
            double uMin = 0;
            double vMin = 0;
            foreach (Vertex v in vertices)
            {
                MeshVertex mv = new(v);
                uMin = Math.Min(mv.TexCoords[0].U, uMin);
                vMin = Math.Min(mv.TexCoords[0].V, vMin);
                _vertices[n++] = mv;
            }
            uMin = Math.Floor(uMin);
            vMin = Math.Floor(vMin);
            if (uMin != 0 || vMin != 0)
            {
                foreach (MeshVertex mv in _vertices)
                {
                    mv.TexCoords[0].Offset(-uMin, -vMin);
                }
            }

            _indizes = new ushort[faces.Count * 3];
            n = 0;
            foreach (Face f in faces)
            {
                for (int i = 0; i < 3; i++)
                {
                    _indizes[n++] = (ushort)f[i];
                }
            }
        }

        //---------------------------------------------------------------------
        public Mesh(Mesh source)
        {
            _vertices = new MeshVertex[source._vertices.Length];
            _indizes = new ushort[source._indizes.Length];

            Array.Copy(source._vertices, _vertices, _vertices.Length);
            Array.Copy(source._indizes, _indizes, _indizes.Length);
        }

        //---------------------------------------------------------------------
        /*
         *                     Z
         *                     |
         *                     |___ X
         *     Z  Y            /
         *     | /            /
         *     |/__ X        Y
         *       WPF           Zusi
         * (right handed)  (left handed)
         * reverse winding of triangles
         */
        public MeshGeometry3D CreateModel()
        {
            if (_cachedModel == null)
            {
                MeshGeometry3D mg3d = new();

                foreach (MeshVertex mv in _vertices)
                {
                    mv.AddToMeshGeometry3D(mg3d, 0);
                }

                // reverse order of triangles
                for (int i = 0; i < _indizes.Length; i += 3)
                {
                    mg3d.TriangleIndices.Add(_indizes[i + 2]);
                    mg3d.TriangleIndices.Add(_indizes[i + 1]);
                    mg3d.TriangleIndices.Add(_indizes[i]);
                }

                _cachedModel = mg3d;
            }

            return _cachedModel;
        }

        //---------------------------------------------------------------------
        public MeshInfo GetMeshInfo()
        {
            return new MeshInfo(_indizes.Length / 3);
        }

        //---------------------------------------------------------------------
        public void MoveBy(double xOfs, double yOfs)
        {
            float fxOfs = (float)xOfs;
            float fyOfs = (float)yOfs;

            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i].MoveBy(fxOfs, fyOfs);
            }
        }

        //---------------------------------------------------------------------
        public void Save(Stream stream)
        {
            BinaryWriter bw = new(stream, Encoding.UTF8, true);
            foreach (MeshVertex mv in _vertices)
            {
                mv.Save(bw);
            }

            foreach (ushort index in _indizes)
            {
                bw.Write(index);
            }
        }

        //---------------------------------------------------------------------
        public void RebuildIndizes(int newCount)
        {
            ushort[] a = new ushort[newCount];
            Array.Copy(_indizes, a, newCount);
            _indizes = a;
        }

        //---------------------------------------------------------------------
        public void Dump(TextWriter writer)
        {
            writer.WriteLine(string.Format("---- {0} vectors ----------------------------------------------------------------------",
                _vertices.Length));
            int n = 0;
            foreach (MeshVertex mv in _vertices)
            {
                writer.Write(string.Format("{0:D4}: ", n++));
                mv.Dump(writer);
                writer.WriteLine();
            }
        }

        public void ExportTexCoords(TextWriter writer)
        {
            TexCoord[] points = new TexCoord[3];

            float onepix = 1f / 512f;

            for (int i = 0, i1 = 2, i2 = 1; i < _indizes.Length; i += 3, i1 += 3, i2 += 3)
            {
                points[0] = _vertices[_indizes[i]].TexCoords[0];
                points[1] = _vertices[_indizes[i1]].TexCoords[0];
                points[2] = _vertices[_indizes[i2]].TexCoords[0];
                float du1 = points[1].OrgU - points[0].OrgU;
                float du2 = points[2].OrgU - points[0].OrgU;
                float du = du1 >= du2 ? du1 : du2;
                float dv1 = points[1].OrgV - points[0].OrgV;
                float dv2 = points[2].OrgV - points[0].OrgV;
                float dv = dv1 >= dv2 ? dv1 : dv2;

                writer.Write(string.Format(@"{0,4:###0}: u {1,7:+#0.000;-#0.000;\ \ 0.000} v {2,7:+#0.000;-#0.000;\ \ 0.000}",
                    i / 3,
                    du,
                    dv));
                if (Math.Abs(du) <= onepix && Math.Abs(dv) <= onepix)
                {
                    writer.Write(string.Format(" ! [{0:N3}; {1:N3}] [{2:N3}; {3:N3}] [{4:N3}; {5:N3}] --> [{6:N0}; {7:N0}] [{8:N0}; {9:N0}] [{10:N0}; {11:N0}]",
                        points[0].OrgU, points[0].OrgV,
                        points[1].OrgU, points[1].OrgV,
                        points[2].OrgU, points[2].OrgV,
                        points[0].OrgU * 512, points[0].OrgV * 512,
                        points[1].OrgU * 512, points[1].OrgV * 512,
                        points[2].OrgU * 512, points[2].OrgV * 512

                        ));
                }
                writer.WriteLine();
            }
        }

        //---------------------------------------------------------------------
        public void Hack()
        {
            _vertices.ToList().ForEach(v => v.Hack());
        }
    }
}
