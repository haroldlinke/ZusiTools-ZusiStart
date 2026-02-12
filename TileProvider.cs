// TileProvider.cs
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

public interface ITileProvider
{
  Task<BitmapImage> GetTileAsync(int z, int x, int y);
}

public class FileTileProvider : ITileProvider
{
  private readonly string _basePath;

  public FileTileProvider(string basePath)
  {
    _basePath = basePath;
  }

  public async Task<BitmapImage> GetTileAsync(int z, int x, int y)
  {
    string path = Path.Combine(_basePath, z.ToString(), x.ToString(), $"{y}.png");
    if (!File.Exists(path)) return null;

    var bmp = new BitmapImage();
    bmp.BeginInit();
    bmp.CacheOption = BitmapCacheOption.OnLoad;
    bmp.UriSource = new Uri(path);
    bmp.EndInit();
    bmp.Freeze();
    return await Task.FromResult(bmp);
  }
}

