

using log4net;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
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

namespace ZusiStart.KI
{

  public class ZusiToolResult
  {
    public string Action { get; set; }
    public string Query { get; set; }
    public List<KIResult> Results { get; set; } = new();
    public string Summary { get; set; }
  }

  public class KIResult
  {
    public string Fahrplan_Name { get; set; }
    public string Fahrplan_Datei { get; set; }

    public string Zug_Name { get; set; }
    public string Zug_Id { get; set; }

    public string Fahrzeug_Baureihe { get; set; }
    public string Fahrzeug_Datei { get; set; }

    public List<string> Strecke { get; set; } = new();
  }

  public static class KiZusiData
  {
    public static List<KiFahrplan> Fahrplaene { get; set; } = new();
    public static List<KiFahrzeug> Fahrzeuge { get; set; } = new();
  }

  public class KiFahrplan
  {
    public string FahrplanName { get; set; }
    public string FahrplanDatei { get; set; }
    public List<KiZug> Zuege { get; set; } = new();
  }

  public class KiZug
  {
    public string Name { get; set; }
    public string Id { get; set; }
    public string FahrzeugDatei { get; set; }
    public List<string> Strecke { get; set; } = new();
  }

  public class KiFahrzeug
  {
    public string FahrzeugDatei { get; set; }
    public string Baureihe { get; set; }
  }


  public class ZusiLocalKiEngine
  {
    private static readonly ILog _log = LogManager.GetLogger(typeof(PictureManager));

    private readonly CombineEngine _combine;

    public ZusiLocalKiEngine()
    {
      _combine = new CombineEngine(this); // <‑‑ Übergabe von "this"
    }


    private readonly HttpClient _http = new HttpClient
    {
      BaseAddress = new Uri("http://localhost:11434") // Ollama
    };

    

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
  Erlaubte Gattungen: ICE, IC, EC, RE, RB, S, IRE, DPN, DPNX.

- zugart:
  {{ ""type"": ""zugart"", ""value"": ""personen"" | ""gueter"" | ""deko"" }}

ZEIT:
- zeit: {{ ""type"": ""zeit"", ""value"": ""..."" }}
- zeitbereich: {{ ""type"": ""zeitbereich"", ""from"": ""..."", ""to"": ""..."" }}

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

    public async Task<JsonElement> AnalyzeQuestionAsync3(string question)
    {
      var prompt = $@"
Du analysierst natürliche Sprachfragen zu Zusi-Fahrplänen und erzeugst eine JSON-Struktur,
die von einer lokalen CombineEngine verarbeitet wird.

WICHTIG:
- Die JSON-Struktur muss syntaktisch korrekt sein.
- Es dürfen KEINE verschachtelten Bedingungen außerhalb von ""group"" vorkommen.
- Jede Bedingung steht als eigenes Objekt in einer flachen Liste.
- Logische Operatoren stehen NUR:
    - auf oberster Ebene (""logical"")
    - oder innerhalb einer Gruppe (""group.logical"")
- Bedingungen dürfen KEIN eigenes ""logical"" enthalten.
- Bedingungen dürfen KEINE eigenen ""conditions"" enthalten.
- Nur ""group"" darf ein ""conditions""-Array enthalten.

------------------------------------------------------------
ERLAUBTE AKTIONEN
------------------------------------------------------------

action = ""combine""

------------------------------------------------------------
TOP-LEVEL FORMAT
------------------------------------------------------------

{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""logical"": ""and"" | ""or"",
    ""conditions"": [
      {{ ... einzelne Bedingung ... }},
      {{ ... einzelne Bedingung ... }},
      {{ ... Gruppe ... }}
    ]
  }}
}}

Standard: logical = ""and""

------------------------------------------------------------
GRUPPENFORMAT (lokales OR/AND)
------------------------------------------------------------

Eine Gruppe ist eine Bedingung vom Typ ""group"":

{{
  ""type"": ""group"",
  ""logical"": ""or"" | ""and"",
  ""conditions"": [
    {{ ... Bedingung ... }},
    {{ ... Bedingung ... }}
  ]
}}

Beispiel für Fahrzeug 216 ODER 218:

{{
  ""type"": ""group"",
  ""logical"": ""or"",
  ""conditions"": [
    {{ ""type"": ""fahrzeug"", ""pattern"": ""216"" }},
    {{ ""type"": ""fahrzeug"", ""pattern"": ""218"" }}
  ]
}}

------------------------------------------------------------
ERLAUBTE BEDINGUNGSTYPEN
------------------------------------------------------------

- ""von_nach""        → {{ ""from"": ""..."", ""to"": ""..."" }}
- ""ueber""           → {{ ""stations"": [""...""] }}
- ""von_nach_ueber""  → {{ ""from"": ""..."", ""to"": ""..."", ""via"": [""...""] }}

- ""fahrzeug""        → {{ ""pattern"": ""..."" }}
- ""zugnummer""       → {{ ""patterns"": [""...""] }}

- ""zeit""            → {{ ""value"": ""..."" }}
- ""zeitbereich""     → {{ ""from"": ""..."", ""to"": ""..."" }}

- ""datum""           → {{ ""value"": ""..."" }}
- ""datum_vor""       → {{ ""value"": ""..."" }}
- ""datum_nach""      → {{ ""value"": ""..."" }}
- ""datum_zwischen""  → {{ ""from"": ""..."", ""to"": ""..."" }}

- ""dauer_min""       → {{ ""value"": ""..."" }}
- ""dauer_max""       → {{ ""value"": ""..."" }}
- ""dauer_zwischen""  → {{ ""from"": ""..."", ""to"": ""..."" }}

- ""zugart""          → {{ ""value"": ""personen"" | ""gueter"" | ""deko"" }}

- ""group""           → siehe oben


------------------------------------------------------------
WENN DER BENUTZER ""ODER"" SAGT
------------------------------------------------------------

Du erzeugst eine Gruppe:

Beispiel:
""Züge von Hagen nach Kassel mit Fahrzeug 216 oder 218""

{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""logical"": ""and"",
    ""conditions"": [
      {{ ""type"": ""von_nach"", ""from"": ""Hagen"", ""to"": ""Kassel"" }},
      {{
        ""type"": ""group"",
        ""logical"": ""or"",
        ""conditions"": [
          {{ ""type"": ""fahrzeug"", ""pattern"": ""216"" }},
          {{ ""type"": ""fahrzeug"", ""pattern"": ""218"" }}
        ]
      }}
    ]
  }}
}}

------------------------------------------------------------
WENN DER BENUTZER ""UND"" SAGT
------------------------------------------------------------

Alle Bedingungen stehen flach in der Liste:

""Züge von Hagen nach Kassel mit Fahrzeug 216 und über Paderborn""

{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""logical"": ""and"",
    ""conditions"": [
      {{ ""type"": ""von_nach"", ""from"": ""Hagen"", ""to"": ""Kassel"" }},
      {{ ""type"": ""fahrzeug"", ""pattern"": ""216"" }},
      {{ ""type"": ""ueber"", ""stations"": [""Paderborn""] }}
    ]
  }}
}}

------------------------------------------------------------
KEINE VERSCHACHTELTEN BEDINGUNGEN
------------------------------------------------------------

NICHT ERLAUBT:

{{
  ""type"": ""von_nach"",
  ""conditions"": [...]
}}

NICHT ERLAUBT:

{{
  ""type"": ""fahrzeug"",
  ""logical"": ""or""
}}

NICHT ERLAUBT:

{{
  ""conditions"": [
    {{ ""conditions"": [...] }}
  ]
}}

------------------------------------------------------------
BEISPIEL: KOMPLEXE ABFRAGE
------------------------------------------------------------

""Züge von Kassel nach Hagen zwischen 8 und 12 Uhr mit Fahrzeug 103 oder 218 über Paderborn""

{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""logical"": ""and"",
    ""conditions"": [
      {{ ""type"": ""von_nach"", ""from"": ""Kassel"", ""to"": ""Hagen"" }},
      {{ ""type"": ""zeitbereich"", ""from"": ""08:00"", ""to"": ""12:00"" }},
      {{ ""type"": ""ueber"", ""stations"": [""Paderborn""] }},
      {{
        ""type"": ""group"",
        ""logical"": ""or"",
        ""conditions"": [
          {{ ""type"": ""fahrzeug"", ""pattern"": ""103"" }},
          {{ ""type"": ""fahrzeug"", ""pattern"": ""218"" }}
        ]
      }}
    ]
  }}
}}

DATUMSREGELN (sehr wichtig):

1. ""vor <Datum>"" bedeutet IMMER:
   - type: ""datum_vor""
   - value: <Datum im Format YYYY-MM-DD>

   Beispiele:
   ""vor 2000"" → {{ ""type"": ""datum_vor"", ""value"": ""2000-01-01"" }}
   ""vor Jahr 2000"" → {{ ""type"": ""datum_vor"", ""value"": ""2000-01-01"" }}
   ""vor 1.1.2000"" → {{ ""type"": ""datum_vor"", ""value"": ""2000-01-01"" }}

2. ""nach <Datum>"" bedeutet IMMER:
   - type: ""datum_nach""
   - value: <Datum im Format YYYY-MM-DD>

   Beispiele:
   ""nach 2000"" → {{ ""type"": ""datum_nach"", ""value"": ""2000-01-01"" }}
   ""nach 1.1.2000"" → {{ ""type"": ""datum_nach"", ""value"": ""2000-01-01"" }}

3. ""zwischen <A> und <B>"" bedeutet IMMER:
   - type: ""datum_zwischen""
   - from: <A im Format YYYY-MM-DD>
   - to:   <B im Format YYYY-MM-DD>

4. Die KI darf NIEMALS:
   - ""vor 2000"" als ""datum_nach: 1999"" interpretieren
   - Jahreszahlen ohne Tag/Monat anders interpretieren

5. Wenn der Benutzer nur eine Jahreszahl nennt:
   - ""vor 2000"" → 2000-01-01
   - ""nach 2000"" → 2000-01-01
   - ""zwischen 1980 und 2000"" → 1980-01-01 bis 2000-12-31

6. Die KI MUSS Datumsangaben IMMER normalisieren:
   - ""1.1.2000"" → ""2000-01-01""
   - ""2000"" → ""2000-01-01""
   - ""1999"" → ""1999-01-01""

DAUERREGELN (sehr wichtig):

1. Dauerangaben müssen IMMER in Minuten normalisiert werden.
2. Die KI darf NIEMALS Strings wie ""1 hour"", ""2 hours"", ""45 mins"" oder ähnliches ausgeben.
3. Die KI MUSS IMMER eine reine Zahl (Integer) in Minuten liefern.

Erlaubte Eingaben und ihre Normalisierung:

- ""1 Stunde"" → 60
- ""2 Stunden"" → 120
- ""30 Minuten"" → 30
- ""1,5 Stunden"" → 90
- ""90 min"" → 90
- ""eine Stunde"" → 60
- ""halbe Stunde"" → 30
- ""¾ Stunde"" → 45
- ""1h"" → 60
- ""1 h 30 min"" → 90

Die KI MUSS IMMER:

- Stunden → Minuten umrechnen
- Dezimalstunden → Minuten umrechnen
- gemischte Angaben → Minuten umrechnen
- Brüche → Minuten umrechnen

Ausgabeformat:

Für Mindestdauer:
{{
  ""type"": ""dauer_min"",
  ""value"": ""<Minuten als Zahl>""
}}

Für Höchstdauer:
{{
  ""type"": ""dauer_max"",
  ""value"": ""<Minuten als Zahl>""
}}

Für Dauerbereich:
{{
  ""type"": ""dauer_zwischen"",
  ""from"": ""<Minuten als Zahl>"",
  ""to"": ""<Minuten als Zahl>""
}}

Die KI darf KEINE anderen Formate verwenden.

BEGRIFFSDEFINITIONEN (sehr wichtig):

1. ZUGNUMMER

""Zugnummer <Wert>"" bedeutet:

- Wenn <Wert> nur aus Ziffern besteht (z.B. ""1234""):
  {{
    ""type"": ""zugnummer"",
    ""patterns"": [""1234""]
  }}

- Wenn <Wert> aus Gattung + Nummer besteht (z.B. ""RE1234"", ""RE 1234""):
  {{
    ""type"": ""zugnummer"",
    ""patterns"": [""RE1234""]
  }}

Die KI MUSS:
- Gattung + Nummer als eine Zugnummer behandeln.
- Leerzeichen ignorieren (""RE 1234"" → ""RE1234"").
- ""Zugnummer 1234"" so interpretieren, dass alle Züge mit Nummerteil ""1234"" gefunden werden.


2. ZUGGATTUNG

""Zuggattung <Gattung>"" bedeutet IMMER:

{{
  ""type"": ""zuggattung"",
  ""value"": ""<Gattung>""
}}

Beispiele:
- ""Zuggattung RE"" → {{ ""type"": ""zuggattung"", ""value"": ""RE"" }}
- ""Zuggattung ICE"" → {{ ""type"": ""zuggattung"", ""value"": ""ICE"" }}

Die KI MUSS:
- Gattungen wie ICE, IC, EC, RE, RB, S als Zuggattung erkennen.
- ""RE"" als Zuggattung behandeln, NICHT als Zugart.


3. ZUGART

""Zugart <Art>"" bedeutet IMMER:

{{
  ""type"": ""zugart"",
  ""value"": ""personen"" | ""gueter"" | ""deko""
}}

Beispiele:
- ""Zugart Personen"" → {{ ""type"": ""zugart"", ""value"": ""personen"" }}
- ""Zugart Güter"" → {{ ""type"": ""zugart"", ""value"": ""gueter"" }}
- ""Zugart Dekozug"" → {{ ""type"": ""zugart"", ""value"": ""deko"" }}

ZUGART (Personen / Güter / Deko):

Die KI MUSS folgende Begriffe als zugart = ""personen"" interpretieren:

- Personenzug
- Personenzüge
- Personen-Zug
- Personen-Züge
- Personenverkehr
- Personenverkehrszug
- Personenverkehrszüge
- Personenverkehr-Zug

Ausgabe:
{{ ""type"": ""zugart"", ""value"": ""personen"" }}

Die KI darf diese Begriffe NICHT als Zuggattung interpretieren.

Die KI MUSS folgende Begriffe als zugart = ""gueter"" interpretieren:

- Güterzug
- Güterzüge
- Gueterzug
- Gueterzüge
- Cargo
- Frachtzug
- Frachtzüge

Ausgabe:
{{ ""type"": ""zugart"", ""value"": ""gueter"" }}

Die KI darf diese Begriffe NICHT als Zuggattung interpretieren.

Die KI darf:
- Zuggattung (RE, ICE, …) NICHT als Zugart interpretieren.
- Zugart ist nur Personen/Güter/Deko.


4. ODER-VERKNÜPFUNG MIT ZUGNUMMER UND ZUGGATTUNG

""Zugnummer 1234 oder Zuggattung RE"" MUSS als Gruppe erzeugt werden:

{{
  ""type"": ""group"",
  ""logical"": ""or"",
  ""conditions"": [
    {{ ""type"": ""zugnummer"", ""patterns"": [""1234""] }},
    {{ ""type"": ""zuggattung"", ""value"": ""RE"" }}
  ]
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

    public async Task<JsonElement> AnalyzeQuestionAsync2(string question)
    {
      var prompt = $@"
Du bist eine KI für die Fahrplananalyse im Programm ZusiStart.

Deine Aufgabe:
Analysiere die Benutzerfrage und gib eine JSON-Struktur zurück, die eine Liste von Bedingungen enthält.
Jede Bedingung wird später einzeln ausgewertet und die Ergebnisse werden logisch verknüpft.

Gib NUR JSON zurück. Keine Erklärungen, keinen Text außerhalb der JSON-Struktur.

Erlaubte Aktionen:
- combine

Erlaubte Bedingungstypen (""type""):
- ""von_nach""                → Züge von A nach B (gerichtet)
- ""ueber""                   → Züge über mehrere Stationen
- ""von_nach_ueber""          → Züge von A nach B über Stationen
- ""fahrzeug""                → Züge mit bestimmter Fahrzeugbaureihe oder Fahrzeugdatei
- ""zugnummer""               → Züge mit bestimmter Zugnummer, Zuggattung oder Nummernmuster

Erlaubte logische Verknüpfungen:
- ""and""  → Schnittmenge
- ""or""   → Vereinigung

Wenn die Frage eine ODER-Verknüpfung enthält, gib:
""logical"": ""or""

Wenn die Frage eine UND-Verknüpfung enthält, gib:
""logical"": ""and""

Standard ist ""and"".

Erlaubte Datum-Bedingungen:
- ""datum"" → exaktes Datum
- ""datum_vor"" → vor einem Datum
- ""datum_nach"" → nach einem Datum
- ""datum_zwischen"" → zwischen zwei Daten

Erlaubte Dauer-Bedingungen:
- ""dauer_min""      → Mindestdauer (z.B. 30 Minuten)
- ""dauer_max""      → Höchstdauer (z.B. 120 Minuten)
- ""dauer_zwischen"" → Dauerbereich (z.B. 30–90 Minuten)

Erlaubte Bedingungstypen:
- ""zugart"" → Personen, Güter oder Deko

Regeln:
- Wenn die Frage mehrere Bedingungen enthält, gib mehrere Objekte in ""conditions"" zurück.
- Wenn eine Bedingung nicht vorkommt, lasse sie weg.
- Wenn eine Bedingung unklar ist, gib sie NICHT aus.
- Wenn die Frage nur eine Bedingung enthält, gib trotzdem eine ""conditions""-Liste mit einem Element zurück.
- Nutze nur die oben definierten Schlüssel.

Beispiele:

Frage: ""Züge von Hagen nach Kassel mit Fahrzeug 103""
Antwort:
{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""conditions"": [
      {{ ""type"": ""von_nach"", ""from"": ""Hagen"", ""to"": ""Kassel"" }},
      {{ ""type"": ""fahrzeug"", ""pattern"": ""103"" }}
    ]
  }}
}}

Frage: ""Züge von Hagen""
Antwort:
{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""conditions"": [
      {{ ""type"": ""von_nach"", ""from"": ""Hagen"", ""to"": """" }}
    ]
  }}
}}

Frage: ""Züge nach Hagen""
Antwort:
{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""conditions"": [
      {{ ""type"": ""von_nach"", ""from"": """", ""to"": ""Hagen"" }}
    ]
  }}
}}

Frage: ""Züge über Bochum und Essen""
Antwort:
{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""conditions"": [
      {{ ""type"": ""ueber"", ""stations"": [""Bochum"", ""Essen""] }}
    ]
  }}
}}

Frage: ""Züge von Köln nach Bonn über Brühl""
Antwort:
{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""conditions"": [
      {{ ""type"": ""von_nach"", ""from"": ""Köln"", ""to"": ""Bonn"" }},
      {{ ""type"": ""ueber"", ""stations"": [""Brühl""] }}
    ]
  }}
}}

Frage: ""Züge vor dem 1.1.2000""
Antwort:
{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""conditions"": [
      {{ ""type"": ""datum_vor"", ""value"": ""2000-01-01"" }}
    ]
  }}
}}

Frage: ""Züge nach 1.1.2000""
Antwort:
{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""conditions"": [
      {{ ""type"": ""datum_nach"", ""value"": ""2000-01-01"" }}
    ]
  }}
}}

Frage: ""Züge zwischen 1980 und 2000""
Antwort:
{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""conditions"": [
      {{ ""type"": ""datum_zwischen"", ""from"": ""1980-01-01"", ""to"": ""2000-12-31"" }}
    ]
  }}
}}

Frage: ""Züge von Kassel nach Hagen maximal 45 Minuten""
Antwort:
{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""conditions"": [
      {{ ""type"": ""von_nach"", ""from"": ""Kassel"", ""to"": ""Hagen"" }},
      {{ ""type"": ""dauer_max"", ""value"": ""45"" }}
    ]
  }}
}}

Frage: ""Züge mindestens 2 Stunden""
Antwort:
{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""conditions"": [
      {{ ""type"": ""dauer_min"", ""value"": ""120"" }}
    ]
  }}
}}

Frage: ""Züge zwischen 30 und 90 Minuten""
Antwort:
{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""conditions"": [
      {{ ""type"": ""dauer_zwischen"", ""from"": ""30"", ""to"": ""90"" }}
    ]
  }}
}}

Frage: ""Personenzüge von Kassel nach Hagen""
Antwort:
{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""conditions"": [
      {{ ""type"": ""zugart"", ""value"": ""personen"" }},
      {{ ""type"": ""von_nach"", ""from"": ""Kassel"", ""to"": ""Hagen"" }}
    ]
  }}
}}

Frage: ""Güterzüge über Essen""
Antwort:
{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""conditions"": [
      {{ ""type"": ""zugart"", ""value"": ""gueter"" }},
      {{ ""type"": ""ueber"", ""stations"": [""Essen""] }}
    ]
  }}
}}

JSON-Ausgabeformat:
{{
  ""action"": ""combine"",
  ""parameters"": {{
    ""conditions"": [
      {{
        ""type"": ""..."",
        ""from"": ""..."",
        ""to"": ""..."",
        ""stations"": [""..."", ""...""],
        ""pattern"": ""..."",
        ""patterns"": [""..."", ""...""]
      }}
    ]
  }}
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
    // 2. Lokales Tool ausführen (robust auf verschiedene Keys)
    // ----------------------------------------------------
    public ZusiToolResult ExecuteLocalTool(JsonElement analysis)
    {
      var action = GetParameter(analysis, "action");
      DataManager.Instance.AddKIResultMessage($"* ExecuteLocalTool called with action: {action}");

      if (string.Equals(action, "find_zuege_ueber_betriebsstelle", StringComparison.OrdinalIgnoreCase))
      {
        var station = GetParameter(analysis, "station", "Betriebsstelle", "Ort", "place");
        if (station == null)
          return new ZusiToolResult
          {
            Action = "find_zuege_ueber_betriebsstelle",
            Query = $"Züge über Betriebsstelle {station}",
            Summary = "Keine Betriebsstelle erkannt."
          };
        return FindZuegeUeberBetriebsstelle(station);
      }

      if (string.Equals(action, "find_fahrzeuge_fuer_zug", StringComparison.OrdinalIgnoreCase))
      {
        var vehicleId = GetParameter(analysis, "zug_id", "zug", "train", "vehicle_id" , "name");
        return FindZuegeMitFahrzeug(vehicleId);
      }

      if (action == "find_zuege_ueber_mehrere_betriebsstellen")
      {
        var stations = GetParameterList(analysis, "stations", "Betriebsstellen", "Orte");
        return FindZuegeUeberStationen(stations);
      }

      if (action == "find_zuege_von_nach")
      {
        var from = GetParameter(analysis, "from", "von", "start");
        var to = GetParameter(analysis, "to", "nach", "ziel", "end");
        return FindZuegeVonNach(from, to);
      }

      if (action == "find_zuege_von_nach_ueber")
      {
        var from = GetParameter(analysis, "from", "von", "start");
        var to = GetParameter(analysis, "to", "nach", "ziel", "end");
        var via = GetParameterList(analysis, "via", "ueber", "zwischen");

        // WICHTIG: via muss mindestens 1 Station enthalten
        if (via == null || via.Count == 0)
        {
          // Fallback: normale Von-Nach-Suche
          return FindZuegeVonNach(from, to);
        }
        return FindZuegeVonNachUeber(from, to, via);
      }

      if (action == "find_zuege_mit_zugnummer")
      {
        var patterns = GetParameterList(analysis, "patterns", "zugnummern", "numbers");
        return FindZuegeMitZugnummer(patterns);
      }

      if (action == "combine")
        return _combine.ExecuteCombine(analysis);

      //if (action == "combine")
      //{
      //  var conditions = GetConditionList(analysis);
      //  var logical = GetParameter(analysis, "logical") ?? "and";
      //  return CombineConditions(conditions, logical);
      //  //return CombineConditions(conditions);
      //}

      return new ZusiToolResult
      {
        Action = action ?? "unknown",
        Query = "Unbekannte Aktion",
        Summary = "Die KI konnte nicht eindeutig bestimmen, was zu tun ist."
      };
    }

    private List<JsonElement> GetConditionList(JsonElement analysis)
    {
      if (analysis.TryGetProperty("parameters", out var p))
      {
        if (p.TryGetProperty("conditions", out var arr) && arr.ValueKind == JsonValueKind.Array)
        {
          return arr.EnumerateArray().ToList();
        }
      }

      return new List<JsonElement>();
    }

    private ZusiToolResult CombineConditions(List<JsonElement> conditions, string logical)
    {
      _log.Debug($"CombineConditions called with {conditions.Count} conditions.\n conditions: {string.Join(", ", conditions.Select(c => c.ToString()))}");
      DataManager.Instance.AddKIResultMessage($"* CombineConditions called with {conditions.Count} conditions. \n conditions: {string.Join(", ", conditions.Select(c => c.ToString()))}");

      var resultSets = new List<List<KIResult>>();

      foreach (var cond in conditions)
      {
        var type = GetParameter(cond, "type");

        switch (type)
        {
          case "von_nach":
            {
              var from = GetParameter(cond, "from", "start", "von");
              var to = GetParameter(cond, "to", "nach", "ziel");
              var r = FindZuegeVonNach(from, to);
              resultSets.Add(r.Results);
              break;
            }

          case "ueber":
            {
              var stations = GetParameterList(cond, "stations", "ueber", "via");
              var r = FindZuegeUeberStationen(stations);
              resultSets.Add(r.Results);
              break;
            }

          case "von_nach_ueber":
            {
              var from = GetParameter(cond, "from");
              var to = GetParameter(cond, "to");
              var via = GetParameterList(cond, "via", "stations", "ueber");
              var r = FindZuegeVonNachUeber(from, to, via);
              resultSets.Add(r.Results);
              break;
            }

          case "fahrzeug":
            {
              var pattern = GetParameter(cond, "pattern", "fahrzeug", "baureihe");
              var r = FindZuegeMitFahrzeug(pattern);
              resultSets.Add(r.Results);
              break;
            }

          case "zugnummer":
            {
              var pattern = GetParameter(cond, "pattern", "zuggattung");
              var patterns = GetParameterList(cond, "patterns", "nummern", "gattung");
              if (pattern != "")
              {
                patterns.Add(pattern);
              }
              var r = FindZuegeMitZugnummer(patterns);
              resultSets.Add(r.Results);
              break;
            }
          case "datum_vor":
            {
              var value = GetParameter(cond, "value");
              var r = FindZuegeMitDatumfilterErweitert("vor", value, null);
              resultSets.Add(r.Results);
              break;
            }

          case "datum_nach":
            {
              var value = GetParameter(cond, "value");
              var r = FindZuegeMitDatumfilterErweitert("nach", value, null);
              resultSets.Add(r.Results);
              break;
            }

          case "datum_zwischen":
            {
              var from = GetParameter(cond, "from");
              var to = GetParameter(cond, "to");
              var r = FindZuegeMitDatumfilterErweitert("zwischen", from, to);
              resultSets.Add(r.Results);
              break;
            }

          case "dauer_min":
            {
              var value = GetParameter(cond, "value");
              var r = FindZuegeMitDauerfilter("min", value, null);
              resultSets.Add(r.Results);
              break;
            }

          case "dauer_max":
            {
              var value = GetParameter(cond, "value");
              var r = FindZuegeMitDauerfilter("max", value, null);
              resultSets.Add(r.Results);
              break;
            }

          case "dauer_zwischen":
            {
              var from = GetParameter(cond, "from");
              var to = GetParameter(cond, "to");
              var r = FindZuegeMitDauerfilter("zwischen", from, to);
              resultSets.Add(r.Results);
              break;
            }

          case "zugart":
            {
              var value = GetParameter(cond, "value");
              var r = FindZuegeMitZugart(value);
              resultSets.Add(r.Results);
              break;
            }

          case "group":
            {
              var groupLogical = GetParameter(cond, "logical") ?? "and";
              var nested = GetConditionList(cond);

              var nestedResults = new List<List<KIResult>>();

              foreach (var inner in nested)
              {
                var type1 = GetParameter(inner, "type");

                switch (type1)
                {
                  case "fahrzeug":
                    {
                      var pattern = GetParameter(inner, "pattern");
                      var r = FindZuegeMitFahrzeug(pattern);
                      nestedResults.Add(r.Results);
                      break;
                    }

                  case "zugnummer":
                    {
                      var patterns = GetParameterList(inner, "patterns");
                      var r = FindZuegeMitZugnummer(patterns);
                      nestedResults.Add(r.Results);
                      break;
                    }

                    // weitere Typen möglich
                }
              }

              // Gruppe verknüpfen
              List<KIResult> groupFinal;

              if (groupLogical.Equals("or", StringComparison.OrdinalIgnoreCase))
              {
                groupFinal = nestedResults
                    .SelectMany(s => s)
                    .GroupBy(r => r.Zug_Id)
                    .Select(g => g.First())
                    .ToList();
              }
              else
              {
                groupFinal = nestedResults.Aggregate((prev, next) =>
                    prev.Where(p => next.Any(n => n.Zug_Id == p.Zug_Id)).ToList());
              }

              resultSets.Add(groupFinal);
              break;
            }


          default:
            break;
        }
      }

      if (resultSets.Count == 0)
      {
        return new ZusiToolResult
        {
          Action = "combine",
          Summary = "Keine gültigen Bedingungen erkannt."
        };
      }

      List<KIResult> final;

      // ⭐ UND-Verknüpfung (Schnittmenge)
      if (logical.Equals("and", StringComparison.OrdinalIgnoreCase))
      {
        final = resultSets.Aggregate((prev, next) =>
            prev.Where(p => next.Any(n => n.Zug_Id == p.Zug_Id)).ToList());
      }
      else
      {
        // ⭐ ODER-Verknüpfung (Vereinigung)
        final = resultSets
            .SelectMany(s => s)
            .GroupBy(r => r.Zug_Id)
            .Select(g => g.First())
            .ToList();
      }

      DataManager.Instance.AddKIResultMessage($"* CombineConditions found {final.Count} results.");

      DataManager.FoundTimeTableIds = final.Select(r => r.Zug_Id);

      var trains = DataManager.Instance._allTrains
          .Where(z =>
          {
            //string zugNummer = (z.ID.ToString())
            //  .Replace(" ", "")
            //  .Trim()
            //  .ToLower();

            //// Match ANY pattern
            //return DataManager.FoundTimeTableIds.Any(p => zugNummer.Contains(p));
            string zugID = z.ID.ToString();

            // Match ANY pattern
            return DataManager.FoundTimeTableIds.Any(p => zugID == p);
          })
          .GroupBy(z => z.BelongsToTimeTable)
          .Select(g => new FoundTimeTable()
          {
            TimeTable = DataManager.Instance._allTimeTables.First(tt => tt.ID == g.Key),
            Trains = g.ToList()
          });

      DataManager.Instance.UpdateFoundTimetables(trains);
      DataManager.SearchTrainValue = null;

      return new ZusiToolResult
      {
        Action = "combine",
        Results = final,
        Summary = $"{final.Count} Züge gefunden (logische Verknüpfung: {logical})."
      };
    }

    public ZusiToolResult FindZuegeMitZuggattung(string gattung)
    {
      var result = new ZusiToolResult
      {
        Action = "zuggattung",
        Query = $"Zuggattung: {gattung}"
      };

      if (string.IsNullOrWhiteSpace(gattung))
      {
        result.Summary = "Keine Zuggattung angegeben.";
        return result;
      }

      string g = gattung.Trim().ToLower();

      var trains = DataManager.Instance._allTrains
          .Where(z => (z.Gattung ?? "").ToLower() == g)
          .GroupBy(z => z.BelongsToTimeTable)
          .Select(gp => new
          {
            TimeTable = DataManager.Instance._allTimeTables.First(tt => tt.ID == gp.Key),
            Trains = gp.ToList()
          });

      foreach (var group in trains)
      {
        foreach (var zug in group.Trains)
        {
          result.Results.Add(new KIResult
          {
            Fahrplan_Name = group.TimeTable.Name,
            Fahrplan_Datei = group.TimeTable.GetDocument().Filename,
            Zug_Name = zug.Gattung + zug.Nummer,
            Zug_Id = zug.Nummer,
            Fahrzeug_Datei = "", //zug.FahrzeugDatei,
            Strecke = zug.FahrplanEintraege
                  .Where(f => DataManager.IsValidName(f.Bestrst))
                  .Select(f => f.Bestrst)
                  .ToList()
          });
        }
      }

      result.Summary = $"{result.Results.Count} Züge gefunden.";
      return result;
    }


    public ZusiToolResult FindZuegeMitZugart(string value)
    {
      var result = new ZusiToolResult
      {
        Action = "zugart",
        Query = $"Zugart: {value}"
      };

      if (string.IsNullOrWhiteSpace(value))
      {
        result.Summary = "Keine Zugart erkannt.";
        return result;
      }

      string v = value.Trim().ToLower();

      bool IsPersonen(Zug z)
      {
        //string g = (z.Gattung ?? "").ToLower();
        //string f = (z.FahrzeugDatei ?? "").ToLower();

        //return g.Contains("ice") || g.Contains("ic") || g.Contains("ec") ||
        //       g.Contains("re") || g.Contains("rb") || g.Contains("s") ||
        //       f.Contains("wagen") || f.Contains("dosto") ||
        //       f.Contains("bpm") || f.Contains("apm") || f.Contains("bimz");
        return z.Type == TrainType.Passenger;
      }

      bool IsGueter(Zug z)
      {
        //string g = (z.Gattung ?? "").ToLower();
        //string f = (z.FahrzeugDatei ?? "").ToLower();

        //return g.Contains("gz") || g.Contains("dgz") ||
        //       f.Contains("eaos") || f.Contains("falns") ||
        //       f.Contains("habbiins") || f.Contains("sgns");
        return z.Type == TrainType.Freight;
      }

      bool IsDeko(Zug z)
      {
        //string g = (z.Gattung ?? "").ToLower();
        //string f = (z.FahrzeugDatei ?? "").ToLower();

        //return g.Contains("deko") || f.Contains("deko") || f.Contains("dummy");
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

      var trains = DataManager.Instance._allTrains
          .Where(IsMatch)
          .GroupBy(z => z.BelongsToTimeTable)
          .Select(g => new
          {
            TimeTable = DataManager.Instance._allTimeTables.First(tt => tt.ID == g.Key),
            Trains = g.ToList()
          });

      foreach (var group in trains)
      {
        foreach (var zug in group.Trains)
        {
          result.Results.Add(new KIResult
          {
            Fahrplan_Name = group.TimeTable.Name,
            Fahrplan_Datei = group.TimeTable.GetDocument().Filename,
            Zug_Name = zug.Gattung + zug.Nummer,
            Zug_Id = zug.ID.ToString(),
            Fahrzeug_Datei = "", //zug.FahrzeugDatei,
            Strecke = zug.FahrplanEintraege
                  .Where(f => DataManager.IsValidName(f.Bestrst))
                  .Select(f => f.Bestrst)
                  .ToList()
          });
        }
      }

      result.Summary = $"{result.Results.Count} Züge gefunden.";
      return result;
    }


    public ZusiToolResult FindZuegeMitDauerfilter(string mode, string value1, string value2)
    {
      var result = new ZusiToolResult
      {
        Action = "dauer",
        Query = $"Dauerfilter: {mode} {value1} {value2}"
      };

      bool ok1 = int.TryParse(value1, out var min1);
      bool ok2 = int.TryParse(value2, out var min2);

      if (!ok1)
      {
        result.Summary = "Ungültige Dauerangabe.";
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

      var trains = DataManager.Instance._allTrains
          .Where(IsMatch)
          .GroupBy(z => z.BelongsToTimeTable)
          .Select(g => new
          {
            TimeTable = DataManager.Instance._allTimeTables.First(tt => tt.ID == g.Key),
            Trains = g.ToList()
          });

      foreach (var group in trains)
      {
        foreach (var zug in group.Trains)
        {
          result.Results.Add(new KIResult
          {
            Fahrplan_Name = group.TimeTable.Name,
            Fahrplan_Datei = group.TimeTable.GetDocument().Filename,
            Zug_Name = zug.Gattung + zug.Nummer,
            Zug_Id = zug.ID.ToString(),
            Fahrzeug_Datei = "", //zug.FahrzeugDatei,
            Strecke = zug.FahrplanEintraege
                  .Where(f => DataManager.IsValidName(f.Bestrst))
                  .Select(f => f.Bestrst)
                  .ToList()
          });
        }
      }

      result.Summary = $"{result.Results.Count} Züge gefunden.";
      return result;
    }

    public ZusiToolResult FindZuegeMitDatumfilterErweitert(string mode, string value1, string value2)
    {
      var result = new ZusiToolResult
      {
        Action = "datum_erweitert",
        Query = $"Datumfilter: {mode} {value1} {value2}"
      };

      // Datum normalisieren
      bool ok1 = DateTime.TryParse(value1, out var d1);
      bool ok2 = DateTime.TryParse(value2, out var d2);

      if (!ok1)
      {
        result.Summary = "Ungültiges Datum.";
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

      var trains = DataManager.Instance._allTrains
          .Where(IsMatch)
          .GroupBy(z => z.BelongsToTimeTable)
          .Select(g => new
          {
            TimeTable = DataManager.Instance._allTimeTables.First(tt => tt.ID == g.Key),
            Trains = g.ToList()
          });

      foreach (var group in trains)
      {
        foreach (var zug in group.Trains)
        {
          result.Results.Add(new KIResult
          {
            Fahrplan_Name = group.TimeTable.Name,
            Fahrplan_Datei = group.TimeTable.GetDocument().Filename,
            Zug_Name = zug.Gattung + zug.Nummer,
            Zug_Id = zug.ID.ToString(),
            Fahrzeug_Datei = "", //zug.FahrzeugDatei,
            Strecke = zug.FahrplanEintraege
                  .Where(f => DataManager.IsValidName(f.Bestrst))
                  .Select(f => f.Bestrst)
                  .ToList()
          });
        }
      }

      result.Summary = $"{result.Results.Count} Züge gefunden.";
      return result;
    }

    public ZusiToolResult FindZuegeMitDatumfilter(string value)
    {
      var result = new ZusiToolResult
      {
        Action = "datum",
        Query = $"Datumfilter: {value}"
      };

      if (!DateTime.TryParse(value, out var date))
      {
        result.Summary = "Ungültiges Datum.";
        return result;
      }

      bool IsMatch(Zug z)
      {
        // 1. Fahrplan-Datum
        //var tt = DataManager.Instance._allTimeTables
        //    .FirstOrDefault(t => t.ID == z.BelongsToTimeTable);

        //if (tt?.Date.Date == date.Date)
        //  return true;

        //// 2. FahrplanEinträge
        //foreach (var f in z.FahrplanEintraege)
        //{
        //  if (f.Datum.HasValue && f.Datum.Value.Date == date.Date)
        //    return true;
        //}

        return false;
      }

      var trains = DataManager.Instance._allTrains
          .Where(IsMatch)
          .GroupBy(z => z.BelongsToTimeTable)
          .Select(g => new
          {
            TimeTable = DataManager.Instance._allTimeTables.First(tt => tt.ID == g.Key),
            Trains = g.ToList()
          });

      foreach (var group in trains)
      {
        foreach (var zug in group.Trains)
        {
          result.Results.Add(new KIResult
          {
            Fahrplan_Name = group.TimeTable.Name,
            Fahrplan_Datei = group.TimeTable.GetDocument().Filename,
            Zug_Name = zug.Gattung + zug.Nummer,
            Zug_Id = zug.Nummer,
            Fahrzeug_Datei = "", //zug.FahrzeugDatei,
            Strecke = zug.FahrplanEintraege
                  .Where(f => DataManager.IsValidName(f.Bestrst))
                  .Select(f => f.Bestrst)
                  .ToList()
          });
        }
      }

      result.Summary = $"{result.Results.Count} Züge gefunden.";
      return result;
    }

    public ZusiToolResult FindZuegeMitUhrzeitfilter(string fromTime, string toTime)
    {
      var result = new ZusiToolResult
      {
        Action = "zeitbereich",
        Query = $"Uhrzeitfilter: {fromTime} - {toTime}"
      };

      if (!TimeSpan.TryParse(fromTime, out var tFrom))
        tFrom = TimeSpan.Zero;

      if (!TimeSpan.TryParse(toTime, out var tTo))
        tTo = TimeSpan.FromHours(24);

      bool IsMatch(Zug z)
      {
        foreach (var f in z.FahrplanEintraege)
        {
          if (f.Arrival.HasValue)
          {
            var a = f.Arrival.Value.TimeOfDay;
            if (a >= tFrom && a <= tTo)
              return true;
          }

          if (f.Departure.HasValue)
          {
            var a = f.Departure.Value.TimeOfDay;
            if (a >= tFrom && a <= tTo)
              return true;
          }
        }

        return false;
      }

      var trains = DataManager.Instance._allTrains
          .Where(IsMatch)
          .GroupBy(z => z.BelongsToTimeTable)
          .Select(g => new
          {
            TimeTable = DataManager.Instance._allTimeTables.First(tt => tt.ID == g.Key),
            Trains = g.ToList()
          });

      foreach (var group in trains)
      {
        foreach (var zug in group.Trains)
        {
          result.Results.Add(new KIResult
          {
            Fahrplan_Name = group.TimeTable.Name,
            Fahrplan_Datei = group.TimeTable.GetDocument().Filename,
            Zug_Name = zug.Gattung + zug.Nummer,
            Zug_Id = zug.Nummer,
            Fahrzeug_Datei = "", //zug.FahrzeugDatei,
            Strecke = zug.FahrplanEintraege
                  .Where(f => DataManager.IsValidName(f.Bestrst))
                  .Select(f => f.Bestrst)
                  .ToList()
          });
        }
      }

      result.Summary = $"{result.Results.Count} Züge gefunden.";
      return result;
    }

    public ZusiToolResult FindZuegeMitZugnummer(List<string> patterns)
    {
      DataManager.Instance.AddKIResultMessage($"* FindZuegeMitZugnummer called with patterns: {string.Join(", ", patterns)}");
      var result = new ZusiToolResult
      {
        Action = "find_zuege_mit_zugnummer",
        Query = $"Züge mit Zugnummern: {string.Join(", ", patterns)}"
      };

      if (patterns == null || patterns.Count == 0)
      {
        result.Summary = "Keine Zugnummern erkannt.";
        return result;
      }

      // Normalisierte Suchmuster
      var normalized = patterns
          .Where(p => !string.IsNullOrWhiteSpace(p))
          .Select(p => p.Replace(" ", "").Trim().ToLower())
          .ToList();

      var trains = DataManager.Instance._allTrains
          .Where(z =>
          {
            string zugNummer = (z.Gattung + z.Nummer)
              .Replace(" ", "")
              .Trim()
              .ToLower();

            // Match ANY pattern
            return normalized.Any(p => zugNummer.Contains(p));
          })
          .GroupBy(z => z.BelongsToTimeTable)
          .Select(g => new FoundTimeTable()
          {
            TimeTable = DataManager.Instance._allTimeTables.First(tt => tt.ID == g.Key),
            Trains = g.ToList()
          });

      foreach (var group in trains)
      {
        foreach (var zug in group.Trains)
        {
          result.Results.Add(new KIResult
          {
            Fahrplan_Name = group.TimeTable.Name,
            Fahrplan_Datei = group.TimeTable.GetDocument().Filename,
            Zug_Name = zug.Gattung + zug.Nummer,
            Zug_Id = zug.ID.ToString(),
            Fahrzeug_Datei = "", //zug.FahrzeugDatei,
            Strecke = zug.FahrplanEintraege
                  .Where(f => DataManager.IsValidName(f.Bestrst))
                  .Select(f => f.Bestrst)
                  .ToList()
          });
        }
      }
      //DataManager.Instance.UpdateFoundTimetables(trains);

      result.Summary = $"{result.Results.Count} Züge gefunden.";
      return result;
    }


    public ZusiToolResult FindZuegeVonNachUeber(string from, string to, List<string> via)
    {
      if (via == null || via.Count == 0)
      {
        return FindZuegeVonNach(from, to);
      }
      
      DataManager.Instance.AddKIResultMessage($"* FindZuegeVonNachUeber called with from: {from}, to: {to}, via: {string.Join(", ", via)}");

      var result = new ZusiToolResult
      {
        Action = "find_zuege_von_nach_ueber",
        Query = $"Züge von {from} nach {to} über {string.Join(", ", via)}"
      };

      if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
      {
        result.Summary = "Start- oder Ziel-Betriebsstelle fehlt.";
        return result;
      }

      if (via == null || via.Count == 0)
      {
        result.Summary = "Keine Zwischenstationen erkannt.";
        return result;
      }

      // Normalisierte Suchbegriffe
      string nf = from.Replace(" ", "").Trim().ToLower();
      string nt = to.Replace(" ", "").Trim().ToLower();
      var nvias = via
          .Where(s => !string.IsNullOrWhiteSpace(s))
          .Select(s => s.Replace(" ", "").Trim().ToLower())
          .ToList();

      var trains = DataManager.Instance._allTrains
          .Where(z =>
          {
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
          .GroupBy(z => z.BelongsToTimeTable)
          .Select(g => new FoundTimeTable()
          {
            TimeTable = DataManager.Instance._allTimeTables.First(tt => tt.ID == g.Key),
            Trains = g.ToList()
          });

      foreach (var group in trains)
      {
        foreach (var zug in group.Trains)
        {
          result.Results.Add(new KIResult
          {
            Fahrplan_Name = group.TimeTable.Name,
            Fahrplan_Datei = group.TimeTable.GetDocument().Filename,
            Zug_Name = zug.Gattung + zug.Nummer,
            Zug_Id = zug.ID.ToString(),
            Fahrzeug_Datei = "", //zug.FahrzeugDatei,
            Strecke = zug.FahrplanEintraege
                  .Where(f => DataManager.IsValidName(f.Bestrst))
                  .Select(f => f.Bestrst)
                  .ToList()
          });
        }
      }
      //DataManager.Instance.UpdateFoundTimetables(trains);

      result.Summary = $"{result.Results.Count} Züge gefunden.";
      return result;
    }


    public ZusiToolResult FindZuegeVonNach(string from, string to)
    {
      DataManager.Instance.AddKIResultMessage($"* FindZuegeVonNach called with from: {from}, to: {to}");
      var result = new ZusiToolResult
      {
        Action = "find_zuege_von_nach",
        Query = $"Züge von {from} nach {to}"
      };

      if (string.IsNullOrWhiteSpace(from) && string.IsNullOrWhiteSpace(to))
      {
        result.Summary = "Start- und Ziel-Betriebsstelle fehlt.";
        return result;
      }

      string nf = from.Replace(" ", "").Trim().ToLower();
      string nt = to.Replace(" ", "").Trim().ToLower();

      var trains = DataManager.Instance._allTrains
          .Where(z =>
          {
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
              if (nt=="" && idxFrom < entries.Count - 2) return true; // nur Startstation angegeben, Zug muss danach noch weiterfahren
            }

            if (!string.IsNullOrWhiteSpace(nt))
            {
              if (!entries.Any(e => e.Contains(nt))) return false;
              idxTo = entries.FindIndex(e => e.Contains(nt));
              if (nf=="" && idxTo > 0) return true; // nur Zielstation angegeben, Zug muss vorher schon gefahren sein
            }

            return idxFrom >= 0 && idxTo >= 0 && idxFrom < idxTo;
          })
          .GroupBy(z => z.BelongsToTimeTable)
          .Select(g => new FoundTimeTable()
          {
            TimeTable = DataManager.Instance._allTimeTables.First(tt => tt.ID == g.Key),
            Trains = g.ToList()
          });

      foreach (var group in trains)
      {
        foreach (var zug in group.Trains)
        {
          result.Results.Add(new KIResult
          {
            Fahrplan_Name = group.TimeTable.Name,
            Fahrplan_Datei = group.TimeTable.GetDocument().Filename,
            Zug_Name = zug.Gattung + zug.Nummer,
            Zug_Id = zug.ID.ToString(),
            Fahrzeug_Datei = "", //zug.FahrzeugDatei,
            Strecke = zug.FahrplanEintraege
                  .Where(f => DataManager.IsValidName(f.Bestrst))
                  .Select(f => f.Bestrst)
                  .ToList()
          });
        }
      }
      //DataManager.Instance.UpdateFoundTimetables(trains);
      //DataManager.SearchTrainValue = nf;

      result.Summary = $"{result.Results.Count} Züge gefunden.";
      return result;
    }


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

    public ZusiToolResult FindZuegeUeberStationen(List<string> stations)
    {
      DataManager.Instance.AddKIResultMessage($"* FindZuegeUeberStationen called with stations: {string.Join(", ", stations)}");
      var result = new ZusiToolResult
      {
        Action = "find_zuege_ueber_mehrere_betriebsstellen",
        Query = $"Züge über mehrere Betriebsstellen: {string.Join(", ", stations)}"
      };

      if (stations == null || stations.Count == 0)
      {
        result.Summary = "Keine Betriebsstellen erkannt.";
        return result;
      }

      // Normalisierte Suchbegriffe
      var normalized = stations
          .Where(s => !string.IsNullOrWhiteSpace(s))
          .Select(s => s.Replace(" ", "").Trim().ToLower())
          .ToList();

      var trains = DataManager.Instance._allTrains
          .Where(z =>
          {
            // Alle Stationen müssen vorkommen
            return normalized.All(st =>
              z.FahrplanEintraege.Any(f =>
                  f.Bestrst != null &&
                  DataManager.IsValidName(f.Bestrst) &&
                  f.Bestrst.Replace(" ", "").Trim()
                      .ToLower()
                      .Contains(st)));
          })
          .GroupBy(z => z.BelongsToTimeTable)
          .Select(g => new FoundTimeTable()
          {
            TimeTable = DataManager.Instance._allTimeTables.First(tt => tt.ID == g.Key),
            Trains = g.ToList()
          });

      foreach (var group in trains)
      {
        foreach (var zug in group.Trains)
        {
          result.Results.Add(new KIResult
          {
            Fahrplan_Name = group.TimeTable.Name,
            Fahrplan_Datei = zug.GetDocument().Filename,
            Zug_Name = zug.Gattung + zug.Nummer,
            Zug_Id = zug.ID.ToString(),
            Fahrzeug_Datei = "", //zug.FahrzeugDatei,
            Strecke = zug.FahrplanEintraege
                  .Where(f => DataManager.IsValidName(f.Bestrst))
                  .Select(f => f.Bestrst)
                  .ToList()
          });
        }
      }

      //DataManager.Instance.UpdateFoundTimetables(trains);
      //DataManager.SearchTrainValue = normalized[0];

      result.Summary = $"{result.Results.Count} Züge gefunden.";
      return result;
    }


    public ZusiToolResult FindZuegeUeberBetriebsstelle(string station)
    {
      DataManager.Instance.AddKIResultMessage($"* FindZuegeUeberBetriebsstelle called with station: {station}");
      var result = new ZusiToolResult
      {
        Action = "find_zuege_ueber_betriebsstelle",
        Query = $"Züge über Betriebsstelle {station}"
      };

      if (string.IsNullOrWhiteSpace(station))
      {
        result.Summary = "Keine Betriebsstelle erkannt.";
        return result;
      }

      string n = station.Replace(" ", "").Trim().ToLower();

      var trains = DataManager.Instance._allTrains
          .Where(z =>
              z.FahrplanEintraege.Any(f =>
                  f.Bestrst != null &&
                  DataManager.IsValidName(f.Bestrst) &&
                  f.Bestrst.Replace(" ", "").Trim()
                      .Contains(n, StringComparison.OrdinalIgnoreCase)))
          .GroupBy(g => g.BelongsToTimeTable)
          .Select(g => new FoundTimeTable()
          {
            TimeTable = DataManager.Instance._allTimeTables.First(tt => tt.ID == g.Key),
            Trains = g.ToList()
          });

      foreach (var group in trains)
      {
        foreach (var zug in group.Trains)
        {
          result.Results.Add(new KIResult
          {
            Fahrplan_Name = group.TimeTable.Name,
            Fahrplan_Datei = zug.GetDocument().Filename, //group.TimeTable.GetDocument().Filename, // falls vorhanden
            Zug_Name = zug.Gattung + zug.Nummer,
            Zug_Id = zug.ID.ToString(),
            Fahrzeug_Datei = "", //zug.Fahrzeuge.Datei.Dateiname,
            Strecke = zug.FahrplanEintraege
                  .Where(f => DataManager.IsValidName(f.Bestrst))
                  .Select(f => f.Bestrst)
                  .ToList()
          });  // 
        }
      }

      //DataManager.Instance.UpdateFoundTimetables(trains);
      //DataManager.SearchTrainValue = station;

      result.Summary = $"{result.Results.Count} Züge gefunden.";
      return result;
    }




    public ZusiToolResult FindZuegeMitFahrzeug(string fahrzeugId)
    {
      var result = new ZusiToolResult
      {
        Action = "find_zuege_mit_fahrzeug",
        Query = $"Züge mit Fahrzeug {fahrzeugId}"
      };

      if (string.IsNullOrWhiteSpace(fahrzeugId))
      {
        result.Summary = "Keine Zug-ID erkannt.";
        return result;
      }

      //foreach (var fp in KiZusiData.Fahrplaene)
      //{
      //  foreach (var zug in fp.Zuege)
      //  {
      //    if (string.Equals(zug.Id, fahrzeugId, StringComparison.OrdinalIgnoreCase) ||
      //        string.Equals(zug.Name, fahrzeugId, StringComparison.OrdinalIgnoreCase))
      //    {
      //      var fahrzeug = KiZusiData.Fahrzeuge
      //          .Find(f => string.Equals(f.FahrzeugDatei, zug.FahrzeugDatei, StringComparison.OrdinalIgnoreCase));

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


      //_log.Debug("OnSearchBR called with value=" + value);
      DataManager.Instance.AddKIResultMessage($"* FindZuegeMitFahrzeug called with fahrzeugId: {fahrzeugId}");

      var trains = DataManager.Instance._allTrains.Where(z => z.Fahrzeuge.ContainsVehicle(fahrzeugId))
          .GroupBy(g => g.BelongsToTimeTable)
          .Select(g => new FoundTimeTable()
          {
            TimeTable = DataManager.Instance._allTimeTables.First(tt => tt.ID == g.Key),
            Trains = g.ToList()
          });

      foreach (var group in trains)
      {
        foreach (var zug in group.Trains)
        {
          result.Results.Add(new KIResult
          {
            Fahrplan_Name = group.TimeTable.Name,
            Fahrplan_Datei = zug.GetDocument().Filename, //group.TimeTable.GetDocument().Filename, // falls vorhanden
            Zug_Name = zug.Gattung + zug.Nummer,
            Zug_Id = zug.ID.ToString(),
            Fahrzeug_Datei = "", //zug.Fahrzeuge.Datei.Dateiname,
            Strecke = zug.FahrplanEintraege
                  .Where(f => DataManager.IsValidName(f.Bestrst))
                  .Select(f => f.Bestrst)
                  .ToList()
          });  // 
        }
      }

      //DataManager.Instance.UpdateFoundTimetables(trains);
      //DataManager.SearchTrainValue = "br" + fahrzeugId;

      result.Summary = $"{result.Results.Count} Fahrzeuge gefunden.";
      return result;
    }

    // ----------------------------------------------------
    // 3. Antwort formulieren
    // ----------------------------------------------------
    public async Task<string> FormulateAnswerAsync(string originalQuestion, ZusiToolResult toolResult)
    {
      var prompt = $@"
Du bist eine KI für ZusiStart-Fahrplananalyse.

Benutzerfrage:
{originalQuestion}

Interne Aktion:
{toolResult.Action}

Ergebnisse (JSON):
{JsonSerializer.Serialize(toolResult)}

Formuliere eine zusammenfassende Antwort für den Benutzer.
Wenn keine Ergebnisse vorhanden sind, erkläre das knapp.
";

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
      return doc.RootElement.GetProperty("response").GetString();
    }

    // ----------------------------------------------------
    // 4. Gesamtablauf
    // ----------------------------------------------------
    public async Task<string> ProcessQuestionAsync(string question)
    {
      var analysis = await AnalyzeQuestionAsync(question);
      var toolResult = ExecuteLocalTool(analysis);
      var answer = await FormulateAnswerAsync(question, toolResult);

      DataManager.Instance.AddKIResultMessage($"* CombineConditions found {toolResult.Results.Count} results.");

      DataManager.FoundTimeTableIds = toolResult.Results.Select(r => r.Zug_Id);

      var trains = DataManager.Instance._allTrains
          .Where(z =>
          {
            //string zugNummer = (z.ID.ToString())
            //  .Replace(" ", "")
            //  .Trim()
            //  .ToLower();

            //// Match ANY pattern
            //return DataManager.FoundTimeTableIds.Any(p => zugNummer.Contains(p));
            string zugID = z.ID.ToString();

            // Match ANY pattern
            return DataManager.FoundTimeTableIds.Any(p => zugID == p);
          })
          .GroupBy(z => z.BelongsToTimeTable)
          .Select(g => new FoundTimeTable()
          {
            TimeTable = DataManager.Instance._allTimeTables.First(tt => tt.ID == g.Key),
            Trains = g.ToList()
          });

      DataManager.Instance.UpdateFoundTimetables(trains);
      DataManager.SearchTrainValue = null;

      return answer;
    }

    // ----------------------------------------------------
    // Hilfsfunktionen: robuster JSON-Parser
    // ----------------------------------------------------
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

