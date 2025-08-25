using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ZusiKlassenLib;
using ZusiKlassenLib.Common;
using ZusiKlassenLib.Fahrplan;
using ZusiKlassenLib.TimeTable;
using ZusiStart.Miscellaneous;
using static System.Net.WebRequestMethods;

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
      Address = "https://www.hlinke.de/ZUSItools/zusistart_dummy_page.html";
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
        string timetabledir = @"\Timetables";
        string timetableshortpath = "";
        int timetables_pos = timetablefilepath.IndexOf(timetabledir);
        if (timetables_pos != -1)
        {
          timetableshortpath = timetablefilepath.Substring(0,timetables_pos+1);
          Begruessungsdatei = $"file:///{Path.Combine(timetableshortpath, timeTable.Begruessungsdatei.Dateiname).Replace("\\", "/")}";
        }
        else
        {
          Begruessungsdatei = "https://www.hlinke.de/ZUSItools/zusistart_dummy_page.html";
        }

        //Begruessungsdatei = $"file:///{Path.Combine(Zusi.DataPath[DataPathType.Official], timeTable.Begruessungsdatei.Dateiname).Replace("\\", "/")}";
        //Begruessungsdatei = $"file:///{Path.Combine(timetableshortpath, timeTable.Begruessungsdatei.Dateiname).Replace("\\", "/")}";
        //DataManager.Instance.webview.Source = new Uri(Begruessungsdatei);
        DataManager.Instance.set_websource(DataManager.Instance.tab_title_docu, Begruessungsdatei);

        //DataManager.Instance.webview.Source = new Uri("https://www.zusidatenbank.de/");
      }
      else
      {
        //Address = string.Format("http://localhost:{0}/default-{1}.html", HttpMiniServer.Port, Guid.NewGuid().ToString("N"));
        //Begruessungsdatei = Path.Combine(Zusi.DataPath[DataPathType.Official], "/DummyPage.html");
        Begruessungsdatei = "https://www.hlinke.de/ZUSItools/zusistart_dummy_page.html";
        //DataManager.Instance.webview.Source = new Uri(Begruessungsdatei);
        DataManager.Instance.set_websource(DataManager.Instance.tab_title_docu, Begruessungsdatei);
      }
      if (timeTable.StartTime != null)
      {
        Hint = string.Format("Fahrplan {0} ab {1}:00 Uhr", timeTable.StartTime.Value.ToString("dd.MMMM yyyy"),timeTable.StartTime.Value.ToString("HH"));
      }

      ZusiDocumentBase doc = timeTable.GetDocument();
      if (false) //!doc.Filename.StartsWith(Zusi.DataPath[DataPathType.Official]))
        TimeTableName = Path.GetFileNameWithoutExtension(doc.Filename) +"(private)";
      else
        TimeTableName = Path.GetFileNameWithoutExtension(doc.Filename);
    }

    //---------------------------------------------------------------------
    public override string ToString()
    {
      return $"{Index} {Address}";
    }
  }
}
