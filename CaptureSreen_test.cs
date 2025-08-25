//using System;
//using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.IO;
//using System.Runtime.InteropServices;
//using System.Threading.Tasks;
//using System.Windows.Media.Imaging;

//[DllImport("user32.dll")]
//private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

//private Bitmap CaptureApplication(string processName)
//{
//  var proc = Process.GetProcessesByName(processName)[0];
//  var rect = new User32.Rect();
//  User32.GetWindowRect(proc.MainWindowHandle, ref rect);
//  int width = rect.right - rect.left;
//  int height = rect.bottom - rect.top;

//  Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
//  Graphics gfxBmp = Graphics.FromImage(bmp);
//  IntPtr hdcBitmap = gfxBmp.GetHdc();

//  PrintWindow(proc.MainWindowHandle, hdcBitmap, 0);

//  gfxBmp.ReleaseHdc(hdcBitmap);
//  gfxBmp.Dispose();

//  return bmp;
//}

//private async Task UpdateImageAsync()
//{
//  while (true)
//  {
//    Bitmap bmp = CaptureApplication("YourPythonAppName");
//    using (MemoryStream memory = new MemoryStream())
//    {
//      bmp.Save(memory, ImageFormat.Bmp);
//      memory.Position = 0;

//      BitmapImage bitmapImage = new BitmapImage();
//      bitmapImage.BeginInit();
//      bitmapImage.StreamSource = memory;
//      bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
//      bitmapImage.EndInit();

//      YourImage.Dispatcher.Invoke(() =>
//      {
//        YourImage.Source = bitmapImage;
//      });
//    }

//    await Task.Delay(1000); // Wartezeit zwischen den Aktualisierungen (in Millisekunden)
//  }
//}

