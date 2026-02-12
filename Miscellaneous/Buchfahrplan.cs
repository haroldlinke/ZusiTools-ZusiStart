using log4net;
using System.Drawing;
using ZusiCLIProject.FileLibrary.Zusi3;
using ZusiCLIProject.FileLibrary.Zusi3.ZusiFdl;

namespace ZusiBuchfahrplanlib
{
  public class ZT_Buchfahrplan
  {
    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private static List<string> AddedPpLog = new List<string>();
    private static bool AddedPp = false;
    public List<string> LaNumberList = new List<string>();

    private static Buchfahrplan.LaTable[] LoadLaTable()
    {
      string[] zusiDirs = Datei.GetZusiDataDirs();
      string localPfad = @"_Setup\lib\timetable\buchfahrplan2\VzG-La-Streckennummern.csv";
      string gaConfigPfad2 = Datei.TryFindFirstExistingFile(zusiDirs, localPfad);
      return Buchfahrplan.LaTable.GetLaFromCsvFile(gaConfigPfad2); //Soll ruhig eine Ausnahme schmeißen, wenn die VsGs dort nicht da liegen.
    }

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

    public Bitmap show_Buchfahrplan(string fileName)
    {
      //string moduleName = "FahrzeitenheftGeschwindigkeitsheft_DB_1988";
      string moduleName = "Buchfahrplan_DB_2006";
      //var fileName = @"C:\Program Files\Zusi3\_ZusiData\Timetables\Deutschland\Hamburg_Kassel\Guntershausen-Edesheim_2020_06Uhr-10Uhr\RB14202_14009.timetable.xml";
      int std = GetStd(moduleName);
      var currentConfigs = new BuchfahrplanCreationEntryPoints.AllConfigs();
      currentConfigs.GeneralInit(std, true, false, false);
      currentConfigs.GeneralLoad(moduleName);
      Zusi z = null;
      //using (var str = new System.IO.MemoryStream(CurrentFile))
      using (var str = System.IO.File.OpenRead(fileName))
      {
        z = Zusi.Deserialize(str);
      }
      currentConfigs.preprocSetting.ExecuteRemoval(ref z, delegate (bool b) { throw new ArgumentOutOfRangeException(); }, null);


      // determine VGZ and La-Numbers for Ersatzfahrplan und LaPDF access
      bool keepExisting = true;
      foreach (Buchfahrplan b in z.Buchfahrplaene)
      {
        b.ResetLas(LoadLaTable(), keepExisting);
        foreach (Buchfahrplan.FplZeile z1 in b.Zeilen)
          if (z1.FahrstrStreckeLa != null)
          {
            if (!LaNumberList.Contains(z1.FahrstrStreckeLa))
            {
              LaNumberList.Add(z1.FahrstrStreckeLa);
            }
          }
      }

      var layout = currentConfigs.buchfahrplanPdfLayout;
      foreach (Buchfahrplan b in z.Buchfahrplaene)
      {
        string header = b.ToHeaderString(currentConfigs.conversionDisplayStyle, currentConfigs.conversionSettings);
        string csvFile = header + System.Environment.NewLine + b.EntrysToString(currentConfigs.conversionSettings, currentConfigs.conversionDisplayStyle == Buchfahrplan.DisplayStyle.DE1988GeH);
        BuchfahrplanLayoutEngine.IDrawMeasureEngine measureEngine = new BuchfahrplanLayoutEngine.DrawMeasureGdiEngine(layout);
        //var size = currentConfigs.buchfahrplanLayout.CalcSize(csvFile, false, out BuchfahrplanLayoutEngine.TableSize tableSize);
        var size = layout.CalcSize(csvFile, false, out BuchfahrplanLayoutEngine.TableSize tableSize);

        var bitmap = new System.Drawing.Bitmap(size.Width, (int)(size.Height)); // *1.25));
        using (var gra = System.Drawing.Graphics.FromImage(bitmap))
        {
          BuchfahrplanLayoutEngine.IDrawDrawEngine drawEngine = new BuchfahrplanLayoutEngine.DrawGdiEngine(layout, gra);
          gra.Clear((layout.Invers) ? System.Drawing.Color.Black : System.Drawing.Color.White);
          //currentConfigs.buchfahrplanLayout.Draw(csvFile, false, tableSize, size.Width, drawEngine);
          layout.Draw(csvFile, false, tableSize, size.Width, drawEngine);
        }
        //bitmap.Save("Data.png");
        return bitmap;


      }

      return null;
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
