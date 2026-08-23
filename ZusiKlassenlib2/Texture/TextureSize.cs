namespace ZusiKlassenLib2.Texture
{
    public class TextureSize
    {
        private static readonly TextureSize _empty = new();
        public static TextureSize Empty => _empty;

        public int Width { get; set; }
        public int Height { get; set; }
    }
}
