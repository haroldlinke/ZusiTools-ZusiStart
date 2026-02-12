using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZusiCLIProject.Utils;

namespace ZusiCLIProject.Zusi3Fahrschule2
{
  public class Zp9Observer
  {
    public WindowsMessageDataSending Handle { get; set; }
    
    public bool Check(Rectangle mySize)
    {
      try
      {
        var data = new WINDOWINFO();
        GetWindowInfo(Handle.Handle, data);

        Rectangle bounds = data.rcClient.DrawingRect;
        byte[] b;
        using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb))
        {
          using (Graphics g = Graphics.FromImage(bitmap))
          {
            g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
            g.FillRectangle(Brushes.Black, new Rectangle(mySize.Location - new Size(bounds.Left, bounds.Top), mySize.Size));
          }
          var d = bitmap.LockBits(new Rectangle(new Point(0, 0), bounds.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
          b = new byte[bounds.Width * bounds.Height * 4];
          System.Runtime.InteropServices.Marshal.Copy(d.Scan0, b, 0, b.Length);
          bitmap.UnlockBits(d);
        }
        for (int i = 0; i < b.Length - 4 - 30 * 4; i = i + 4)
        {
          if (b[i] != 0 && b[i + 1] != 215 && b[i + 2] != 0 && b[i + 3] != 255)
            continue;

          for (int j = i; j < i + 29 * 4; j++)
          {
            if (j % 2 == 0 && b[j] != 0)
              goto NichtPassend;
            else if (j % 4 == 1 && b[j] != 213 && b[j] != 214 && b[j] != 215)
              goto NichtPassend;
            else if (j % 4 == 3 && b[j] != 255)
              goto NichtPassend;
          }
          return true;
        NichtPassend:
          continue;
        }
      }
      catch
      {
        return false;
      }
      return false;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowInfo", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
    private static extern System.IntPtr GetWindowInfo(System.IntPtr hwnd, WINDOWINFO data);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private class WINDOWINFO
    {
      public Int32 SIZE = /*new System.IntPtr*/(typeof(WINDOWINFO).StructLayoutAttribute.Size);
      public RECT rcWindow;
      public RECT rcClient;
      public System.IntPtr dwStyle;
      public System.IntPtr dwExStyle;
      public System.IntPtr dwWindowStatus;
      public uint cxWindowBorders;
      public uint cyWindowBorders;
      public byte atomWindowType; //ATOM
      public System.Int16 wCreatorVersion; //WORD
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct RECT
    {
      private Int32 LEFT;
      private Int32 TOP;
      private Int32 RIGHT;
      private Int32 BOTTOM;
      public System.Drawing.Rectangle DrawingRect
      {
        get
        {
          return new System.Drawing.Rectangle(LEFT, TOP, RIGHT - LEFT, BOTTOM - TOP);
        }
        set
        {
          LEFT = value.Left;
          TOP = value.Top;
          RIGHT = value.Right;
          BOTTOM = value.Bottom;
        }
      }
    }
  }
}
