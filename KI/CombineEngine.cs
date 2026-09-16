// CombineEngine.cs
using GMap.NET.MapProviders;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using ZusiKlassenLib2.Fahrplan;

using ZusiStart.Data;
using ZusiStart.Miscellaneous;

namespace ZusiStart.KI
{
  public class CombineEngine
  {
    private static readonly ILog _log = LogManager.GetLogger(typeof(CombineEngine));
    private readonly ZusiLocalKiEngine _local;
    //private List<KIResult> _allResults = new List<KIResult>();
    private HashSet<ulong> _allResults2 = new HashSet<ulong>();

    /******************************************
     * CombineEngine
     * *************************************/
    public CombineEngine(ZusiLocalKiEngine localEngine)
    {
      _local = localEngine ?? throw new ArgumentNullException(nameof(localEngine));
    }

    // ---------------------------------------------------------
    // GRUPPENVERARBEITUNG (LOKALES AND / OR)
    // ---------------------------------------------------------
    // EvaluateGroup2
    // *************************************/
    private HashSet<ulong> EvaluateGroup2(JsonElement cond)
    {
      var logical = GetParameter(cond, "logical");
      var conditions = cond.GetProperty("conditions");

      var sets = new List<HashSet<ulong>>();

      foreach (var c in conditions.EnumerateArray())
        sets.Add(EvaluateCondition2(c));

      return logical == "or"
          ? CombineOr(sets)
          : CombineAnd(sets);
    }

    /******************************************
     * CombineAnd
     * *************************************/
    public HashSet<ulong> CombineAnd(List<HashSet<ulong>> sets)
    {
      if (sets.Count == 0)
        return new HashSet<ulong>();

      var result = new HashSet<ulong>(sets[0]);

      for (int i = 1; i < sets.Count; i++)
        result.IntersectWith(sets[i]);

      return result;
    }

    /******************************************
     * CombineOr
     * *************************************/
    public HashSet<ulong> CombineOr(List<HashSet<ulong>> sets)
    {
      var result = new HashSet<ulong>();

      foreach (var s in sets)
        result.UnionWith(s);

      return result;
    }

    /******************************************
     * ApplyNegation
     * *************************************/
    public HashSet<ulong> ApplyNegation(HashSet<ulong> all, HashSet<ulong> matches)
    {
      var result = new HashSet<ulong>(all);
      result.ExceptWith(matches);
      return result;
    }

    /******************************************
     * EvaluateCondition2
     * *************************************/
    public HashSet<ulong> EvaluateCondition2(JsonElement cond)
    {
      var type = GetParameter(cond, "type");
      var negate = cond.TryGetProperty("negate", out var n) && n.ValueKind == JsonValueKind.True;

      HashSet<ulong> result = type switch
      {
        "von_nach" => EvaluateVonNach2(cond),
        "ueber" => EvaluateUeber2(cond),
        "von_nach_ueber" => EvaluateVonNachUeber2(cond),
        "fahrzeug" => EvaluateFahrzeug2(cond),
        "zugnummer" => EvaluateZugnummer2(cond),
        "zuggattung" => EvaluateZuggattung2(cond),
        "zugart" => EvaluateZugart2(cond),
        "datum_vor" => EvaluateDatumErweitert2(cond, type),
        "datum_nach" => EvaluateDatumErweitert2(cond, type),
        "datum_zwischen" => EvaluateDatumErweitert2(cond, type),
        "dauer_min" => EvaluateDauer2(cond, type),
        "dauer_max" => EvaluateDauer2(cond, type),
        "dauer_zwischen" => EvaluateDauer2(cond, type),
        "ml_anomaly" => EvaluateMlAnomaly(cond),

        "ml_route_unique" => EvaluateMlRouteUnique(cond),
        "ml_route_rare" => EvaluateMlRouteRare(cond),
        "ml_route_outlier" => EvaluateMlRouteOutlier(cond),
        "ml_route_cluster_size" => EvaluateMlRouteClusterSize(cond),
        "ml_route_debug" => EvaluateMlRouteDebug(cond),

        "group" => EvaluateGroup2(cond),
        _ => new HashSet<ulong>()
      };

      if (negate)
      {
        if (_allResults2.Count == 0)
        {
          // Wenn _allResults leer ist, fülle es mit allen Zügen
          _allResults2 = DataManager.Instance._allTrains.Select(t => t.ID).ToHashSet();
        }
        var all = _allResults2;
        result = ApplyNegation(all, result);
      }

      return result;
    }

        // ---------------------------------------------------------
    // EINZELNE BEDINGUNGSTYPEN
    // ---------------------------------------------------------
    /******************************************
     * EvaluateVonNach2
     * *************************************/
    private HashSet<ulong> EvaluateVonNach2(JsonElement cond)
    {
      var from = GetParameter(cond, "from", "start", "von");
      var to = GetParameter(cond, "to", "nach", "ziel");
      
      return _local.FindZuegeVonNach2(from, to);
    }

    /******************************************
     * EvaluateUeber2
     * *************************************/
    private HashSet<ulong> EvaluateUeber2(JsonElement cond)
    {
      var stations = GetParameterList(cond, "stations", "ueber", "via");

      return _local.FindZuegeUeberStationen2(stations);
    }

    /******************************************
     * EvaluateVonNachUeber2
     * *************************************/
    private HashSet<ulong> EvaluateVonNachUeber2(JsonElement cond)
    {
      var from = GetParameter(cond, "from");
      var to = GetParameter(cond, "to");
      var via = GetParameterList(cond, "via");

      return _local.FindZuegeVonNachUeber2(from, to, via);
    }

    /******************************************
     * EvaluateFahrzeug2
     * *************************************/
    private HashSet<ulong> EvaluateFahrzeug2(JsonElement cond)
    {
      var pattern = GetParameter(cond, "pattern", "fahrzeug", "baureihe");

      return _local.FindZuegeMitFahrzeug2(pattern);
    }

    /******************************************
     * EvaluateZugnummer2
     * *************************************/
    private HashSet<ulong> EvaluateZugnummer2(JsonElement cond)
    {
      var patterns = GetParameterList(cond, "patterns", "nummern");

      return _local.FindZuegeMitZugnummer2(patterns);
    }

    /******************************************
     * EvaluateDatumErweitert2
     * *************************************/
    private HashSet<ulong> EvaluateDatumErweitert2(JsonElement cond, string mode)
    {
      var value1 = GetParameter(cond, "value", "from");
      var value2 = GetParameter(cond, "to");

      return _local.FindZuegeMitDatumfilterErweitert2(mode.Replace("datum_", ""), value1, value2);
    }

    /******************************************
     * EvaluateDauer2
     * *************************************/
    private HashSet<ulong> EvaluateDauer2(JsonElement cond, string mode)
    {
      var value1 = GetParameter(cond, "value", "from");
      var value2 = GetParameter(cond, "to");

      return _local.FindZuegeMitDauerfilter2(mode.Replace("dauer_", ""), value1, value2);
    }

    /******************************************
     * EvaluateZugart2
     * *************************************/
    private HashSet<ulong> EvaluateZugart2(JsonElement cond)
    {
      var value = GetParameter(cond, "value");

      return _local.FindZuegeMitZugart2(value);
    }
    
    /******************************************
     * EvaluateZuggattung2
     * *************************************/
    private HashSet<ulong> EvaluateZuggattung2(JsonElement cond)
    {
      var value = GetParameter(cond, "value");

      if (string.IsNullOrWhiteSpace(value))
        return new HashSet<ulong>();

      return _local.FindZuegeMitZuggattung2(value);
    }

    /******************************************
     * EvaluateZuggattung2
     * *************************************/
    private HashSet<ulong> EvaluateMlAnomaly(JsonElement cond)
    {
      return _local.EvaluateMlAnomaly(cond);
    }

    /******************************************
     * EvaluateMlRouteUnique
     * *************************************/
    private HashSet<ulong> EvaluateMlRouteUnique(JsonElement cond)
    {
      return _local.EvaluateMlRouteUnique(cond);
    }

    /******************************************
     * EvaluateMlRouteRare
     * *************************************/
    private HashSet<ulong> EvaluateMlRouteRare(JsonElement cond)
    {
      return _local.EvaluateMlRouteRare(cond);
    }

    /******************************************
     * EvaluateMlRouteOutlier
     * *************************************/
    private HashSet<ulong> EvaluateMlRouteOutlier(JsonElement cond)
      {
      return _local.EvaluateMlRouteOutlier(cond);
    }

    /******************************************
     * EvaluateMlRouteClusterSize
     * *************************************/
    private HashSet<ulong> EvaluateMlRouteClusterSize(JsonElement cond)
      {
      return _local.EvaluateMlRouteClusterSize(cond);
    }

    /******************************************
     * EvaluateMlRouteDebug
     * *************************************/
    private HashSet<ulong> EvaluateMlRouteDebug(JsonElement cond)
      {
      return _local.EvaluateMlRouteDebug(cond);
    }



    // ---------------------------------------------------------
    // HELFER: PARAMETER AUS JSON
    // ---------------------------------------------------------
    /******************************************
     * GetParameter
     * *************************************/
    private string GetParameter(JsonElement cond, params string[] names)
    {
      foreach (var name in names)
      {
        if (!cond.TryGetProperty(name, out var prop))
          continue;

        switch (prop.ValueKind)
        {
          case JsonValueKind.String:
            return prop.GetString();

          case JsonValueKind.Number:
            // Zahl in String umwandeln
            if (prop.TryGetInt32(out var i))
              return i.ToString();
            if (prop.TryGetDouble(out var d))
              return d.ToString(CultureInfo.InvariantCulture);
            break;

          case JsonValueKind.Null:
            return null;
        }
      }

      return null;
    }


    /******************************************
     * GetParameterList
     * *************************************/
    private List<string> GetParameterList(JsonElement obj, string name, params string[] aliases)
    {
      var list = new List<string>();

      if (obj.ValueKind != JsonValueKind.Object)
        return list;

      if (obj.TryGetProperty(name, out var arr) && arr.ValueKind == JsonValueKind.Array)
      {
        foreach (var e in arr.EnumerateArray())
        {
          if (e.ValueKind == JsonValueKind.String)
            list.Add(e.GetString());
        }
        return list;
      }

      foreach (var a in aliases)
      {
        if (obj.TryGetProperty(a, out var arr2) && arr2.ValueKind == JsonValueKind.Array)
        {
          foreach (var e in arr2.EnumerateArray())
          {
            if (e.ValueKind == JsonValueKind.String)
              list.Add(e.GetString());
          }
          return list;
        }
      }

      return list;
    }

    /******************************************
     * GetConditionList
     * *************************************/
    public List<JsonElement> GetConditionList(JsonElement analysis)
    {
      var list = new List<JsonElement>();

      if (analysis.ValueKind != JsonValueKind.Object)
        return list;

      if (analysis.TryGetProperty("parameters", out var p) &&
          p.ValueKind == JsonValueKind.Object &&
          p.TryGetProperty("conditions", out var arr) &&
          arr.ValueKind == JsonValueKind.Array)
      {
        foreach (var e in arr.EnumerateArray())
          list.Add(e);
        return list;
      }

      // Für Gruppen: direkt "conditions" im Objekt
      if (analysis.TryGetProperty("conditions", out var arr2) &&
          arr2.ValueKind == JsonValueKind.Array)
      {
        foreach (var e in arr2.EnumerateArray())
          list.Add(e);
        return list;
      }

      return list;
    }
  }
}

