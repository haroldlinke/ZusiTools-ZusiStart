using System.IO;
using System.Text;

namespace ZusiKlassenLib2.Common
{
    public static class IDManager
    {
        private static ulong _id = 0;

        public static ulong GetNextID()
        {
            return ++_id;
        }

        public static void ReadID(Stream s)
        {
            BinaryReader br = new(s, Encoding.Default, true);
            _id = br.ReadUInt64();
        }

        public static void WriteID(Stream s)
        {
            BinaryWriter bw = new(s, Encoding.Default, true);
            bw.Write(_id);
        }
    }
}
