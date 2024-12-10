#define NO_CACHE

//using CefSharp.DevTools.Page;
using log4net;
using Microsoft.Web.WebView2.WinForms;
using Sovoma;
using Sovoma.WPF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using ZusiKlassenLib;
using ZusiKlassenLib.Common;
using ZusiKlassenLib.Fahrplan;
using ZusiKlassenLib.TimeTable;
using ZusiKlassenLib.Vehicle;
using ZusiMeterGaugesLib.Gauges;
using ZusiStart.Dialogs;

//using ZusiPicLib;
using ZusiStart.Miscellaneous;

namespace ZusiStart.Data
{
  //=========================================================================
  public enum LoaderType
  {
    LoadComplete,
    LoadFromCache,
    LoadVehicleGroups
  }

  //=========================================================================
  public class NotifyDataLoadStartedEventArgs : EventArgs
  {
    public LoaderType LoaderType { get; private set; }

    //---------------------------------------------------------------------
    public NotifyDataLoadStartedEventArgs(LoaderType loaderType)
    {
      LoaderType = loaderType;
    }
  }

  public delegate void NotifyDataLoadStartedEventHandler(object sender, NotifyDataLoadStartedEventArgs e);

#if false
    //=========================================================================
    public class ProgressChangedEventArgs
    {
        public double Step { get; private set; }

        //---------------------------------------------------------------------
        public ProgressChangedEventArgs(double step)
        {
            Step = step;
        }
    }

    public delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs e);
#endif

  //=========================================================================
  public class TimeTableGroup
  {
    public string GroupID { get; set; }
    public List<TimeTable> Members { get; set; }
  }

  //=========================================================================
  public class DataManager : DependencyObject, IDisposable
  {
#if false
        //---------------------------------------------------------------------
        private class TrainGroupConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return ((string)value).Substring(0, 1);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
#endif

    //---------------------------------------------------------------------
    private class DataLoaderResult
    {
      public List<TimeTableGroup> TimeTableGroups { get; set; }
      public int CountFreightTrains { get; set; }
      public int CountWindowTrains { get; set; }
      public IEnumerable<VehicleContainer> FilteredVehicles { get; set; }
    }

    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private static DataManager? __instance = null;
    //private static readonly TrainGroupConverter _trainGroupConverter = new TrainGroupConverter();
    public DataLoaderWindow dataLoaderWindow { get; set; }
    public Microsoft.Web.WebView2.Wpf.WebView2 webview{ get; set; }
    private static readonly string[] __foldersToExclude = new string[]
    {
            @"ZusiRotstift"
    };

    private readonly string[] _foldersToExclude = Array.Empty<string>();
    private string[] _blackListedClasses = Array.Empty<string>();
    private readonly ObservableCollection<TimeTable> _allTimeTables = new();
    private readonly ObservableCollection<TimeTableGroup> _groupedTimeTables = new();


#if SEQ
        private readonly ObservableCollection<Zug> _allTrains = new();
#else
    private readonly ConcurrentBag<Zug> _allTrains = new();
#endif


    private readonly ObservableCollection<TimeTableRelation> _relations = new();
    private readonly ObservableCollection<TrainsViewModel> _timeTableTrains = new();
    private readonly ObservableCollection<Notch> _trainKindNotches = new()
        {
            new Notch() { Label = "beliebig" },
            new Notch() { Label = "Lok" },
            new Notch() { Label = "Triebwagen" }
        };
    private readonly ObservableCollection<FoundTimeTableViewModel> _foundTimeTables = new();
    private readonly ObservableCollection<TrainsViewModel> _foundTrains = new();
    private List<VehicleGroup> _allVehicles = new();
    private readonly List<FahrzeugVariante> _allVariants = new();
    private readonly RecentTrainsCollection _recentTrains = new();
    private AutoResetEvent _queueFinished = new(false);
    private AutoResetEvent _waitTimeTables = new(false);
    private AutoResetEvent _waitVehicleData = new(false);
    private AutoResetEvent _waitVehicleFilter = new(false);

    private int _nQueuedItems;

    //---------------------------------------------------------------------
    public static readonly DependencyProperty FrictionPresetProperty = DependencyProperty.Register(
        "FrictionPreset",
        typeof(FrictionPreset),
        typeof(DataManager),
        new PropertyMetadata(FrictionPreset.Dry, OnFrictionPresetChanged));
    public FrictionPreset FrictionPreset
    {
      get => (FrictionPreset)GetValue(FrictionPresetProperty);
      set => SetValue(FrictionPresetProperty, value);
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty IsDecoTrainsAllowedProperty = DependencyProperty.Register(
        "IsDecoTrainsAllowed",
        typeof(bool),
        typeof(DataManager),
        new PropertyMetadata(false, OnIsDecoTrainsAllowedChanged));
    public bool IsDecoTrainsAllowed
    {
      get => (bool)GetValue(IsDecoTrainsAllowedProperty);
      set => SetValue(IsDecoTrainsAllowedProperty, value);
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _countTimeTablesKey = DependencyProperty.RegisterReadOnly(
        "CountTimeTables",
        typeof(int),
        typeof(DataManager),
        new PropertyMetadata(0));
    public static readonly DependencyProperty CountTimeTablesProperty = _countTimeTablesKey.DependencyProperty;
    public int CountTimeTables
    {
      get { return (int)GetValue(CountTimeTablesProperty); }
      private set { SetValue(_countTimeTablesKey, value); }
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _countFilteredTrainsKey = DependencyProperty.RegisterReadOnly(
        "CountFilteredTrains",
        typeof(int),
        typeof(DataManager),
        new PropertyMetadata(0));
    public static readonly DependencyProperty CountFilteredTrainsProperty = _countFilteredTrainsKey.DependencyProperty;
    public int CountFilteredTrains
    {
      get { return (int)GetValue(CountFilteredTrainsProperty); }
      private set { SetValue(_countFilteredTrainsKey, value); }
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _countFreightTrainsKey = DependencyProperty.RegisterReadOnly(
        "CountFreightTrains",
        typeof(int),
        typeof(DataManager),
        new PropertyMetadata(0));
    public static readonly DependencyProperty CountFreightTrainsProperty = _countFreightTrainsKey.DependencyProperty;
    public int CountFreightTrains
    {
      get { return (int)GetValue(CountFreightTrainsProperty); }
      private set { SetValue(_countFreightTrainsKey, value); }
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _countFilteredVehiclesKey = DependencyProperty.RegisterReadOnly(
        "CountFilteredVehicles",
        typeof(int),
        typeof(DataManager),
        new PropertyMetadata(0));
    public static readonly DependencyProperty CountFilteredVehiclesProperty = _countFilteredVehiclesKey.DependencyProperty;
    public int CountFilteredVehicles
    {
      get { return (int)GetValue(CountFilteredVehiclesProperty); }
      private set { SetValue(_countFilteredVehiclesKey, value); }
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _countWindowTrainsKey = DependencyProperty.RegisterReadOnly(
        "CountWindowTrains",
        typeof(int),
        typeof(DataManager),
        new PropertyMetadata(0));
    public static readonly DependencyProperty CountWindowTrainsProperty = _countWindowTrainsKey.DependencyProperty;
    public int CountWindowTrains
    {
      get { return (int)GetValue(CountWindowTrainsProperty); }
      private set { SetValue(_countWindowTrainsKey, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty CurrentTrainProperty = DependencyProperty.Register(
        "CurrentTrain",
        typeof(Zug),
        typeof(DataManager),
        new PropertyMetadata(null));
    public Zug? CurrentTrain
    {
      get { return (Zug)GetValue(CurrentTrainProperty); }
      set { SetValue(CurrentTrainProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty CurrentVehicleContainerProperty = DependencyProperty.Register(
        "CurrentVehicleContainer",
        typeof(VehicleContainer),
        typeof(DataManager),
        new PropertyMetadata(null, OnCurrentVehicleContainerChanged));
    public VehicleContainer CurrentVehicleContainer
    {
      get { return (VehicleContainer)GetValue(CurrentVehicleContainerProperty); }
      set { SetValue(CurrentVehicleContainerProperty, value); }
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _filteredVehiclesKey = DependencyProperty.RegisterReadOnly(
        "FilteredVehicles",
        typeof(IEnumerable<VehicleContainer>),
        typeof(DataManager),
        new PropertyMetadata(null));
    public static readonly DependencyProperty FilteredVehiclesProperty = _filteredVehiclesKey.DependencyProperty;
    public IEnumerable<VehicleContainer> FilteredVehicles
    {
      get { return (IEnumerable<VehicleContainer>)GetValue(FilteredVehiclesProperty); }
      private set { SetValue(_filteredVehiclesKey, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty FoundTrainProperty = DependencyProperty.Register(
        "FoundTrain",
        typeof(Zug),
        typeof(DataManager),
        new PropertyMetadata(null));
    public Zug FoundTrain
    {
      get { return (Zug)GetValue(FoundTrainProperty); }
      set { SetValue(FoundTrainProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty SelectedRecentTrainProperty = DependencyProperty.Register(
        "SelectedRecentTrain",
        typeof(RecentTrain),
        typeof(DataManager),
        new PropertyMetadata(null, OnSelectedRecentTrainChanged));
    public RecentTrain SelectedRecentTrain
    {
      get { return (RecentTrain)GetValue(SelectedRecentTrainProperty); }
      set { SetValue(SelectedRecentTrainProperty, value); }
    }

    //---------------------------------------------------------------------
    public static DependencyProperty SelectedTimeTableRelationProperty = DependencyProperty.Register(
        "SelectedTimeTableRelation",
        typeof(TimeTableRelation),
        typeof(DataManager),
        new PropertyMetadata(TimeTableRelation.DummyRelation, OnSelectedTimeTableChanged));
    public TimeTableRelation? SelectedTimeTableRelation
    {
      get { return (TimeTableRelation)GetValue(SelectedTimeTableRelationProperty); }
      set { SetValue(SelectedTimeTableRelationProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty TractionKindProperty = DependencyProperty.Register(
        "TractionKind",
        typeof(uint),
        typeof(DataManager),
        new PropertyMetadata(0u, OnTractionKindChanged));
    public uint TractionKind
    {
      get { return (uint)GetValue(TractionKindProperty); }
      set { SetValue(TractionKindProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty TrainKindProperty = DependencyProperty.Register(
        "TrainKind",
        typeof(uint),
        typeof(DataManager),
        new PropertyMetadata(0u, OnFilterChanged));
    public uint TrainKind
    {
      get { return (uint)GetValue(TrainKindProperty); }
      set { SetValue(TrainKindProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty EpocheProperty = DependencyProperty.Register(
        "Epoche",
        typeof(uint),
        typeof(DataManager),
        new PropertyMetadata(0u, OnFilterChanged));
    public uint Epoche
    {
      get { return (uint)GetValue(EpocheProperty); }
      set { SetValue(EpocheProperty, value); }
    }

    //---------------------------------------------------------------------
    public static DataManager Instance
    {
      get
      {
        if (__instance == null)
        {
          __instance = new DataManager();
        }
        return __instance;
      }
    }

    public List<FahrzeugVariante> AllVariants { get => AllVariants1; }
#if SEQ
        public ObservableCollection<Zug> AllTrains { get { return _allTrains; } }
#else
    public ConcurrentBag<Zug> AllTrains { get { return _allTrains; } }
#endif
    public ObservableCollection<TimeTableGroup> GroupedTimeTables { get { return _groupedTimeTables; } }
    public ObservableCollection<TimeTableRelation> Relations { get { return _relations; } }
    public ObservableCollection<TrainsViewModel> FoundTrains { get { return _foundTrains; } }
    public ObservableCollection<Notch> TrainKindNotches => _trainKindNotches;
    public ObservableCollection<FoundTimeTableViewModel> FoundTimeTables { get => _foundTimeTables; }
    public ObservableCollection<TrainsViewModel> TimeTableTrains { get => _timeTableTrains; }
    public RecentTrainsCollection RecentTrains { get => _recentTrains; }

    public List<FahrzeugVariante> AllVariants1 => _allVariants;

    public event EventHandler DecoTrainsAllowed;
    public event EventHandler SelectedTimeTableChanged;
    public event NotifyDataLoadStartedEventHandler NotifyDataLoadStarted;
    public event EventHandler NotifyDataLoadCompleted;
    //public event ProgressChangedEventHandler ProgressChanged;

    //---------------------------------------------------------------------
    public static void UpdateTimeTableRelations(List<TimeTable> timeTables, int preferredSelection)
    {
      Instance?.UpdateTimeTableRelations_impl(timeTables, preferredSelection);
    }

    //---------------------------------------------------------------------
    private DataManager()
    {
      double friction = ZusiSettings.Friction;
      if (friction >= 0.4)
      {
        FrictionPreset = FrictionPreset.Dry;
      }
      else if (friction >= 0.2)
      {
        FrictionPreset = FrictionPreset.Wet;
      }
      else if (friction >= 0.1)
      {
        FrictionPreset = FrictionPreset.Misty;
      }
      else
      {
        FrictionPreset = FrictionPreset.Icy;
      }

      _foldersToExclude = ArrayEx.SafeConcat(__foldersToExclude, GetExcludedFolders().ToArray());
    }

    //---------------------------------------------------------------------
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    //---------------------------------------------------------------------
    public void InitializeData()
    {
#if true || !DEBUG
      LoadTimeTableDataAsync();
      LoadVehicleDataAsync();
      FinishLoadingAsync();
#else
            OnNotifyDataLoadCompleted();
#endif
    }

    //---------------------------------------------------------------------
    public void ClearRecentTrains()
    {
      _recentTrains.Clear();
    }

    //---------------------------------------------------------------------
    public void SetFoundTimeTable(FoundTimeTable? value)
    {
      _foundTrains.Clear();
      if (value != null)
      {
        foreach (TrainsViewModel tvm in TrainsViewModel.BuildViewModel(value.Trains).Children)
        {
          _foundTrains.Add(tvm);
        }
      }
    }

    //---------------------------------------------------------------------
    public TimeTable GetTimeTableOfTrain(Zug zug)
    {
      return _allTimeTables.FirstOrDefault(tt => tt.ID == zug.BelongsToTimeTable);
    }

    //---------------------------------------------------------------------
    public bool SearchTrain(string number)
    {
      _foundTimeTables.Clear();
      CurrentTrain = null;

      string n = number.Replace(" ", "").Trim().ToLower();

      bool show_decotrains = true; // IsDecoTrainsAllowed;

      var trains = _allTrains.Where(z =>
      {
        return (string.Compare(n, (z.Nummer).Replace(" ", "").Trim(), true) == 0 ||
                  string.Compare(n, (z.Gattung + z.Nummer).Replace(" ", "").Trim(), true) == 0);
      })
      .GroupBy(g => g.BelongsToTimeTable)
      .Select(g => new FoundTimeTable()
      {
        TimeTable = _allTimeTables.First(tt => tt.ID == g.Key),
        Trains = g.ToList()
      });

      FoundTimeTableViewModel vm = FoundTimeTableViewModel.BuildViewModel(trains);
      vm.Children.ForEach(c => _foundTimeTables.Add(c));

      if (vm.Children.Count == 1)
      {
        if (vm.Children[0].Children.Count == 1)
        {
          FoundTimeTableViewModel sttvm = vm.Children[0].Children[0];
          FoundTimeTable tt = sttvm.Object;
          if (tt.Trains.Count == 1)
          {
            sttvm.IsSelected = true;
            SetFoundTimeTable(tt);
            CurrentTrain = tt.Trains[0];
          }
        }
      }

      if (_foundTimeTables.Count == 0)
      {
        SetFoundTimeTable(null);
      }

      return _foundTimeTables.Count > 0;
    }

    //---------------------------------------------------------------------
    private void Dispose(bool disposing)
    {
      if (disposing)
      {
        Disposable.Dispose(ref _queueFinished);
        Disposable.Dispose(ref _waitTimeTables);
        Disposable.Dispose(ref _waitVehicleData);
        Disposable.Dispose(ref _waitVehicleFilter);

        __instance = null;
      }
    }

    //---------------------------------------------------------------------
    private static void OnFrictionPresetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as DataManager)?.OnFrictionPresetChanged((FrictionPreset)e.NewValue);
    }

    //---------------------------------------------------------------------
    private void OnFrictionPresetChanged(FrictionPreset value)
    {
      ZusiSettings.SetFrictionPreset(value);
    }

    //---------------------------------------------------------------------
    private static void OnIsDecoTrainsAllowedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as DataManager)?.OnIsDecoTrainsAllowedChanged(/*(bool)e.NewValue*/);
    }

    //---------------------------------------------------------------------
    private void OnIsDecoTrainsAllowedChanged(/*bool value*/)
    {
      DecoTrainsAllowed?.Invoke(this, EventArgs.Empty);
    }

    //---------------------------------------------------------------------
    private static void OnSelectedTimeTableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      DataManager dm = d as DataManager;
      dm?.OnSelectedTimeTableChanged((TimeTableRelation)e.NewValue);
    }

    //---------------------------------------------------------------------
    private void OnSelectedTimeTableChanged(TimeTableRelation value)
    {
      _timeTableTrains.Clear();
      if (value != null)
      {
        foreach (TrainsViewModel tvm in TrainsViewModel.BuildViewModel(value.TimeTable).Children)
        {
          _timeTableTrains.Add(tvm);
        }
      }
      SelectedTimeTableChanged?.Invoke(this, EventArgs.Empty);
    }

    //---------------------------------------------------------------------
    private static void OnSelectedRecentTrainChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as DataManager)?.OnSelectedRecentTrainChanged((RecentTrain)e.NewValue);
    }

    //---------------------------------------------------------------------
    private void OnSelectedRecentTrainChanged(RecentTrain value)
    {
      if (value != null)
      {

      }
    }

    //---------------------------------------------------------------------
    private void OnNotifyDataLoadStarted(LoaderType loaderType)
    {
      Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action<LoaderType>((t) =>
      {
        NotifyDataLoadStarted?.Invoke(this, new NotifyDataLoadStartedEventArgs(t));
      }), loaderType);
    }

    //---------------------------------------------------------------------
    private void OnNotifyDataLoadCompleted()
    {
      NotifyDataLoadCompleted?.Invoke(this, EventArgs.Empty);
    }

    //---------------------------------------------------------------------
    private async void FinishLoadingAsync()
    {
      DataLoaderResult result = await Task.Run(() =>
      {
        return WaitHandle.WaitAll(new WaitHandle[] { _waitTimeTables, _waitVehicleData }) ? RebuildFilter() : null;
      });



      //DummyWindow dummywindow=new DummyWindow();
      //dummywindow.Show();

      
      dataLoaderWindow.SetLoaderType(LoaderType.LoadVehicleGroups);
      dataLoaderWindow.UpdateLayout();



      _groupedTimeTables.Clear();
      
      result.TimeTableGroups.ForEach(t => _groupedTimeTables.Add(t));
      
      

      CountTimeTables = _allTimeTables.Count;
      CountFreightTrains = result.CountFreightTrains;
      CountWindowTrains = result.CountWindowTrains;

      FilteredVehicles = result.FilteredVehicles;
      CountFilteredVehicles = FilteredVehicles.Count();

      //CountFilteredVehicles = result.FilteredVehicles.Count();
      //dummywindow.Close();

      _recentTrains.LoadFromFile(_allTrains, _allTimeTables);

      OnNotifyDataLoadCompleted();

      _log.Debug("loading data completed");
    }

    struct TimeTableState
    {
      public TimeTable TimeTable { get; set; }
      public TrainReference Reference { get; set; }
    }

    //---------------------------------------------------------------------
    private async void LoadTimeTableDataAsync()
    {
      await Task.Run(() =>
      {
        _log.Debug("load time tables");

        DateTime dtStart = DateTime.Now;
        _nQueuedItems = 0;

        OnNotifyDataLoadStarted(LoaderType.LoadComplete);

        using CancellationTokenSource cts = new();
        TimeTables.EnumerateTimeTables(_foldersToExclude, cts.Token).ForEach(t => _allTimeTables.Add(t));

        int n = _allTimeTables.Count;
        _allTimeTables.ForEach(tt =>
              {
                if (tt != null)
                {
                  var q = from t in tt.Trains
                          where t.Link != null | t.Train != null
                          select t;
                  foreach (var train in q)
                  {
#if SEQ
                        try
                        {
                            ZugDatei zd = new(train.Link, train.Link.Datei.FullPath);
                            zd.Parse();
                            Zug z = zd.Root;
                            z.BelongsToTimeTable = tt.ID;
                            z.CheckDecoTrain();
                            _allTrains.Add(z);
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex.ToString());
                        }
#else
                    Interlocked.Increment(ref _nQueuedItems);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(EvaluateTimetable), new TimeTableState() { TimeTable = tt, Reference = train });
#endif
                  }

                  //ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(72.0 / n));
                }
              });

        _queueFinished.WaitOne();

        DateTime dtEnd = DateTime.Now;
        TimeSpan duration = dtEnd - dtStart;
        _log.Debug($"loading time tables completed ({duration.TotalMilliseconds} ms)");
      });

      _waitTimeTables.Set();
    }

    private void EvaluateTimetable(object state)
    {
      if (state is TimeTableState tts)
      {
        try
        {
          if (tts.Reference.Link != null)
          {

            ZugDatei zd = new(tts.Reference.Link, tts.Reference.Link.Datei.FullPath);
            zd.Parse();
            Zug z = zd.Root;
            if (zd.Root != null)
            {
              z.BelongsToTimeTable = tts.TimeTable.ID;
              z.CheckDecoTrain();
              _allTrains.Add(z);
            }
          }
          else
          {
            Zug z = tts.Reference.Train;
            if (z != null)
            {
              z.BelongsToTimeTable = tts.TimeTable.ID;
              z.CheckDecoTrain();
              _allTrains.Add(z);
            }
          }
        }
        catch (Exception ex)
        {
          _log.Error(ex.ToString());
        }

        if (Interlocked.Decrement(ref _nQueuedItems) == 0)
        {
          _queueFinished.Set();
        }
      }
    }

    //---------------------------------------------------------------------
    // progress 72%..96%
    private async void LoadVehicleDataAsync()
    {
      await Task.Run(() =>
      {
        _log.Debug("load vehicle data");

        DateTime dtStart = DateTime.Now;

        EnsureBlacklistedClasses();

        List<Fahrzeug> vehicles = Fahrzeuge.EnumerateVehicles(null, new CancellationTokenSource().Token);
        List<FahrzeugVariante> variants = new();

        foreach (Fahrzeug f in vehicles
                  .Where(v => (v.Kind & VehicleKind.IsPowered) != 0 && !IsBlacklistedClass(v)))
        {
          f.Varianten.ForEach(fv =>
                {
                  if (!fv.Dekozug && !fv.DateiFuehrerstand[0].IsEmpty)
                  {
                    variants.Add(fv);
                  }
                  AllVariants1.Add(fv);
                });
        }

        //ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(12));

        //Log.DebugFormat("{0} vehicle variants loaded", AllVariants1.Count);

        //Log.Debug("grouping vehicles by class");

        _allVehicles = variants.GroupBy(v =>
              {
                ClassFamily cf = ClassFamilies.First(v.BR);
                return cf != null ? cf.Name : v.BR;
              }).Select(g => new VehicleGroup(g.Key, g.ToList())).ToList();

        //ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(12));

        DateTime dtEnd = DateTime.Now;
        TimeSpan duration = dtEnd - dtStart;
        _log.Debug($"loading completed ({duration.TotalMilliseconds} ms)");
      });

      _waitVehicleData.Set();
    }

    //---------------------------------------------------------------------
    private void EnsureBlacklistedClasses()
    {
      if (_blackListedClasses == null || _blackListedClasses.Length == 0)
      {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"ZusiStart\classes.black");
        if (File.Exists(path))
        {
          List<string> temp = new();
          using (FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.Read))
          {
            using StreamReader sr = new(fs);
            string line;
            while (!string.IsNullOrEmpty((line = sr.ReadLine())))
            {
              temp.Add(line);
            }
          }
          _blackListedClasses = temp.ToArray();
        }
      }
    }

    //---------------------------------------------------------------------
    private IEnumerable<string> GetExcludedFolders()
    {
      foreach (string s in Properties.Settings.Default.FoldersToExclude)
      {
        yield return s;
      }
    }

    //---------------------------------------------------------------------
    private bool IsBlacklistedClass(Fahrzeug fahrzeug)
    {
      FahrzeugVariante fv = fahrzeug.GetVariante(0, 0, 0);
      return _blackListedClasses.Contains(fv.BR);
    }

    //---------------------------------------------------------------------
    private DataLoaderResult RebuildFilter()
    {
      DataLoaderResult result = new();
      string path1 = Zusi.DataPath[DataPathType.Official] + @"Timetables\";
      string path2 = Zusi.DataPath[DataPathType.DataDir] + @"Timetables\";

      List<TimeTableGroup> temp = _allTimeTables.GroupBy(tt =>
      {
        ZusiDocumentBase doc = tt.GetDocument();
        string s = doc.Path.StripPrefix(path1);
        if (s.Length == doc.Path.Length)
        {
          s = doc.Path.StripPrefix(path2);
        }
        return s;
      }).Select(grp => new TimeTableGroup() { GroupID = grp.Key, Members = grp.ToList() }).ToList();

      //ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(1));

      int nFreightTrains = 0;
      int nWindowtrains = 0;
#if SEQ
            _allTrains.ForEach(t =>
            {
                switch (t.Type)
                {
                    case TrainType.Freight:
                        nFreightTrains++;
                        break;
                    case TrainType.Passenger:
                        nWindowtrains++;
                        break;
                }
            });
#else
      foreach (Zug z in _allTrains)
      {
        switch (z.Type)
        {
          case TrainType.Freight:
            nFreightTrains++;
            break;
          case TrainType.Passenger:
            nWindowtrains++;
            break;
        }
      }
#endif

      //ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(1));

      result.TimeTableGroups = temp;
      result.CountFreightTrains = nFreightTrains;
      result.CountWindowTrains = nWindowtrains;
      result.FilteredVehicles = FilterVehicles();

      //ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(1));

      return result;
    }

    //---------------------------------------------------------------------
    private void UpdateTimeTableRelations_impl(List<TimeTable> timeTables, int preferredSelection)
    {
      SelectedTimeTableRelation = null;

      int n = timeTables.Count;

      _relations.Clear();
      for (int i = 0; i < n; i++)
      {
        TimeTableRelation ttr = new(i + 1, timeTables[i]);
        _relations.Add(ttr);
      }

      if (_relations.Count >= 1)
      {
        int ix = Math.Min(Math.Max(preferredSelection, 0), _relations.Count - 1);
        _log.DebugFormat("set time table relation: {0}", _relations[ix].ToString());
        SelectedTimeTableRelation = _relations[ix];
      }
      else
      {
        if (timeTables == null)
        {
          _log.Debug("could not set time table relation, due to timeTables is null");
        }
        else
        {
          _log.Debug("could not set time table relation, due to timeTables is empty");
        }
      }
    }

    //---------------------------------------------------------------------
    private static void OnTractionKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      DataManager dm = d as DataManager;
      dm?.OnTractionKindChanged((uint)e.NewValue);
    }

    //---------------------------------------------------------------------
    /*
     * 0 beliebig
     * 1 Dampf
     * 2 Diesel
     * 3 Elektrisch
     * 4 Akku
     * 
     * 0 beliebig
     * 1 Lok
     * 2 Triebwagen
     */
    private void OnTractionKindChanged(uint value)
    {
      _trainKindNotches.ForEach((n) => n.IsLocked = false);

      switch (value)
      {
        case 1: // Dampf
                // keine Triebwagen
          if (TrainKind == 2) TrainKind = 0;
          _trainKindNotches[2].IsLocked = true;
          break;
        case 4: // Akku
                // keine Loks
          if (TrainKind == 1) TrainKind = 0;
          _trainKindNotches[1].IsLocked = true;
          break;
      }

      OnFilterChanged();
    }

    //---------------------------------------------------------------------
    private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      DataManager dm = d as DataManager;
      dm?.OnFilterChanged();
    }

    //---------------------------------------------------------------------
    private void OnFilterChanged()
    {
      FilteredVehicles = FilterVehicles();
      CountFilteredVehicles = FilteredVehicles.Count();
    }

    //---------------------------------------------------------------------
    private IEnumerable<VehicleContainer> FilterVehicles()
    {
      int Fahrzeug_num = 0;
      return _allVehicles.Where(vg =>
          {
            bool res = true;
            Fahrzeug v = vg.Vehicle;
            //DataManager.Instance.dataLoaderWindow.pbLoaded.Value = Fahrzeug_num*100/_allVehicles.Count;
            //DataManager.Instance.dataLoaderWindow.percentageText.Text = (Fahrzeug_num * 100 / _allVehicles.Count).ToString();
            //DataManager.Instance.dataLoaderWindow.UpdateLayout();

            if (v == null)
            {
              _log.Warn("VehicleGroup has no vehicle :(");
              return false;
            }

            switch (TractionKind)
            {
              case 1: // steam
                res &= v.Kind.HasFlag(VehicleKind.Steam);
                break;
              case 2: // diesel
                res &= v.Kind.HasFlag(VehicleKind.Diesel);
                break;
              case 3: // electric
                res &= v.Kind.HasFlag(VehicleKind.Electric);
                break;
              case 4: // accu
                res &= v.Kind.HasFlag(VehicleKind.Battery);
                break;
              default:
                //_log.Warn("Vehicle has no traction kind:" + v.ToString());
                break;
            }

            switch (TrainKind)
            {
              case 1:
                res &= v.Kind.HasFlag(VehicleKind.Locomotive);
                break;
              case 2:
                res &= v.Kind.HasFlag(VehicleKind.RailCar);
                break;
              default:
                //_log.Warn("Vehicle has no train kind:" + v.ToString());
                break;
            }

            if (Epoche > 0)
            {
              res &= v.Epoche == Epoche;
            }

            return res;
          })
          .Select(vg => new VehicleContainer(this, vg))
          .OrderBy(vc => vc.DisplayName);
    }

    //---------------------------------------------------------------------
    private static void OnCurrentVehicleContainerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as DataManager).OnCurrentVehicleContainerChanged((VehicleContainer)e.NewValue);
    }

    //---------------------------------------------------------------------
    private void OnCurrentVehicleContainerChanged(VehicleContainer value)
    {
      _foundTimeTables.Clear();

      if (value != null)
      {
        var trains = _allTrains.Where(z => z.Fahrzeuge.ContainsVehicle(value.Group.AllVariants))
            .GroupBy(g => g.BelongsToTimeTable)
            .Select(g => new FoundTimeTable()
            {
              TimeTable = _allTimeTables.First(tt => tt.ID == g.Key),
              Trains = g.ToList()
            });

        FoundTimeTableViewModel vm = FoundTimeTableViewModel.BuildViewModel(trains);
        vm.Children.ForEach(c => _foundTimeTables.Add(c));
      }
    }
  }
}
