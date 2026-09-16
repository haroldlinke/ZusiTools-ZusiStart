using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZusiKlassenLib2.Fahrplan;
using ZusiStart.Data;


namespace ZusiStart.KI
{
  public class RouteClusterInfo
  {
    public ulong ZugId { get; set; }
    public int ClusterId { get; set; }
    public int ClusterSize { get; set; }
    public bool IsUnique { get; set; }
    public bool IsRare { get; set; }
    public bool IsOutlier { get; set; } // bleibt immer false
    public string RouteText { get; set; }
  }

  public class RouteClusterService
  {
    private readonly Dictionary<ulong, RouteClusterInfo> _info;

    public RouteClusterService()
    {
      _info = BuildClusterInfo();
    }

    private static List<string> ExtractStationsFromFahrplan(Zug zug)
    {
      var list = new List<string>();

      foreach (var fp in zug.FahrplanEintraege)
      {
        string name = fp.Bestrst?.Trim();
        if (string.IsNullOrWhiteSpace(name))
          continue;

        list.Add(name);
      }

      return list;
    }

    private static bool IsValidName(string name)
    {
      if (!string.IsNullOrEmpty(name))
      {
        string s = name.ToLower();
        if (s.StartsWith("- zbf") ||
            s.StartsWith("- zf") ||
            s.StartsWith("- kein ") ||
            s.StartsWith("sbk") ||
            s.StartsWith("va") ||
            s.StartsWith("ve") ||
            s.StartsWith("abzw") ||
            s.StartsWith("esig") ||
            s.StartsWith("asig") ||
            s.StartsWith("avsig") ||
            s.StartsWith("bksig") ||
            s.StartsWith("zsig") ||
            s.StartsWith("zvsig") ||
            s.StartsWith("bü") ||
            s.StartsWith("üs") ||
            s.StartsWith("lzb") ||
            s.StartsWith("- eingl") ||
            s.StartsWith("betriebs") ||
            s.StartsWith("strende") ||
            s.StartsWith("streckenende") ||
            s.StartsWith("ende") ||
            s.StartsWith("ri.") ||
            s.StartsWith("von ") ||
            s.StartsWith("nach ") ||
            s.StartsWith("aufgl"))
        {
          return false;
        }
        // Erstes Zeichen prüfen
        char firstChar = s[0];
        return char.IsLetter(firstChar);
      }

      return false;
    }

    private static string NormalizeRoute(Zug zug)
    {
      var rawStations = ExtractStationsFromFahrplan(zug);

      var cleaned = new List<string>();

      foreach (var s in rawStations)
      {
        if (!IsValidName(s))
          continue;

        if (cleaned.Count == 0 || cleaned[^1] != s)
          cleaned.Add(s);
      }

      return string.Join(">", cleaned);
    }



    private Dictionary<string, List<ulong>> BuildRouteDictionary()
    {
      var dict = new Dictionary<string, List<ulong>>();

      foreach (var zug in DataManager.Instance._allTrains)
      {
        string route = NormalizeRoute(zug);

        if (!dict.TryGetValue(route, out var list))
        {
          list = new List<ulong>();
          dict[route] = list;
        }

        list.Add(zug.ID);
      }

      return dict;
    }


    private Dictionary<ulong, RouteClusterInfo> BuildClusterInfo()
    {
      var routeDict = BuildRouteDictionary();
      var clusterInfo = new Dictionary<ulong, RouteClusterInfo>();

      int clusterCounter = 1;

      foreach (var kvp in routeDict)
      {
        string route = kvp.Key;
        var ids = kvp.Value;
        int size = ids.Count;

        foreach (var id in ids)
        {
          clusterInfo[id] = new RouteClusterInfo
          {
            ZugId = id,
            ClusterId = clusterCounter,
            ClusterSize = size,
            IsUnique = size == 1,
            IsRare = size <= 3,
            IsOutlier = false,
            RouteText = route
          };
        }

        clusterCounter++;
      }

      return clusterInfo;
    }



    public RouteClusterInfo GetInfo(Zug zug)
    {
      if (_info.TryGetValue(zug.ID, out var info))
        return info;

      return new RouteClusterInfo
      {
        ZugId = zug.ID,
        ClusterId = 0,
        ClusterSize = 999,
        IsUnique = false,
        IsRare = false,
        IsOutlier = false,
        RouteText = ""
      };
    }
  }

}
