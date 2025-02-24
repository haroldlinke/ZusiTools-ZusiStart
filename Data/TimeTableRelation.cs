using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZusiKlassenLib;
using ZusiKlassenLib.Common;
using ZusiKlassenLib.Fahrplan;
using ZusiKlassenLib.TimeTable;
using ZusiStart.Miscellaneous;

namespace ZusiStart.Data
{
  public class TimeTableRelation
  {
    public static TimeTableRelation DummyRelation = new TimeTableRelation();

    public int Index { get; private set; }
    public string Address { get; private set; }
    public string Begruessungsdatei { get; private set; }
    public TimeTable TimeTable { get; private set; }
    public string Hint { get; private set; }
    public string TimeTableName { get; private set; }

    //---------------------------------------------------------------------
    private TimeTableRelation()
    {
      Address = $"http://localhost:{HttpMiniServer.Port}/DummyPage.html";
    }

    //---------------------------------------------------------------------
    public TimeTableRelation(int index, TimeTable timeTable)
    {
      Index = index;
      TimeTable = timeTable;
      if (timeTable.Begruessungsdatei != null && !string.IsNullOrEmpty(timeTable.Begruessungsdatei.Dateiname))
      {
        //Address = $"http://localhost:{HttpMiniServer.Port}/" + timeTable.Begruessungsdatei.Dateiname.Replace('\\', '/');
        TimeTableFile timetablefile = timeTable.Parent as TimeTableFile;
        string timetablefilepath = timetablefile.Path;
        string timetableshortpath = Path.Combine(timetablefilepath, "..\\..\\..");

        //Begruessungsdatei = $"file:///{Path.Combine(Zusi.DataPath[DataPathType.Official], timeTable.Begruessungsdatei.Dateiname).Replace("\\", "/")}";
        Begruessungsdatei = $"file:///{Path.Combine(timetableshortpath, timeTable.Begruessungsdatei.Dateiname).Replace("\\", "/")}";
        DataManager.Instance.webview.Source = new Uri(Begruessungsdatei);
        //DataManager.Instance.webview.Source = new Uri("https://www.zusidatenbank.de/");
      }
      else
      {
        Address = string.Format("http://localhost:{0}/default-{1}.html", HttpMiniServer.Port, Guid.NewGuid().ToString("N"));
        Begruessungsdatei = Path.Combine(Zusi.DataPath[DataPathType.Official], "/DummyPage.html");
      }
      if (timeTable.StartTime != null)
      {
        Hint = string.Format("Fahrplan ab {0}:00 Uhr", timeTable.StartTime.Value.ToString("HH"));
      }

      ZusiDocumentBase doc = timeTable.GetDocument();
      TimeTableName = Path.GetFileNameWithoutExtension(doc.Filename);
    }

    //---------------------------------------------------------------------
    public override string ToString()
    {
      return $"{Index} {Address}";
    }
  }
}
