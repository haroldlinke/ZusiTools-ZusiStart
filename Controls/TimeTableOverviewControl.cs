using Sovoma;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZusiKlassenLib2.TimeTable;
using ZusiStart.Data;
using System.Globalization;
using System.Collections.ObjectModel;
using Sovoma.WPF;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2;

namespace ZusiStart.Controls
{
  //=========================================================================
  public class SelectedTimeTableChangedEventArgs : EventArgs
  {
    public DateTime SelectedDate { get; private set; }
    public List<TimeTable> TimeTables { get; private set; }
    public int PreferredSelection { get; private set; }

    //---------------------------------------------------------------------
    public SelectedTimeTableChangedEventArgs(DateTime date, List<TimeTable> timeTables, int preferredSelection)
    {
      SelectedDate = date;
      TimeTables = timeTables;
      PreferredSelection = preferredSelection;
    }
  }

  //=========================================================================
  public delegate void SelectedTimeTableChangedEventHandler(object sender, SelectedTimeTableChangedEventArgs e);

  //=========================================================================
  public class TimeTableInfo
  {
    public DateTime? Date { get; private set; }
    public string Name { get; set; }

    //---------------------------------------------------------------------
    public TimeTableInfo(TimeTable timeTable)
    {
      if (timeTable.StartTime is DateTime st)
      {
        Date = st.Date;
      }

      ZusiDocumentBase doc = timeTable.GetDocument();

      if (!doc.Filename.StartsWith(Zusi.DataPath[DataPathType.Official]))
      {
        if (doc.Filename.StartsWith(Zusi.DataPath[DataPathType.DataDir]))
          Name = System.IO.Path.GetFileNameWithoutExtension(doc.Filename) + " (private)";
        else
        {
          if (doc.Filename.StartsWith(Zusi.DataPath[DataPathType.OfficialProf]))
            Name = System.IO.Path.GetFileNameWithoutExtension(doc.Filename) + " (professional)";
          else
            Name = System.IO.Path.GetFileNameWithoutExtension(doc.Filename) + " (professional-private)";
        }
      }
      else
        Name = System.IO.Path.GetFileNameWithoutExtension(doc.Filename);
    }
  }

  //=========================================================================
  /// <summary>
  /// </summary>
  public class TimeTableOverviewControl : Control
  {
    #region private fields

    private List<ZusiKlassenLib2.Fahrplan.Zug> _trains;
    private readonly ObservableCollection<DateTime> _dates = new();
    private readonly ObservableCollection<TimeTableInfo> _infos = new();
    private int _preferredSelection;

    #endregion

    #region public properties

    //---------------------------------------------------------------------
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        "CornerRadius",
        typeof(CornerRadius),
        typeof(TimeTableOverviewControl),
        new PropertyMetadata(new CornerRadius(0)));
    [Category("Darstellung")]
    public CornerRadius CornerRadius
    {
      get { return (CornerRadius)GetValue(CornerRadiusProperty); }
      set { SetValue(CornerRadiusProperty, value); }
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _countFreightTrainsKey = DependencyProperty.RegisterReadOnly(
        "CountFreightTrains",
        typeof(int),
        typeof(TimeTableOverviewControl),
        new PropertyMetadata(0));
    public static readonly DependencyProperty CountFreightTrainsProperty = _countFreightTrainsKey.DependencyProperty;
    public int CountFreightTrains
    {
      get { return (int)GetValue(CountFreightTrainsProperty); }
      private set { SetValue(_countFreightTrainsKey, value); }
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _countPassengerTrainsKey = DependencyProperty.RegisterReadOnly(
        "CountPassengerTrains",
        typeof(int),
        typeof(TimeTableOverviewControl),
        new PropertyMetadata(0));
    public static readonly DependencyProperty CountPassengerTrainsProperty = _countPassengerTrainsKey.DependencyProperty;
    public int CountPassengerTrains
    {
      get { return (int)GetValue(CountPassengerTrainsProperty); }
      private set { SetValue(_countPassengerTrainsKey, value); }
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _countTimeTablesKey = DependencyProperty.RegisterReadOnly(
        "CountTimeTables",
        typeof(int),
        typeof(TimeTableOverviewControl),
        new PropertyMetadata(0));
    public static readonly DependencyProperty CountTimeTablesProperty = _countTimeTablesKey.DependencyProperty;
    public int CountTimeTables
    {
      get { return (int)GetValue(CountTimeTablesProperty); }
      private set { SetValue(_countTimeTablesKey, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
        "IsSelected",
        typeof(bool),
        typeof(TimeTableOverviewControl),
        new PropertyMetadata(false, OnIsSelectedChanged));
    public bool IsSelected
    {
      get { return (bool)GetValue(IsSelectedProperty); }
      set { SetValue(IsSelectedProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(
        "SelectedDate",
        typeof(DateTime?),
        typeof(TimeTableOverviewControl),
        new PropertyMetadata(null, OnSelectedDateChanged));
    public DateTime? SelectedDate
    {
      get { return (DateTime?)GetValue(SelectedDateProperty); }
      set { SetValue(SelectedDateProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty SelectedTimeTableInfoProperty = DependencyProperty.Register(
        "SelectedTimeTableInfo",
        typeof(TimeTableInfo),
        typeof(TimeTableOverviewControl),
        new PropertyMetadata(null, OnSelectedTimeTableInfoChanged));
    public TimeTableInfo SelectedTimeTableInfo
    {
      get { return (TimeTableInfo)GetValue(SelectedTimeTableInfoProperty); }
      set { SetValue(SelectedTimeTableInfoProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        "Source",
        typeof(TimeTableGroup),
        typeof(TimeTableOverviewControl),
        new PropertyMetadata(null, OnSourceChanged));
    public TimeTableGroup Source
    {
      get { return (TimeTableGroup)GetValue(SourceProperty); }
      set { SetValue(SourceProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        "Title",
        typeof(string),
        typeof(TimeTableOverviewControl),
        new PropertyMetadata(null));
    public string Title
    {
      get { return (string)GetValue(TitleProperty); }
      set { SetValue(TitleProperty, value); }
    }

    //---------------------------------------------------------------------
    public ObservableCollection<DateTime> Dates { get { return _dates; } }
    public ObservableCollection<TimeTableInfo> TimeTableInfos { get { return _infos; } }

    #endregion

    #region events

    //---------------------------------------------------------------------
    public event SelectedTimeTableChangedEventHandler SelectedTimeTableChanged;

    #endregion

    //---------------------------------------------------------------------
    static TimeTableOverviewControl()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeTableOverviewControl), new FrameworkPropertyMetadata(typeof(TimeTableOverviewControl)));
    }

    //---------------------------------------------------------------------
    public void RaiseSelectedTimeTableChanged()
    {
      OnSelectedDateChanged(SelectedDate);
    }

    //---------------------------------------------------------------------
    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as TimeTableOverviewControl)?.OnIsSelectedChanged((bool)e.NewValue);
    }

    //---------------------------------------------------------------------
    private void OnIsSelectedChanged(bool value)
    {
      if (!value)
      {
        SelectedDate = null;
      }
    }

    //---------------------------------------------------------------------
    private void OnSelectedTimeTableChanged(DateTime date, List<TimeTable> timeTables, int preferredSelection)
    {
      DataManager.Instance.SkipTimeTableSelectionUpdate += 1;
      SelectedTimeTableChanged?.Invoke(this, new SelectedTimeTableChangedEventArgs(date, timeTables, preferredSelection));
    }

    //---------------------------------------------------------------------
    private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      TimeTableOverviewControl t = d as TimeTableOverviewControl;
      t?.OnSelectedDateChanged((DateTime?)e.NewValue);
    }

    //---------------------------------------------------------------------
    private void OnSelectedDateChanged(DateTime? value)
    {
      if (value != null)
      {
        OnSelectedTimeTableChanged(value.Value,
        Source.Members.FindAll(tt => tt.StartTime != null && tt.StartTime.Value.Date == value.Value), _preferredSelection);
        //DataManager.Instance.SkipTimeTableSelectionUpdate = true;
        //Source.Members.FindAll(tt => tt.StartTime != null), _preferredSelection);

        _preferredSelection = 0;
      }
    }

    //---------------------------------------------------------------------
    private static void OnSelectedTimeTableInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as TimeTableOverviewControl)?.OnSelectedTimeTableInfoChanged((TimeTableInfo)e.NewValue);
    }

    //---------------------------------------------------------------------
    private void OnSelectedTimeTableInfoChanged(TimeTableInfo value)
    {
      if (value != null)
      {
        IsSelected = true;

        var members = Source.Members.FindAll(tt => tt.StartTime != null && tt.StartTime.Value.Date == value.Date.Value);
        //var members = Source.Members.FindAll(tt => tt.StartTime != null);
        //DataManager.Instance.SkipTimeTableSelectionUpdate = true;
        _preferredSelection = members.IndexOf(members.FirstOrDefault(tt =>
        {
          ZusiDocumentBase doc = tt.GetDocument();
          return string.Compare(System.IO.Path.GetFileNameWithoutExtension(doc.Filename), value.Name, true) == 0;
        }));
       

        if (SelectedDate != value.Date)
        {
          SelectedDate = value.Date;
        }
        else
        {
          OnSelectedTimeTableChanged(value.Date.Value, members, _preferredSelection);
          //_preferredSelection = 0;
        }
      }
    }

    //---------------------------------------------------------------------
    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      TimeTableOverviewControl t = d as TimeTableOverviewControl;
      t?.OnSourceChanged(e.NewValue as TimeTableGroup);
    }

    //---------------------------------------------------------------------
    private void OnSourceChanged(TimeTableGroup value)
    {
      string[] ss = value.GroupID.Split('\\');
      if (ss.Length >= 2)
      {
        Title = string.Format("[{0}] {1}", ss[0], ss[1].Replace('_', '-'));
      }
      else
      {
        Title = string.Format("[inoffiziell] {0}", value.GroupID.Replace('\\', '-').Replace('_', '-'));
      }

      CountTimeTables = value.Members.Count;

      int nf = 0;
      int np = 0;
      List<DateTime> tmpDates = new();
      List<string> tmpNames = new();
      value.Members.ForEach(tt =>
      {
        if (tt.StartTime != null)
        {
          DateTime dt = tt.StartTime.Value.Date;
          if (!tmpDates.Contains(dt))
          {
            tmpDates.Add(dt);
          }
        }

        _infos.Add(new TimeTableInfo(tt));
#if SEQ
                _trains = DataManager.Instance.AllTrains.FindAll(t => t.BelongsToTimeTable == tt.ID).ToList();
#else
        _trains = DataManager.Instance.AllTrains.Where(t => t.BelongsToTimeTable == tt.ID).ToList();
#endif

        int n = _trains.Count;
        int p = _trains.FindAll(t => t.Type == ZusiKlassenLib2.Fahrplan.TrainType.Passenger).Count;
        nf += (n - p);
        np += p;
      });

      CountFreightTrains = nf;
      CountPassengerTrains = np;

      _dates.Clear();
      tmpDates.Sort();
      tmpDates.ForEach(dt => _dates.Add(dt));
    }
  }

  //=========================================================================
  public class DateConverter : IValueConverter
  {
    //---------------------------------------------------------------------
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value is DateTime dt ? dt.ToString("dd.MM.yyyy") : "-";
    }

    //---------------------------------------------------------------------
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
