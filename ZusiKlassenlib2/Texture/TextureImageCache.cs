using DDSLib.WPF;
using log4net;
using Sovoma;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ZusiKlassenLib2.Texture
{
    public class TextureImageCache : Singleton<TextureImageCache>, ISingletonBase
    {
        private class CacheItem
        {
            public ImageBrush Brush { get; set; }
            public TextureSize Size { get; set; }
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(TextureImageCache));
        private static readonly byte[] _signature = new byte[4];

        private readonly Dictionary<int, CacheItem> _cache = new();
        private readonly object _lock = new();

        //---------------------------------------------------------------------
        public static void CleanUp()
        {
            Instance.CleanUp_impl();
        }

        //---------------------------------------------------------------------
        public static ImageBrush GetElement(string filepath)
        {
            return Instance.GetElement_impl(filepath, out _);
        }

        //---------------------------------------------------------------------
        public static ImageBrush GetElement(string filepath, out TextureSize size)
        {
            return Instance.GetElement_impl(filepath, out size);
        }

        //---------------------------------------------------------------------
        public void Initialize()
        { }

        //---------------------------------------------------------------------
        private void CleanUp_impl()
        {
            lock (_lock)
            {
                _cache.Clear();
            }
        }

        //---------------------------------------------------------------------
        private bool Exists(int hash)
        {
            lock (_lock)
            {
                return _cache.ContainsKey(hash);
            }
        }

        //---------------------------------------------------------------------
        private ImageBrush GetElement_impl(string filepath, out TextureSize size)
        {
            size = TextureSize.Empty;

            try
            {
                int hash = filepath.ToLower().GetHashCode();
                CacheItem item;
                if (!Exists(hash))
                {
                    ImageSource src;

                    if (IsDDSImage(filepath))
                    {
                        DDSImage dds = new(new Uri(filepath));
                        src = dds;
                        size = new TextureSize() { Width = dds.PixelWidth, Height = dds.PixelHeight };
                    }
                    else
                    {
                        BitmapImage bi = new();
                        using (FileStream fs = new(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            bi.BeginInit();
                            bi.CacheOption = BitmapCacheOption.OnLoad;
                            bi.StreamSource = fs;
                            bi.EndInit();
                        }
                        src = bi;
                        size = new TextureSize() { Width = bi.PixelWidth, Height = bi.PixelHeight };
                    }
                    item = new CacheItem
                    {
                        Brush = new ImageBrush(src)
                        {
                            ViewportUnits = BrushMappingMode.Absolute,
                            TileMode = TileMode.Tile
                        },
                        Size = size
                    };

                    lock (_lock)
                    {
                        _cache[hash] = item;
                    }
                }
                else
                {
                    lock (_lock)
                    {
                        item = _cache[hash];
                    }
                }

                size = item.Size;
                return item.Brush;
            }
            catch (Exception ex)
            {
                Log.FatalFormat("{0}{1}   while reading {2}", ex.ToString(), Environment.NewLine, filepath);
            }

            return null;
        }

        //---------------------------------------------------------------------
        private static bool IsDDSImage(string filepath)
        {
            using FileStream fs = new(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
            lock (_signature)
            {
                fs.Read(_signature, 0, 4);
                return _signature[0] == 'D' && _signature[1] == 'D' && _signature[2] == 'S' && _signature[3] == ' ';
            }
        }
    }
}
