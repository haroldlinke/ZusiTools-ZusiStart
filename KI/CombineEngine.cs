// CombineEngine.cs
using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using ZusiStart.Data;
using ZusiStart.Miscellaneous;

namespace ZusiStart.KI
{
  public class CombineEngine2
  {
    private static readonly ILog _log = LogManager.GetLogger(typeof(CombineEngine));
    private readonly ZusiLocalKiEngine _local;

    public CombineEngine2(ZusiLocalKiEngine local)
    {
      _local = local;
    }

    // ------------------------------------------------------------
    // PARAMETER-PARSER (String oder Zahl)
    // ------------------------------------------------------------
    private string GetParam(JsonElement cond, params string[] names)
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
            if (prop.TryGetInt32(out var i))
              return i.ToString();
            if (prop.TryGetDouble(out var d))
              return d.ToString(CultureInfo.InvariantCulture);
            break;
        }
      }
      return null;
    }

    // ------------------------------------------------------------
    // NEGATION ERKENNEN
    // ------------------------------------------------------------
    private bool IsNegation(string value)
    {
      if (value == null) return false;
      value = value.ToLower();
      return value.StartsWith("not ") || value.StartsWith("nicht ");
    }

    private string ExtractNegated(string value)
    {
      if (value == null) return null;
      value = value.ToLower();

      if (value.StartsWith("not "))
        return value.Substring(4).Trim();

      if (value.StartsWith("nicht "))
        return value.Substring(6).Trim();

      return value;
    }

    // ------------------------------------------------------------
    // NEGATION DURCH ERGEBNIS-SUBTRAKTION
    // ------------------------------------------------------------
    private List<KIResult> ApplyNegation(List<KIResult> current, List<KIResult> exclude)
    {
      if (current == null) return new List<KIResult>();
      if (exclude == null) return current;

      return current.Where(r => !exclude.Any(e => e.Zug_Id == r.Zug_Id)).ToList();
    }

    // ------------------------------------------------------------
    // DAUER IN SEKUNDEN
    // ------------------------------------------------------------
    private int? ToInt(string raw)
    {
      if (raw == null) return null;
      if (int.TryParse(raw, out var n))
        return n;
      return null;
    }

    // ------------------------------------------------------------
    // REKURSIVER CONDITION-EVALUATOR
    // ------------------------------------------------------------
    private List<KIResult> Evaluate(JsonElement cond)
    {
      var type = cond.GetProperty("type").GetString();

      // -------------------------
      // GRUPPE (rekursiv)
      // -------------------------
      if (type == "group")
      {
        var logical = cond.GetProperty("logical").GetString();
        var nested = cond.GetProperty("conditions");

        List<KIResult> result = null;

        foreach (var n in nested.EnumerateArray())
        {
          var r = Evaluate(n);

          if (result == null)
          {
            result = r;
            continue;
          }

          if (logical == "and")
            result = result.Intersect(r).ToList();
          else if (logical == "or")
            result.AddRange(r);
        }

        return result ?? new List<KIResult>();
      }

      // -------------------------
      // EINZELBEDINGUNG
      // -------------------------
      return EvaluateSingle(cond);
    }

    // ------------------------------------------------------------
    // EINZELBEDINGUNG
    // ------------------------------------------------------------
    private List<KIResult> EvaluateSingle(JsonElement cond)
    {
      var type = cond.GetProperty("type").GetString();

      switch (type)
      {
        case "von_nach":
          return _local.FindZuegeVonNach(
              GetParam(cond, "from"),
              GetParam(cond, "to")
          ).Results;

        case "ueber":
          var stations = cond.GetProperty("stations")
                             .EnumerateArray()
                             .Select(s => ExtractNegated(s.GetString()))
                             .ToList();

          var ueberResults = _local.FindZuegeUeberStationen(stations).Results;

          if (IsNegation(cond.GetProperty("stations")[0].GetString()))
          {
            // Negation: alle Züge über diese Station entfernen
            var exclude = ueberResults;
            return exclude; // wird später subtrahiert
          }

          return ueberResults;

        case "von_nach_ueber":
          return _local.FindZuegeVonNachUeber(
              GetParam(cond, "from"),
              GetParam(cond, "to"),
              cond.GetProperty("via").EnumerateArray().Select(v => v.GetString()).ToList()
          ).Results;

        case "fahrzeug":
          var pattern = GetParam(cond, "pattern");
          var neg = IsNegation(pattern);
          var val = ExtractNegated(pattern);

          var fahrzeugResults = _local.FindZuegeMitFahrzeug(val).Results;

          if (neg)
            return fahrzeugResults; // später subtrahiert

          return fahrzeugResults;

        case "zugnummer":
          var patterns = cond.GetProperty("patterns")
                             .EnumerateArray()
                             .Select(p => p.GetString())
                             .ToList();
          return _local.FindZuegeMitZugnummer(patterns).Results;

        case "zuggattung":
          var g = GetParam(cond, "value");
          var gNeg = IsNegation(g);
          var gVal = ExtractNegated(g);

          var gResults = _local.FindZuegeMitZuggattung(gVal).Results;

          if (gNeg)
            return gResults; // später subtrahiert

          return gResults;

        case "zugart":
          var a = GetParam(cond, "value");
          var aNeg = IsNegation(a);
          var aVal = ExtractNegated(a);

          var aResults = _local.FindZuegeMitZugart(aVal).Results;

          if (aNeg)
            return aResults; // später subtrahiert

          return aResults;

        case "dauer_min":
          return _local.FindZuegeMitDauerfilter("min", ToInt(GetParam(cond, "value")).ToString(), null).Results;

        case "dauer_max":
          return _local.FindZuegeMitDauerfilter("max", ToInt(GetParam(cond, "value")).ToString(), null).Results;

        case "dauer_zwischen":
          return _local.FindZuegeMitDauerfilter(
              "between",
              ToInt(GetParam(cond, "from")).ToString(),
              ToInt(GetParam(cond, "to")).ToString()
          ).Results;

        default:
          return new List<KIResult>();
      }
    }

    // ------------------------------------------------------------
    // HAUPTLOGIK: REKURSIV + NEGATION
    // ------------------------------------------------------------
    public ZusiToolResult ExecuteCombine(JsonElement analysis)
    {
      var parameters = analysis.GetProperty("parameters");
      var logical = parameters.GetProperty("logical").GetString();
      var conditions = parameters.GetProperty("conditions");

      _log.Debug($"CombineConditions: {string.Join(", ", conditions.EnumerateArray().Select(c => c.ToString()))}");
      DataManager.Instance.AddKIResultMessage($"* CombineConditions: {string.Join(", ", conditions.EnumerateArray().Select(c => c.ToString()))}");


      List<KIResult> result = null;

      foreach (var cond in conditions.EnumerateArray())
      {
        var r = Evaluate(cond);

        // Negation?
        bool neg =
            IsNegation(GetParam(cond, "pattern")) ||
            IsNegation(GetParam(cond, "value")) ||
            (cond.TryGetProperty("stations", out var st) &&
             st.GetArrayLength() > 0 &&
             IsNegation(st[0].GetString()));

        if (neg)
        {
          // Subtraktion
          result = result == null ? new List<KIResult>() : ApplyNegation(result, r);
        }
        else
        {
          // normale AND-Verknüpfung
          result = result == null ? r : result.Intersect(r).ToList();
        }
      }
      return new ZusiToolResult
      {
        Action = "combine",
        Results = result,
        Summary = $"{result.Count} Züge gefunden (Kombination)."
      };
    }

  }
    



  public class CombineEngine
  {
    private static readonly ILog _log = LogManager.GetLogger(typeof(CombineEngine));
    private readonly ZusiLocalKiEngine _local;

    public CombineEngine(ZusiLocalKiEngine localEngine)
    {
      _local = localEngine ?? throw new ArgumentNullException(nameof(localEngine));
    }

    // ---------------------------------------------------------
    // ENTRY POINT FROM YOUR LOCAL TOOL EXECUTION
    // ---------------------------------------------------------

    public ZusiToolResult ExecuteCombine(JsonElement analysis)
    {
      // Top-Level logical (AND/OR) – default AND
      var logical = GetParameter(analysis, "logical") ?? "and";

      // Flache Top-Level-Bedingungen
      var conditions = GetConditionList(analysis);

      _log.Debug($"CombineConditions called with {conditions.Count} conditions.\n conditions: {string.Join(", ", conditions.Select(c => c.ToString()))}");
      DataManager.Instance.AddKIResultMessage($"* CombineConditions called with {conditions.Count} conditions. \n conditions: {string.Join(", ", conditions.Select(c => c.ToString()))}");

      var resultSets = new List<List<KIResult>>();

      foreach (var cond in conditions)
      {
        var r = EvaluateCondition(cond);
        resultSets.Add(r);
      }

      var final = CombineTopLevel(resultSets, logical);

      return new ZusiToolResult
      {
        Action = "combine",
        Results = final,
        Summary = $"{final.Count} Züge gefunden (Kombination)."
      };
    }

    // ---------------------------------------------------------
    // CORE: REKURSIVE BEDINGUNGSAUSWERTUNG
    // ---------------------------------------------------------

    private List<KIResult> EvaluateCondition(JsonElement cond)
    {
      var type = GetParameter(cond, "type");

      switch (type)
      {
        case "von_nach":
          return EvaluateVonNach(cond);

        case "ueber":
          return EvaluateUeber(cond);

        case "von_nach_ueber":
          return EvaluateVonNachUeber(cond);

        case "fahrzeug":
          return EvaluateFahrzeug(cond);

        case "zugnummer":
          return EvaluateZugnummer(cond);

        //case "zeit":
        //  return EvaluateZeit(cond);

        case "zeitbereich":
          return EvaluateZeitbereich(cond);

        case "datum":
          return EvaluateDatum(cond);

        case "datum_vor":
        case "datum_nach":
        case "datum_zwischen":
          return EvaluateDatumErweitert(cond, type);

        case "dauer_min":
        case "dauer_max":
        case "dauer_zwischen":
          return EvaluateDauer(cond, type);

        case "zugart":
          return EvaluateZugart(cond);

        case "group":
          return EvaluateGroup(cond);

        case "zuggattung":
          return EvaluateZuggattung(cond);


        default:
          // Unbekannte Bedingung → leeres Resultset
          return new List<KIResult>();
      }
    }

    // ---------------------------------------------------------
    // TOP-LEVEL KOMBINATION (AND / OR)
    // ---------------------------------------------------------

    private List<KIResult> CombineTopLevel(List<List<KIResult>> resultSets, string logical)
    {
      if (resultSets.Count == 0)
        return new List<KIResult>();

      if (logical.Equals("or", StringComparison.OrdinalIgnoreCase))
      {
        return resultSets
            .SelectMany(s => s)
            .GroupBy(r => r.Zug_Id)
            .Select(g => g.First())
            .ToList();
      }

      // Default: AND → Schnittmenge
      return resultSets.Aggregate((prev, next) =>
          prev.Where(p => next.Any(n => n.Zug_Id == p.Zug_Id)).ToList());
    }

    // ---------------------------------------------------------
    // GRUPPENVERARBEITUNG (LOKALES AND / OR)
    // ---------------------------------------------------------

    private List<KIResult> EvaluateGroup(JsonElement cond)
    {
      var groupLogical = GetParameter(cond, "logical") ?? "and";
      var nested = GetConditionList(cond);

      var nestedResults = new List<List<KIResult>>();

      foreach (var inner in nested)
      {
        nestedResults.Add(EvaluateCondition(inner)); // REKURSION
      }

      if (nestedResults.Count == 0)
        return new List<KIResult>();

      if (groupLogical.Equals("or", StringComparison.OrdinalIgnoreCase))
      {
        return nestedResults
            .SelectMany(s => s)
            .GroupBy(r => r.Zug_Id)
            .Select(g => g.First())
            .ToList();
      }

      // Default: AND → Schnittmenge innerhalb der Gruppe
      return nestedResults.Aggregate((prev, next) =>
          prev.Where(p => next.Any(n => n.Zug_Id == p.Zug_Id)).ToList());
    }

    // ---------------------------------------------------------
    // EINZELNE BEDINGUNGSTYPEN
    // ---------------------------------------------------------

    private List<KIResult> EvaluateVonNach(JsonElement cond)
    {
      var from = GetParameter(cond, "from", "start", "von");
      var to = GetParameter(cond, "to", "nach", "ziel");

      var r = FindZuegeVonNach(from, to);
      return r.Results ?? new List<KIResult>();
    }

    private List<KIResult> EvaluateUeber(JsonElement cond)
    {
      var stations = GetParameterList(cond, "stations", "ueber", "via");

      var r = FindZuegeUeberStationen(stations);
      return r.Results ?? new List<KIResult>();
    }

    private List<KIResult> EvaluateVonNachUeber(JsonElement cond)
    {
      var from = GetParameter(cond, "from");
      var to = GetParameter(cond, "to");
      var via = GetParameterList(cond, "via");

      var r = FindZuegeVonNachUeber(from, to, via);
      return r.Results ?? new List<KIResult>();
    }

    private List<KIResult> EvaluateFahrzeug(JsonElement cond)
    {
      var pattern = GetParameter(cond, "pattern", "fahrzeug", "baureihe");

      var r = FindZuegeMitFahrzeug(pattern);
      return r.Results ?? new List<KIResult>();
    }

    private List<KIResult> EvaluateZugnummer(JsonElement cond)
    {
      var patterns = GetParameterList(cond, "patterns", "nummern");

      var r = FindZuegeMitZugnummer(patterns);
      return r.Results ?? new List<KIResult>();
    }

    //private List<KIResult> EvaluateZeit(JsonElement cond)
    //{
    //  var value = GetParameter(cond, "value", "zeit", "day");

    //  var r = FindZuegeMitZeitfilter(value);
    //  return r.Results ?? new List<KIResult>();
    //}

    private List<KIResult> EvaluateZeitbereich(JsonElement cond)
    {
      var from = GetParameter(cond, "from", "start");
      var to = GetParameter(cond, "to", "end");

      var r = FindZuegeMitUhrzeitfilter(from, to);
      return r.Results ?? new List<KIResult>();
    }

    private List<KIResult> EvaluateDatum(JsonElement cond)
    {
      var value = GetParameter(cond, "value", "datum", "date");

      var r = FindZuegeMitDatumfilter(value);
      return r.Results ?? new List<KIResult>();
    }

    private List<KIResult> EvaluateDatumErweitert(JsonElement cond, string mode)
    {
      var value1 = GetParameter(cond, "value", "from");
      var value2 = GetParameter(cond, "to");

      var r = FindZuegeMitDatumfilterErweitert(mode.Replace("datum_", ""), value1, value2);
      return r.Results ?? new List<KIResult>();
    }

    private List<KIResult> EvaluateDauer(JsonElement cond, string mode)
    {
      var value1 = GetParameter(cond, "value", "from");
      var value2 = GetParameter(cond, "to");

      var r = FindZuegeMitDauerfilter(mode.Replace("dauer_", ""), value1, value2);
      return r.Results ?? new List<KIResult>();
    }

    private List<KIResult> EvaluateZugart(JsonElement cond)
    {
      var value = GetParameter(cond, "value");

      var r = FindZuegeMitZugart(value);
      return r.Results ?? new List<KIResult>();
    }

    private List<KIResult> EvaluateZuggattung(JsonElement cond)
    {
      var value = GetParameter(cond, "value");

      if (string.IsNullOrWhiteSpace(value))
        return new List<KIResult>();

      var r = FindZuegeMitZuggattung(value);
      return r.Results ?? new List<KIResult>();
    }


    // ---------------------------------------------------------
    // HELFER: PARAMETER AUS JSON
    // ---------------------------------------------------------

    private string GetParameter2(JsonElement obj, string name, params string[] aliases)
    {
      if (obj.ValueKind != JsonValueKind.Object)
        return null;

      if (obj.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String)
        return v.GetString();

      foreach (var a in aliases)
      {
        if (obj.TryGetProperty(a, out var va) && va.ValueKind == JsonValueKind.String)
          return va.GetString();
      }

      return null;
    }

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

    private List<JsonElement> GetConditionList(JsonElement analysis)
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

    // ------------------------------------------------------------
    // NEGATION ERKENNEN
    // ------------------------------------------------------------
    private bool IsNegation(string value)
    {
      if (value == null) return false;
      value = value.ToLower();
      return value.StartsWith("not ") || value.StartsWith("nicht ");
    }

    private string ExtractNegated(string value)
    {
      if (value == null) return null;
      value = value.ToLower();

      if (value.StartsWith("not "))
        return value.Substring(4).Trim();

      if (value.StartsWith("nicht "))
        return value.Substring(6).Trim();

      return value;
    }

    // ------------------------------------------------------------
    // NEGATION DURCH ERGEBNIS-SUBTRAKTION
    // ------------------------------------------------------------
    private List<KIResult> ApplyNegation(List<KIResult> current, List<KIResult> exclude)
    {
      if (current == null) return new List<KIResult>();
      if (exclude == null) return current;

      return current.Where(r => !exclude.Any(e => e.Zug_Id == r.Zug_Id)).ToList();
    }


    // ---------------------------------------------------------
    // DEINE EXISTIERENDEN SUCHFUNKTIONEN (SIGNATUREN)
    // ---------------------------------------------------------

    private ZusiToolResult FindZuegeVonNach(string from, string to)
    => _local.FindZuegeVonNach(from, to);

    private ZusiToolResult FindZuegeUeberStationen(List<string> stations)
        => _local.FindZuegeUeberStationen(stations);

    private ZusiToolResult FindZuegeVonNachUeber(string from, string to, List<string> via)
        => _local.FindZuegeVonNachUeber(from, to, via);

    private ZusiToolResult FindZuegeMitFahrzeug(string pattern)
        => _local.FindZuegeMitFahrzeug(pattern);

    private ZusiToolResult FindZuegeMitZugnummer(List<string> patterns)
        => _local.FindZuegeMitZugnummer(patterns);

    //private ZusiToolResult FindZuegeMitZeitfilter(string value)
    //    => _local.FindZuegeMitZeitfilter(value);

    private ZusiToolResult FindZuegeMitUhrzeitfilter(string from, string to)
        => _local.FindZuegeMitUhrzeitfilter(from, to);

    private ZusiToolResult FindZuegeMitDatumfilter(string value)
        => _local.FindZuegeMitDatumfilter(value);

    private ZusiToolResult FindZuegeMitDatumfilterErweitert(string mode, string v1, string v2)
        => _local.FindZuegeMitDatumfilterErweitert(mode, v1, v2);

    private ZusiToolResult FindZuegeMitDauerfilter(string mode, string v1, string v2)
        => _local.FindZuegeMitDauerfilter(mode, v1, v2);

    private ZusiToolResult FindZuegeMitZugart(string value)
        => _local.FindZuegeMitZugart(value);

    private ZusiToolResult FindZuegeMitZuggattung(string value)
    => _local.FindZuegeMitZuggattung(value);


  }
}

