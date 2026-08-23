using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ZusiStart.Data;
using ZusiKlassenLib2;
using ZusiKlassenLib2.Cab;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Fahrplan;
using ZusiKlassenLib2.TimeTable;
using ZusiKlassenLib2.Vehicle;
using ZusiMeterGaugesLib.Gauges;
using ZusiStart.Classes;
using ZusiStart.Dialogs;
using ZusiStart.Gleisbelegung;
using ZusiStart.KlLib2;

namespace ZusiStart.KI
{
  public class OllamaFunctionCalling
  {
    // -----------------------------
    // DATA MODELS
    // -----------------------------

    public class OllamaMessage
    {
      public string role { get; set; }
      public string content { get; set; }
      public string name { get; set; } // nur für tool messages
    }

    public class OllamaTool
    {
      public string name { get; set; }
      public string description { get; set; }
      public object parameters { get; set; }
    }

    public class OllamaToolCall
    {
      public string name { get; set; }
      public Dictionary<string, object> arguments { get; set; }
    }

    public class OllamaResponse
    {
      public string model { get; set; }
      public string created_at { get; set; }
      public string response { get; set; }
      public bool done { get; set; }

      public OllamaToolCall tool_call { get; set; }
    }

    public class ZusiToolResult
    {
      public string Query { get; set; }
      public List<KIResult> Results { get; set; } = new();
      public string Summary { get; set; }
    }

    public class KIResult
    {
      public string Fahrplan_Name { get; set; }
      //public string Fahrplan_Datei { get; set; }

      public string Zug_Name { get; set; }
      public string Zug_Id { get; set; }

      public string Fahrzeug_Baureihe { get; set; }
      //public string Fahrzeug_Datei { get; set; }

      //public List<string> Strecke { get; set; } = new();
    }

    // -----------------------------
    // TOOL DEFINITIONS
    // -----------------------------

    private readonly List<OllamaTool> tools = new()
    {
        new OllamaTool
        {
            name = "find_zuege_ueber_betriebsstelle",
            description = "Findet alle Züge, deren Fahrplan über eine bestimmte Betriebsstelle/Station führt.",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    station = new { type = "string", description = "Name oder ID der Betriebsstelle" }
                },
                required = new[] { "station" }
            }
        },
        new OllamaTool
        {
            name = "find_fahrzeuge_fuer_zug",
            description = "Findet das Fahrzeug, das einen bestimmten Zug fährt.",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    zug_id = new { type = "string", description = "Zug-ID oder Zugname" }
                },
                required = new[] { "zug_id" }
            }
        }
    };

    // -----------------------------
    // 1. ERSTER KI-AUFRUF
    // -----------------------------

    public async Task<OllamaResponse> AskOllamaAsync(string userQuestion)
    {
      using var client = new HttpClient();

      var request = new
      {
        model = "llama3:8b",
        stream = false,
        messages = new[]
          {
                new OllamaMessage { role = "system", content = "Du bist eine KI für ZusiStart-Fahrplananalyse." },
                new OllamaMessage { role = "user", content = userQuestion }
            },
        tools
      };

      var httpResponse = await client.PostAsync(
          "http://localhost:11434/api/chat",
          new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
      );

      string json = await httpResponse.Content.ReadAsStringAsync();
      return JsonSerializer.Deserialize<OllamaResponse>(json);
    }

    // -----------------------------
    // TOOL-CALL ERKENNEN
    // -----------------------------

    public bool HasToolCall(OllamaResponse response)
    {
      return response.tool_call != null;
    }

    // -----------------------------
    // 2. TOOL AUSFÜHREN (ZUSI-DATEN)
    // -----------------------------

    public ZusiToolResult ExecuteTool(OllamaToolCall call)
    {
      return call.name switch
      {
        "find_zuege_ueber_betriebsstelle" =>
            FindZuegeUeberBetriebsstelle(call.arguments["station"].ToString()),

        "find_fahrzeuge_fuer_zug" =>
            FindFahrzeugFuerZug(call.arguments["zug_id"].ToString()),

        _ => throw new Exception("Unbekanntes Tool")
      };
    }

    // -----------------------------
    // TOOL 1: ZÜGE ÜBER BETRIEBSSTELLE
    // -----------------------------

    private ZusiToolResult FindZuegeUeberBetriebsstelle(string station)
    {
      var result = new ZusiToolResult
      {
        Query = $"Züge über Betriebsstelle {station}"
      };

      foreach (TimeTable fp in DataManager.Instance._allTimeTables)
      {
        foreach (TrainReference trainref in fp.Trains)
        {
          if (trainref != null)
          {
            Zug zug = trainref.Train;
            foreach (FahrplanEintrag fpe in zug.FahrplanEintraege)
            if (fpe.Bestrst.Contains(station))
            {
              result.Results.Add(new KIResult
              {
                Fahrplan_Name = fp.Name,
                //Fahrplan_Datei = fp.,
                Zug_Name = zug.Gattung+zug.Nummer,
                Zug_Id = zug.Nummer,
                //Fahrzeug_Datei = zug.FahrzeugDatei,
                //Strecke = zug.Strecke
              });
            }
          }
        }
      }

      result.Summary = $"{result.Results.Count} Züge gefunden.";
      return result;
    }

    // -----------------------------
    // TOOL 2: FAHRZEUG FÜR ZUG
    // -----------------------------

    private ZusiToolResult FindFahrzeugFuerZug(string zugId)
    {
      var result = new ZusiToolResult
      {
        Query = $"Fahrzeug für Zug {zugId}"
      };

      //foreach (var fp in DataManager.Instance._allTimeTables)
      //{
      //  foreach (var zug in fp.Trains)
      //  {
      //    if (zug.Id == zugId || zug.Name == zugId)
      //    {
      //      var fahrzeug = ZusiData.Fahrzeuge
      //          .Find(f => f.FahrzeugDatei == zug.FahrzeugDatei);

      //      result.Results.Add(new KIResult
      //      {
      //        Fahrplan_Name = fp.FahrplanName,
      //        Fahrplan_Datei = fp.FahrplanDatei,
      //        Zug_Name = zug.Name,
      //        Zug_Id = zug.Id,
      //        Fahrzeug_Datei = zug.FahrzeugDatei,
      //        Fahrzeug_Baureihe = fahrzeug?.Baureihe,
      //        Strecke = zug.Strecke
      //      });
      //    }
      //  }
      //}

      result.Summary = $"{result.Results.Count} Fahrzeuge gefunden.";
      return result;
    }

    // -----------------------------
    // 3. ZWEITER KI-AUFRUF (ANTWORT FORMULIEREN)
    // -----------------------------

    public async Task<OllamaResponse> AskOllamaWithToolResultAsync(
        string userQuestion,
        ZusiToolResult toolResult)
    {
      using var client = new HttpClient();

      var request = new
      {
        model = "llama3:8b",
        stream = false,
        messages = new[]
          {
                new OllamaMessage { role = "system", content = "Du bist eine KI für ZusiStart-Fahrplananalyse." },
                new OllamaMessage { role = "user", content = userQuestion },
                new OllamaMessage
                {
                    role = "tool",
                    name = toolResult.Query,
                    content = JsonSerializer.Serialize(toolResult)
                }
            }
      };

      var httpResponse = await client.PostAsync(
          "http://localhost:11434/api/chat",
          new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
      );

      string json = await httpResponse.Content.ReadAsStringAsync();
      return JsonSerializer.Deserialize<OllamaResponse>(json);
    }

    // -----------------------------
    // 4. GESAMTABLAUF
    // -----------------------------

    public async Task<string> ProcessUserQuestion(string question)
    {
      var response = await AskOllamaAsync(question);

      if (HasToolCall(response))
      {
        var toolResult = ExecuteTool(response.tool_call);
        var finalResponse = await AskOllamaWithToolResultAsync(question, toolResult);
        return finalResponse.response;
      }

      return response.response;
    }
  }
}
