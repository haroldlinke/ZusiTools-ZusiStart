using log4net;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ZusiKlassenLib2;
using ZusiKlassenLib2.Fahrplan;
using ZusiKlassenLib2.TimeTable;
using ZusiStart.Data;
using ZusiStart.Gleisbelegung;

namespace ZusiStart.Controls
{
  public partial class BildfahrplanControl : UserControl
  {
    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public BildfahrplanControl()
    {
      InitializeComponent();
    }

    // ---------------------------------------------------------
    // Dependency Properties
    // ---------------------------------------------------------

    public static readonly DependencyProperty SelectedZugProperty =
        DependencyProperty.Register(nameof(SelectedZug), typeof(Zug),
            typeof(BildfahrplanControl), new PropertyMetadata(null));

    public Zug SelectedZug
    {
      get => (Zug)GetValue(SelectedZugProperty);
      set => SetValue(SelectedZugProperty, value);
    }

    public static readonly DependencyProperty ZuglisteProperty =
        DependencyProperty.Register(nameof(Zugliste), typeof(IEnumerable<Zug>),
            typeof(BildfahrplanControl), new PropertyMetadata(null));

    public IEnumerable<Zug> Zugliste
    {
      get => (IEnumerable<Zug>)GetValue(ZuglisteProperty);
      set => SetValue(ZuglisteProperty, value);
    }

    // ---------------------------------------------------------
    // Zeitsteuerung
    // ---------------------------------------------------------

    private DateTime _absoluteStartTime;
    private DateTime _displayStartTime;
    private DateTime _displayEndTime;

    private const double TopOffset = 100;

    double LeftOffset = 50;

    // ---------------------------------------------------------
    // Zoom / Pan
    // ---------------------------------------------------------

    private double _zoom = 1.0;
    private const double ZoomMin = 0.2;
    private const double ZoomMax = 5.0;

    private Point _lastPanPoint;
    private bool _isPanning = false;

    private double _ppm = 2.0; // Pixel pro Minute
    private double PxPerMinuteX = 20.0;
    private double PxPerMinuteY = 8.0;

    private Polyline _selectedLine = null;



    private void ChkFullPlan_Checked(object sender, RoutedEventArgs e)
    {
      //RenderBildfahrplan(fullPlan: true);
    }

    private void ChkFullPlan_Unchecked(object sender, RoutedEventArgs e)
    {
     // RenderBildfahrplan(fullPlan: false);
    }



    // ---------------------------------------------------------
    // Button
    // ---------------------------------------------------------

    private void OnGenerateBildfahrplan(object sender, RoutedEventArgs e)
    {
      SelectedZug = DataManager.Instance.SelectedZug;

      TimeTableRelation timetablerelation = DataManager.Instance.SelectedTimeTableRelation;
      ZusiKlassenLib2.TimeTable.TimeTable timeTable = timetablerelation?.TimeTable;

      var new_zug_list = new List<Zug>();

      if (timeTable != null)
      {
        foreach (var train in timeTable.Trains)
        {
          try
          {
            if (train.Train != null)
              new_zug_list.Add(train.Train);

            if (train.Link != null)
            {
              ZugDatei zd = train.Link.ZugDatei;
              zd.Parse();
              if (zd.Root != null)
                new_zug_list.Add(zd.Root);
            }
          }
          catch (Exception ex)
          {
            _log.Error($"OnGenerateBildfahrplan ERROR: {ex}");
          }
        }
      }

      Zugliste = new_zug_list;

      if (SelectedZug == null)
      {
        MessageBox.Show("Bitte zuerst einen Zug auswählen.");
        return;
      }

      RenderBildfahrplan(fullPlan: ChkFullPlan.IsChecked == true);
    }

    // ---------------------------------------------------------
    // Rendering
    // ---------------------------------------------------------

    private List<(string Name, DateTime Time)> GetRawEntries(Zug zug)
    {
      return zug.FahrplanEintraege
          .Select(e => new { e.Bestrst, Time = (e.Arrival ?? e.Departure), e.FplEintrag })
          .Where(x => x.Time != null && x.FplEintrag != TimeTableItemType.Helper)
          .Select(x => (Name: x.Bestrst, Time: x.Time.Value))
          .OrderBy(x => x.Time)
          .ToList();
    }

    private void InitTimeWindowFromSelected(List<(string Name, DateTime Time)> selRaw)
    {
      _absoluteStartTime = selRaw.First().Time;

      int m = _absoluteStartTime.Minute;
      int rounded = (m / 15) * 15;

      _displayStartTime = new DateTime(
          _absoluteStartTime.Year,
          _absoluteStartTime.Month,
          _absoluteStartTime.Day,
          _absoluteStartTime.Hour,
          rounded,
          0);

      _displayEndTime = selRaw.Last().Time + TimeSpan.FromHours(1);
    }


    private class StationPoint
    {
      public string Name;
      public TimeSpan Time;
      public double X;

      public bool IsArrival;
      public bool IsDeparture;
    }

    private List<StationPoint> BuildStations(List<(string Name, DateTime Time)> raw)
    {
      // nur Einträge im Zeitfenster
      var filtered = raw
          .Where(r => r.Time >= _displayStartTime && r.Time <= _displayEndTime)
          .ToList();

      var result = new List<StationPoint>();

      int i = 0;
      while (i < filtered.Count)
      {
        string name = filtered[i].Name;

        // alle Einträge mit gleichem Stationsnamen sammeln
        var same = new List<(string Name, DateTime Time)>();
        same.Add(filtered[i]);
        int j = i + 1;

        while (j < filtered.Count && filtered[j].Name == name)
        {
          same.Add(filtered[j]);
          j++;
        }

        // Regel:
        // 1. Eintrag mit Ankunftszeit (Halt)
        // 2. sonst letzter Eintrag
        (string Name, DateTime Time) chosen;

        var halt = same.FirstOrDefault(x =>
            SelectedZug.FahrplanEintraege.Any(e =>
                e.Bestrst == x.Name &&
                e.Arrival.HasValue &&
                e.Arrival.Value == x.Time));

        if (halt.Name != null)
          chosen = halt;
        else
          chosen = same.Last();

        result.Add(new StationPoint
        {
          Name = chosen.Name,
          Time = chosen.Time - _displayStartTime
        });

        i = j;
      }

      return result;
    }

    private List<StationPoint> BuildStationsFromZug(Zug zug)
    {
      var entries = zug.FahrplanEintraege
          .OrderBy(e => e.Arrival ?? e.Departure ?? DateTime.MaxValue)
          .ToList();

      var result = new List<StationPoint>();

      int i = 0;
      while (i < entries.Count)
      {
        string name = entries[i].Bestrst;

        // alle aufeinanderfolgenden Einträge mit gleichem Stationsnamen sammeln
        var group = new List<FahrplanEintrag>();
        group.Add(entries[i]);
        int j = i + 1;
        while (j < entries.Count && entries[j].Bestrst == name)
        {
          group.Add(entries[j]);
          j++;
        }

        // Ankunfts- und Abfahrtszeit bestimmen
        DateTime? arr = group
            .Where(e => e.Arrival.HasValue && e.FplEintrag != TimeTableItemType.Helper)
            .Select(e => e.Arrival.Value)
            .OrderBy(t => t)
            .FirstOrDefault();

        DateTime? dep = group
            .Where(e => e.Departure.HasValue && e.FplEintrag != TimeTableItemType.Helper)
            .Select(e => e.Departure.Value)
            .OrderBy(t => t)
            .LastOrDefault();

        // 1) Wenn Ankunft vorhanden → StationPoint für Ankunft
        if (arr.HasValue &&
            arr.Value >= _displayStartTime &&
            arr.Value <= _displayEndTime)
        {
          result.Add(new StationPoint
          {
            Name = name,
            Time = arr.Value - _displayStartTime,
            IsArrival = true,
            IsDeparture = false
          });
        }
        else
        {
          if (arr.HasValue && ChkFullPlan.IsChecked == true && arr.Value != DateTime.MinValue)
          {
            // error in time
            _log.Error($"Error in time for train {zug.Gattung}{zug.Nummer} at station {name} at {arr.Value}");
            result.Add(new StationPoint
            {
              Name = name,
              Time = arr.Value - _displayStartTime,
              IsArrival = true,
              IsDeparture = false
            });
          }
        }

        // 2) Wenn Abfahrt vorhanden → StationPoint für Abfahrt
        //    (kann dieselbe Zeit sein wie Ankunft, dann liegen die Punkte übereinander)
        if (dep.HasValue &&
            dep.Value >= _displayStartTime &&
            dep.Value <= _displayEndTime)
        {
          result.Add(new StationPoint
          {
            Name = name,
            Time = dep.Value - _displayStartTime,
            IsArrival = false,
            IsDeparture = true
          });
        }
        else
        {
          if (dep.HasValue && ChkFullPlan.IsChecked == true && arr.Value != DateTime.MinValue)
          {
            // error in time
            _log.Error($"Error in time for train {zug.Gattung}{zug.Nummer} at station {name} at {arr.Value}");
            result.Add(new StationPoint
            {
              Name = name,
              Time = dep.Value - _displayStartTime,
              IsArrival = false,
              IsDeparture = true
            });
          }
        }

        i = j;
      }

      return result;
    }

    private void ComputeStationPositions(List<StationPoint> st)
    {
      st[0].X = LeftOffset;

      for (int i = 1; i < st.Count; i++)
      {
        var dt = st[i].Time - st[i - 1].Time;
        st[i].X = st[i - 1].X + dt.TotalMinutes * PxPerMinuteX * _ppm;
      }
    }

    private void InitTimeWindowFullPlan()
    {
      var allTimes = Zugliste
          .SelectMany(z => z.FahrplanEintraege)
          .SelectMany(e => new[]
          {
            e.Arrival,
            e.Departure
          })
          .Where(t => t.HasValue)
          .Select(t => t.Value)
          .OrderBy(t => t)
          .ToList();

      if (allTimes.Count == 0)
        return;

      var first = allTimes.First();

      var last = allTimes.Last();

      if (first.AddDays(2) < last)
      {
        first = last.AddDays(-2);
      }

      // Startzeit auf 00/15/30/45 runden
      int m = first.Minute;
      int rounded = (m / 15) * 15;

      _displayStartTime = new DateTime(
          first.Year, first.Month, first.Day,
          first.Hour, rounded, 0);

      _displayEndTime = last + TimeSpan.FromHours(1);
    }


    private void RenderBildfahrplan(bool fullPlan)
    {
      PlanCanvas.Children.Clear();
      
      // 1) Zeitfenster aus SelectedZug bestimmen
      var selRaw = GetRawEntries(SelectedZug);
      if (selRaw.Count < 2)
        return;

      if (fullPlan)
        InitTimeWindowFullPlan();
      else
        InitTimeWindowFromSelected(selRaw);

      // 2) StationPoints für SelectedZug mit Ankunft/Abfahrt-Regel
      var stations = BuildStations(selRaw);
      if (stations.Count < 2)
        return;

      ComputeStationPositions(stations);

      double _maxX = stations.Max(s => s.X);
      
      double maxY = (_displayEndTime - _displayStartTime).TotalMinutes * PxPerMinuteY *_ppm;

      
      PlanCanvas.Width = _maxX + 200; // Stationsbreite + Puffer
      PlanCanvas.Height = TopOffset + (_displayEndTime - _displayStartTime).TotalMinutes * PxPerMinuteY * _ppm + 200;

      DrawStationLines(stations);
      DrawStationLabels(stations);
      DrawTimeGrid();

      string selName = $"{SelectedZug.Gattung}{SelectedZug.Nummer}";
      //DrawTrainLine(stations, Brushes.Yellow, selName);

      // 3) Andere Züge
      foreach (var zug in Zugliste)
      {
        //if (zug == SelectedZug)
        //  continue;

        var pts = BuildStationsFromZug(zug);
        if (pts.Count < 2)
          continue;

        ComputeStationPositionsForOtherTrain(pts, stations);

        string zugName = $"{zug.Gattung}{zug.Nummer}";
        DrawTrainLine(pts, Brushes.Red, zugName, zug);
      }
    }


    private void ComputeStationPositionsForOtherTrain(List<StationPoint> train, List<StationPoint> reference)
    {
      foreach (var p in train)
      {
        var refStation = reference.FirstOrDefault(r => r.Name == p.Name);
        if (refStation != null)
          p.X = refStation.X;
        else
          p.X = -1; // Default X-Position, falls Station nicht gefunden wird
      }
    }

    private double TimeToY(TimeSpan t) => t.TotalMinutes * PxPerMinuteY * _ppm;

    // ---------------------------------------------------------
    // Zeichnen
    // ---------------------------------------------------------

    

    private void DrawTimeGrid()
    {
      DateTime t = _displayStartTime;

      while (t <= _displayEndTime)
      {
        double minutes = (t - _displayStartTime).TotalMinutes;
        double y = TopOffset + minutes * PxPerMinuteY * _ppm;

        var line = new Line
        {
          X1 = 0,
          X2 = PlanCanvas.Width,
          Y1 = y,
          Y2 = y,
          Stroke = Brushes.DarkGray,
          StrokeThickness = 2.0,
          StrokeDashArray = new DoubleCollection { 4, 4 },
          ToolTip = $"Datum/Uhrzeit: {t.ToString()}",
          Cursor = Cursors.Hand
        };

        PlanCanvas.Children.Add(line);

        var tb = new TextBlock
        {
          Text = t.ToString("HH:mm"),
          Foreground = Brushes.White,
          FontSize = 14,
          FontWeight = FontWeights.Bold
        };

        Canvas.SetLeft(tb, 5);
        Canvas.SetTop(tb, y - 10);

        PlanCanvas.Children.Add(tb);

        t = t.AddMinutes(15);
      }
    }

    private void DrawStationLabels(List<StationPoint> stations)
    {
      foreach (var p in stations)
      {
        if (p.X < 0)
          continue;

        double y = 40; // TopOffset + p.Time.TotalMinutes * _ppm;

        var tb = new TextBlock
        {
          Text = p.Name,
          Foreground = Brushes.White,
          FontSize = 14,
          FontWeight = FontWeights.Bold,
          RenderTransformOrigin = new Point(0.5, 0.5)
        };

        tb.RenderTransform = new RotateTransform(-45);

        //Kontextmenü hinzufügen
        var cm = new ContextMenu();

        var mi = new MenuItem
        {
          Header = "Gleisbelegung anzeigen",
          Tag = p.Name   // Station mitgeben
        };
        mi.Click += StationContextMenu_Click;

        cm.Items.Add(mi);
        tb.ContextMenu = cm;

        Canvas.SetLeft(tb, p.X - 6);
        Canvas.SetTop(tb, y - 6);

        PlanCanvas.Children.Add(tb);
      }
    }

    private void StationContextMenu_Click(object sender, RoutedEventArgs e)
    {
      if (sender is MenuItem mi && mi.Tag is string stationName)
      {
        // Hier öffnest du den Gleisbelegungsplan
        // Beispiel:
        TimeTableRelation timetablerelation = DataManager.Instance.SelectedTimeTableRelation;
        TimeTable timeTable = timetablerelation?.TimeTable;

        var new_zug_list = new List<Zug>();

        foreach (var train in timeTable.Trains)
        {


          try
          {
            if (train.Train != null)
            {
              new_zug_list.Add(train.Train);
            }


            if (train.Link != null)
            {
              ZugDatei zd = train.Link.ZugDatei;
              zd.Parse();
              Zug zug = zd.Root;
              if (zd.Root != null)
              {
                new_zug_list.Add(zug);
              }
            }

          }

          catch (Exception ex)
          {
            _log.Error(string.Format("show_gleisbelegung2: \nERROR {0} ", ex.ToString()));
          }
        }

        new GleisbelegungWindow(new_zug_list.ToArray(), stationName).Show();



        //var win = new GleisbelegungWindow(stationName);
        //win.Show();
      }
    }

    private void TrainContextMenu_Click(object sender, RoutedEventArgs e)
    {
      if (sender is MenuItem mi && mi.Tag is Zug zug)
      {
        DataManager.Instance.CurrentTrain = zug;
      }
    }

    //private void DrawStationLines(List<StationPoint> st)
    //{
    //  foreach (var s in st)
    //  {
    //    double y = TopOffset + TimeToY(s.Time);

    //    var line = new Line
    //    {
    //      X1 = s.X,
    //      X2 = s.X,
    //      Y1 = y - 2000,   // weit nach oben
    //      Y2 = y + 2000,   // weit nach unten
    //      Stroke = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
    //      StrokeThickness = 1.0 // / _zoom
    //    };

    //    PlanCanvas.Children.Add(line);
    //  }
    //}

    //private void DrawStationLines(List<StationPoint> stations)
    //{
    //  foreach (var p in stations)
    //  {
    //    if (p.X < 0)
    //      continue;

    //    double x = p.X;

    //    var line = new Line
    //    {
    //      X1 = x,
    //      X2 = x,
    //      Y1 = TopOffset,
    //      Y2 = TopOffset + TimeToY(_displayEndTime - _displayStartTime),
    //      Stroke = Brushes.Gray,
    //      StrokeThickness = 1.0,
    //      StrokeDashArray = new DoubleCollection { 2, 2 }
    //    };

    //    PlanCanvas.Children.Add(line);
    //  }
    //}

    private void DrawStationLines(List<StationPoint> stations)
    {
      foreach (var p in stations)
      {
        if (p.X < 0)
          continue;

        double yStart = TopOffset;
        double yEnd = TopOffset + (_displayEndTime - _displayStartTime).TotalMinutes * PxPerMinuteY * _ppm;

        var line = new Line
        {
          X1 = p.X,
          X2 = p.X,
          Y1 = yStart,
          Y2 = yEnd,
          Stroke = Brushes.Gray,
          StrokeThickness = 2.0,
          StrokeDashArray = new DoubleCollection { 4, 4 },
          ToolTip = $"Station: {p.Name}",
          Cursor = Cursors.Hand
        };

        PlanCanvas.Children.Add(line);
      }
    }



    private int GetDirection(List<StationPoint> st, int index)
    {
      if (index == 0) return +1; // Start → Richtung nach rechts
      if (index == st.Count - 1) return -1; // Ende → Richtung nach links

      double prevX = st[index - 1].X;
      double nextX = st[index + 1].X;

      return nextX > prevX ? +1 : -1;
    }

    private bool IsArrivalPoint(Zug zug, StationPoint p)
    {
      return zug.FahrplanEintraege.Any(e =>
          e.Bestrst == p.Name &&
          e.Arrival.HasValue &&
          (e.Arrival.Value - _displayStartTime) == p.Time);
    }

    private bool IsDeparturePoint(Zug zug, StationPoint p)
    {
      return zug.FahrplanEintraege.Any(e =>
          e.Bestrst == p.Name &&
          e.Departure.HasValue &&
          (e.Departure.Value - _displayStartTime) == p.Time);
    }


    private double GetAngleBetween(StationPoint a, StationPoint b)
    {
      double dx = b.X - a.X;
      double dy = (TopOffset + TimeToY(b.Time)) - (TopOffset + TimeToY(a.Time));

      return Math.Atan2(dy, dx) * 180.0 / Math.PI;
    }

    private Point GetMidPoint(StationPoint a, StationPoint b)
    {
      double x = (a.X + b.X) / 2.0;
      double y = (TopOffset + TimeToY(a.Time) + TopOffset + TimeToY(b.Time)) / 2.0;

      return new Point(x, y);
    }

    private double NormalizeAngle(double angle)
    {
      // Text soll nie auf dem Kopf stehen
      if (angle > 90) angle -= 180;
      if (angle < -90) angle += 180;
      return angle;
    }

    private string BuildTrainTooltip(Zug zug, List<StationPoint> st)
    {
      var firstDep = st.FirstOrDefault(p => p.IsDeparture);
      var lastArr = st.LastOrDefault(p => p.IsArrival);

      string startStation = firstDep?.Name ?? st.First().Name;
      string endStation = lastArr?.Name ?? st.Last().Name;

      string startTime = firstDep != null
          ? (_displayStartTime + firstDep.Time).ToString("HH:mm")
          : "-";

      string endTime = lastArr != null
          ? (_displayStartTime + lastArr.Time).ToString("HH:mm")
          : "-";

      int halte = st.Count(p => p.IsArrival && p.IsDeparture == false);

      return
          $"Zug:       {zug.Gattung}{zug.Nummer}\n" +
          $"Start:     {startStation} {startTime}\n" +
          $"Ziel:      {endStation} {endTime}\n" +
          $"Fplgruppe: {zug.FahrplanGruppe}\n" +
          $"Zuglauf:   {zug.Zuglauf}\n" +
          $"Halte:     {halte}\n";
    }

    private string BuildTrainTooltipStation(Zug zug, StationPoint sp)
    {
      

      string startStation = sp.Name;
      string startTime = sp != null
         ? (_displayStartTime + sp.Time).ToString()
         : "-";


      return
          $"Zug:       {zug.Gattung}{zug.Nummer}\n" +
          $"Uhrzeit:   {startStation} {startTime}\n" +
          $"Fplgruppe: {zug.FahrplanGruppe}\n" +
          $"Zuglauf:   {zug.Zuglauf}\n";
          
    }



    private void DrawTrainLine(List<StationPoint> st, Brush color, string zugName, Zug zug)
    {
      var poly = new Polyline
      {
        Stroke = color,
        StrokeThickness = 2.0, // / _zoom,
        ToolTip = BuildTrainTooltip(zug, st),
        Cursor = Cursors.Hand
      };

      // Hover-Effekt
      poly.MouseEnter += (s, e) =>
      {
        if (_selectedLine != poly)
          poly.StrokeThickness = 4.0; 
      };

      poly.MouseLeave += (s, e) =>
      {
        if (_selectedLine != poly)
          poly.StrokeThickness = 2.0; 
      };

      // Klick-Effekt (dauerhafte Markierung)
      poly.MouseLeftButtonDown += (s, e) =>
      {
        // alte Markierung zurücksetzen
        if (_selectedLine != null)
          _selectedLine.StrokeThickness = 2.0; 

        // neue Markierung setzen
        _selectedLine = poly;
        poly.StrokeThickness = 6.0;   // dauerhaft dick
        e.Handled = true;
      };

      poly.ToolTipOpening += (s, e) =>
      {
        if (poly.ToolTip is ToolTip tt)
        {
          tt.FontSize = 14;   // bleibt unskaliert
        }
      };



      foreach (var s in st)
      {
        if (s.X >= 0) // Nur hinzufügen, wenn die X-Position gültig ist
        {
          //double y = TopOffset + p.Time.TotalMinutes * _ppm;
          //poly.Points.Add(new Point(
          //    s.X,
          //    TopOffset + TimeToY(s.Time)));
          double y = TopOffset + s.Time.TotalMinutes * PxPerMinuteY * _ppm;
          poly.Points.Add(new Point(s.X, y));
        }
      }

      //Kontextmenü hinzufügen
      var cm = new ContextMenu();

      var mi = new MenuItem
      {
        Header = "Zug auswählen",
        Tag = zug   
      };
      mi.Click += TrainContextMenu_Click;

      cm.Items.Add(mi);
      poly.ContextMenu = cm;

      PlanCanvas.Children.Add(poly);

      for (int i = 0; i < st.Count; i++)
      {
        var p = st[i];
        if (p.X < 0)
          continue;
        //double y = TopOffset + TimeToY(p.Time);
        double y = TopOffset + p.Time.TotalMinutes * PxPerMinuteY * _ppm;

        int dir = GetDirection(st, i);

        // Minuten extrahieren
        string minute = (_displayStartTime + p.Time).ToString("mm");

        var tb = new TextBlock
        {
          Text = minute,
          Foreground = Brushes.White,
          FontSize = 12,
          ToolTip = BuildTrainTooltipStation(zug, p),
        };

        //tb.LayoutTransform = new ScaleTransform(1.0 / _zoom, 1.0 / _zoom);

        // Position:
        // Ankunft → unter der Linie auf der Seite von der der Zug kommt
        // Abfahrt → über der Linie auf der Seite in die der Zug fährt

        double offsetX = dir > 0 ? -18 : +2;

        // Ankunftszeit (wenn dieser Punkt eine Ankunft ist)
        if (p.IsArrival)
        {
          Canvas.SetLeft(tb, p.X + offsetX);
          Canvas.SetTop(tb, y - 18); // über der Linie
          PlanCanvas.Children.Add(tb);
          
        }

        // Abfahrtszeit (wenn dieser Punkt eine Abfahrt ist)
        if (p.IsDeparture)
        {
          offsetX = dir > 0 ? +2 : -18;
          Canvas.SetLeft(tb, p.X + offsetX);
          Canvas.SetTop(tb, y + 2); // unter der Linie
          PlanCanvas.Children.Add(tb);
        }
      }

      // --- Zugnummer zwischen erster Abfahrt und nächstem Punkt ---
      int depIndex = st.FindIndex(p => p.IsDeparture && p.X > 0);

      if (depIndex >= 0 && depIndex < st.Count - 1)
      {
        var p1 = st[depIndex];
        var p2 = st[depIndex + 1];

        if (p2.X >= 0)
        {

          double y1 = TopOffset + TimeToY(p1.Time);
          double y2 = TopOffset + TimeToY(p2.Time);

          // Mittelpunkt
          double midX = (p1.X + p2.X) / 2.0;
          double midY = (y1 + y2) / 2.0;

          // Winkel der Linie
          double dx = p2.X - p1.X;
          double dy = y2 - y1;
          double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;

          if (angle > 90) midY -= 10;
          if (angle < -90) midY -= 10;

          angle = NormalizeAngle(angle);

          var tb = new TextBlock
          {
            Text = zugName,
            Foreground = color,
            FontWeight = FontWeights.Bold,
            Background = new SolidColorBrush(Color.FromArgb(160, 0, 0, 0)),
            Padding = new Thickness(3, 1, 3, 1),
            FontSize = 12,
            ToolTip = BuildTrainTooltip(zug, st),
          };

          tb.RenderTransform = new RotateTransform(angle);
          tb.RenderTransformOrigin = new Point(0.5, 0.5);

          Canvas.SetLeft(tb, midX);
          Canvas.SetTop(tb, midY);

          //PlanCanvas.Children.Add(tb);
          PlanCanvas.Children.Add(tb);
        }
      }
    }

    // ---------------------------------------------------------
    // Zoom / Pan
    // ---------------------------------------------------------

    //private const double ZoomStep = 0.1;

    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
      if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
      {
        e.Handled = true;

        double oldPPM = _ppm;

        if (e.Delta > 0)
          _ppm *= 1.1;
        else
          _ppm /= 1.1;

        _ppm = Math.Clamp(_ppm, 0.1, 20.0);

        Point mousePos = e.GetPosition(Scroll);

        double oldOffset = Scroll.VerticalOffset;
        double factor = _ppm / oldPPM;

        double newOffset =
            (mousePos.Y + oldOffset - TopOffset) * factor
            + TopOffset
            - mousePos.Y;

        Scroll.ScrollToVerticalOffset(newOffset);

        double oldHOffset = Scroll.HorizontalOffset;

        double newHOffset =
            (mousePos.X + oldHOffset - LeftOffset) * factor
            + LeftOffset
            - mousePos.X;

        Scroll.ScrollToHorizontalOffset(newHOffset);

        // Neu zeichnen
        RenderBildfahrplan(fullPlan: ChkFullPlan.IsChecked == true);
      }
    }

    //protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    //{
    //  base.OnPreviewMouseDown(e);

    //  if (e.MiddleButton == MouseButtonState.Pressed ||
    //      e.RightButton == MouseButtonState.Pressed)
    //  {
    //    _isPanning = true;
    //    _lastPanPoint = e.GetPosition(this);
    //    Cursor = Cursors.Hand;
    //    CaptureMouse();
    //  }
    //}

    //protected override void OnPreviewMouseMove(MouseEventArgs e)
    //{
    //  base.OnPreviewMouseMove(e);

    //  if (_isPanning)
    //  {
    //    var pos = e.GetPosition(this);
    //    var delta = pos - _lastPanPoint;

    //    PanTransform.X += delta.X;
    //    PanTransform.Y += delta.Y;

    //    _lastPanPoint = pos;
    //  }
    //}

    //protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
    //{
    //  base.OnPreviewMouseUp(e);

    //  if (_isPanning)
    //  {
    //    _isPanning = false;
    //    Cursor = Cursors.Arrow;
    //    ReleaseMouseCapture();
    //  }
    //}

    //private void RootGrid_MouseDown(object sender, MouseButtonEventArgs e)
    //{
    //  if (e.RightButton == MouseButtonState.Pressed)
    //  {
    //    _isPanning = true;
    //    _lastPanPoint = e.GetPosition(this);
    //    RootGrid.CaptureMouse();
    //  }
    //}

    //private void RootGrid_MouseMove(object sender, MouseEventArgs e)
    //{
    //  if (_isPanning)
    //  {
    //    Point pos = e.GetPosition(this);

    //    double dx = pos.X - _lastPanPoint.X;
    //    double dy = pos.Y - _lastPanPoint.Y;

    //    Scroll.ScrollToHorizontalOffset(Scroll.HorizontalOffset - dx);
    //    Scroll.ScrollToVerticalOffset(Scroll.VerticalOffset - dy);

    //    _lastPanPoint = pos;
    //  }
    //}

    //private void RootGrid_MouseUp(object sender, MouseButtonEventArgs e)
    //{
    //  if (_isPanning)
    //  {
    //    _isPanning = false;
    //    RootGrid.ReleaseMouseCapture();
    //  }
    //}


  }
}
