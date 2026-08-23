using log4net;
using Sovoma;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;

namespace ZusiKlassenLib2.TimeTable
{
  public static class TimeTables
  {
    private static readonly ILog Log = LogManager.GetLogger(typeof(TimeTables));

    [StructLayout(LayoutKind.Sequential)]
    private struct FileData
    {
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
      public string RelativePath;
      public long Size;
      public DateTime LastWriteTime;
    }

    private static readonly List<string> _blacklist = new()
        {
            //"_Docu",
            @"Deutschland\Infrastrukturdaten"
        };

    private static List<string> _dataPathcovered = new()
        {
            //"_Docu"
        };

    public static void init_timetabledata()
    {
      _dataPathcovered.Clear();
    }

    //---------------------------------------------------------------------
    public static List<TimeTable> EnumerateTimeTables(string[] excludeFolders, CancellationToken cancellationToken)
    {
      List<TimeTable> result = new();

      if (excludeFolders != null && excludeFolders.Length > 0)
      {
        _blacklist.AddRange(excludeFolders);
      }

      // Definiere die gewünschte Reihenfolge
      DataPathType[] Pathorder = new DataPathType[]
      {
            DataPathType.DataDir,
            DataPathType.DataDirProf,
            DataPathType.Official,
            DataPathType.OfficialProf
      };

      // Schleife durch die Werte in der definierten Reihenfolge
      foreach (var dataPathType in Pathorder)
      //foreach (string dataPath in Zusi.DataPath)
      {
        string dataPath = Zusi.DataPath[dataPathType];
        if (string.IsNullOrEmpty(dataPath))
          continue;

        string path = dataPath + @"Timetables\";
        if (Directory.Exists(path))
        {
          foreach (string s in Directory.EnumerateFiles(path, "*.fpn", SearchOption.AllDirectories).Where((s, b) => !IgnoreDataPath(s[path.Length..])))
          {
            try
            {
              TimeTableFile ttf = new(s);
              ttf.Parse();
              result.Add(ttf.Root);
              _dataPathcovered.Add(s[path.Length..]);
            }
            catch (Exception ex)
            {
              Log.ErrorFormat("Couldn't read time table file: {0}{1} {2}", s[path.Length..], Environment.NewLine, ex.ToString());
            }
            if (cancellationToken.IsCancellationRequested)
            {
              return null;
            }
          }
        }
      }

      return result;
    }

    //---------------------------------------------------------------------
    public static string GetHash(params string[] excludeFolders)
    {
      if (excludeFolders != null && excludeFolders.Length > 0)
      {
        _blacklist.AddRange(excludeFolders);
      }

      using MemoryStream ms = new(4096 * 1024);
      foreach (string dataPath in Zusi.DataPath)
      {
        if (string.IsNullOrEmpty(dataPath))
          continue;

        string path = dataPath + @"Timetables\";
        if (Directory.Exists(path))
        {

          foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories).Where((s, b) => !IsBlacklisted(s[path.Length..])))
          {
            FileInfo fi = new(file);
            FileData fd = default;
            fd.RelativePath = file[path.Length..].ToLower();
            fd.Size = fi.Length;
            fd.LastWriteTime = fi.LastWriteTime;
            byte[] contentBytes = fd.GetBytes();
            ms.Write(contentBytes, 0, contentBytes.Length);
          }
        }
      }

      ms.Seek(0, SeekOrigin.Begin);
      using MD5 md5 = MD5.Create();
      byte[] hash = md5.ComputeHash(ms);
      return Convert.ToBase64String(hash);
    }

    //---------------------------------------------------------------------
    private static bool IsBlacklisted(string dir)
    {
      foreach (string s in _blacklist)
      {
        if (dir.StartsWith(s, true, System.Globalization.CultureInfo.CurrentCulture))
          return true;
      }

      return false;
    }

    //---------------------------------------------------------------------
    private static bool IgnoreDataPath(string dir)
    {
      foreach (string s in _blacklist)
      {
        if (dir.StartsWith(s, true, System.Globalization.CultureInfo.CurrentCulture))
          return true;
      }
      foreach (string s in _dataPathcovered)
      {
        if (dir.StartsWith(s, true, System.Globalization.CultureInfo.CurrentCulture))
          return true;
      }

      return false;
    }
  }
}
