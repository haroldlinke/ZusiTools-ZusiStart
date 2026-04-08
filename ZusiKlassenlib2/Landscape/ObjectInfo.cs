using System.Collections.Generic;
using System.Linq;
using ZusiKlassenLib2.Texture;

namespace ZusiKlassenLib2.Landscape
{
    public class MeshInfo
    {
        private TextureSize _textureSize = TextureSize.Empty;

        public int CountTriangles { get; private set; }

        public TextureSize TextureSize { get => _textureSize; }

        internal MeshInfo(int countTriangles)
        {
            CountTriangles = countTriangles;
        }

        internal void AddTextureSize(TextureSize size)
        {
            if (size.Width > _textureSize.Width || size.Height > _textureSize.Height)
            {
                _textureSize = size;
            }
        }
    }

    public class VisibilityRange
    {
        public float From { get; private set; }
        public float To { get; private set; }

        public VisibilityRange(float from, float to)
        {
            From = from;
            To = to;
        }
    }

    public class LoDInfo
    {
        private readonly List<MeshInfo> _meshInfos = new();

        public int CountTriangles { get => _meshInfos.Sum(mi => mi.CountTriangles); }

        public int LoD { get; private set; }

        public List<MeshInfo> MeshInfos { get => _meshInfos; }

        public VisibilityRange Range { get; private set; }

        internal LoDInfo(int lod, float from, float to)
        {
            LoD = lod;
            Range = new VisibilityRange(from, to);
        }

        internal void AddMeshInfo(MeshInfo mi)
        {
            _meshInfos.Add(mi);
        }
    }

    public class ObjectInfo
    {
        private readonly LoDInfo[] _loDInfos = new LoDInfo[4];

        public LoDInfo this[int lod]
        {
            get => lod >= 0 && lod < 4 ? _loDInfos[lod] : null;
            internal set
            {
                if (lod >= 0 && lod < 4)
                {
                    _loDInfos[lod] = value;
                }
            }
        }
    }
}
