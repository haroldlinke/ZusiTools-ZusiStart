using Sovoma;
using System;
using System.IO;
using ZusiKlassenLib2.Landscape;

namespace ZusiKlassenLib2.Common
{
    public class XFile : IDisposable
    {
        //---------------------------------------------------------------------
        public enum FormatType
        {
            Text,
            Binary,
            CompressedText,
            CompressedBinary
        }

        //---------------------------------------------------------------------
        public enum FloatSizeType
        {
            Single,
            Double
        }

        //---------------------------------------------------------------------
        private static readonly string[] _formatStrings = new string[]
        {
            "txt",
            "bin",
            "tzip",
            "bzip"
        };

        //---------------------------------------------------------------------
        private static readonly string[] _floatSizeStrings = new string[]
        {
            "0032",
            "0064"
        };

        private FileStream _stream;
        private StreamWriter _writer;
        private readonly Indent _indent = new(0, 2);

        private readonly int _majorVersion = 3;
        private readonly int _minorVersion = 2;
        private readonly FormatType _format = FormatType.Text;
        private readonly FloatSizeType _floatSize = FloatSizeType.Single;

        //---------------------------------------------------------------------
        public XFile(string filename)
        {
            _stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(_stream);
        }

        //---------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //---------------------------------------------------------------------
        public void BeginSave()
        {
            WriteHeader();
            _writer.WriteLine("Frame Root {");
            _indent.Increase();
        }

        //---------------------------------------------------------------------
        public void EndSave()
        {
            _indent.Decrease();
            _writer.WriteLineIndented(_indent, "} // end of Frame Root");
            _writer.Flush();
        }

        //---------------------------------------------------------------------
        /*
         * Z  Y     Y  Z
         * | /      | /
         * |/__ X   |/__ X
         *  Zusi      Direct-X
         * 
         */
        public void Write(SubSet subSet, string name)
        {
            // Frame
            _writer.WriteLineIndented(_indent, string.Format("Frame {0} {{", name));
            _indent.Increase();

#if false
            sb.AppendFormat("{0}FrameTransformMatrix {{", indent.ToString());
            sb.AppendLine();
            indent.ToIndent();
            sb.AppendFormat("{0}1.000000, 0.000000, 0.000000, 0.000000,", indent.ToString());
            sb.AppendLine();
            sb.AppendFormat("{0}0.000000, 1.000000, 0.000000, 0.000000,", indent.ToString());
            sb.AppendLine();
            sb.AppendFormat("{0}0.000000, 0.000000, 1.000000, 0.000000,", indent.ToString());
            sb.AppendLine();
            sb.AppendFormat("{0}0.000000, 0.000000, 0.000000, 1.000000;;", indent.ToString());
            sb.AppendLine();
            indent.ToRevIndent();
            sb.AppendFormat("{0}}}", indent.ToString());
            sb.AppendLine();
#endif

            // Mesh
            _writer.WriteFormatLineIndented(_indent, "Mesh {{ // {0} mesh", name);
            _indent.Increase();

            // vector's
            // Zusi X --> x-Achse
            //      Y --> z-Achse
            //      Z --> y-Achse
            _writer.WriteFormatLineIndented(_indent, "{0};", subSet.MeshV);
            MeshVertex[] vertices = subSet.Mesh.Vertices;
            int n = subSet.MeshV - 1;
            for (int i = 0; i < n; i++)
            {
                _writer.WriteFormatLineIndented(_indent, "{0}; {1}; {2};,",
                    vertices[i].Position.X.ToString("N6", Cultures.EnUS),
                    vertices[i].Position.Z.ToString("N6", Cultures.EnUS),
                    vertices[i].Position.Y.ToString("N6", Cultures.EnUS));
            }
            _writer.WriteFormatLineIndented(_indent, "{0}; {1}; {2};;",
                vertices[n].Position.X.ToString("N6", Cultures.EnUS),
                vertices[n].Position.Z.ToString("N6", Cultures.EnUS),
                vertices[n].Position.Y.ToString("N6", Cultures.EnUS));

            // indizes
            _writer.WriteFormatLineIndented(_indent, "{0};", subSet.MeshI / 3);
            n = subSet.MeshI - 3;
            ushort[] indizes = subSet.Mesh.Indizes;
            for (int i = 0; i < n; i += 3)
            {
                _writer.WriteFormatLineIndented(_indent, "3; {0},{1},{2};,",
                    indizes[i], indizes[i + 1], indizes[i + 2]);
            }
            _writer.WriteFormatLineIndented(_indent, "3; {0},{1},{2};;",
                indizes[n], indizes[n + 1], indizes[n + 2]);

            // mesh normals
            _writer.WriteFormatLineIndented(_indent, "MeshNormals {{ // {0} normals", name);
            _indent.Increase();
            _writer.WriteFormatLineIndented(_indent, "{0};", subSet.MeshI / 3);
            n = subSet.MeshI - 3;
            int k;
            for (int i = 0; i < n; i += 3)
            {
                k = indizes[i];
                _writer.WriteFormatLineIndented(_indent, "{0}; {1}; {2};,",
                    vertices[k].Normal.X.ToString("N6", Cultures.EnUS),
                    vertices[k].Normal.Z.ToString("N6", Cultures.EnUS),
                    vertices[k].Normal.Y.ToString("N6", Cultures.EnUS));
            }
            k = indizes[n];
            _writer.WriteFormatLineIndented(_indent, "{0}; {1}; {2};;",
                vertices[k].Normal.X.ToString("N6", Cultures.EnUS),
                vertices[k].Normal.Z.ToString("N6", Cultures.EnUS),
                vertices[k].Normal.Y.ToString("N6", Cultures.EnUS));

            // face normals
            _writer.WriteFormatLineIndented(_indent, "{0};", subSet.MeshI / 3);
            n = subSet.MeshI - 3;
            for (int i = 0; i < n; i += 3)
            {
                _writer.WriteFormatLineIndented(_indent, "3; {0},{1},{2};,",
                    indizes[i], indizes[i + 1], indizes[i + 2]);
            }
            _writer.WriteFormatLineIndented(_indent, "3; {0},{1},{2};;",
                indizes[n], indizes[n + 1], indizes[n + 2]);
            _indent.Decrease();
            _writer.WriteLineIndented(_indent, "}");

            // tex coords
            _writer.WriteFormatLineIndented(_indent, "MeshTextureCoords {{ // {0} UV coordinates", name);
            _indent.Increase();
            _writer.WriteFormatLineIndented(_indent, "{0};", subSet.MeshV);
            n = subSet.MeshV - 1;
            for (int i = 0; i < n; i++)
            {
                _writer.WriteFormatLineIndented(_indent, "{0}; {1};,",
                    vertices[i].TexCoords[0].OrgU.ToString("N6", Cultures.EnUS),
                    (1 - vertices[i].TexCoords[0].OrgV).ToString("N6", Cultures.EnUS));
            }
            _writer.WriteFormatLineIndented(_indent, "{0}; {1};;",
                (vertices[n].TexCoords[0].OrgU).ToString("N6", Cultures.EnUS),
                (1 - vertices[n].TexCoords[0].OrgV).ToString("N6", Cultures.EnUS));
            _indent.Decrease();
            _writer.WriteLineIndented(_indent, "}");

            // material
            _writer.WriteLineIndented(_indent, "Material Material {");
            _indent.Increase();
            _writer.WriteLineIndented(_indent, "1;");
            _writer.WriteFormatLineIndented(_indent, "{0};", subSet.MeshI / 3);
            n = subSet.MeshI / 3 - 1;
            for (int i = 0; i < n; i++)
            {
                _writer.WriteLineIndented(_indent, "0,");
            }
            _writer.WriteLineIndented(_indent, "0;;");
            // face color
            XColor xclr = new(subSet.DiffuseColor);
            _writer.WriteFormatLineIndented(_indent, "{0}; {1}; {2}; {3};;",
                xclr.R.ToString("N6", Cultures.EnUS),
                xclr.G.ToString("N6", Cultures.EnUS),
                xclr.B.ToString("N6", Cultures.EnUS),
                xclr.A.ToString("N6", Cultures.EnUS));
            // power
            _writer.WriteLineIndented(_indent, "100.000000;");
            // specular color
            _writer.WriteLineIndented(_indent, "0.000000; 0.000000; 0.000000;;");
            // emissive color
            if (string.IsNullOrEmpty(subSet.EmissiveColor))
            {
                _writer.WriteLineIndented(_indent, "0.000000; 0.000000; 0.000000;;");
            }
            else
            {
                xclr = new XColor(subSet.EmissiveColor);
                _writer.WriteFormatLineIndented(_indent, "{0}; {1}; {2}; {3};;",
                    xclr.R.ToString("N6", Cultures.EnUS),
                    xclr.G.ToString("N6", Cultures.EnUS),
                    xclr.B.ToString("N6", Cultures.EnUS),
                    xclr.A.ToString("N6", Cultures.EnUS));
            }
            // texture
            if (subSet.Textures.Count > 0)
            {
                _writer.WriteLineIndented(_indent, "TextureFilename {");
                _indent.Increase();
                _writer.WriteFormatLineIndented(_indent, "\"{0}\";", Path.GetFileName(subSet.Textures[0].Filename));
                _indent.Decrease();
                _writer.WriteLineIndented(_indent, "}");
            }
            _indent.Decrease();
            _writer.WriteLineIndented(_indent, "}");
            // end of mesh
            _indent.Decrease();
            _writer.WriteLineIndented(_indent, "} end of mesh");
            // end of frame
            _indent.Decrease();
            _writer.WriteLineIndented(_indent, "} end of frame");
        }

        //---------------------------------------------------------------------
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _writer?.Dispose();
                _writer = null;
                _stream?.Dispose();
                _stream = null;
            }
        }

        //---------------------------------------------------------------------
        private void WriteHeader()
        {
            _writer.Write("xof ");
            _writer.Write(_majorVersion.ToString("D2"));
            _writer.Write(_minorVersion.ToString("D2"));
            _writer.Write(_formatStrings[(int)_format]);
            _writer.Write(" ");
            _writer.WriteLine(_floatSizeStrings[(int)_floatSize]);
            _writer.WriteLine();
            _writer.WriteLine("// Created by ZusiKlassenLib2");
            _writer.WriteLine();
        }
    }

    static class Helper
    {
        //---------------------------------------------------------------------
        public static void WriteIndented(this TextWriter self, Indent indent, string s)
        {
            indent.Write(self);
            self.Write(s);
        }

        //---------------------------------------------------------------------
        public static void WriteLineIndented(this TextWriter self, Indent indent, string s)
        {
            indent.Write(self);
            self.WriteLine(s);
        }

        //---------------------------------------------------------------------
        public static void WriteFormatLineIndented(this TextWriter self, Indent indent, string format, params object[] args)
        {
            indent.Write(self);
            self.WriteLine(string.Format(format, args));
        }
    }
}
