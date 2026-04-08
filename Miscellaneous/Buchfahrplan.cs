using log4net;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZusiCLIProject.FileLibrary.Zusi3;
using ZusiCLIProject.FileLibrary.Zusi3.ZusiFdl;
using ZusiStart.Data;

namespace ZusiBuchfahrplanlib
{

  public class DrawMeasureWpfEngine : BuchfahrplanLayoutEngine.IDrawMeasureEngine
  {
    public DrawMeasureWpfEngine(BuchfahrplanLayoutEngine layout)
    {
      Layout = layout;
    }
    public BuchfahrplanLayoutEngine Layout { get; private set; }

    protected float m_FontSizeAdjustWpfGdi;
    protected float FontSizeAdjustWpfGdi
    {
      get
      {
        if (m_FontSizeAdjustWpfGdi == 0)
        {
          System.Windows.Media.FormattedText formattedText2 =
          new System.Windows.Media.FormattedText("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789",
          System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
          new System.Windows.Media.Typeface(
          new System.Windows.Media.FontFamily(Layout.FontData.FontFamily.Name),
          FontStyles.Normal,
          FontWeights.Normal,
          FontStretches.Normal),
          Layout.FontData.Size, System.Windows.Media.Brushes.Black
          //,(96.0 * 1.25)
          );
          m_FontSizeAdjustWpfGdi = Layout.FontData.Height / (float)formattedText2.Height;
        }
        return m_FontSizeAdjustWpfGdi;
      }
    }

    public virtual int MeasureString(string text, float fontHeightLines, bool i, bool b, bool s, bool ul)
    {

      if (string.IsNullOrEmpty(text))
        return 0;

      var font = Layout.FontData;
      float newFontHeight = font.Size * fontHeightLines * FontSizeAdjustWpfGdi;

      if (Layout.KlammerSpacingWorkaround && text.Contains("〉"))
      {
        if (text.StartsWith("〈"))
          text = text.Replace("〉", "\u00A0"); //normales, geschütztes Leerzeichen
      }

      System.Windows.Media.FormattedText formattedText =
      new System.Windows.Media.FormattedText(text,
      System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
      new System.Windows.Media.Typeface(
      new System.Windows.Media.FontFamily(font.FontFamily.Name),
      i ? FontStyles.Italic : FontStyles.Normal,
      b ? FontWeights.Bold : FontWeights.Normal,
      FontStretches.Normal),
      newFontHeight, System.Windows.Media.Brushes.Black
      //,(96.0 * 1.25)
      );

      return (int)System.Math.Round(formattedText.Width, System.MidpointRounding.AwayFromZero);
    }
  }
  public class DrawWpfEngine : DrawMeasureWpfEngine, BuchfahrplanLayoutEngine.IDrawDrawEngine
  {
    public DrawWpfEngine(BuchfahrplanLayoutEngine layout, Panel baseControl)
      : base(layout)
    {
      BaseControl = baseControl;
    }
    public Panel BaseControl { get; protected set; }

    public void StartPage(int width, int height)
    {
      //IsInDrawPage = true;
    }
    public void FillColoredRectangle(int x, int y, int width, int height)
    {
      UsedMoreThanTwoColors = true;
      var shape1 = new System.Windows.Shapes.Rectangle();
      shape1.Width = width;
      shape1.Height = height;
      shape1.Fill = System.Windows.Media.Brushes.DimGray;
      shape1.RenderTransform = new System.Windows.Media.TranslateTransform(x, y);
      BaseControl.Children.Add(shape1);
    }
    public void DrawGridLine(bool big, int x1, int y1, int x2, int y2)
    {
      var line1 = new System.Windows.Shapes.Line();
      line1.Stroke = System.Windows.Media.Brushes.Black;
      line1.X1 = x1;
      line1.X2 = x2;
      line1.Y1 = y1;
      line1.Y2 = y2;
      line1.StrokeThickness = big ? Layout.LargeLines : Layout.SmallLines;
      BaseControl.Children.Add(line1);
    }
    public void DrawSzLine(int x1, int y1, int x2, int y2)
    {
      var line1 = new System.Windows.Shapes.Line();
      line1.Stroke = System.Windows.Media.Brushes.Black;
      line1.StrokeStartLineCap = System.Windows.Media.PenLineCap.Round;
      line1.StrokeEndLineCap = line1.StrokeStartLineCap;
      line1.X1 = x1;
      line1.X2 = x2;
      line1.Y1 = y1;
      line1.Y2 = y2;
      line1.StrokeThickness = Layout.DrawingLines;
      BaseControl.Children.Add(line1);
    }
    public void DrawTunnel(System.Drawing.Rectangle r, bool isStart)
    {
      var path1 = new System.Windows.Shapes.Path();
      path1.Stroke = System.Windows.Media.Brushes.Black;
      path1.StrokeThickness = Layout.DrawingLines;
      path1.HorizontalAlignment = HorizontalAlignment.Left;
      path1.VerticalAlignment = VerticalAlignment.Top;
      path1.RenderTransform = new System.Windows.Media.TranslateTransform(r.X, r.Y);
      var arc1 = new System.Windows.Media.ArcSegment();
      arc1.Size = new System.Windows.Size(r.Width / 2, r.Height / 2);
      arc1.Point = new System.Windows.Point(r.Width, r.Height / 2);
      arc1.SweepDirection = isStart ? System.Windows.Media.SweepDirection.Counterclockwise : System.Windows.Media.SweepDirection.Clockwise;
      path1.Data = new System.Windows.Media.PathGeometry(
      new System.Windows.Media.PathFigure[] {
      new System.Windows.Media.PathFigure(new System.Windows.Point(0,r.Height / 2),
      new System.Windows.Media.PathSegment[] {arc1}, false)});
      BaseControl.Children.Add(path1);
    }
    public void DrawTunnelLine(int x1, int y1, int x2, int y2)
    {
      var line1 = new System.Windows.Shapes.Line();
      line1.Stroke = System.Windows.Media.Brushes.Black;
      line1.StrokeStartLineCap = System.Windows.Media.PenLineCap.Round;
      line1.StrokeEndLineCap = line1.StrokeStartLineCap;
      line1.X1 = x1;
      line1.X2 = x2;
      line1.Y1 = y1;
      line1.Y2 = y2;
      line1.StrokeThickness = Layout.DrawingLines;
      BaseControl.Children.Add(line1);
    }
    public void FillTextDecorationBackground(int x, int y, int w, int h)
    {
      var shape1 = new System.Windows.Shapes.Rectangle();
      shape1.Width = w;
      shape1.Height = h;
      shape1.Fill = System.Windows.Media.Brushes.Black;
      shape1.RenderTransform = new System.Windows.Media.TranslateTransform(x, y);
      BaseControl.Children.Add(shape1);
    }
    public void DrawTextLine(int x1, int y1, int x2, int y2, bool dashed)
    {
      var line1 = new System.Windows.Shapes.Line();
      line1.Stroke = System.Windows.Media.Brushes.Black;
      if (dashed)
      {
        line1.StrokeDashArray.Add(6 * Layout.SmallLines);
        line1.StrokeDashArray.Add(6 * Layout.SmallLines);
      }
      line1.X1 = x1;
      line1.X2 = x2;
      line1.Y1 = y1;
      line1.Y2 = y2;
      line1.StrokeThickness = Layout.DrawingLines;
      BaseControl.Children.Add(line1);
    }
    public void DrawText(string text, float fontHeightLines, bool i, bool b, bool s, bool ul, int x, int y, bool invers)
    {
      var font = Layout.FontData;
      float newFontHeight = font.Size * fontHeightLines * FontSizeAdjustWpfGdi;
      var textBl = new TextBlock();
      textBl.Text = text;
      textBl.FontFamily = new System.Windows.Media.FontFamily(font.FontFamily.Name);
      textBl.FontSize = newFontHeight;
      textBl.FontStyle = i ? FontStyles.Italic : FontStyles.Normal;
      textBl.FontWeight = b ? FontWeights.Bold : FontWeights.Normal;
      textBl.Foreground = invers ? System.Windows.Media.Brushes.White : System.Windows.Media.Brushes.Black;
      textBl.TextDecorations.Clear();
      if (s)
        textBl.TextDecorations.Add(TextDecorations.Strikethrough);
      if (ul)
        textBl.TextDecorations.Add(TextDecorations.Underline);
      int paddingL;
      int paddingR;
      MeasureFontPadding(font.FontFamily.Name, font.Size, out paddingL, out paddingR);
      textBl.RenderTransform = new System.Windows.Media.TranslateTransform(x - paddingL, y);
      BaseControl.Children.Add(textBl);
    }
    public void DrawTextDecoration(System.Drawing.Rectangle target, bool round, bool inverted)
    {
      System.Windows.Shapes.Shape shape1;
      if (round)
      {
        var shape2 = new System.Windows.Shapes.Ellipse();
        shape2.Width = target.Width;
        shape2.Height = target.Height;
        shape1 = shape2;
      }
      else
      {
        var shape2 = new System.Windows.Shapes.Rectangle();
        shape2.Width = target.Width;
        shape2.Height = target.Height;
        shape1 = shape2;
      }
      if (inverted)
        shape1.Fill = System.Windows.Media.Brushes.Black;
      shape1.Stroke = System.Windows.Media.Brushes.Black;
      shape1.RenderTransform = new System.Windows.Media.TranslateTransform(target.X, target.Y);
      BaseControl.Children.Add(shape1);
    }
    public void DrawTextDecorationLine(int x1, int y1, int x2, int y2, bool inverted)
    {
      var line1 = new System.Windows.Shapes.Line();
      line1.Stroke = inverted ? System.Windows.Media.Brushes.White : System.Windows.Media.Brushes.Black;
      line1.X1 = x1;
      line1.X2 = x2;
      line1.Y1 = y1;
      line1.Y2 = y2;
      line1.StrokeThickness = Layout.DrawingLines;
      BaseControl.Children.Add(line1);
    }
    public void FillTextDecorationPolygon(System.Drawing.Point[] pts)
    {
      var path1 = new System.Windows.Shapes.Path();
      path1.Fill = System.Windows.Media.Brushes.Black;
      path1.HorizontalAlignment = HorizontalAlignment.Left;
      path1.VerticalAlignment = VerticalAlignment.Top;
      var pfui = new System.Collections.Generic.List<System.Windows.Media.PathSegment>();
      for (int i = 1; i < pts.Length; ++i)
        pfui.Add(new System.Windows.Media.LineSegment(new System.Windows.Point(pts[i].X, pts[i].Y), false));
      path1.Data = new System.Windows.Media.PathGeometry(
      new System.Windows.Media.PathFigure[] {
      new System.Windows.Media.PathFigure(new System.Windows.Point(pts[0].X, pts[0].Y),
      pfui, true)});
      BaseControl.Children.Add(path1);
    }
    public void EndPage()
    {
      //IsInDrawPage = false;
    }


    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Advanced)]
    public bool UsedMoreThanTwoColors { get; set; }

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
      paddingL = 0;
      paddingR = 0;
    }

  }
  
  public class ZT_Buchfahrplan
  {
    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private static List<string> AddedPpLog = new List<string>();
    private static bool AddedPp = false;
    public List<string> LaNumberList = new List<string>();

    //private static Buchfahrplan.LaTable[] LoadLaTable()
    //{
    //  string[] zusiDirs = Datei.GetZusiDataDirs();
    //  string localPfad = @"_Setup\lib\timetable\buchfahrplan2\VzG-La-Streckennummern.csv";
    //  string gaConfigPfad2 = Datei.TryFindFirstExistingFile(zusiDirs, localPfad);
    //  return Buchfahrplan.LaTable.GetLaFromCsvFile(gaConfigPfad2); //Soll ruhig eine Ausnahme schmeißen, wenn die VsGs dort nicht da liegen.
    //}

    internal static void InitPp() //Wird auch von ExtensionMethods.AddZusiMenuAddToMenue aufgerufen.
    {
      if (!AddedPp)
      {
        //System.AppDomain.CurrentDomain.AppendPrivatePath(System.IO.Path.GetDirectoryName(me.Location) + @"\lib");
        var me = System.Reflection.Assembly.GetExecutingAssembly();
        var myLocation = System.IO.Path.GetDirectoryName(me.Location);
        System.AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args)
        {
          string[] nameOptions = args.Name.Split(new string[] { ", ", "," }, StringSplitOptions.None);
          string[] options = new string[] {myLocation + @"\..\..\_InstSetup\lib\timetable\lib\" + nameOptions[0] + ".dll",
                    myLocation + @"\..\..\_InstSetup\lib\timetable\lib\" + nameOptions[0] + ".exe"};
          foreach (string s in options)
          {
            if (System.IO.File.Exists(s))
            {
              AddedPpLog.Clear();
              return System.Reflection.Assembly.LoadFile(s);
            }
            else
            {
              AddedPpLog.Add(s);
              Console.WriteLine("Resolving " + args.Name + "=> " + s + " = no");
            }
          }
          //Console.WriteLine("Resolving " + args.Name);
          return null;
        };
      }
      AddedPp = true;
    }

    //public Bitmap show_Buchfahrplan(string fileName)
    //{
    //  //string moduleName = "FahrzeitenheftGeschwindigkeitsheft_DB_1988";
    //  string moduleName = "Buchfahrplan_DB_2006";
    //  //var fileName = @"C:\Program Files\Zusi3\_ZusiData\Timetables\Deutschland\Hamburg_Kassel\Guntershausen-Edesheim_2020_06Uhr-10Uhr\RB14202_14009.timetable.xml";
    //  int std = GetStd(moduleName);
    //  var currentConfigs = new BuchfahrplanCreationEntryPoints.AllConfigs();
    //  currentConfigs.GeneralInit(std, true, false, false);
    //  currentConfigs.GeneralLoad(moduleName);
    //  Zusi z = null;
    //  //using (var str = new System.IO.MemoryStream(CurrentFile))
    //  using (var str = System.IO.File.OpenRead(fileName))
    //  {
    //    z = Zusi.Deserialize(str);
    //  }
    //  currentConfigs.preprocSetting.ExecuteRemoval(ref z, delegate (bool b) { throw new ArgumentOutOfRangeException(); }, null);


    //  // determine VGZ and La-Numbers for Ersatzfahrplan und LaPDF access
    //  bool keepExisting = true;
    //  foreach (Buchfahrplan b in z.Buchfahrplaene)
    //  {
    //    b.ResetLas(LoadLaTable(), keepExisting);
    //    foreach (Buchfahrplan.FplZeile z1 in b.Zeilen)
    //      if (z1.FahrstrStreckeLa != null)
    //      {
    //        if (!LaNumberList.Contains(z1.FahrstrStreckeLa))
    //        {
    //          LaNumberList.Add(z1.FahrstrStreckeLa);
    //        }
    //      }
    //  }

    //  var layout = currentConfigs.buchfahrplanPdfLayout;
    //  foreach (Buchfahrplan b in z.Buchfahrplaene)
    //  {
    //    string header = b.ToHeaderString(currentConfigs.conversionDisplayStyle, currentConfigs.conversionSettings);
    //    string csvFile = header + System.Environment.NewLine + b.EntrysToString(currentConfigs.conversionSettings, currentConfigs.conversionDisplayStyle == Buchfahrplan.DisplayStyle.DE1988GeH);
    //    BuchfahrplanLayoutEngine.IDrawMeasureEngine measureEngine = new BuchfahrplanLayoutEngine.DrawMeasureGdiEngine(layout);
    //    //var size = currentConfigs.buchfahrplanLayout.CalcSize(csvFile, false, out BuchfahrplanLayoutEngine.TableSize tableSize);
    //    var size = layout.CalcSize(csvFile, false, out BuchfahrplanLayoutEngine.TableSize tableSize);

    //    var bitmap = new System.Drawing.Bitmap(size.Width, (int)(size.Height)); // *1.25));
    //    using (var gra = System.Drawing.Graphics.FromImage(bitmap))
    //    {
    //      BuchfahrplanLayoutEngine.IDrawDrawEngine drawEngine = new BuchfahrplanLayoutEngine.DrawGdiEngine(layout, gra);
    //      gra.Clear((layout.Invers) ? System.Drawing.Color.Black : System.Drawing.Color.White);
    //      //currentConfigs.buchfahrplanLayout.Draw(csvFile, false, tableSize, size.Width, drawEngine);
    //      layout.Draw(csvFile, false, tableSize, size.Width, drawEngine);
    //    }
    //    //bitmap.Save("Data.png");
    //    return bitmap;


    //  }

    //  return null;
    //}

    // Hilfsmethode zum Konvertieren von RenderTargetBitmap zu System.Drawing.Bitmap
    public static System.Drawing.Bitmap RenderTargetBitmapToBitmap(System.Windows.Media.Imaging.RenderTargetBitmap bmp)
    {
      // Benutze PNG-Encoder, damit der Alpha-Kanal erhalten bleibt
      using (var memoryStream = new System.IO.MemoryStream())
      {
        var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmp));
        encoder.Save(memoryStream);
        memoryStream.Seek(0, System.IO.SeekOrigin.Begin);

        // tempBitmap enthält Alpha-Kanal
        using (var tempBitmap = new System.Drawing.Bitmap(memoryStream))
        {
          // Wenn transparente Bereiche einen weißen Hintergrund haben sollen,
          // auf ein neues Bitmap ohne Alpha compositen.
          var result = new System.Drawing.Bitmap(tempBitmap.Width, tempBitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
          using (var g = System.Drawing.Graphics.FromImage(result))
          {
            g.Clear(System.Drawing.Color.White); // gewünschter Hintergrund (hier: Weiß). Bei Bedarf ändern.
                                                 // DrawImage composited tempBitmap (mit Alpha) über den weißen Hintergrund
            g.DrawImage(tempBitmap, new System.Drawing.Rectangle(0, 0, tempBitmap.Width, tempBitmap.Height));
            g.Flush();
          }
          return result;
        }
      }
    }

    public List<Bitmap> show_Buchfahrplan2(string fileName, string buchfahrplanDll)
    {
      ZusiKlassenLib2.DataPathType _dataPath = ZusiKlassenLib2.DataPathType.Unknown;
      string nativestandards_csv_file = ZusiKlassenLib2.Zusi.GetAbsolutePathOf(@"\..\_InstSetup\lib\timetable\lib\NativeStandards.csv",ref _dataPath);

      ZusiCLIProject.FileLibrary.Zusi3.BuchfahrplanCreationEntryPoints.BuchfahrplanCreationEpoche.ManualInitNativeStandardsFileName(nativestandards_csv_file);
      
      List<Bitmap> result_bitmaps = [];
      
      string buchfahrplanDLL_name = Path.GetFileNameWithoutExtension(buchfahrplanDll);

      if (DataManager.Instance.options.Buchfahrplanlayout != "Automatisch aus TRN-Datei")
        buchfahrplanDLL_name = DataManager.Instance.options.Buchfahrplanlayout;

      var epoche = ZusiCLIProject.FileLibrary.Zusi3.BuchfahrplanCreationEntryPoints.BuchfahrplanCreationEpoche.CreateByDllPfadLikeWrittenInTrnFiles(buchfahrplanDLL_name);
      var logging = new List<string>();

      string workingdir = ZusiKlassenLib2.Zusi.DataPath[ZusiKlassenLib2.DataPathType.Official];
      var context = epoche.CreateRenderingContext(workingdir, 
        fileName,  
        null,
        logging);
      
      var closeAndSave = new List<Action<int>>();
      context.SetConfigForEngineOverride(false, delegate (BuchfahrplanLayoutEngine[] engines, bool[] isBesidePrevious, int[] numberOfPagesRequired)
      {
        var value = new BuchfahrplanLayoutEngine.IDrawMeasureEngine[engines.Length][];
        for (int i = 0; i < engines.Length; ++i)
        {
          value[i] = new BuchfahrplanLayoutEngine.IDrawMeasureEngine[numberOfPagesRequired[i]];
          if (engines[i].FontData.Name.Contains("Bahnschrift"))
            value[i][0] = new BuchfahrplanLayoutEngine.DrawMeasureGdiEngine(engines[i]);
          else
            value[i][0] = new DrawMeasureWpfEngine(engines[i]);
            for (int j = 0; j < numberOfPagesRequired[i]; ++j)
              value[i][j] = value[i][0];
        }
        return value;
      },
      delegate (BuchfahrplanLayoutEngine[] engines, bool[] isBesidePrevious, int[] numberOfPagesRequired, System.Drawing.Size[][] paperSizes)
      {

        var value = new BuchfahrplanLayoutEngine.IDrawDrawEngine[engines.Length][];
        for (int i = 0; i < engines.Length; ++i)
        {
          value[i] = new BuchfahrplanLayoutEngine.IDrawDrawEngine[numberOfPagesRequired[i]];
          for (int j = 0; j < numberOfPagesRequired[i]; ++j)
          {
            if (engines[i].FontData.Name.Contains("Bahnschrift"))
            {
              var paperSize = paperSizes[i][j];
              var engine = engines[i];
              var bmp = new Bitmap(paperSize.Width, paperSize.Height);
              var gra = Graphics.FromImage(bmp);
              var drawEngine = new BuchfahrplanLayoutEngine.DrawGdiEngine(engines[i], gra);
              value[i][j] = drawEngine;

              closeAndSave.Add(delegate (int fileIndex)
              {
                gra.Dispose();
                if ((engine.TextRenderingHint == System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit) || (engine.TextRenderingHint == System.Drawing.Text.TextRenderingHint.SingleBitPerPixel))
                  bmp = bmp.Clone(new Rectangle(new System.Drawing.Point(0, 0), paperSize), (drawEngine.UsedMoreThanTwoColors) ? System.Drawing.Imaging.PixelFormat.Format4bppIndexed : System.Drawing.Imaging.PixelFormat.Format1bppIndexed);
                bmp.Save("Result_" + fileIndex + ".gdi.png");
                result_bitmaps.Add(bmp);
              });
            }
            else
            {
              var paperSize = paperSizes[i][j];
              var engine = engines[i];
              var canvas = new System.Windows.Controls.Canvas();
              var drawEngine = new DrawWpfEngine(engines[i], canvas);
              value[i][j] = drawEngine;


              closeAndSave.Add(delegate (int fileIndex)
              {
                canvas.Measure(new System.Windows.Size(paperSize.Width, paperSize.Height));
                canvas.Arrange(new System.Windows.Rect(0, 0, paperSize.Width, paperSize.Height));
                var rbmp = new System.Windows.Media.Imaging.RenderTargetBitmap(paperSize.Width, paperSize.Height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                rbmp.Render(canvas);
                Bitmap bmp = RenderTargetBitmapToBitmap(rbmp);
                result_bitmaps.Add(bmp);
                //result_bitmap = RenderTargetBitmapToBitmap(bmp);
                
                //PngBitmapEncoder png = new PngBitmapEncoder();
                //png.Frames.Add(BitmapFrame.Create(bmp));

                //using (MemoryStream memoryStream = new MemoryStream())
                //{
                //  png.Save(memoryStream);
                //  memoryStream.Seek(0, SeekOrigin.Begin);

                //  // Create a GDI+ Bitmap from the stream
                //  result_bitmap = new Bitmap(memoryStream);
                //}

                //using (Stream stm = File.Create("Result_" + fileIndex + ".wpf.png"))
                //{
                //  png.Save(stm);
                //}
              });
            }
          }
        }
        return value;

      });
      context.ModifyAndRender();
      {
        int i = -1;
        foreach (var action in closeAndSave)
        {
          ++i;
          action(i);
        }
      }
      return result_bitmaps; // BitmapFrame.FromFile("Result_0.gdi.png");
    }

    private static int GetStd(string moduleName)
    {
      try
      {
        var mth1 = typeof(BuchfahrplanCreationEntryPoints).GetMethod("LoadNativeDirectory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        var std1 = typeof(BuchfahrplanCreationEntryPoints).GetField("NativeDirectorys", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        mth1.Invoke(null, new object[] { });
        var nativeDirectorys = (System.Collections.Generic.Dictionary<string, int>)std1.GetValue(null);
        if (nativeDirectorys.ContainsKey(moduleName))
          return nativeDirectorys[moduleName]; //Soll ruhig eine Ausnahme werfen.
        else
          return nativeDirectorys["*"]; //Soll ruhig eine Ausnahme werfen.
      }
      catch
      {
        int testvalue = 3;
        return testvalue;
      }
    }

    public void TestLoadingFramework(string fileName, string nummer, out int utmx, out int utmy, out int zone, out string zone2)
    {
      try
      {
        var lFr = new ZusiCLIProject.FileLibrary.Zusi3.ZusiFdl.LoadingFramework<int>();
        string[] zddir = new string[] { System.IO.Directory.GetCurrentDirectory() + @"\..\..\Zusi3DatenDir" };
        if (System.IO.Directory.Exists(zddir[0]))
          lFr.DataDirs = zddir;
        lFr.FahrplanPfade = new string[] {
		//@"Timetables\Deutschland\SFS_Goettingen_Kassel\Goettingen_Kassel_2017_14Uhr-01Uhr.fpn"
		//@"Timetables\Deutschland\Hamburg_Kassel\Lehrte-Veddel_Berlinumleiter_2021_15Uhr-00Uhr.fpn"
    fileName
    };
        int linesTotal = 0;
        lFr.InitItems(delegate (string s)
        {
          ++linesTotal;
          //System.Console.WriteLine(s);
          return linesTotal - 1;
        });
        lFr.StartLoading(delegate (string nw, string ol, int oi)
        {
          //System.Console.CursorTop -= linesTotal - oi;
          //System.Console.CursorLeft = 0;
          while (nw.Length < ol.Length)
            nw = nw + " ";
          //System.Console.Write(nw);
          //System.Console.CursorLeft = 0;
          //System.Console.CursorTop += linesTotal - oi;
          return oi;
        }, delegate (string nw, int oi)
        {
          //System.Console.CursorTop -= linesTotal - oi;
          //System.Console.CursorLeft = nw.Length;
          //System.Console.Write(" OK");
          //System.Console.CursorLeft = 0;
          //System.Console.CursorTop += linesTotal - oi;
        });
        Zug zug = lFr.Zuege.FirstOrDefault(z => z.Nummer == nummer);

        ZugDispoState dispo = new ZugDispoState(lFr.Fdl2, zug);
        //Finden des bevorzugten Fahrweges
        List<KeyValuePair<Zug.FahrplanEintrag, Strecke.Fahrstrasse>> hauptpfad = dispo.CalculateHauptbuchfplFstr();
        Strecke.ElementInfo[][] alleMoeglichenFahrwegeDieserFahrstrasseInclNichtDefinierterWeichen = hauptpfad.First().Value.GetFahrwege();
        Strecke.ElementInfo[] relevantesterFahrweg = alleMoeglichenFahrwegeDieserFahrstrasseInclNichtDefinierterWeichen.First();
        Location elementPunkt = relevantesterFahrweg.First().ParentBuffer.BlueDirectionInfo == relevantesterFahrweg.First() ?
          relevantesterFahrweg.First().ParentBuffer.BlueLocation : relevantesterFahrweg.First().ParentBuffer.GreenLocation;
        Strecke.UTM utm = relevantesterFahrweg.First().ParentBuffer.ParentBuffer.UTMPoint;
        utmx = utm.WE;
        utmy = utm.NS;
        zone = utm.Zone;
        zone2 = utm.Zone2;
        elementPunkt = elementPunkt.Add(relevantesterFahrweg.First().ParentBuffer.ParentBuffer.UTMPoint.ToLocation());
        elementPunkt = elementPunkt;
      }
      catch (Exception ex)
      {
        _log.Error(ex);
        _log.Info(fileName + "-" + nummer + ":Could not load framework, setting UTM to 0");
        utmx = 0; utmy = 0; zone = 0; zone2 = "U";
      }
    }
  }
}
