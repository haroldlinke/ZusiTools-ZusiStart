using log4net;
using Sovoma;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ZusiKlassenLib2.Vehicle
{
  public static class Fahrzeuge
  {
    private static readonly ILog Log = LogManager.GetLogger(typeof(Fahrzeuge));
    private static readonly string fahrzeug_extension = "*.fzg";

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
            "Diverse",
        };

    //---------------------------------------------------------------------
    public static List<Fahrzeug> EnumerateVehicles(string[] excludeFolders, CancellationToken cancellationToken)
    {
      List<Fahrzeug> result = new();

      if (excludeFolders != null && excludeFolders.Length > 0)
      {
        _blacklist.AddRange(excludeFolders);
      }

      foreach (string dataPath in Zusi.DataPath)
      {
        if (string.IsNullOrEmpty(dataPath))
          continue;

        string path = dataPath + @"RollingStock\";
        if (Directory.Exists(path))
        {
#if NET48
          foreach (string s in Directory.EnumerateFiles(path, fahrzeug_extension, SearchOption.AllDirectories).Where((s, b) => !IsBlacklisted(s.Substring(path.Length))))
#else
                    foreach (string s in Directory.EnumerateFiles(path, fahrzeug_extension, SearchOption.AllDirectories).Where((s, b) => !IsBlacklisted(s[path.Length..])))
#endif
          {
            Fahrzeug f = ProcessVehicleFile(path, s);
            if (f != null)
            {
              result.Add(f);
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

        string path = dataPath + @"RollingStock\";
        if (Directory.Exists(path))
        {
#if NET48
          foreach (string file in Directory.GetFiles(path, fahrzeug_extension, SearchOption.AllDirectories).Where((s, b) => !IsBlacklisted(s.Substring(path.Length))))
#else
                    foreach (string file in Directory.GetFiles(path, fahrzeug_extension, SearchOption.AllDirectories).Where((s, b) => !IsBlacklisted(s[path.Length..])))
#endif
          {
            FileInfo fi = new(file);
            FileData fd = default;
#if NET48
            fd.RelativePath = file.Substring(path.Length).ToLower();
#else
                        fd.RelativePath = file[path.Length..].ToLower();
#endif
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
    private static Fahrzeug ProcessVehicleFile(string basePath, string filename)
    {
      Fahrzeug result = null;

      try
      {
        FahrzeugDatei fd = new(filename);
        fd.Parse();
        result = fd.Root;
        if (result == null)
        {
#if NET48
          Log.ErrorFormat("Couldn't read vehicle file: {0}", filename.Substring(basePath.Length));
#else
                    Log.ErrorFormat("Couldn't read vehicle file: {0}", filename[basePath.Length..]);
#endif
        }
        else if (result.Varianten.Count > 0)
        {
          VehicleCache.Add(filename, result);
        }
        else
        {
          result = null;
        }
      }
      catch (Exception ex)
      {
#if NET48
        Log.ErrorFormat("Couldn't read vehicle file: {0}{1}   {2}", filename.Substring(basePath.Length), Environment.NewLine, ex.ToString());
#else
                Log.ErrorFormat("Couldn't read vehicle file: {0}{1}   {2}", filename[basePath.Length..], Environment.NewLine, ex.ToString());
#endif
      }

      return result;
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
  }
}
