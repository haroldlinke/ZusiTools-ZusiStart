using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit.Zoombox;
using ZusiCLIProject.FileLibrary.Zusi3;
using ZusiFahrpultLib;
using ZusiKlassenLib2.Fahrplan;
using ZusiStart.Gleisbelegung;
using ZusiStart.KlLib2;

namespace ZusiStart.Controls
{

  public class GleisSection
  {
    public string Name { get; set; }
    public int Index { get; set; }
  }

  public class GleisBlock
  {
    public GleisSection Section { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string TrainName { get; set; }
    public string Zuglauf { get; set; }
    public string Signal { get; set; }   // NEU
    public bool IsConflict { get; set; }
    public string Richtung { get; set; }
    public string Einfahrtvon { get; set; }
    public string Ausfahrtnach { get; set; } 

    public bool StartsHere { get; set; }
    public bool EndsHere { get; set; }
    public bool TurnsHere { get; set; }
    public bool Durchfahrt { get; set; }
    public GleisBlock ConflictPartner { get; set; }

  }


  public class TimeScaler
  {
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double PixelsPerMinute { get; set; } = 8.0;

    public double ToX(DateTime t)
        => (t - StartTime).TotalMinutes * PixelsPerMinute;
  }

  public class GleisbelegungColors
  {
    public Color Background { get; set; } = (Color)ColorConverter.ConvertFromString("#1E1E1E");
    public Color TimeLine { get; set; } = Colors.Gray;
    public Color TrackLine { get; set; } = Colors.Gray;

    // Konflikte
    public Color ConflictBorder { get; set; } = Colors.White;
    public Color ConflictOverlay { get; set; } = Color.FromArgb(150, 255, 0, 0);
  }



  /// <summary>
  /// Interaktionslogik für GleisbelegungControl.xaml
  /// </summary>
  public partial class GleisbelegungControl : UserControl
  {
    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public ZusiCLIProject.FileLibrary.Zusi3.Zug[] Zugliste
    {
      get => _zugliste;
      set { _zugliste = value; Update(); }
    }
    private ZusiCLIProject.FileLibrary.Zusi3.Zug[] _zugliste;

    public ZusiKlassenLib2.Fahrplan.Zug[] Zugliste2
    {
      get => _zugliste2;
      set { _zugliste2 = value; Update2(); }
    }
    private ZusiKlassenLib2.Fahrplan.Zug[] _zugliste2;

    public string FahrplanName
    {
      get => _fahrplanName;
      set
      {
        _fahrplanName = value;
        UpdateHeader();
      }
    }
    private string _fahrplanName;

    public string StationName
    {
      get => _stationName;
      set
      {
        _stationName = value;
        UpdateHeader();
        Update();
      }
    }
    private string _stationName;


    private readonly List<GleisBlock> _blocks = new();
    private readonly List<GleisSection> _sections = new();
    private readonly TimeScaler _scaler = new();

    private class SignalMapping
    {
      public int Gleis { get; set; }
      public string Richtung { get; set; }
    }

    private Dictionary<string, SignalMapping> _signalMap = new();

    private double _zoom = 1.0;
    private const double ZoomStep = 0.1;
    private const double ZoomMin = 0.2;
    private const double ZoomMax = 4.0;

    private const double RowHeight = 40.0;

    private bool _isPanning = false;
    private Point _panStart;
    private double _scrollStartX;
    private double _scrollStartY;

    private GleisbelegungColors _colors;


    public GleisbelegungControl()
    {
      InitializeComponent();
      //ScrollMain.ScrollChanged += ScrollMain_ScrollChanged;
      _colors = GleisbelegungColorManager.Load();
    }

    private void ScrollMain_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      // Horizontal: Zeitachse folgt
      //ScrollTop.ScrollToHorizontalOffset(e.HorizontalOffset);

      // Vertikal: Gleisnamen folgen
      //ScrollLeft.ScrollToVerticalOffset(e.VerticalOffset);
    }

    public void Update()
    {
      if (Zugliste2 != null)
      {
        Update2();
        return;
      }
      if (Zugliste == null || StationName == null)
        return;
      LoadSignalMapping(StationName);

      _blocks.Clear();
      _sections.Clear();

      ExtractTrackSections();
      if (_sections.Count == 0)
      {
        int num = (int)System.Windows.MessageBox.Show("Keine Gleise gefunden", "Gleisbelegung: " + StationName, MessageBoxButton.OK);
        return;
      }
      ExtractBlocks();
      if (_blocks.Count == 0)
      {
        int num = (int)System.Windows.MessageBox.Show("Keine Blöcke gefunden", "Gleisbelegung: " + StationName, MessageBoxButton.OK);
        return;
      }
      DetectConflicts();

      CanvasMain.Height = _sections.Count * 40;
      CanvasMain.Width = _blocks.Max(b => _scaler.ToX(b.End)) + 200;

      DrawTimeGrid();
      DrawTrackGrid(_sections);

      RenderGleisnamen();
      RenderZeitachse();
      RenderBalken();
    }

    public void Update2()
    {
      if (Zugliste2 == null || StationName == null)
        return;
      LoadSignalMapping(StationName);

      _blocks.Clear();
      _sections.Clear();

      ExtractTrackSections2();
      if (_sections.Count == 0)
      {
        int num = (int)System.Windows.MessageBox.Show("Keine Gleise gefunden", "Gleisbelegung: " + StationName, MessageBoxButton.OK);
        return;
      }
      ExtractBlocks2();
      if (_blocks.Count == 0)
      {
        int num = (int)System.Windows.MessageBox.Show("Keine Blöcke gefunden", "Gleisbelegung: " + StationName, MessageBoxButton.OK);
        return;
      }
      DetectConflicts();

      CanvasMain.Height = _sections.Count * 40;
      CanvasMain.Width = _blocks.Max(b => _scaler.ToX(b.End)) + 200;

      DrawTimeGrid();
      DrawTrackGrid(_sections);

      RenderGleisnamen();
      RenderZeitachse();
      RenderBalken();
    }


    // ---------------------------------------------------------
    // 1. Signal → Gleisnummer
    // ---------------------------------------------------------
    //private int? ExtractGleisFromSignal(string signal)
    //{
    //  if (string.IsNullOrWhiteSpace(signal))
    //    return null;

    //  //var digits = new string(signal.Skip(1).TakeWhile(char.IsDigit).ToArray());
    //  var digits = new string(signal.TakeWhile(char.IsDigit).ToArray());

    //  if (int.TryParse(digits, out int gleis))
    //    return gleis;

    //  return null;
    //}

    private int? ExtractGleisFromSignal2(string signal)
    {
      if (string.IsNullOrWhiteSpace(signal))
        return null;

      // Erste Ziffernfolge im Signal extrahieren
      var digits = new string(signal
          .SkipWhile(c => !char.IsDigit(c))   // bis zur ersten Ziffer springen
          .TakeWhile(char.IsDigit)            // alle folgenden Ziffern nehmen
          .ToArray());

      if (int.TryParse(digits, out int gleis))
        return gleis;

      return null;
    }

    private int? ExtractGleisFromSignal(string signal)
    {
      if (string.IsNullOrWhiteSpace(signal))
        return null;

      // 1. Benutzerdefinierte Zuordnung?
      if (_signalMap.TryGetValue(signal, out var map))
        return (map.Gleis);

      // Alle Ziffernfolgen extrahieren
      var matches = Regex.Matches(signal, @"\d+");

      if (matches.Count == 0)
        return null;

      // Die LETZTE Ziffernfolge ist die Gleisnummer
      string lastDigits = matches[matches.Count - 1].Value;

      if (int.TryParse(lastDigits, out int track))
        return track;

      return null;
    }



    // ---------------------------------------------------------
    // 2. Gleise extrahieren
    // ---------------------------------------------------------
    private void ExtractTrackSections()
    {
      var gleise = Zugliste
          .SelectMany(z => z.Eintraege)
          .Where(e => e.Betrst != null &&
            e.Betrst.Trim().Equals(StationName.Trim(), StringComparison.OrdinalIgnoreCase))
          .Select(e => ExtractGleisFromSignal(e.Eintraege?.FirstOrDefault()?.FahrplanSignal))
          .Where(g => g != null)
          .Distinct()
          .OrderBy(g => g)
          .ToList();

      int index = 0;
      foreach (var g in gleise)
      {
        _sections.Add(new GleisSection
        {
          Name = g.ToString(),
          Index = index++
        });
      }
    }

    // ---------------------------------------------------------
    // 2. Gleise extrahieren
    // ---------------------------------------------------------
    private void ExtractTrackSections2()
    {
      var gleise = Zugliste2
          .SelectMany(z => z.FahrplanEintraege)
          .Where(e => e.Bestrst != null &&
            e.Bestrst.Trim().Equals(StationName.Trim(), StringComparison.OrdinalIgnoreCase))
          .Select(e => ExtractGleisFromSignal(e.FahrplanSignalEintraege?.FirstOrDefault()?.FahrplanSignal))
          .Where(g => g != null)
          .Distinct()
          .OrderBy(g => g)
          .ToList();

      int index = 0;
      foreach (var g in gleise)
      {
        _sections.Add(new GleisSection
        {
          Name = g.ToString(),
          Index = index++
        });
      }
    }

    // ---------------------------------------------------------
    // 3. Belegungsblöcke extrahieren
    // ---------------------------------------------------------
    private void ExtractBlocks()
    {
      foreach (var zug in Zugliste)
      {
        foreach (var e in zug.Eintraege.Where(e =>
                 e.Betrst != null &&
                 e.Betrst.Trim().Equals(StationName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
          string signal = e.Eintraege?.FirstOrDefault()?.FahrplanSignal;

          //int? gleis = ExtractGleisFromSignal(signal);
          var (gleis, richtung) = GetGleisUndRichtung(signal);

          if (gleis == null)
          {
            Log.Error("ExtractBlocks: Gleis konnte nicht extrahiert werden:" + StationName + "-" + signal);
            continue;
          }

          var section = _sections.FirstOrDefault(s => s.Name == gleis.ToString());
          if (section == null)
          {
            section = new GleisSection
            {
              Name = gleis.ToString(),
              Index = _sections.Count
            };
            _sections.Add(section);
          }

          DateTime start = e.Ank;
          DateTime end = e.Abf;
          bool var_Durchfahrt = false;

          if (e.Ank == DateTime.MinValue)
          {
            var_Durchfahrt = true;
            start = e.Abf;
          }

          if (start != DateTime.MinValue && end != DateTime.MinValue)
          {

            _blocks.Add(new GleisBlock
            {
              Section = section,
              Start = start,
              End = end,
              TrainName = zug.Gattung + zug.Nummer,
              Zuglauf = zug.Zuglauf,
              Signal = signal,
              Richtung = richtung,
              //StartsHere = (start == zug.GetAufgleiszeit()),
              StartsHere = e == zug.Eintraege.First(),
              //EndsHere = (end == zug.GetAbgleiszeit()),
              EndsHere = e == zug.Eintraege.Last(),
              TurnsHere = e.FzgVerbandAktion != 0,
              Durchfahrt = var_Durchfahrt
            });
          }
          else
          {
            //int num = (int)System.Windows.MessageBox.Show("Fehler in Start oder Endzeit", "Gleisbelegung: " + zug.Gattung+zug.Nummer, MessageBoxButton.OK);
          }

        }
      }

      if (_blocks.Count > 0)
        CalculateTimeRange();
    }

    // ---------------------------------------------------------
    // 3. Belegungsblöcke extrahieren
    // ---------------------------------------------------------
    private void ExtractBlocks2()
    {
      foreach (var zug in Zugliste2)
      {
        foreach (var e in zug.FahrplanEintraege.Where(e =>
                 e.Bestrst != null &&
                 e.Bestrst.Trim().Equals(StationName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
          string signal = e.FahrplanSignalEintraege?.FirstOrDefault()?.FahrplanSignal;

          //int? gleis = ExtractGleisFromSignal(signal);
          var (gleis, richtung) = GetGleisUndRichtung(signal);

          var dir = DetectDirectionFromFahrplan(zug.FahrplanEintraege, StationName);

          dir = DetectDirection(zug.FahrplanEintraege, StationName);

          //block.EinfahrtVon = dir.EinfahrtVon;
          //block.AusfahrtNach = dir.AusfahrtNach;


          if (gleis == null)
          {
            Log.Error("ExtractBlocks: Gleis konnte nicht extrahiert werden:" + StationName + "-" + signal);
            continue;
          }

          var section = _sections.FirstOrDefault(s => s.Name == gleis.ToString());
          if (section == null)
          {
            section = new GleisSection
            {
              Name = gleis.ToString(),
              Index = _sections.Count
            };
            _sections.Add(section);
          }

          DateTime start = DateTime.MinValue;
          if (e.Arrival != null)
          {
            start = e.Arrival.Value;
          }


          DateTime end = DateTime.MinValue;
          if (e.Departure != null)
          {
            end = e.Departure.Value;
          }

          bool var_Durchfahrt = false;

          if (start == DateTime.MinValue)
          {
            var_Durchfahrt = true;
            start = end;
          }

          if (start != DateTime.MinValue && end != DateTime.MinValue)
          {

            bool turnsHere = !e.FzgVerbandAktion.Equals(ZusiKlassenLib2.Fahrplan.TrainSetActionType.Default);
            if (turnsHere)
            {

              Log.Info($"Zug {zug.Gattung}{zug.Nummer} wendet in {StationName}");

            }

            _blocks.Add(new GleisBlock
            {
              Section = section,
              Start = start,
              End = end,
              TrainName = zug.Gattung + zug.Nummer,
              Zuglauf = zug.Zuglauf,
              Signal = signal,
              Richtung = richtung,
              Einfahrtvon = dir.EinfahrtVon,
              Ausfahrtnach = dir.AusfahrtNach,
              //StartsHere = (start == zug.GetAufgleiszeit()),
              StartsHere = e == zug.FahrplanEintraege.First(),
              //EndsHere = (end == zug.GetAbgleiszeit()),
              EndsHere = e == zug.FahrplanEintraege.Last(),
              TurnsHere = turnsHere,
              Durchfahrt = var_Durchfahrt
            });
          }
          else
          {
            //int num = (int)System.Windows.MessageBox.Show("Fehler in Start oder Endzeit", "Gleisbelegung: " + zug.Gattung+zug.Nummer, MessageBoxButton.OK);
          }

        }
      }

      if (_blocks.Count > 0)
        CalculateTimeRange();
    }

    // ---------------------------------------------------------
    // 4. Konflikte erkennen
    // ---------------------------------------------------------
    private void DetectConflicts()
    {
      foreach (var group in _blocks.GroupBy(b => b.Section))
      {
        var list = group.OrderBy(b => b.Start).ToList();

        for (int i = 0; i < list.Count - 1; i++)
        {
          if (list[i].End > list[i + 1].Start)
          {
            list[i].IsConflict = true;
            list[i + 1].IsConflict = true;
            list[i].ConflictPartner = list[i + 1];
            list[i + 1].ConflictPartner = list[i];
          }
        }
      }
    }

    // ---------------------------------------------------------
    // 5. Rendering
    // ---------------------------------------------------------
    private void RenderGleisnamen()
    {
      CanvasLeft.Children.Clear();

      foreach (var section in _sections)
      {
        double y = section.Index * 40;

        var label = new TextBlock
        {
          Text = "Gleis " + section.Name,
          Foreground = Brushes.White,
          FontSize = 14
        };

        Canvas.SetLeft(label, 5);
        Canvas.SetTop(label, y + 8);
        CanvasLeft.Children.Add(label);
      }

      CanvasLeft.Height = _sections.Count * 40;
    }

    private void RenderZeitachse()
    {
      CanvasTop.Children.Clear();

      if (_blocks.Count == 0)
        return;

      DateTime t = _scaler.StartTime;
      DateTime max = _blocks.Max(b => b.End);

      while (t <= max)
      {
        double x = _scaler.ToX(t);

        var label = new TextBlock
        {
          Text = t.ToString("HH:mm"),
          Foreground = Brushes.LightGray,
          FontSize = 12
        };

        Canvas.SetLeft(label, x + 2);
        Canvas.SetTop(label, 10);
        CanvasTop.Children.Add(label);

        t = t.AddMinutes(15);
      }

      CanvasTop.Width = _blocks.Max(b => _scaler.ToX(b.End)) + 200;
    }

    private void RenderBalken()
    {
      //CanvasMain.Children.Clear();

      foreach (var block in _blocks)
      {
        double x = _scaler.ToX(block.Start);
        double width = _scaler.ToX(block.End) - x;
        double y = block.Section.Index * 40;
        double height = RowHeight;

        if (width < 4)
          width = 4;

        string signal = block.Signal; // speichern wir gleich im Block
        //string tooltip = BuildTooltip(block);

        string tooltip = "";

        if (block.IsConflict && block.ConflictPartner != null)
        {
          tooltip = BuildConflictTooltip(block, block.ConflictPartner);
        }
        else
        {
          tooltip = BuildTooltip(block);
        }

        var rect = new Rectangle
        {
          Width = width,
          Height = RowHeight - 4,
          RadiusX = 4,
          RadiusY = 4,
          Fill = block.IsConflict ? Brushes.IndianRed : Brushes.SteelBlue,
          Stroke = Brushes.Black,
          StrokeThickness = 1,
          ToolTip = tooltip
        };


        var rect_markstart = new Rectangle
        {
          Width = 2,
          Height = RowHeight - 4,
          RadiusX = 4,
          RadiusY = 4,
          Fill = Brushes.Yellow,
          Stroke = Brushes.Yellow,
          StrokeThickness = 1,
          ToolTip = tooltip
        };

        var rect_markend = new Rectangle
        {
          Width = 2,
          Height = RowHeight - 4,
          RadiusX = 4,
          RadiusY = 4,
          Fill = Brushes.Yellow,
          Stroke = Brushes.Yellow,
          StrokeThickness = 1,
          ToolTip = tooltip
        };

        if (block.IsConflict)
        {
          rect.Opacity = 0.8;          // leicht transparent
          rect.Stroke = Brushes.White; // weißer Rand
          rect.StrokeThickness = 2;    // gut sichtbar
        }
        else
        {
          rect.Opacity = 1.0;
          rect.StrokeThickness = 0;
        }

        Canvas.SetLeft(rect, x);
        Canvas.SetTop(rect, y + 2);
        if (!block.Durchfahrt)
        {
          CanvasMain.Children.Add(rect);

          if (block.StartsHere)
          {
            Canvas.SetLeft(rect_markstart, x);
            Canvas.SetTop(rect_markstart, y + 2);
            CanvasMain.Children.Add(rect_markstart);
          }
          if (block.EndsHere)
          {
            Canvas.SetLeft(rect_markend, x + width - 2);
            Canvas.SetTop(rect_markend, y + 2);
            CanvasMain.Children.Add(rect_markend);
          }
        }
        else
        {

        }

        bool direction = ComesFromBottom(block.Richtung);

        double size = (RowHeight - 4) / 2;

        if (direction)
        {
          // unten nach oben
          if (!block.StartsHere)
          {
            // Einfahrt
            DrawDiagonal(
                CanvasMain,
                x - size + 2,  // links
                y + size,            // oben
                size,          // Breite
                size,          // Höhe
                direction,
                block.Durchfahrt? Brushes.Yellow : Brushes.SteelBlue,
                tooltip: tooltip
            );
          }

          if (block.TurnsHere)
          {
            // Ausfahrt
            DrawDiagonal(
              CanvasMain,
              x + width - 2,    // rechte Seite
              y + size ,
              size,
              size,
              !direction, // Ausfahrt ist invertiert
              tooltip: tooltip
          );
          }
          else
          {
            if (!block.EndsHere)
            {
              // Ausfahrt
              DrawDiagonal(
                CanvasMain,
                x + width - 2,    // rechte Seite
                y,
                size,
                size,
                direction,
                block.Durchfahrt ? Brushes.Yellow : Brushes.SteelBlue,
                tooltip: tooltip
            );
            }
          }
        }
        else
        {
          if (!block.StartsHere)
          {
            // Einfahrt
            DrawDiagonal(
              CanvasMain,
              x - size + 2,                      // linke Seite
              y ,
              size,                     // Breite der Schräge
              size,
              direction,
              block.Durchfahrt ? Brushes.Yellow : Brushes.SteelBlue,
              tooltip: tooltip
          );
          }

          if (block.TurnsHere)
          {
            // Ausfahrt zurück
            DrawDiagonal(
              CanvasMain,
              x + width - 2,    // rechte Seite
              y,
              size,
              size,
              !direction, // Ausfahrt ist invertiert
              tooltip: tooltip
          );
          }
          else
          {
            if (!block.EndsHere)
            {
              // Ausfahrt
              DrawDiagonal(
                CanvasMain,
                x + width - 2,    // rechte Seite
                y + size,
                size,
                size,
                direction,
                block.Durchfahrt ? Brushes.Yellow : Brushes.SteelBlue,
                tooltip: tooltip
            );
            }
          }
        }

        //var text = new TextBlock
        //{
        //  Text = block.TrainName,
        //  Foreground = Brushes.White,
        //  FontSize = 12
        //};
        var text = new TextBlock
        {
          Text = block.TrainName,
          Foreground = Brushes.White,
          FontSize = 12,
          ToolTip = tooltip
        };


        Canvas.SetLeft(text, x + 4);
        Canvas.SetTop(text, y + 6);
        CanvasMain.Children.Add(text);
      }

      CanvasMain.Width = _blocks.Max(b => _scaler.ToX(b.End)) + 200;
      CanvasMain.Height = _sections.Count * 40;
      CanvasMain.Background = new SolidColorBrush(_colors.Background);

    }

    private string ExtractRichtung2(string signal)
    {
      if (string.IsNullOrWhiteSpace(signal))
        return "?";

      char c = char.ToUpper(signal[0]);

      return c switch
      {
        'N' => "Norden",
        'P' => "Süden",
        'S' => "Süden",
        'A' => "Osten",
        'B' => "Westen",
        _ => "?"
      };
    }

    private string ExtractRichtung(string signal)
    {
      if (string.IsNullOrWhiteSpace(signal))
        return null;

      // Alle Ziffernfolgen finden
      var matches = Regex.Matches(signal, @"\d+");
      if (matches.Count == 0)
        return null;

      // Die letzte Ziffernfolge
      var last = matches[matches.Count - 1];

      int index = last.Index; // Position der letzten Ziffernfolge im String

      // Buchstabe direkt davor?
      if (index > 0)
      {
        char c = signal[index - 1];

        if (char.IsLetter(c))
        {
          char d = c.ToString().ToUpper()[0];
          return d switch
          {
            'N' => "Norden",
            'P' => "Süden",
            'S' => "Süden",
            'A' => "Osten",
            'B' => "Westen",
            _ => "?"
          };
        }

      }

      return "?";
    }


    private string BuildTooltip(GleisBlock block)
    {
      string startstring = "";
      if (block.Durchfahrt)
      {
        startstring = "-----";
      }
      else
      {
        startstring = block.Start.ToString("HH:mm:ss");
      }
      return
          $"Zug: {block.TrainName}\n" +
          $"Zuglauf: {block.Zuglauf}\n" +
          $"Signal: {block.Signal}\n" +
          $"Gleis: {block.Section.Name}\n" +
          $"Richtung: {block.Richtung}\n" +
          $"Einfahrt von: {block.Einfahrtvon}\n" +
          $"Ausfahrt nach: {block.Ausfahrtnach}\n" +
          $"Ankunft: {startstring}\n" +
          $"Abfahrt: {block.End:HH:mm:ss}";
    }

    private string BuildConflictTooltip(GleisBlock a, GleisBlock b)
    {
      return
          $"Konflikt zwischen:\n" +
          $"{a.TrainName}  ({a.Start:HH:mm} – {a.End:HH:mm}) - {a.Signal}\n" +
          $"{b.TrainName}  ({b.Start:HH:mm} – {b.End:HH:mm}) - {b.Signal}\n\n" +
          $"Gleis: {a.Section.Name}\n";

    }

    public static string GetMappingFileForStation(string stationName)
    {
      string safe = stationName
          .Trim()
          .Replace(" ", "_")
          .Replace("/", "_")
          .Replace("\\", "_");

      string dir = System.IO.Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
          "ZusiStart",
          "SignalMappings");

      if (!Directory.Exists(dir))
        Directory.CreateDirectory(dir);

      return System.IO.Path.Combine(dir, $"SignalGleisMapping_{safe}.csv");
    }


    //private Dictionary<string, SignalMapping> _signalMap = new();
    private string _currentMappingFile;

    public void LoadSignalMapping(string stationName)
    {
      _signalMap.Clear();

      _currentMappingFile = GetMappingFileForStation(stationName);

      if (!File.Exists(_currentMappingFile))
        return;

      foreach (var line in File.ReadAllLines(_currentMappingFile).Skip(1))
      {
        var parts = line.Split(',');
        if (parts.Length >= 2 && int.TryParse(parts[1], out int gleis))
        {
          string richtung = parts.Length >= 3 ? parts[2] : "";
          _signalMap[parts[0]] = new SignalMapping
          {
            Gleis = gleis,
            Richtung = richtung
          };
        }
      }
    }

    private void EnsureMappingFileExists()
    {
      if (!File.Exists(_currentMappingFile))
      {
        File.WriteAllText(_currentMappingFile, "Signal,Gleis,Richtung\n");
      }
    }

    private void AppendMapping(string signal, int gleis, string richtung)
    {
      EnsureMappingFileExists();

      using (var sw = new StreamWriter(_currentMappingFile, append: true))
      {
        sw.WriteLine($"{signal},{gleis},{richtung}");
      }
    }

    private (int? gleis, string richtung) GetGleisUndRichtung(string signal)
    {
      if (string.IsNullOrWhiteSpace(signal))
        return (null, null);

      // 1. Benutzerdefinierte Zuordnung?
      if (_signalMap.TryGetValue(signal, out var map))
        return (map.Gleis, map.Richtung);

      // 2. Automatische Gleisbestimmung
      int? gleis = ExtractGleisFromSignal(signal);

      // 3. Automatische Richtungsbestimmung
      string richtung = ExtractRichtung(signal);

      if (gleis != null)
      {
        // 4. Automatisch in CSV ergänzen
        AppendMapping(signal, gleis.Value, richtung);

        _signalMap[signal] = new SignalMapping
        {
          Gleis = gleis.Value,
          Richtung = richtung
        };
      }

      return (gleis, richtung);
    }

    private void DrawTimeGrid()
    {
      CanvasMain.Children.Clear();
      CanvasTop.Children.Clear();

      DateTime t = _scaler.StartTime;
      while (t <= _scaler.EndTime)
      {
        //double x = (t - _scaler.StartTime).TotalMinutes * PixelPerMinute;
        double x = _scaler.ToX(t);

        // Linie im Hauptbereich
        var line = new Line
        {
          X1 = x,
          Y1 = 0,
          X2 = x,
          Y2 = CanvasMain.Height,
          Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
          StrokeThickness = 1
        };
        line.Stroke = new SolidColorBrush(_colors.TimeLine);
        CanvasMain.Children.Add(line);

        // Linie in der Zeitachse
        var topLine = new Line
        {
          X1 = x,
          Y1 = 0,
          X2 = x,
          Y2 = 40,
          Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
          StrokeThickness = 1
        };
        topLine.Stroke = new SolidColorBrush(_colors.TimeLine);
        CanvasTop.Children.Add(topLine);

        // Zeitlabel
        var label = new TextBlock
        {
          Text = t.ToString("HH:mm"),
          Foreground = Brushes.White
        };
        Canvas.SetLeft(label, x + 2);
        Canvas.SetTop(label, 2);
        CanvasTop.Children.Add(label);

        t = t.AddMinutes(15);
      }
    }

    private void DrawTrackGrid(List<GleisSection> sections)
    {
      double y = 0;

      foreach (var section in sections)
      {
        // Linie über die gesamte Breite
        var line = new Line
        {
          X1 = 0,
          Y1 = y,
          X2 = CanvasMain.Width,
          Y2 = y,
          Stroke = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
          StrokeThickness = 1
        };
        line.Stroke = new SolidColorBrush(_colors.TrackLine);

        CanvasMain.Children.Add(line);

        y += 40;
      }

      // Unterste Linie
      var last = new Line
      {
        X1 = 0,
        Y1 = y,
        X2 = CanvasMain.Width,
        Y2 = y,
        Stroke = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
        StrokeThickness = 1
      };
      last.Stroke = new SolidColorBrush(_colors.TrackLine);

      CanvasMain.Children.Add(last);
    }

    private DateTime RoundDownToQuarter(DateTime t)
    {
      int minutes = (t.Minute / 15) * 15;
      return new DateTime(t.Year, t.Month, t.Day, t.Hour, minutes, 0);
    }

    private DateTime RoundUpToQuarter(DateTime t)
    {
      int minutes = ((t.Minute + 14) / 15) * 15;
      if (minutes == 60)
        return new DateTime(t.Year, t.Month, t.Day, t.Hour, 0, 0).AddHours(1);

      return new DateTime(t.Year, t.Month, t.Day, t.Hour, minutes, 0);
    }

    private void CalculateTimeRange()
    {
      if (_blocks.Count == 0)
      {
        _scaler.StartTime = RoundDownToQuarter(DateTime.Today);
        _scaler.EndTime = RoundUpToQuarter(DateTime.Today.AddHours(1));
        return;
      }

      var min = _blocks.Min(b => b.Start);
      var max = _blocks.Max(b => b.End);

      _scaler.StartTime = RoundDownToQuarter(min.AddMinutes(-10));
      _scaler.EndTime = RoundUpToQuarter(max.AddMinutes(+10));
    }

    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
      if ((System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.None) ||
          (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
      //  if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
      {
        e.Handled = true;

        double oldZoom = _zoom;

        if (e.Delta > 0)
          _zoom += ZoomStep;
        else
          _zoom -= ZoomStep;

        _zoom = Math.Max(ZoomMin, Math.Min(ZoomMax, _zoom));

        ZoomTransform.ScaleX = _zoom;
        ZoomTransform.ScaleY = _zoom;

        // Scrollposition stabilisieren
        Point mousePos = e.GetPosition(Scroll);
        Scroll.ScrollToHorizontalOffset((mousePos.X + Scroll.HorizontalOffset) * (_zoom / oldZoom) - mousePos.X);
        Scroll.ScrollToVerticalOffset((mousePos.Y + Scroll.VerticalOffset) * (_zoom / oldZoom) - mousePos.Y);
      }
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
      base.OnPreviewMouseDown(e);

      //if (e.MiddleButton == MouseButtonState.Pressed)
      if ((e.ChangedButton == System.Windows.Input.MouseButton.Left) || (e.MiddleButton == MouseButtonState.Pressed))
      {
        _isPanning = true;
        _panStart = e.GetPosition(this);

        _scrollStartX = Scroll.HorizontalOffset;
        _scrollStartY = Scroll.VerticalOffset;

        Cursor = Cursors.Hand;
        CaptureMouse();
      }
    }

    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
      base.OnPreviewMouseMove(e);

      if (_isPanning)
      {
        Point pos = e.GetPosition(this);
        Vector delta = pos - _panStart;

        Scroll.ScrollToHorizontalOffset(_scrollStartX - delta.X);
        Scroll.ScrollToVerticalOffset(_scrollStartY - delta.Y);
      }
    }

    protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
    {
      base.OnPreviewMouseUp(e);

      if (_isPanning && e.MiddleButton == MouseButtonState.Released)
      {
        _isPanning = false;
        Cursor = Cursors.Arrow;
        ReleaseMouseCapture();
      }
    }

    private bool ComesFromBottom(string direction)
    {
      return direction.StartsWith("N") || direction.StartsWith("W");
    }

    private void DrawDiagonal(Canvas canvas, double x, double y, double width, double height, bool fromBottom, Brush color = null, string? tooltip = null)
    {
      Line line = new Line
      {
        Stroke = color ??  Brushes.SteelBlue,
        StrokeThickness = 2,
        ToolTip = tooltip
      };

      if (fromBottom)
      {
        // Linie von unten nach oben
        line.X1 = x;
        line.Y1 = y + height;
        line.X2 = x + width;
        line.Y2 = y;
      }
      else
      {
        // Linie von oben nach unten
        line.X1 = x;
        line.Y1 = y;
        line.X2 = x + width;
        line.Y2 = y + height;
      }

      canvas.Children.Add(line);
    }

    private void UpdateHeader()
    {
      TxtFahrplan.Text = string.IsNullOrWhiteSpace(FahrplanName)
          ? "Fahrplan: (unbekannt)"
          : $"Fahrplan: {FahrplanName}";

      TxtStation.Text = string.IsNullOrWhiteSpace(StationName)
          ? "Station: (unbekannt)"
          : $"Station: {StationName}";
    }

    public class FahrtrichtungInfo
    {
      public string EinfahrtVon { get; set; }
      public string AusfahrtNach { get; set; }
    }

    public FahrtrichtungInfo DetectDirectionFromFahrplan(List<FahrplanEintrag> entries, string station)
    {
      var info = new FahrtrichtungInfo();

      int index = entries.FindIndex(e =>
          e.Bestrst.Equals(station, StringComparison.OrdinalIgnoreCase));

      if (index < 0)
        return info;

      // Einfahrt = vorherige Betriebsstelle
      if (index > 0)
        info.EinfahrtVon = entries[index - 1].Bestrst;

      // Ausfahrt = nächste Betriebsstelle
      if (index < entries.Count - 1)
        info.AusfahrtNach = entries[index + 1].Bestrst;

      return info;
    }

    private string FindNextRealStation(List<FahrplanEintrag> entries, int startIndex, string station)
    {
      for (int i = startIndex; i < entries.Count; i++)
      {
        if (!entries[i].Bestrst.Equals(station, StringComparison.OrdinalIgnoreCase))
          return entries[i].Bestrst;
      }
      return null;
    }

    private string FindPreviousRealStation(List<FahrplanEintrag> entries, int startIndex, string station)
    {
      for (int i = startIndex; i >= 0; i--)
      {
        if (!entries[i].Bestrst.Equals(station, StringComparison.OrdinalIgnoreCase))
          return entries[i].Bestrst;
      }
      return null;
    }


    public FahrtrichtungInfo DetectDirection(List<FahrplanEintrag> entries, string station)
    {
      var info = new FahrtrichtungInfo();

      int index = entries.FindIndex(e =>
          e.Bestrst.Equals(station, StringComparison.OrdinalIgnoreCase));

      if (index < 0)
        return info;

      info.EinfahrtVon = FindPreviousRealStation(entries, index - 1, station);
      info.AusfahrtNach = FindNextRealStation(entries, index + 1, station);

      return info;
    }



  }

}
