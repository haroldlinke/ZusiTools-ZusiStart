

using GMap.NET.MapProviders;
using log4net;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Newtonsoft.Json.Linq;
using Sovoma;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ZusiDisplayLib;
using ZusiKlassenLib2;
using ZusiKlassenLib2.Cab;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Fahrplan;
using ZusiKlassenLib2.TimeTable;
using ZusiKlassenLib2.Vehicle;
using ZusiMeterGaugesLib.Gauges;
using ZusiStart.Classes;
using ZusiStart.Data;
using ZusiStart.Dialogs;
using ZusiStart.Gleisbelegung;
using ZusiStart.KlLib2;
using ZusiStart.Miscellaneous;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace ZusiStart.KI
{
  public class ZusiLocalKiEngine
  {
    private static readonly ILog _log = LogManager.GetLogger(typeof(PictureManager));

    private readonly CombineEngine _combine;

    /******************************************
     * ZusiLocalKiEngine
     * *************************************/
    public ZusiLocalKiEngine()
    {
      _combine = new CombineEngine(this); // <‑‑ Übergabe von "this"
    }

    /******************************************
     * HttpClient
     * *************************************/
    private readonly HttpClient _http = new HttpClient
    {
      BaseAddress = new Uri("http://localhost:11434") // Ollama
    };

    /******************************************
     * AnalyzeQuestionAsync
     * *************************************/
    public async Task<JsonElement> AnalyzeQuestionAsync(string question)
    {
      var prompt = $@"
Du analysierst Fragen zu Zusi-Fahrplänen und erzeugst eine JSON-Struktur für eine lokale CombineEngine.

REGELN:
- Gib NUR JSON zurück.
- Keine Erklärungen, kein Text außerhalb der JSON-Struktur.
- Top-Level: {{ ""action"": ""combine"", ""parameters"": {{ ""logical"": ""..."", ""conditions"": [ ... ] }} }}
- Standard logical = ""and"".
- Jede Bedingung ist ein eigenes Objekt in ""conditions"".
- Nur ""group"" darf ein eigenes ""conditions""-Array haben.
- Bedingungen haben KEIN eigenes ""logical"".
- Keine verschachtelten Bedingungen außer in ""group"".
- KEINE neuen Bedingungstypen erfinden. Insbesondere:
  - KEIN {{ ""type"": ""negate"", ... }}
  - KEIN {{ ""not"": {{ ... }} }}
  - KEIN {{ ""condition"": {{ ... }}, ""negate"": {{ ... }} }}

BEDINGUNGSTYPEN:
- von_nach: {{ ""type"": ""von_nach"", ""from"": ""..."", ""to"": ""..."" }}
- ueber: {{ ""type"": ""ueber"", ""stations"": [""...""] }}
- von_nach_ueber: {{ ""type"": ""von_nach_ueber"", ""from"": ""..."", ""to"": ""..."", ""via"": [""...""] }}

- fahrzeug: {{ ""type"": ""fahrzeug"", ""pattern"": ""..."" }}

- zugnummer:
  - Nur Ziffern: {{ ""type"": ""zugnummer"", ""patterns"": [""1234""] }}
  - Gattung+Nummer: {{ ""type"": ""zugnummer"", ""patterns"": [""RE1234""] }}

- zuggattung:
  {{ ""type"": ""zuggattung"", ""value"": ""RE"" }}

- zugart:
  {{ ""type"": ""zugart"", ""value"": ""personen"" | ""gueter"" | ""deko"" }}

DATUM:
- ""vor <Datum>"": {{ ""type"": ""datum_vor"", ""value"": ""YYYY-MM-DD"" }}
- ""nach <Datum>"": {{ ""type"": ""datum_nach"", ""value"": ""YYYY-MM-DD"" }}
- ""zwischen A und B"": {{ ""type"": ""datum_zwischen"", ""from"": ""YYYY-MM-DD"", ""to"": ""YYYY-MM-DD"" }}
- Jahreszahlen IMMER als YYYY-01-01 normalisieren.

DAUER (immer in Sekunden):

- Die KI MUSS Dauerangaben als reine Zahl in SEKUNDEN ausgeben.
- Keine Minuten, keine Stunden, keine Einheiten.
- Nur Integer.

Beispiele (intern, nicht ausgeben):
""1 Stunde"" → 3600
""30 Minuten"" → 1800
""1,5 Stunden"" → 5400

Ausgabeformate:
dauer_min:       {{ ""type"": ""dauer_min"", ""value"": <Sekunden> }}
dauer_max:       {{ ""type"": ""dauer_max"", ""value"": <Sekunden> }}
dauer_zwischen:  {{ ""type"": ""dauer_zwischen"", ""from"": <Sekunden>, ""to"": <Sekunden> }}

NEGATION:
- Negation (""nicht"", ""keine"", ""ohne"", ""no"", ""not"") MUSS ausschließlich über das Feld ""negate"": true dargestellt werden.
- Negation DARF NIEMALS einen eigenen Bedingungstyp erzeugen.
- Insbesondere VERBOTEN:
  {{ ""type"": ""negate"", ... }}
  {{ ""not"": {{ ... }} }}
  {{ ""condition"": {{ ... }}, ""negate"": {{ ... }} }}

- Der eigentliche Wert MUSS bereinigt sein:
  - KEINE Wörter wie ""nicht"", ""keine"", ""ohne"", ""no"", ""not"" im Wert.
  - Beispiel:
      ""nicht 218"" → {{ ""type"": ""fahrzeug"", ""pattern"": ""218"", ""negate"": true }}
      ""no 218""    → {{ ""type"": ""fahrzeug"", ""pattern"": ""218"", ""negate"": true }}
      ""nicht über KasselHBF"" → {{ ""type"": ""ueber"", ""stations"": [""KasselHBF""], ""negate"": true }}

- Negation ist IMMER lokal:
  - Sie gilt nur für die Bedingung, auf die sich das Wort direkt bezieht.
  - Negation DARF NICHT auf andere Bedingungen übertragen werden.

- ""von_nach"" darf NIEMALS negiert werden.

### ML‑basierte Strecken‑Clusteranalyse

Die folgenden Condition‑Typen ermöglichen es, Züge anhand ihrer Streckenführung
(Bahnhofsliste) zu filtern. Diese Informationen stammen aus einem externen
Clustering‑Modell (TF‑IDF + DBSCAN), das Streckenvarianten erkennt.

Die KI DARF diese Condition‑Typen verwenden, wenn der Nutzer nach:
- „besondere Strecke“
- „ungewöhnliche Strecke“
- „seltene Strecke“
- „einmalige Strecke“
- „abweichende Route“
- „anderer Weg“
- „fährt anders als die anderen“
- „Streckenvariante“
fragt.

Die Condition‑Typen:

- `ml_route_unique`  
  → Züge, deren Strecke nur einmal vorkommt (Clustergröße = 1)

  Beispiel:
  `{{ ""type"": ""ml_route_unique"" }}`

- `ml_route_rare`  
  → Züge, deren Strecke selten ist (Clustergröße ≤ 3)

  Beispiel:
  `{{ ""type"": ""ml_route_rare"" }}`

- `ml_route_outlier`  
  → Züge, die vom Strecken‑Clustering als Ausreißer erkannt wurden (DBSCAN = -1)

  Beispiel:
  `{{ ""type"": ""ml_route_outlier"" }}`

- `ml_route_cluster_size`  
  → Filtert nach Clustergröße

  Beispiel:
  `{{ ""type"": ""ml_route_cluster_size"", ""min"": 1, ""max"": 5 }}`

- `ml_route_debug`  
  → Gibt alle Züge zurück; dient zur Anzeige von Strecken‑Debug‑Informationen

  Beispiel:
  `{{ ""type"": ""ml_route_debug"" }}`

Hinweise:
- Diese Condition‑Typen funktionieren wie alle anderen Bedingungen.
- Sie dürfen in Gruppen verwendet werden.
- Sie dürfen negiert werden (außer `ml_route_cluster_size`).
- Sie erzeugen KEINE neuen Bedingungstypen.


GRUPPEN:
- ""A oder B"" MUSS eine Gruppe erzeugen:
  {{
    ""type"": ""group"",
    ""logical"": ""or"",
    ""conditions"": [ {{ ... }}, {{ ... }} ]
  }}



Frage: {question}
";

      _log.Debug($"AnalyzeQuestionAsync called with question: {question}");
      DataManager.Instance.AddKIResultMessage($"* AnalyzeQuestionAsync called with question: {question}");

      var req = new
      {
        model = "llama3:8b",
        prompt,
        stream = false
      };

      var resp = await _http.PostAsync("/api/generate",
          new StringContent(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json"));

      resp.EnsureSuccessStatusCode();

      var json = await resp.Content.ReadAsStringAsync();
      using var doc = JsonDocument.Parse(json);
      var text = doc.RootElement.GetProperty("response").GetString();

      return SafeParseJson(text);
    }

    // ----------------------------------------------------
    // 2. Lokales Tool ausführen
    // ----------------------------------------------------
    /******************************************
     * ExecuteLocalTool2
     * *************************************/
    public HashSet<ulong> ExecuteLocalTool2(JsonElement analysis)
    {
      var sets = new List<HashSet<ulong>>();

      // Top-Level logical (AND/OR) – default AND
      var logical = GetParameter(analysis, "logical") ?? "and";

      // Flache Top-Level-Bedingungen
      var conditions = _combine.GetConditionList(analysis);

      DataManager.Instance.AddKIResultMessage($"* CombineConditions called with {conditions.Count} conditions. \n conditions: {string.Join(", ", conditions.Select(c => c.ToString()))}");

      foreach (var cond in conditions)
        sets.Add(_combine.EvaluateCondition2(cond));

      return logical == "or"
          ? _combine.CombineOr(sets)
          : _combine.CombineAnd(sets);
    }

    /******************************************
     * FindZuegeMitZuggattung2
     * *************************************/
    public HashSet<ulong> FindZuegeMitZuggattung2(string gattung)
    {
      var result = new HashSet<ulong>();


      if (string.IsNullOrWhiteSpace(gattung))
      {
        return result;
      }

      string g = gattung.Trim().ToLower();

      bool IsMatch(Zug z)
      {
        return (z.Gattung ?? "").ToLower() == g;
      }

      return DataManager.Instance._allTrains
        .Where(IsMatch)
        .Select(z => z.ID)
        .ToHashSet();
    }

    /******************************************
     * FindZuegeMitZugart2
     * *************************************/
    public HashSet<ulong> FindZuegeMitZugart2(string value)
    {
      var result = new HashSet<ulong>();
            

      if (string.IsNullOrWhiteSpace(value))
      {
        return result;
      }

      string v = value.Trim().ToLower();

      bool IsPersonen(Zug z)
      {
        return z.Type == TrainType.Passenger;
      }

      bool IsGueter(Zug z)
      {
        return z.Type == TrainType.Freight;
      }

      bool IsDeko(Zug z)
      {
        return z.IsDecoTrain;
      }

      bool IsMatch(Zug z)
      {
        return v switch
        {
          "personen" => IsPersonen(z),
          "personenzug" => IsPersonen(z),
          "gueter" => IsGueter(z),
          "güter" => IsGueter(z),
          "gueterzug" => IsGueter(z),
          "güterzug" => IsGueter(z),
          "deko" => IsDeko(z),
          "dekozug" => IsDeko(z),
          _ => false
        };
      }

      return DataManager.Instance._allTrains
        .Where(IsMatch)
        .Select(z => z.ID)
        .ToHashSet();
    }

    /******************************************
     * FindZuegeMitDauerfilter2
     * *************************************/
    public HashSet<ulong> FindZuegeMitDauerfilter2(string mode, string value1, string value2)
    {
      var result = new HashSet<ulong>();
      //return result;

      bool ok1 = int.TryParse(value1, out var min1);
      bool ok2 = int.TryParse(value2, out var min2);

      if (!ok1)
      {
        
        return result;
      }

      bool IsMatch(Zug z)
      {
        var duration = z.JourneyTime.Value.TotalSeconds;

        switch (mode)
        {
          case "min":
            return duration >= min1;

          case "max":
            return duration <= min1;

          case "zwischen":
            if (!ok2) return false;
            bool matchok = duration >= min1 && duration <= min2;

            return matchok;

          default:
            return false;
        }
      }

      return DataManager.Instance._allTrains
        .Where(IsMatch)
        .Select(z => z.ID)
        .ToHashSet();
    }

    /******************************************
     * FindZuegeMitDatumfilterErweitert2
     * *************************************/
    public HashSet<ulong> FindZuegeMitDatumfilterErweitert2(string mode, string value1, string value2)
    {
      var result = new HashSet<ulong>();
      //return result;

      // Datum normalisieren
      bool ok1 = DateTime.TryParse(value1, out var d1);
      bool ok2 = DateTime.TryParse(value2, out var d2);

      if (!ok1)
      {
        return result;
      }

      bool IsMatch(Zug z)
      {
        // Datum aus Fahrplan
        var tt = DataManager.Instance._allTimeTables
            .FirstOrDefault(t => t.ID == z.BelongsToTimeTable);

        DateTime? date = tt?.StartTime;

        if (date == null)
          return false;

        var d = date.Value.Date;

        switch (mode)
        {
          case "vor":
            return d < d1;

          case "nach":
            return d > d1;

          case "zwischen":
            if (!ok2) return false;
            return d >= d1 && d <= d2;

          case "exakt":
            return d == d1;

          default:
            return false;
        }
      }

      return DataManager.Instance._allTrains
        .Where(IsMatch)
        .Select(z => z.ID)
        .ToHashSet();
    }

    /******************************************
     * FindZuegeMitZugnummer2
     * *************************************/
    public HashSet<ulong> FindZuegeMitZugnummer2(IEnumerable<string> patterns)
    {
      DataManager.Instance.AddKIResultMessage($"* FindZuegeMitZugnummer called with patterns: {string.Join(", ", patterns)}");

      if (patterns == null || patterns.Count() == 0)
      {
        return new HashSet<ulong>();
      }

      // Normalisierte Suchmuster
      var normalized = patterns
          .Where(p => !string.IsNullOrWhiteSpace(p))
          .Select(p => p.Replace(" ", "").Trim().ToLower())
          .ToList();

      return DataManager.Instance._allTrains
          .Where(z => {
            string zugNummer = (z.Gattung + z.Nummer)
              .Replace(" ", "")
              .Trim()
              .ToLower();
            // Match ANY pattern
            return normalized.Any(p => zugNummer.Contains(p));
          })
          .Select(z => z.ID)
          .ToHashSet();
    }

    /******************************************
     * FindZuegeVonNachUeber2
     * *************************************/
    public HashSet<ulong> FindZuegeVonNachUeber2(string from, string to, List<string> via)
    {
      DataManager.Instance.AddKIResultMessage($"* FindZuegeVonNachUeber2 called with from: {from}, to: {to}, via: {string.Join(", ", via)}");

      if (string.IsNullOrWhiteSpace(from) && string.IsNullOrWhiteSpace(to))
      {
        return new HashSet<ulong>();
      }

      if (via == null || via.Count == 0)
      {
        return new HashSet<ulong>();
      }

      // Normalisierte Suchbegriffe
      string nf = from.Replace(" ", "").Trim().ToLower();
      string nt = to.Replace(" ", "").Trim().ToLower();
      var nvias = via
          .Where(s => !string.IsNullOrWhiteSpace(s))
          .Select(s => s.Replace(" ", "").Trim().ToLower())
          .ToList();

      return DataManager.Instance._allTrains
          .Where(z => {
            // Strecke normalisieren
            var entries = z.FahrplanEintraege
              .Where(f => f.Bestrst != null && DataManager.IsValidName(f.Bestrst))
              .Select(f => f.Bestrst.Replace(" ", "").Trim().ToLower())
              .ToList();

            // Muss Start und Ziel enthalten
            if (!entries.Any(e => e.Contains(nf))) return false;
            if (!entries.Any(e => e.Contains(nt))) return false;

            // Reihenfolge Start < Ziel
            int idxFrom = entries.FindIndex(e => e.Contains(nf));
            int idxTo = entries.FindIndex(e => e.Contains(nt));
            if (!(idxFrom >= 0 && idxTo >= 0 && idxFrom < idxTo)) return false;

            // Alle via-Stationen müssen vorkommen
            foreach (var v in nvias)
            {
              if (!entries.Any(e => e.Contains(v)))
                return false;
            }

            // Reihenfolge prüfen: A < via1 < via2 < ... < B
            int lastIndex = idxFrom;
            foreach (var v in nvias)
            {
              int idxVia = entries.FindIndex(e => e.Contains(v));
              if (idxVia < 0 || idxVia <= lastIndex)
                return false;
              lastIndex = idxVia;
            }

            // Und via-Stationen müssen vor B liegen
            if (lastIndex >= idxTo)
              return false;

            return true;
          })
          .Select(z => z.ID)
          .ToHashSet();
    }

    /******************************************
     * FindZuegeVonNach2
     * *************************************/
    public HashSet<ulong> FindZuegeVonNach2(string from, string to)
    {
      DataManager.Instance.AddKIResultMessage($"* FindZuegeVonNach called with from: {from}, to: {to}");

      if (string.IsNullOrWhiteSpace(from) && string.IsNullOrWhiteSpace(to))
      {
        return new HashSet<ulong>();
      }

      string nf = from.Replace(" ", "").Trim().ToLower();
      string nt = to.Replace(" ", "").Trim().ToLower();


      return DataManager.Instance._allTrains
          .Where(z => {
            // Alle FahrplanEinträge normalisieren
            var entries = z.FahrplanEintraege
              .Where(f => f.Bestrst != null && DataManager.IsValidName(f.Bestrst))
              .Select(f => f.Bestrst.Replace(" ", "").Trim().ToLower())
              .ToList();

            // Muss beide enthalten
            int idxFrom = 0;
            int idxTo = 99999;

            if (!string.IsNullOrWhiteSpace(nf))
            {
              if (!entries.Any(e => e.Contains(nf))) return false;
              idxFrom = entries.FindIndex(e => e.Contains(nf));
              if (nt == "" && idxFrom < entries.Count - 2) return true; // nur Startstation angegeben, Zug muss danach noch weiterfahren
            }

            if (!string.IsNullOrWhiteSpace(nt))
            {
              if (!entries.Any(e => e.Contains(nt))) return false;
              idxTo = entries.FindIndex(e => e.Contains(nt));
              if (nf == "" && idxTo > 0) return true; // nur Zielstation angegeben, Zug muss vorher schon gefahren sein
            }

            return idxFrom >= 0 && idxTo >= 0 && idxFrom < idxTo;
          })
          .Select(z => z.ID)
          .ToHashSet();
    }


    /******************************************
     * GetParameterList
     * *************************************/
    public List<string> GetParameterList(JsonElement json, params string[] keys)
    {
      foreach (var key in keys)
      {
        if (json.TryGetProperty(key, out var arr) && arr.ValueKind == JsonValueKind.Array)
        {
          var list = new List<string>();
          foreach (var item in arr.EnumerateArray())
            if (item.ValueKind == JsonValueKind.String)
              list.Add(item.GetString());
          return list;
        }
      }

      if (json.TryGetProperty("parameters", out var p))
      {
        foreach (var key in keys)
        {
          if (p.TryGetProperty(key, out var arr) && arr.ValueKind == JsonValueKind.Array)
          {
            var list = new List<string>();
            foreach (var item in arr.EnumerateArray())
              if (item.ValueKind == JsonValueKind.String)
                list.Add(item.GetString());
            return list;
          }
        }
      }

      return new List<string>();
    }


    /******************************************
     * FindZuegeUeberStationen2
     * *************************************/
    public HashSet<ulong> FindZuegeUeberStationen2(IEnumerable<string> stations)
    {
      if (stations == null || stations.Count() == 0)
      {
        return new HashSet<ulong>();
      }

      // Normalisierte Suchbegriffe
      var normalized = stations
          .Where(s => !string.IsNullOrWhiteSpace(s))
          .Select(s => s.Replace(" ", "").Trim().ToLower())
          .ToList();

      return DataManager.Instance._allTrains
          .Where(z => {
            // Alle Stationen müssen vorkommen
            return normalized.All(st =>
              z.FahrplanEintraege.Any(f =>
                  f.Bestrst != null &&
                  f.Bestrst.Replace(" ", "").Trim()
                      .ToLower()
                      .Contains(st)));
          })
          .Select(z => z.ID)
          .ToHashSet();
    }

    /******************************************
     * FindZuegeMitFahrzeug2
     * *************************************/
    public HashSet<ulong> FindZuegeMitFahrzeug2(string pattern)
    {
      DataManager.Instance.AddKIResultMessage($"* FindZuegeMitFahrzeug2 called with pattern: {pattern}");

      return DataManager.Instance._allTrains
          .Where(z => z.Fahrzeuge.ContainsVehicle(pattern))
          .Select(z => z.ID)
          .ToHashSet();
    }


    public static long ToUnixTimestamp(DateTime? dt)
    {
      if (!dt.HasValue) return 0;
      return ((DateTimeOffset)dt.Value).ToUnixTimeSeconds();
    }

    public static List<string> ExtractUnterwegsbahnhoefe(Zug zug)
    {
      var result = new List<string>();

      foreach (var entry in zug.FahrplanEintraege)
      {
        if (!string.IsNullOrWhiteSpace(entry.Bestrst))
          result.Add(entry.Bestrst);
      }

      return result;
    }

    public static string GetStartBahnhof(List<string> bhf)
    {
      return bhf.Count > 0 ? bhf[0] : "";
    }

    public static string GetZielBahnhof(List<string> bhf)
    {
      return bhf.Count > 1 ? bhf[^1] : "";
    }

    public float GetStartDay(Zug z)
    {
      return (z?.Aufgleiszeit?.Hour ?? 0)
       + (z?.Aufgleiszeit?.Minute ?? 0) / 60f
       + (z?.Aufgleiszeit?.Second ?? 0) / 3600f;
      }

    public float GetEndDay(Zug z)
    {
      return (z?.Abgleiszeit?.Hour ?? 0)
       + (z?.Abgleiszeit?.Minute ?? 0) / 60f
       + (z?.Abgleiszeit?.Second ?? 0) / 3600f;
    }

    private readonly AnomalyOnnxService _anomalyService =
        new AnomalyOnnxService("D:\\Development\\ZUSI-Tools\\_updated_sources\\_net8\\ZusiStart\\KI-Python\\zusi_anomaly_model.onnx");

    private readonly RouteClusterService _routeService
    = new RouteClusterService();

    public HashSet<ulong> EvaluateMlAnomaly(JsonElement cond)
    {
      string mode = GetParameter(cond, "mode"); // "unusual" oder "normal"

      var allTrains = DataManager.Instance._allTrains;

      var features = allTrains.Select(z =>
      {
        var bhf = ExtractUnterwegsbahnhoefe(z);

        return new ZugNumericFeatures
        {
          ZugId = z.ID,

          Dauer = (float)(z.JourneyTime?.TotalMinutes ?? 0f),
          StartDay = GetStartDay(z),
          EndDay = GetEndDay(z),
          AnzahlUnterwegsbahnhoefe = bhf.Count,
          AnzahlFahrzeuge = 0 //z.Fahrzeuge.FahrzeugGruppen.Count
        };
      }).ToList();


      var pred = _anomalyService.Predict(features);

      var ids = new HashSet<ulong>();

      for (int i = 0; i < features.Count; i++)
      {
        bool isUnusual = pred[i] < 0;   // One-Class SVM: -1 = ungewöhnlich

        if (mode == "unusual" && isUnusual)
          ids.Add(features[i].ZugId);

        if (mode == "normal" && !isUnusual)
          ids.Add(features[i].ZugId);
      }

      return ids;
    }

    public HashSet<ulong> EvaluateMlRouteUnique(JsonElement cond)
    {
      var result = new HashSet<ulong>();

      foreach (var zug in DataManager.Instance._allTrains)
      {
        var info = _routeService.GetInfo(zug);
        if (info.IsUnique)
          result.Add(zug.ID);
      }

      return result;
    }

    public HashSet<ulong> EvaluateMlRouteRare(JsonElement cond)
    {
      var result = new HashSet<ulong>();

      foreach (var zug in DataManager.Instance._allTrains)
      {
        var info = _routeService.GetInfo(zug);
        if (info.IsRare)
          result.Add(zug.ID);
      }

      return result;
    }

    public HashSet<ulong> EvaluateMlRouteOutlier(JsonElement cond)
    {
      var result = new HashSet<ulong>();

      foreach (var zug in DataManager.Instance._allTrains)
      {
        var info = _routeService.GetInfo(zug);
        if (info.IsUnique)
          result.Add(zug.ID);
      }

      return result;
    }

    public HashSet<ulong> EvaluateMlRouteClusterSize(JsonElement cond)
    {
      int min = cond.TryGetProperty("min", out var minProp) ? minProp.GetInt32() : int.MinValue;
      int max = cond.TryGetProperty("max", out var maxProp) ? maxProp.GetInt32() : int.MaxValue;

      var result = new HashSet<ulong>();

      foreach (var zug in DataManager.Instance._allTrains)
      {
        var info = _routeService.GetInfo(zug);
        if (info.ClusterSize >= min && info.ClusterSize <= max)
          result.Add(zug.ID);
      }

      return result;
    }

    public HashSet<ulong> EvaluateMlRouteDebug(JsonElement cond)
    {
      // Debug liefert ALLE Züge, aber du kannst es filtern
      var result = new HashSet<ulong>();

      foreach (var zug in DataManager.Instance._allTrains)
      {
        result.Add(zug.ID);
      }

      return result;
    }


    // ----------------------------------------------------
    // 3. Antwort formulieren
    // ----------------------------------------------------
    //    public async Task<string> FormulateAnswerAsync(string originalQuestion, ZusiToolResult toolResult)
    //    {
    //      var prompt = $@"
    //Du bist eine KI für ZusiStart-Fahrplananalyse.

    //Benutzerfrage:
    //{originalQuestion}

    //Interne Aktion:
    //{toolResult.Action}

    //Ergebnisse (JSON):
    //{JsonSerializer.Serialize(toolResult)}

    //Formuliere eine zusammenfassende Antwort für den Benutzer.
    //Wenn keine Ergebnisse vorhanden sind, erkläre das knapp.
    //";

    //      var req = new
    //      {
    //        model = "llama3:8b",
    //        prompt,
    //        stream = false
    //      };

    //      var resp = await _http.PostAsync("/api/generate",
    //          new StringContent(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json"));

    //      resp.EnsureSuccessStatusCode();

    //      var json = await resp.Content.ReadAsStringAsync();
    //      using var doc = JsonDocument.Parse(json);
    //      return doc.RootElement.GetProperty("response").GetString();
    //    }

    // ----------------------------------------------------
    // 4. Gesamtablauf
    // ----------------------------------------------------
    /******************************************
     * ProcessQuestionAsync2
     * *************************************/
    public async Task<string> ProcessQuestionAsync2(string question)
    {
      var analysis = await AnalyzeQuestionAsync(question);
      var toolResult = ExecuteLocalTool2(analysis);
      var answer = ""; //await FormulateAnswerAsync(question, toolResult);

      // UI‑Update → zurück auf UI‑Thread
      await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
      {

        DataManager.Instance.AddKIResultMessage($"* CombineConditions found {toolResult.Count} results.");

        //  DataManager.Instance.FoundTimeTableIds = toolResult.Results
        //.Select(r => r.Zug_Id)
        //.ToHashSet();
        DataManager.Instance.FoundTimeTableIds = toolResult;

        var trains = DataManager.Instance._allTrains
            .Where(z =>
            {
              ulong zugID = z.ID;

              // Match ANY pattern
              return DataManager.Instance.FoundTimeTableIds.Any(p => zugID == p);
            })
            .GroupBy(z => z.BelongsToTimeTable)
            .Select(g => new FoundTimeTable()
            {
              TimeTable = DataManager.Instance._allTimeTables.First(tt => tt.ID == g.Key),
              Trains = g.ToList()
            });

        DataManager.Instance.UpdateFoundTimetables(trains);
        DataManager.SearchTrainValue = null;
      });

      return answer;
    }

    // ----------------------------------------------------
    // Hilfsfunktionen: robuster JSON-Parser
    // ----------------------------------------------------
    /******************************************
     * SafeParseJson
     * *************************************/
    private JsonElement SafeParseJson(string json)
    {
      try
      {
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
      }
      catch
      {
        json = TryFixJson(json);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
      }
    }

    /******************************************
     * TryFixJson
     * *************************************/
    private string TryFixJson(string json)
    {
      json = json.Trim();

      int start = json.IndexOf('{');
      int end = json.LastIndexOf('}');
      if (start >= 0 && end > start)
        json = json.Substring(start, end - start + 1);

      json = json.Replace("„", "\"")
                 .Replace("“", "\"")
                 .Replace("'", "\"");

      json = json.Replace("\"params\"", "\"parameters\"");
      json = json.Replace("\"Betriebsstelle\"", "\"station\"");

      return json;
    }

    /******************************************
     * GetParameter
     * *************************************/
    private string GetParameter(JsonElement json, params string[] keys)
    {
      foreach (var key in keys)
      {
        if (json.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
          return value.GetString();
      }

      if (json.TryGetProperty("parameters", out var p) && p.ValueKind == JsonValueKind.Object)
      {
        foreach (var key in keys)
        {
          if (p.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
            return value.GetString();
        }
      }

      return null;
    }
  }
}

