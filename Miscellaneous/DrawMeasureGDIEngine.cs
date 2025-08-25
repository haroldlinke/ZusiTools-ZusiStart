using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using ZusiCLIProject.FileLibrary.Zusi3;

namespace ZusiBuchfahrplanlib
{
  public class DrawMeasureGdiEngine : BuchfahrplanLayoutEngine.IDrawMeasureEngine
  {
    public DrawMeasureGdiEngine(BuchfahrplanLayoutEngine layout)
      : this(layout, Graphics.FromHwnd(System.IntPtr.Zero))
    {
    }
    protected DrawMeasureGdiEngine(BuchfahrplanLayoutEngine layout, Graphics graphics)
    {
      Layout = layout;
      BaseGraphic = graphics;
      PrinterAdjustment = (graphics.DpiY == 600) ? (Graphics.FromHwnd(System.IntPtr.Zero).DpiY / 96) : 1.0f;
    }
    public BuchfahrplanLayoutEngine Layout { get; private set; }
    public Graphics BaseGraphic { get; protected set; }
    public float PrinterAdjustment { get; set; }

    protected System.Drawing.Font GetFontWithStyle(float fontHeightLines, bool draw, bool i, bool b, bool s, bool ul)
    {
      var font = Layout.FontData;
      float newFontHeight = font.Size * fontHeightLines * PrinterAdjustment;
      if (i || b || s || ul || (newFontHeight != font.Size))
      {
        var fontStyle = font.Style;
        if (i)
          fontStyle |= FontStyle.Italic;
        if (b)
          fontStyle |= FontStyle.Bold;
        if (s)
          fontStyle |= FontStyle.Strikeout;
        if (ul)
          fontStyle |= FontStyle.Underline;
        font = new System.Drawing.Font(font.FontFamily, newFontHeight, fontStyle, font.Unit, font.GdiCharSet, font.GdiVerticalFont);
      }
      return font;
    }
    public int MeasureString(string text, float fontHeightLines, bool i, bool b, bool s, bool ul)
    {
      if (string.IsNullOrEmpty(text))
        return 0;

      var font = GetFontWithStyle(fontHeightLines, false, i, b, s, ul);
      if (Layout.KlammerSpacingWorkaround && text.Contains("〉"))
      {
        if (text.StartsWith("〈"))
          text = text.Replace("〉", "\u00A0"); //normales, geschütztes Leerzeichen
      }

      string prefix = "a←ab ";
      string sufix = " a←ab";

      SizeF meas1 = BaseGraphic.MeasureString(prefix + text + sufix, font);

      var format = StringFormat.GenericDefault;
      format.SetMeasurableCharacterRanges(new CharacterRange[] { new CharacterRange(prefix.Length, text.Length) });
      var drawTarget = new System.Drawing.RectangleF(0, 0, meas1.Width, meas1.Height);
      System.Drawing.Region[] reg = BaseGraphic.MeasureCharacterRanges(prefix + text + sufix, font, drawTarget, format);

      RectangleF meas6 = reg[0].GetBounds(BaseGraphic);

      return (int)System.Math.Round(meas6.Width, System.MidpointRounding.AwayFromZero) - 1
      + System.Math.Max((int)(font.Size / 3.0f) - 3, 0);//System.Math.Max(((int)(2 * PrinterAdjustment)) - 2, 0);
    }
  }
  public class DrawGdiEngine : DrawMeasureGdiEngine, BuchfahrplanLayoutEngine.IDrawDrawEngine
  {
    public DrawGdiEngine(BuchfahrplanLayoutEngine layout, Graphics graphics)
      : base(layout, graphics)
    {
      BaseGraphic.TextRenderingHint = Layout.TextRenderingHint;
    }
    public void StartPage(int width, int height) { }
    private Color ColBlack { get { return (Layout.Invers) ? Color.White : Color.Black; } }
    private Color ColWhite { get { return (Layout.Invers) ? Color.Black : Color.White; } }
    public void FillColoredRectangle(int x, int y, int width, int height)
    {
      BaseGraphic.FillRectangle(new SolidBrush((Layout.Invers) ? Color.DimGray : Color.LightGray), x, y, width, height);
    }
    public void DrawGridLine(bool big, int x1, int y1, int x2, int y2)
    {
      BaseGraphic.DrawLine((big) ? new Pen(ColBlack, Layout.LargeLines) : new Pen(ColBlack, Layout.SmallLines), x1, y1, x2, y2);
    }
    public void DrawSzLine(int x1, int y1, int x2, int y2)
    {
      var pen = new Pen(ColBlack, Layout.DrawingLines);
      pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
      pen.EndCap = pen.StartCap;
      BaseGraphic.DrawLine(pen, x1, y1, x2, y2);
    }
    public void DrawTunnel(System.Drawing.Rectangle r, bool isStart)
    {
      int circleRadius = Layout.GetWidthForTunnel(true);
      float arcOffset = 0;
      if (((Layout.SmoothingMode & System.Drawing.Drawing2D.SmoothingMode.AntiAlias) == 0) && (r.Width > 0))
        arcOffset = (1.0f / circleRadius) * 180 / (float)System.Math.PI;
      var pen = new Pen(ColBlack, Layout.DrawingLines);
      if (isStart)
        BaseGraphic.DrawArc(pen, r, -0.5f * arcOffset, 180.0f + 0.5f * arcOffset);
      else
        BaseGraphic.DrawArc(pen, r, 180.0f - 0.5f * arcOffset, 180.0f + 0.5f * arcOffset);
    }
    public void DrawTunnelLine(int x1, int y1, int x2, int y2)
    {
      var pen = new Pen(ColBlack, Layout.DrawingLines);
      pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
      pen.EndCap = pen.StartCap;
      BaseGraphic.DrawLine(pen, x1, y1, x2, y2);
    }
    public void FillTextDecorationBackground(int x, int y, int w, int h)
    {
      BaseGraphic.FillRectangle(new SolidBrush(ColBlack), x, y, w, h);
    }
    public void DrawTextLine(int x1, int y1, int x2, int y2, bool dashed)
    {
      var pn = new Pen(ColBlack, Layout.SmallLines);
      if (dashed)
      {
        pn.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom; //Dash ;
        pn.DashPattern = new float[] { 6 * Layout.SmallLines, 6 * Layout.SmallLines };
      }
      BaseGraphic.DrawLine(pn, x1, y1, x2, y2);
    }
    public void DrawText(string text, float fontHeightLines, bool i, bool b, bool s, bool ul, int x, int y, bool invers)
    {
      var format2 = StringFormat.GenericDefault;
      format2.Trimming = System.Drawing.StringTrimming.None;
      var font = GetFontWithStyle(fontHeightLines, true, i, b, s, ul);
      int paddingL;
      int paddingR;
      MeasureFontPadding(font.FontFamily.Name, font.Size, out paddingL, out paddingR);
      BaseGraphic.DrawString(text, font, (invers ^ Layout.Invers) ? Brushes.White : Brushes.Black, x - paddingL, y, format2);
    }
    public void DrawTextDecoration(Rectangle target, bool round, bool inverted/*, bool strikedTrough*/)
    {
      if (inverted)
      {
        if (round)
          BaseGraphic.FillEllipse(new SolidBrush(ColBlack), target);
        else
          BaseGraphic.FillRectangle(new SolidBrush(ColBlack), target);
      }
      if (round)
        BaseGraphic.DrawEllipse(new Pen(ColBlack, Layout.DrawingLines), target);
      else
        BaseGraphic.DrawRectangle(new Pen(ColBlack, Layout.DrawingLines), target);
      //if (strikedTrough)
      //	BaseGraphic.DrawLine(new Pen(ColBlack, Layout.DrawingLines), targetOld.Location + new Size(targetOld.Width, 0), targetOld.Location + new Size(0, targetOld.Height));
    }
    public void DrawTextDecorationLine(int x1, int y1, int x2, int y2, bool inverted)
    {
      var pen = new Pen((inverted) ? ColWhite : ColBlack, Layout.DrawingLines);
      pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
      pen.EndCap = pen.StartCap;
      BaseGraphic.DrawLine(pen, x1, y1, x2, y2);
    }
    public void FillTextDecorationPolygon(Point[] pts)
    {
      BaseGraphic.FillPolygon(new SolidBrush(ColBlack), pts);
    }
    public void EndPage() { }


    private static Dictionary<string, Dictionary<float, KeyValuePair<int, int>>> fontPaddingBuffer = new Dictionary<string, Dictionary<float, KeyValuePair<int, int>>>();
    public static void MeasureFontPadding(string fontFamily, float fontHeight, out int paddingL, out int paddingR)
    {
      Dictionary<float, KeyValuePair<int, int>> dc1;
      if (!fontPaddingBuffer.TryGetValue(fontFamily, out dc1))
      {
        dc1 = new Dictionary<float, KeyValuePair<int, int>>();
        fontPaddingBuffer.Add(fontFamily, dc1);
      }
      KeyValuePair<int, int> val1;
      if (dc1.TryGetValue(fontHeight, out val1))
      {
        paddingL = val1.Key;
        paddingR = val1.Value;
        return;
      }

      AddMeasureFontPadding(fontFamily, fontHeight, out paddingL, out paddingR);

      dc1.Add(fontHeight, new KeyValuePair<int, int>(paddingL, paddingR));
    }
    private static void AddMeasureFontPadding(string fontFamily, float fontHeight, out int paddingL, out int paddingR)
    {
      int paddingLTemp1, paddingRTemp1;
      AddMeasureFontPadding(fontFamily, "H", fontHeight, out paddingLTemp1, out paddingRTemp1);
      int paddingLTemp2, paddingRTemp2;
      AddMeasureFontPadding(fontFamily, "0", fontHeight, out paddingLTemp2, out paddingRTemp2);
      paddingL = System.Math.Max(paddingLTemp1, paddingLTemp2);
      paddingR = System.Math.Max(paddingRTemp1, paddingRTemp2);
    }
    private static void AddMeasureFontPadding(string fontFamily, string testChr, float fontHeight, out int paddingL, out int paddingR)
    {
      SizeF size0;
      using (var gr = Graphics.FromHwnd(System.IntPtr.Zero))
      {
        size0 = gr.MeasureString(testChr, new System.Drawing.Font(fontFamily, fontHeight));
      }
      Size size1 = new Size((int)System.Math.Round(size0.Width, System.MidpointRounding.AwayFromZero) - 1,
                            (int)System.Math.Round(size0.Height, System.MidpointRounding.AwayFromZero) - 1);
      var bm = new Bitmap(size1.Width, size1.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
      using (var gr = Graphics.FromImage(bm))
      {
        gr.Clear(Color.White);
        gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
        gr.DrawString(testChr, new System.Drawing.Font(fontFamily, fontHeight), Brushes.Black, 0, 0);
      }
      var bmpData = bm.LockBits(new Rectangle(new Point(0, 0), size1), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
      byte[] rgbValues = new byte[System.Math.Abs(bmpData.Stride) * bm.Height];
      System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, rgbValues, 0, rgbValues.Length);
      for (int i = System.Math.Abs(bmpData.Stride); i < rgbValues.Length; ++i)
      {
        rgbValues[i % System.Math.Abs(bmpData.Stride)] = (byte)(rgbValues[i % System.Math.Abs(bmpData.Stride)] & rgbValues[i]);
      }
      //int paddingL = 0;
      for (paddingL = 0; paddingL < System.Math.Abs(bmpData.Stride); paddingL = paddingL + 4)
      {
        if (rgbValues[paddingL] == 0)
          break;
      }
      //int paddingR = 0;
      for (paddingR = 0; paddingR < System.Math.Abs(bmpData.Stride); paddingR = paddingR + 4)
      {
        if (rgbValues[System.Math.Abs(bmpData.Stride) - paddingR - 4] == 0)
          break;
      }
      paddingL = paddingL / 4 - 1;
      paddingR = paddingR / 4 - 1;
    }

  }
}
