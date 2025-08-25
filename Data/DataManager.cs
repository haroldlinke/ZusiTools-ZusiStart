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
using System.Xml;
using ZusiKlassenLib;
using ZusiKlassenLib.Cab;
using ZusiKlassenLib.Common;
using ZusiKlassenLib.Fahrplan;
using ZusiKlassenLib.TimeTable;
using ZusiKlassenLib.Vehicle;
using ZusiMeterGaugesLib.Gauges;
using ZusiStart.Dialogs;
using ZusiDisplayLib;

//using ZusiPicLib;
using ZusiStart.Miscellaneous;
using Microsoft.VisualBasic.Logging;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using static System.Net.WebRequestMethods;
using System.Runtime.Intrinsics.Arm;
using System.Net;
using ZusiStart.Classes;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ZusiStart.ViewModels;
using System.Security.Policy;
using Xceed.Wpf.Toolkit.Primitives;

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

  public class Options
  {
    public bool Show_ZSK { get; set; }
    public string ZSK_Url { get; set; }
    public bool Show_ZDB { get; set; }
    public string ZDB_Url { get; set; }
    public bool Show_Bfpl { get; set; }
    public string Bfpl_Exe { get; set; }
    public bool Zusi_Exe_indiv { get; set; }
    public string Zusi_Exe { get; set; }
    public bool Start_FIS { get; set; }
    public string ZusiDisplay_Exe { get; set; }
    public string ZusiDisplay_Param { get; set; }
    public bool Start_ZusiMeter { get; set; }
    public string ZusiMeter_Exe { get; set; }
    public string ZusiMeter_Param { get; set; }

    public Options()
    {
      Show_ZSK = true;
      ZSK_Url = MainWindow._url_zusi_strecken_karte;
      Show_ZDB = true;
      ZDB_Url = MainWindow._url_zusi_datenbank;
      Show_Bfpl = false;
      Bfpl_Exe = "";
      Zusi_Exe_indiv = false;
      Zusi_Exe = "";
      Start_FIS = false;
      ZusiDisplay_Exe = "";
      ZusiDisplay_Param = "-fis_neu";
      Start_ZusiMeter = false;
      ZusiMeter_Exe = "";
      ZusiMeter_Param = "";
    }
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
    //public WebView_Window webViewWindow_ZDB { get; set; }
    //public WebView_Window webViewWindow_ZSK { get; set; }
    //public Microsoft.Web.WebView2.Wpf.WebView2 webview { get; set; }
    //public Microsoft.Web.WebView2.Wpf.WebView2 webview_ZDB { get; set; }
    //public Microsoft.Web.WebView2.Wpf.WebView2 webview_ZSK { get; set; }
    //public Microsoft.Web.WebView2.Wpf.WebView2 webview_Win { get; set; }
    string last_tt_name_processed = "";
    int processed_tt_no = 0;
    int total_tt_no = 0;
    //public CoreWebView2Environment webview_environment
    //{
    //  get;
    //  set;
    //}
    //public CoreWebView2Environment webview_environment_ZSK
    //{
    //  get;
    //  set;
    //}
    //public CoreWebView2Environment webview_environment_ZDB
    //{
    //  get;
    //  set;
    //}

    public Dictionary<string, string> ZSK_locationGroupId_dict = new Dictionary<string, string>();

    private static readonly string[] __foldersToExclude = new string[]
    {
            @"ZusiRotstift"
    };

    public double ScreenScaleFactor = 1.0;

    public static readonly string localfoldername = "zusistart8";
    private static readonly string _locosPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).EnsureTrailingBackslash() + @"ZusiStart\ReplacementLoco.xml";
    private static readonly string _trainsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).EnsureTrailingBackslash() + @"ZusiStart\ReplacementTrain.xml";
    private readonly ObservableCollection<ReplacementTrain> _replacementTrains = new();
    private readonly ObservableCollection<string> _replacementVehicles = new();
    private readonly ObservableCollection<object> _replacementTrainMenus = new();
    public static readonly DependencyProperty SelectedTrainRProperty = DependencyProperty.Register(
        "SelectedTrainR",
        typeof(ReplacementTrain),
        typeof(DataManager),
        new PropertyMetadata(null));
    public ReplacementTrain? SelectedTrainR
    {
      get => (ReplacementTrain)GetValue(SelectedTrainRProperty);
      set => SetValue(SelectedTrainRProperty, value);
    }
    public static readonly DependencyProperty SelectedReplacementTrainProperty = DependencyProperty.Register(
        "SelectedReplacementTrain",
        typeof(ReplacementTrain),
        typeof(DataManager),
        new PropertyMetadata(null, OnSelectedReplacementTrainChanged));
    public ReplacementTrain? SelectedReplacementTrain
    {
      get => (ReplacementTrain)GetValue(SelectedReplacementTrainProperty);
      set => SetValue(SelectedReplacementTrainProperty, value);
    }
    //public static readonly DependencyProperty SelectedTrain2ReplaceProperty = DependencyProperty.Register(
    //   "SelectedTrain2Replace",
    //   typeof(TrainItem),
    //   typeof(DataManager),
    //   new PropertyMetadata(null, OnSelectedTrain2ReplaceChanged));
    //public TrainItem SelectedTrain2Replace
    //{
    //  get => (TrainItem)GetValue(SelectedTrain2ReplaceProperty);
    //  set => SetValue(SelectedTrain2ReplaceProperty, value);
    //}
    public ObservableCollection<ReplacementTrain> ReplacementTrains => _replacementTrains;
    public ObservableCollection<string> ReplacementVehicles => _replacementVehicles;
    public ObservableCollection<object> ReplacementTrainMenus => _replacementTrainMenus;

    public string tmpTimeTableFileName = "";
    public string cachepath = "";
    public string webview_ZDB_source = "";
    public string webview_ZSK_source = "";
    public string BildfahrplanExePath = "";
    public string ZusiDisplayStartCmd = "";
    public string ZusiDisplayStartParam = "";
    public string ZusiMeterStartCmd = "";
    public string ZusiMeterStartParam = "";
    public bool AllVehicles_loaded = false;
    public int SkipTimeTableSelectionUpdate = 0;
    public bool ZSK_Fadenkreuz_active = false;
    //public List<string> LaNumberList = new List<string>();
    public double spMax = 0;
    public string FilterZugNummer = "";

    public MainWindow main_window;

    private readonly string[] _foldersToExclude = Array.Empty<string>();
    private string[] _blackListedClasses = Array.Empty<string>();
    private readonly ObservableCollection<TimeTable> _allTimeTables = new();
    private readonly ObservableCollection<TimeTableGroup> _groupedTimeTables = new();


#if SEQ
        private readonly ObservableCollection<Zug> _allTrains = new();
#else
    private readonly ConcurrentBag<Zug> _allTrains = new();
#endif
    private readonly ConcurrentBag<Zug> _allTimeTableTrains = new();

    private readonly ObservableCollection<TimeTableRelation> _relations = new();
    private readonly ObservableCollection<string> _laNumberList = new();
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
    private readonly RecentTrainsCollection _recentTrainsTimeTables = new();
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
      set { SetValue(_countFilteredVehiclesKey, value); }
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
    public static readonly DependencyProperty CurrentTrainItemProperty = DependencyProperty.Register(
        "CurrentTrainItem",
        typeof(TrainItem),
        typeof(DataManager),
        new PropertyMetadata(null, OnCurrentTrainItemChanged));
    public TrainItem? CurrentTrainItem
    {
      get { return (TrainItem)GetValue(CurrentTrainItemProperty); }
      set { SetValue(CurrentTrainItemProperty, value); }
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
      set { SetValue(_filteredVehiclesKey, value); }
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
      set
      {
        SetValue(SelectedTimeTableRelationProperty, value);
      }
    }

    //---------------------------------------------------------------------
    public static DependencyProperty SelectedLaNumberProperty = DependencyProperty.Register(
        "SelectedLaNumber",
        typeof(string),
        typeof(DataManager),
        new PropertyMetadata("", OnSelectedLaNumberChanged));
    public string? SelectedLaNumber
    {
      get { return (string)GetValue(SelectedLaNumberProperty); }
      set
      {
        SetValue(SelectedLaNumberProperty, value);
      }
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
    public static readonly DependencyProperty SearchResultTitleProperty = DependencyProperty.Register(
        "SearchResultTitle",
        typeof(string),
        typeof(DataManager),
        new PropertyMetadata(null));
    public string SearchResultTitle
    {
      get { return (string)GetValue(SearchResultTitleProperty); }
      set { SetValue(SearchResultTitleProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty LoadingStatusProperty = DependencyProperty.Register(
        "LoadingStatus",
        typeof(string),
        typeof(DataManager),
        new PropertyMetadata(null));
    public string LoadingStatus
    {
      get { return (string)GetValue(LoadingStatusProperty); }
      set { SetValue(LoadingStatusProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty FoundTimeTableTitleProperty = DependencyProperty.Register(
        "FoundTimeTableTitle",
        typeof(string),
        typeof(DataManager),
        new PropertyMetadata(null));
    public string FoundTimeTableTitle
    {
      get { return (string)GetValue(FoundTimeTableTitleProperty); }
      set { SetValue(FoundTimeTableTitleProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty FoundStationTitleProperty = DependencyProperty.Register(
        "FoundStationTitle",
        typeof(string),
        typeof(DataManager),
        new PropertyMetadata(null));
    public string FoundStationTitle
    {
      get { return (string)GetValue(FoundStationTitleProperty); }
      set { SetValue(FoundStationTitleProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty FoundTimeTableGroupTitleProperty = DependencyProperty.Register(
        "FoundTimeTableGroupTitle",
        typeof(string),
        typeof(DataManager),
        new PropertyMetadata(null));
    public string FoundTimeTableGroupTitle
    {
      get { return (string)GetValue(FoundTimeTableGroupTitleProperty); }
      set { SetValue(FoundTimeTableGroupTitleProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty ZSK_Fadenkreuz_ButtonTitleProperty = DependencyProperty.Register(
        "ZSK_Fadenkreuz_ButtonTitle",
        typeof(string),
        typeof(DataManager),
        new PropertyMetadata(null));
    public string ZSK_Fadenkreuz_ButtonTitle
    {
      get { return (string)GetValue(ZSK_Fadenkreuz_ButtonTitleProperty); }
      set { SetValue(ZSK_Fadenkreuz_ButtonTitleProperty, value); }
    }


    //public static readonly DependencyProperty SelectedLocoProperty = DependencyProperty.Register(
    //        "SelectedLoco",
    //        typeof(ReplacementLoco),
    //        typeof(DataManager),
    //        new PropertyMetadata(null));
    //public ReplacementLoco? SelectedLoco
    //{
    //  get => (ReplacementLoco)GetValue(SelectedLocoProperty);
    //  set => SetValue(SelectedLocoProperty, value);
    //}

    //public static readonly DependencyProperty SelectedReplacementLocoProperty = DependencyProperty.Register(
    //    "SelectedReplacementLoco",
    //    typeof(ReplacementLoco),
    //    typeof(DataManager),
    //    new PropertyMetadata(null, OnSelectedReplacementLocoChanged));
    //public ReplacementLoco? SelectedReplacementLoco
    //{
    //  get => (ReplacementLoco)GetValue(SelectedReplacementLocoProperty);
    //  set => SetValue(SelectedReplacementLocoProperty, value);
    //}



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
    public ConcurrentBag<Zug> AllTimeTableTrains { get { return _allTimeTableTrains; } }
    public ObservableCollection<TimeTableGroup> GroupedTimeTables { get { return _groupedTimeTables; } }
    public ObservableCollection<TimeTableRelation> Relations { get { return _relations; } }
    public ObservableCollection<TrainsViewModel> FoundTrains { get { return _foundTrains; } }
    public ObservableCollection<Notch> TrainKindNotches => _trainKindNotches;
    public ObservableCollection<FoundTimeTableViewModel> FoundTimeTables { get => _foundTimeTables; }
    public ObservableCollection<TrainsViewModel> TimeTableTrains { get => _timeTableTrains; }
    public RecentTrainsCollection RecentTrains { get => _recentTrains; }

    public ObservableCollection<string> LaNumberList { get { return _laNumberList; } }
    public Options options { get; set; }
    // Publicly accessible dictionary
    public Dictionary<string, string> Fpn2zsklinkDictionary { get; private set; }
    public bool FIS_available { get; set; }
    public string OptionsFilePath = "";
    public string VehiclesFilePath = "";
    public string Fpn2zsklinkFilePath = "";

    public List<FahrzeugVariante> AllVariants1 => _allVariants;

    public event EventHandler DecoTrainsAllowed;
    public event EventHandler SelectedTimeTableChanged;
    public event NotifyDataLoadStartedEventHandler NotifyDataLoadStarted;
    public event EventHandler NotifyDataLoadCompleted;
    private List<string> favorite_timetables;
    //public event ProgressChangedEventHandler ProgressChanged;

    public string tab_title_intro = "Intro";
    public string tab_title_docu = "FPL-Docu";
    public string tab_title_Streckenkarte = "Streckenkarte";
    public string tab_title_Zusi_DB = "Zusi-DB";
    public string tab_title_La_Hdb = "LA Handbuch";
    public string tab_title_Ersatzfahrplan = "Ersatzfahrplan";
    public string tab_title_StreBu = "Streckenbuch";
    public string tab_title_buchfahrplan = "Buchfahrplan";
    public string tab_title_OeRilSK = "ÖRil-SK";
    public string tab_title_favorites = "Favoriten";
    public string tab_title_ZusiStartHdb = "ZusiStart-Handbuch";


    //---------------------------------------------------------------------
    public static void UpdateTimeTableRelations(List<TimeTable> timeTables, int preferredSelection)
    {
      Instance?.UpdateTimeTableRelations_impl(timeTables, preferredSelection);
    }

    //-----------------------------------------------------------
    // Tabs List
    public ObservableCollection<TabViewModel> Tabs { get; set; }

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
      ReadReplacementTrains();

      string zusistartdocu_relativePath = "help/ZusiStart_Docu_Deutsch.pdf";
      string zusistartdoc_absolutePath = Path.GetFullPath(zusistartdocu_relativePath);

      Tabs = new ObservableCollection<TabViewModel>
      {

        
        new TabViewModel(tab_title_docu, "https://zusi-tools.de/"),
          new TabViewModel(tab_title_Streckenkarte, "https://www.zusi-sk.eu/#10.5/50.96983/6/"),
          new TabViewModel(tab_title_Zusi_DB, "https://www.zusidatenbank.de/?zusistart"),
          new TabViewModel(tab_title_buchfahrplan, "", isImageTab: true),
          new TabViewModel(tab_title_La_Hdb, @"Timetables\Deutschland\Infrastrukturdaten\Zusi_La.pdf", true),
          new TabViewModel(tab_title_Ersatzfahrplan, @"Timetables\Deutschland\Infrastrukturdaten\Zusi_Ersatzfahrplan_999.pdf,Timetables\Deutschland\Infrastrukturdaten\Zusi_Ersatzfahrplan_996.pdf,Timetables\Deutschland\Infrastrukturdaten\Zusi_Ersatzfahrplan_993.pdf", true),
          new TabViewModel(tab_title_StreBu, @"Timetables\Deutschland\Infrastrukturdaten\Zusi_Oeril.pdf", true),
          new TabViewModel(tab_title_OeRilSK, @"Timetables\Deutschland\Infrastrukturdaten\Oeril_Sk-Signale.pdf", true),
          new TabViewModel(tab_title_ZusiStartHdb, zusistartdoc_absolutePath, true),
          new TabViewModel(tab_title_favorites, ""),
          //new TabViewModel(tab_title_intro, "", isIntro: true),
          

      };
    }

    public async void set_websource(string tabtitle, string url)
    {
      // adapted to new concept
      var zdbTab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == tabtitle);
      if (zdbTab?.WebViewInstance?.CoreWebView2 != null)
      {
        //zdbTab.WebViewInstance.CoreWebView2.Navigate(value);
        zdbTab.WebViewInstance.Source = new Uri(url);
      }
    }


    //---------------------------------------------------------------------
    public void AcceptNewTrain()
    {
      _replacementTrains.Add(SelectedTrainR);
      SelectedReplacementTrain = SelectedTrainR;
    }

    //---------------------------------------------------------------------
    public void ApplyChangesTrain()
    {
      int ix = _replacementTrains.IndexOf(SelectedReplacementTrain);
      ReplacementTrain rt = new(SelectedTrainR);
      _replacementTrains.RemoveAt(ix);
      _replacementTrains.Insert(ix, rt);
      SelectedReplacementTrain = rt;
    }

    //---------------------------------------------------------------------
    public void RemoveSelectedTrain()
    {
      if (SelectedReplacementTrain != null)
      {
        _replacementTrains.Remove(SelectedReplacementTrain);
      }
    }

    //---------------------------------------------------------------------
    public void SaveReplacementTrains()
    {
      ReplacementTrainFile file = new(_trainsPath);
      file.SaveDocument += File_SaveDocument_Train;
      file.Save();

      UpdateContextMenu();
    }

    //---------------------------------------------------------------------
    private void ReadReplacementTrains()
    {
      if (System.IO.File.Exists(_trainsPath))
      {
        try
        {
          ReplacementTrainFile file = new(_trainsPath);
          file.ParseDocument += File_ParseDocument_Train;
          file.Parse();
        }
        catch (Exception ex)
        {
          _log.Error(ex.ToString());
        }
      }
    }

    //---------------------------------------------------------------------
    private void UpdateContextMenu()
    {
      //_replacementLocoMenus.Clear();

      //List<object> mm = _replacementLocos.Select(rl => new MenuItem
      //{
      //  Command = MainWindow.ReplaceLocoCommand,
      //  CommandParameter = rl,
      //  Header = rl.Name
      //}).ToList<object>();
      //mm.Insert(0, new MenuItem { Header = "Loktausch", FontWeight = FontWeights.Bold });
      //mm.Insert(1, new Separator());
      //if (mm.Count == 2)
      //{
      //  mm.Add(new MenuItem { Header = "<keine Austauschlok definiert>" });
      //}
      //else
      //{
      //  mm.Insert(2, new MenuItem { Command = MainWindow.UndoReplLocoCommand });
      //  mm.Insert(3, new Separator());

      //  Binding b = new("ForceDoubleHeading") { Mode = BindingMode.TwoWay };
      //  MenuItem mi = new() { Header = "Lok in Doppeltraktion einsetzen", IsCheckable = true, StaysOpenOnClick = true };
      //  BindingOperations.SetBinding(mi, MenuItem.IsCheckedProperty, b);
      //  mm.Insert(4, mi);

      //  b = new Binding("ForceSingleHeading") { Mode = BindingMode.TwoWay };
      //  mi = new MenuItem { Header = "Doppeltraktion ersetzen", IsCheckable = true, StaysOpenOnClick = true };
      //  BindingOperations.SetBinding(mi, MenuItem.IsCheckedProperty, b);
      //  mm.Insert(5, mi);

      //  mm.Insert(6, new Separator());
      //}
      //mm.ForEach(m => _replacementLocoMenus.Add(m));

      _replacementTrainMenus.Clear();

      List<object> mm2 = _replacementTrains.Select(rt => new MenuItem
      {
        Command = MainWindow.ReplaceTrainCommand,
        CommandParameter = rt,
        Header = rt.Name
      }).ToList<object>();
      mm2.Insert(0, new MenuItem { Header = "Zugtausch", FontWeight = FontWeights.Bold });
      mm2.Insert(1, new Separator());
      if (mm2.Count == 2)
      {
        mm2.Add(new MenuItem { Header = "<kein Austauschzug definiert>" });
      }
      else
      {
        mm2.Insert(2, new MenuItem { Command = MainWindow.UndoReplTrainCommand });
        mm2.Insert(3, new Separator());


      }
      mm2.ForEach(m => _replacementTrainMenus.Add(m));
    }

    //---------------------------------------------------------------------
    private void File_ParseDocument_Train(object sender, ReadTrainFileEventArgs e)
    {
      foreach (XElement x in e.Trains.Elements("train"))
      {
        try
        {
          _replacementTrains.Add(new ReplacementTrain(x));
        }
        catch (ReplacementTrainException ex)
        {
          _log.Warn(ex.Message);
        }
        catch (Exception ex)
        {
          _log.Error(ex.ToString());
        }
      }
    }

    //---------------------------------------------------------------------
    private void File_SaveDocument_Train(object sender, SaveTrainFileEventArgs e)
    {
      _replacementTrains.ForEach(t => t.Save(e.Writer));
    }

    //---------------------------------------------------------------------
    private static void OnSelectedReplacementTrainChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as DataManager)?.OnSelectedReplacementTrainChanged((ReplacementTrain)e.NewValue);
    }

    //---------------------------------------------------------------------
    private void OnSelectedReplacementTrainChanged(ReplacementTrain value)
    {
      SelectedTrainR = value != null ? new ReplacementTrain(value) : null;
    }


    //---------------------------------------------------------------------
    private void OnSelectedReplacementTrainChanged(TrainItem value)
    {
      int saving = 0;

      if (value != null)
      {
        value.IsImportant = true;
        Zug reference = value.Zug;

        //  foreach (TrainItem ti in _trains)
        //  {
        //    if (ti == value) continue;

        //    if (!ti.CheckImportance(reference))
        //    {
        //      saving++;
        //    }
        //  }
      }

      //Saving = _trains.Count == 0 ? 0 : (uint)(saving * 100 / _trains.Count);
    }


    //---------------------------------------------------------------------
    private static void OnCurrentTrainItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as DataManager)?.OnCurrentTrainItemChanged((TrainItem)e.NewValue);
    }

    //---------------------------------------------------------------------
    private void OnCurrentTrainItemChanged(TrainItem value)
    {
      int saving = 0;

      if (value != null)
      {
        value.IsImportant = true;
        Zug reference = value.Zug;

        //  foreach (TrainItem ti in _trains)
        //  {
        //    if (ti == value) continue;

        //    if (!ti.CheckImportance(reference))
        //    {
        //      saving++;
        //    }
        //  }
        //}

        //Saving = _trains.Count == 0 ? 0 : (uint)(saving * 100 / _trains.Count);
      }
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
      LoadGeneralOptions();


#else
            OnNotifyDataLoadCompleted();
#endif
    }

    public void LoadGeneralOptions()
    {
      if (string.IsNullOrEmpty(OptionsFilePath))
      {
        //OptionsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ZusiStart\\config.json";
        OptionsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DataManager.localfoldername, "config.json");

      }

      if (System.IO.File.Exists(OptionsFilePath))
      {
        var json = System.IO.File.ReadAllText(OptionsFilePath);
        options = JsonSerializer.Deserialize<Options>(json);
        if (options == null)
        {
          options = new Options();
        }
      }
      else
      {
        options = new Options();
      }

      main_window.ZSKButtonVisibility = options.Show_ZSK ? Visibility.Visible : Visibility.Collapsed;
      main_window.ZDBButtonVisibility = options.Show_ZDB ? Visibility.Visible : Visibility.Collapsed;
      main_window.BfpButtonVisibility = options.Show_Bfpl ? Visibility.Visible : Visibility.Collapsed;

      if (options.ZDB_Url == "https://www.zusidatenbank.de")
        options.ZDB_Url = "https://www.zusidatenbank.de/?zusistart";
    }

    public void SaveGeneralOptions()
    {
      if (options != null)
      {
        var json = JsonSerializer.Serialize(options);
        System.IO.File.WriteAllText(OptionsFilePath, json);
      }
    }

    public void Loadfpn2zsk_link_json()
    {
      if (string.IsNullOrEmpty(Fpn2zsklinkFilePath))
      {
        Fpn2zsklinkFilePath = "ZusiStart_fpn2zsklink.json";
      }

      if (System.IO.File.Exists(Fpn2zsklinkFilePath))
      {
        string jsonString = System.IO.File.ReadAllText(Fpn2zsklinkFilePath);

        // Deserialize JSON string to Dictionary
        Fpn2zsklinkDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

        //Console.WriteLine($"Value of key1: {dataManager.Dictionary["key1"]}");
        //Console.WriteLine($"Value of key2: {dataManager.Dictionary["key2"]}");
        //Console.WriteLine($"Value of key3: {dataManager.Dictionary["key3"]}");
      }
    }

    public void Savefpn2zsk_link_json()
    {
      if (options != null)
      {
        var json = JsonSerializer.Serialize(Fpn2zsklinkDictionary);
        System.IO.File.WriteAllText(Fpn2zsklinkFilePath, json);
      }
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
        if (value.Trains == null)
        {
          if (value.Timetables != null)
          {
            foreach (var t in value.Timetables.FirstOrDefault().Trains)
            {
              if (t.Train != null)
              {
                value.Trains.Add(t.Train);
              }
            }
          }
        }
        if (value.Trains != null)
        {
          foreach (TrainsViewModel tvm in TrainsViewModel.BuildViewModel(value.Trains).Children)
          {
            _foundTrains.Add(tvm);
          }
        }
      }
    }

    //---------------------------------------------------------------------
    public TimeTable GetTimeTableOfTrain(Zug zug)
    {
      return _allTimeTables.FirstOrDefault(tt => tt.ID == zug.BelongsToTimeTable);
    }

    //---------------------------------------------------------------------
    public TimeTable GetTimeTableOfFpnName(string fpnname)
    {
      return _allTimeTables.FirstOrDefault(tt => tt.FindParent<TimeTableFile>().Filename.Contains(fpnname));
    }

    //---------------------------------------------------------------------
    public bool SearchBetriebsstelleTrain(string number)
    {
      List<TimeTable> foundTimeTables = new();


      foundTimeTables.Clear();
      CurrentTrain = null;
      CurrentTrainItem = null;



      FoundTimeTableViewModel vm = null;

      string n = number.Replace(" ", "").Trim().ToLower();
      var trains = _allTrains.Where(z =>
      {
        //return z.FahrplanEintraege.Any(f => f.Bestrst != null && string.Compare(n, f.Bestrst.Replace(" ", "").Trim(), true) == 0);
        return z.FahrplanEintraege.Any(f => f.Bestrst != null && f.Bestrst.Replace(" ", "").Trim().Contains(n, StringComparison.OrdinalIgnoreCase));
      })
      .GroupBy(g => g.BelongsToTimeTable)
      .Select(g => new FoundTimeTable()
      {
        TimeTable = _allTimeTables.First(tt => tt.ID == g.Key),
        Trains = g.ToList()
      });


      foreach (var m in trains)
      {
        foundTimeTables.Add(m.TimeTable);
      }

      //vm = FoundTimeTableViewModel.BuildViewModel(trains);
      //vm.Children.ForEach(c => _foundTimeTables.Add(c));

      UpdateTimeTableRelations(foundTimeTables, 0);

      return foundTimeTables.Count > 0;
    }


    //---------------------------------------------------------------------
    public bool SearchTrain(string number)
    {
      _foundTimeTables.Clear();
      CurrentTrain = null;
      CurrentTrainItem = null;

      FoundTimeTableViewModel vm = null;



      bool show_decotrains = true; // IsDecoTrainsAllowed;

      if (true)  // suche Zugnummer
      {

        string n = number.Replace(" ", "").Trim().ToLower();
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


        vm = FoundTimeTableViewModel.BuildViewModel(trains);
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
      }

      if (vm == null || vm.Children.Count == 0) // Keine Zugnummer gefunden, dann suche Betriebsstelle
      {

        string n = number.Replace(" ", "").Trim().ToLower();
        var trains = _allTrains.Where(z =>
        {
          //return z.FahrplanEintraege.Any(f => f.Bestrst != null && string.Compare(n, f.Bestrst.Replace(" ", "").Trim(), true) == 0);
          return z.FahrplanEintraege.Any(f => f.Bestrst != null && f.Bestrst.Replace(" ", "").Trim().Contains(n, StringComparison.OrdinalIgnoreCase));
        })
        .GroupBy(g => g.BelongsToTimeTable)
        .Select(g => new FoundTimeTable()
        {
          TimeTable = _allTimeTables.First(tt => tt.ID == g.Key),
          Trains = g.ToList()
        });

        vm = FoundTimeTableViewModel.BuildViewModel(trains);
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

      }

      if (vm == null || vm.Children.Count == 0) // Keine Zugnummer/Bestriebsstelle gefunden, dann suche Modulnamen
      {
        string n = number.Replace(" ", "_").Trim().ToLower();
        if (n.EndsWith(".st3")) // search for module
        {
          n = n.Replace(".st3", "");
          var timetables = _allTimeTables.Where(z =>
          {
            return z.StrModules.Any(f => f.Datei != null && f.Datei.Dateiname.Replace(" ", "").Trim().Contains(n, StringComparison.OrdinalIgnoreCase));
          });

          var timetableIds = timetables.Select(t => t.ID).ToList();

          var trains = _allTrains.Where(t => timetableIds.Contains(t.BelongsToTimeTable)).ToList()
            .GroupBy(g => g.BelongsToTimeTable)
            .Select(g => new FoundTimeTable()
            {
              TimeTable = _allTimeTables.First(tt => tt.ID == g.Key),
              Trains = g.ToList()
            });

          vm = FoundTimeTableViewModel.BuildViewModel(trains);
          vm.Children.ForEach(c => _foundTimeTables.Add(c));
        }
      }

      if (_foundTimeTables.Count == 0)
      {
        SetFoundTimeTable(null);
      }

      if (_foundTimeTables.Count > 0)
      {
        SearchResultTitle = number;
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
    public void OnSelectedTimeTableChanged(TimeTableRelation value)
    {
      _timeTableTrains.Clear();
      //_allTimeTableTrains.Clear();
      if (value != null)
      {
        foreach (TrainsViewModel tvm in TrainsViewModel.BuildViewModel(value.TimeTable).Children)
        {
          _timeTableTrains.Add(tvm);
        }

        //foreach (var g in AllTrains
        //       .Where(z => z.BelongsToTimeTable == value.TimeTable.ID))
        //{
        //  _allTimeTableTrains.Add(g);
        //}
      }
      SelectedTimeTableChanged?.Invoke(this, EventArgs.Empty);
    }

    //---------------------------------------------------------------------
    private static void OnSelectedLaNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      DataManager dm = d as DataManager;
      dm?.OnSelectedLaNumberChanged((TimeTableRelation)e.NewValue);
    }

    //---------------------------------------------------------------------
    public void OnSelectedLaNumberChanged(TimeTableRelation value)
    {
      //_timeTableTrains.Clear();
      ////_allTimeTableTrains.Clear();
      //if (value != null)
      //{
      //  foreach (TrainsViewModel tvm in TrainsViewModel.BuildViewModel(value.TimeTable).Children)
      //  {
      //    _timeTableTrains.Add(tvm);
      //  }

      //  //foreach (var g in AllTrains
      //  //       .Where(z => z.BelongsToTimeTable == value.TimeTable.ID))
      //  //{
      //  //  _allTimeTableTrains.Add(g);
      //  //}
      //}
      //SelectedTimeTableChanged?.Invoke(this, EventArgs.Empty);
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
        foreach (RecentTrain rt in RecentTrains)
        {
          rt.CommentEdit = false;
        }
      }
    }

    //---------------------------------------------------------------------
    public void OnNotifyDataLoadStarted(LoaderType loaderType)
    {
      Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action<LoaderType>((t) =>
      {
        NotifyDataLoadStarted?.Invoke(this, new NotifyDataLoadStartedEventArgs(t));
      }), loaderType);
    }

    //---------------------------------------------------------------------
    public void OnNotifyDataLoadCompleted()
    {
      NotifyDataLoadCompleted?.Invoke(this, EventArgs.Empty);
    }

    //---------------------------------------------------------------------
    private async void FinishLoadingAsync()
    {
      DataLoaderResult result = await System.Threading.Tasks.Task.Run(() =>
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

      // FilteredVehicles = result.FilteredVehicles;
      CountFilteredVehicles = 0; // FilteredVehicles.Count();

      //CountFilteredVehicles = result.FilteredVehicles.Count();
      //dummywindow.Close();

      LoadTimeTableDataAsync2();

      //_recentTrains.LoadFromFile(_allTrains, _allTimeTables);

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
      await System.Threading.Tasks.Task.Run(() =>
      {
        _log.Debug("load time tables");

        Application.Current.Dispatcher.Invoke(() =>
        {
          LoadingStatus = "Loading...";
        });

        DateTime dtStart = DateTime.Now;
        _nQueuedItems = 0;

        OnNotifyDataLoadStarted(LoaderType.LoadComplete);

        using CancellationTokenSource cts = new();
        TimeTables.EnumerateTimeTables(_foldersToExclude, cts.Token).ForEach(t => _allTimeTables.Add(t));

        //_allTimeTables.ForEach(tt =>
        //             {

        //               if (tt != null)
        //               {
        //                 if (!tt.GetDocument().Filename.StartsWith(Zusi.DataPath[0]))
        //                 {
        //                   tt.Name = tt.Name + " (private)";
        //                 }
        //               }
        //             });

        int n = _allTimeTables.Count;
        //        _allTimeTables.ForEach(tt =>
        //              {

        //                if (tt != null)
        //                {
        //                  var q = from t in tt.Trains
        //                          where t.Link != null | t.Train != null
        //                          select t;
        //                  foreach (var train in q)
        //                  {
        //#if SEQ
        //                        try
        //                        {
        //                            ZugDatei zd = new(train.Link, train.Link.Datei.FullPath);
        //                            zd.Parse();
        //                            Zug z = zd.Root;
        //                            z.BelongsToTimeTable = tt.ID;
        //                            z.CheckDecoTrain();
        //                            _allTrains.Add(z);
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            _log.Error(ex.ToString());
        //                        }
        //#else
        //                    Interlocked.Increment(ref _nQueuedItems);
        //                    ThreadPool.QueueUserWorkItem(new WaitCallback(EvaluateTimetable), new TimeTableState() { TimeTable = tt, Reference = train });
        //#endif
        //                  }

        //                  //ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(72.0 / n));
        //                }
        //              });

        //_queueFinished.WaitOne();

        DateTime dtEnd = DateTime.Now;
        TimeSpan duration = dtEnd - dtStart;
        _log.Debug($"loading time tables completed ({duration.TotalMilliseconds} ms)");
      });

      _waitTimeTables.Set();
    }

    //---------------------------------------------------------------------
    private bool isFavoriteTimetable(string timetablename)
    {
      timetablename = timetablename.Replace(" ", "_");
      foreach (string t in favorite_timetables)
      {
        if (timetablename == t)
          return true;
      }

      return false;
    }

    //---------------------------------------------------------------------
    private async void LoadTimeTableDataAsync2()
    {
      await System.Threading.Tasks.Task.Run(() =>
      {
        DataLoaderResult result;
        _log.Debug("load time tables");

        favorite_timetables = _recentTrains.LoadTimeTableListFromFile();

        DateTime dtStart = DateTime.Now;
        _nQueuedItems = 0;

        OnNotifyDataLoadStarted(LoaderType.LoadComplete);

        using CancellationTokenSource cts = new();
        //TimeTables.EnumerateTimeTables(_foldersToExclude, cts.Token).ForEach(t => _allTimeTables.Add(t));
        processed_tt_no = 0;
        for (int i = 1; i <= 2; i++)
        {
          // loop i=1 load only favorite timetables
          //      i=2 load all other timetables
          if (i == 1 && favorite_timetables.Count == 0)
          {
            continue;
          }
          int n = _allTimeTables.Count;
          total_tt_no = n;
          _allTimeTables.ForEach(tt =>
                {

                  if (tt != null)
                  {
                    if ((i == 2) ^ (isFavoriteTimetable(tt.Name) == true))
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

                      // Process modules
                      TimeTable? timetable = null; // tt; do not analyse modules!! Takes too much time
                      if (timetable != null)
                      {
                        // Englische Kultur für die korrekte Erkennung von '.'
                        CultureInfo englishCulture = CultureInfo.InvariantCulture;

                        foreach (StrModul strmodul in timetable.StrModules)
                        {
                          Point point = strmodul.Strecke.Utm.ToLatLon();
                          double longitude = Math.Round(point.X, 1);
                          double latitude = Math.Round(point.Y, 1);

                          // Wieder in einen String umwandeln
                          string roundedCoordinates = $"{longitude.ToString("F1", englishCulture)}/{latitude.ToString("F1", englishCulture)}";

                          string modul_filename = strmodul.Datei.FullPath;
                          string modulename = Path.GetFileName(modul_filename);

                          DataManager.Instance.ZSK_locationGroupId_dict[roundedCoordinates] = modulename;
                          //System.Diagnostics.Debug.WriteLine($"***** Filename for {roundedCoordinates}: {modulename} *****");

                        }

                      }


                    }
                    //DataLoaderResult result = RebuildFilter();

                    //Application.Current.Dispatcher.Invoke(() =>
                    //{
                    //  CountTimeTables = _allTimeTables.Count;
                    //  CountFreightTrains = result.CountFreightTrains;
                    //  CountWindowTrains = result.CountWindowTrains;
                    //  _groupedTimeTables.Clear();
                    //  result.TimeTableGroups.ForEach(t => _groupedTimeTables.Add(t));
                    //  // Update your UI elements here
                    //  //main_window.UpdateLayout();
                    //  //main_window.NotifyCanExecuteChanged(); // Notify that the CanExecute state has changed
                    //});
                    ////ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(72.0 / n));
                  }
                });


          _queueFinished.WaitOne();

          ////ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(72.0 / n));
          if (i == 1)
          {
            _recentTrains.LoadFromFile(_allTrains, _allTimeTables);
            // Update the UI on the main thread
            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //  // Update your UI elements here
            //  main_window.UpdateLayout();
            //  main_window.NotifyCanExecuteChanged(); // Notify that the CanExecute state has changed
            //});
          }

          result = RebuildFilter();

          Application.Current.Dispatcher.Invoke(() =>
          {
            CountTimeTables = _allTimeTables.Count;
            CountFreightTrains = result.CountFreightTrains;
            if (i == 2)
            {
              CountWindowTrains = result.CountWindowTrains;
            }
            _groupedTimeTables.Clear();
            result.TimeTableGroups.ForEach(t => _groupedTimeTables.Add(t));
            // Update your UI elements here
            main_window.UpdateLayout();
            main_window.NotifyCanExecuteChanged(); // Notify that the CanExecute state has changed
          });

        }

        //DataLoaderResult result = RebuildFilter();
        //// Update the UI on the main thread
        //Application.Current.Dispatcher.Invoke(() =>
        //    {
        //      CountTimeTables = _allTimeTables.Count;
        //      CountFreightTrains = result.CountFreightTrains;
        //      CountWindowTrains = result.CountWindowTrains;
        //      _groupedTimeTables.Clear();
        //      result.TimeTableGroups.ForEach(t => _groupedTimeTables.Add(t));
        //      // Update your UI elements here
        //      main_window.UpdateLayout();
        //      main_window.NotifyCanExecuteChanged(); // Notify that the CanExecute state has changed
        //    });

        DateTime dtEnd = DateTime.Now;
        TimeSpan duration = dtEnd - dtStart;

        _log.Debug($"loading time tables completed ({duration.TotalMilliseconds} ms)");
      });

      // continue with vehicle data, if not switched off
      if (true)
      {
        LoadVehicleDataAsync2();
      }

      //_waitTimeTables.Set();
    }

    private void EvaluateTimetable(object state)
    {
      if (state is TimeTableState tts)
      {
        try
        {
          string name = tts.TimeTable.Name;

          if (name != last_tt_name_processed)
          {
            last_tt_name_processed = name;
            processed_tt_no++;
            Application.Current.Dispatcher.Invoke(() =>
            {
              LoadingStatus = $"Loading {processed_tt_no} of {total_tt_no} - {name}";
            });

          }


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
      await System.Threading.Tasks.Task.Run(() =>
      {
        _log.Debug("load vehicle data");

        DateTime dtStart = DateTime.Now;

        EnsureBlacklistedClasses();

        //List<Fahrzeug> vehicles = Fahrzeuge.EnumerateVehicles(null, new CancellationTokenSource().Token);
        //List<FahrzeugVariante> variants = new();

        //foreach (Fahrzeug f in vehicles
        //          .Where(v => (v.Kind & VehicleKind.IsPowered) != 0 && !IsBlacklistedClass(v)))
        ////foreach (Fahrzeug f in vehicles
        ////          .Where(v => (v.Kind & VehicleKind.IsPowered) != 0 && v.Name == "611" && !IsBlacklistedClass(v)))
        //{
        //  f.Varianten.ForEach(fv =>
        //        {
        //          if (!fv.Dekozug && !fv.DateiFuehrerstand[0].IsEmpty)
        //          {
        //            variants.Add(fv);
        //          }
        //          AllVariants1.Add(fv);
        //        });
        //}

        ////ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(12));

        ////Log.DebugFormat("{0} vehicle variants loaded", AllVariants1.Count);

        ////Log.Debug("grouping vehicles by class");

        //_allVehicles = variants.GroupBy(v =>
        //      {
        //        ClassFamily cf = ClassFamilies.First(v.BR);
        //        return cf != null ? cf.Name : v.BR;
        //      }).Select(g => new VehicleGroup(g.Key, g.ToList())).ToList();

        //ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(12));

        DateTime dtEnd = DateTime.Now;
        TimeSpan duration = dtEnd - dtStart;
        _log.Debug($"loading completed ({duration.TotalMilliseconds} ms)");
      });

      _waitVehicleData.Set();
    }

    private async void LoadVehicleDataAsync2()
    {
      await System.Threading.Tasks.Task.Run(async () =>
      {
        Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
        _log.Debug("load vehicle data");

        DateTime dtStart = DateTime.Now;

        EnsureBlacklistedClasses();

        List<Fahrzeug> vehicles = Fahrzeuge.EnumerateVehicles(null, new CancellationTokenSource().Token);
        List<FahrzeugVariante> variants = new();

        foreach (Fahrzeug f in vehicles
                  .Where(v => (v.Kind & VehicleKind.IsPowered) != 0 && !IsBlacklistedClass(v)))
        //foreach (Fahrzeug f in vehicles
        //          .Where(v => (v.Kind & VehicleKind.IsPowered) != 0 && v.Name == "611" && !IsBlacklistedClass(v)))
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
        // Yield control to UI thread
        await System.Threading.Tasks.Task.Delay(10);

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


      await System.Threading.Tasks.Task.Delay(10);
      AllVehicles_loaded = true;

      //_waitVehicleData.Set();
      // FilteredVehicles = FilterVehicles();
      // CountFilteredVehicles = FilteredVehicles.Count();


      //Save_allvehicles();

      //CountFilteredVehicles = result.FilteredVehicles.Count();
    }

    //---------------------------------------------------------------------
    private void EnsureBlacklistedClasses()
    {
      if (_blackListedClasses == null || _blackListedClasses.Length == 0)
      {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"ZusiStart\classes.black");
        if (System.IO.File.Exists(path))
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
    public void UpdateTimeTableRelations_impl(List<TimeTable> timeTables, int preferredSelection)
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

        if (!string.IsNullOrEmpty(SelectedTimeTableRelation.Begruessungsdatei))
        {
          //DataManager.Instance.webview.Source = new Uri(SelectedTimeTableRelation.Begruessungsdatei);
          DataManager.Instance.set_websource(DataManager.Instance.tab_title_docu, SelectedTimeTableRelation.Begruessungsdatei);
        }
        if (!string.IsNullOrEmpty(SelectedTimeTableRelation.TimeTable.GetDocument().Filename))
        {

          DataPathType dtp = DataPathType.Unknown;
          string orgRelativeTimetableName = Zusi.GetRelativePathOf(SelectedTimeTableRelation.TimeTable.GetDocument().Filename, ref dtp);
          orgRelativeTimetableName = orgRelativeTimetableName.Replace("\\", "%5C");
          string url = "https://www.zusidatenbank.de/fahrplan/" + orgRelativeTimetableName;


          //DataManager.Instance.webview_ZDB.Source = new Uri(url);
        }
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
    public IEnumerable<VehicleContainer> FilterVehicles()
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

    private bool Save_allvehicles()
    {
      var json = JsonSerializer.Serialize(_allVehicles);
      System.IO.File.WriteAllText(VehiclesFilePath, json);
      return true;
    }

    public void Load_allVehicles()
    {
      if (string.IsNullOrEmpty(VehiclesFilePath))
      {
        //VehiclesFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ZusiStart\\vehiclesdata.json";
        VehiclesFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DataManager.localfoldername, "vehiclesdata.json");
      }

      if (System.IO.File.Exists(VehiclesFilePath))
      {
        var json = System.IO.File.ReadAllText(VehiclesFilePath);
        _allVehicles = JsonSerializer.Deserialize<List<VehicleGroup>>(json);
      }

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

        SearchResultTitle = value.DisplayName;
      }
    }

    public bool CheckImportance(Zug Selected_Train, Zug zug)
    {
      bool IsImportant;
      if (Selected_Train.Aufgleiszeit == null || Selected_Train.Abgleiszeit == null || zug.Aufgleiszeit == null || zug.Abgleiszeit == null)
      {
        IsImportant = true;
        //_log.Info("CheckImportance: One Time is NULL");
      }
      else
      {
        IsImportant = !(Selected_Train.Aufgleiszeit.Value > zug.Abgleiszeit.Value || Selected_Train.Abgleiszeit.Value < zug.Aufgleiszeit.Value);
        //if (IsImportant )
        //    _log.Info("CheckImportance TRUE: "+ Zug.Nummer.ToString() + "-" +Zug.Aufgleiszeit.ToString()+ "-" + zug.Abgleiszeit.ToString() + " -- " + Zug.Abgleiszeit.ToString() + "-" + zug.Aufgleiszeit.ToString());
        //else
        //    _log.Info("CheckImportance FALSE: " + Zug.Nummer.ToString() + "-" + Zug.Aufgleiszeit.ToString() + "-" + zug.Abgleiszeit.ToString() + " -- " + Zug.Abgleiszeit.ToString() + "-" + zug.Aufgleiszeit.ToString());
      }
      return IsImportant;
    }

    public bool check_for_FIS(Zug zug)
    {
      FIS_available = false;
      try
      {
        // ... via TCP interface
        //TimeTable tt = DataManager.Instance.GetTimeTableOfTrain(zug);

        ZugDatei? CurrentZugFile = zug.Parent as ZugDatei;

        if (CurrentZugFile != null)
        {
          string zusiDisplayFolder = Path.Combine(CurrentZugFile.Path, "..\\ZusiDisplay");
          if (Directory.Exists(zusiDisplayFolder))
          {
            foreach (string zda in Directory.EnumerateFiles(zusiDisplayFolder, "*.zda", SearchOption.AllDirectories))
            {
              ZusiDisplayFile zdaFile = new(zda);
              zdaFile.Parse(true);
              ZDLine line = zdaFile.Root;
              foreach (ZDTrack track in line.Tracks)
              {
                foreach (ZDTrains trains in track.Trains)
                {
                  foreach (ZDTrain train in trains.Trains)
                  {
                    string train_type = train.Type.Replace(" ", "_");
                    if (zug.Gattung.StartsWith(train_type) && zug.Nummer.Contains(train.Number))
                      FIS_available = true;
                  }
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        _log.Error(ex.ToString());
      }

      return FIS_available;
    }

    //---------------------------------------------------------------------
    public string BuildTempTimeTable(Zug Selected_Train, TimeTable timeTable, out string tempTimeTableFilename)
    {
      string result = null;
      DateTime? newStartTime = null;
      _log.Info(string.Format("BuildTempTimeTable for Train: {0} getstartet", Selected_Train.ToString()));
      // create temporary timetable folder
      //string tmpBaseFolder = Zusi.DataPath[2] + @"_ZusiData\Timetables\Temp\ZusiStart\";
      //string tmpBaseFolder = Zusi.DataPath[2] + @"Timetables\Temp\ZusiStart\";
      string tmpBaseFolder = Zusi.DataPath[2] + @"Temp\ZusiStart\TempTimetable\";

      string tempPath = System.IO.Path.GetTempPath();
      string tmp1 = Zusi.DataPath[1];
      string tmp2 = Zusi.DataPath[2];
      string tmp3 = Zusi.DataPath[3];
      //string tmpBaseFolder = Path.Combine(tempPath.ToString().EnsureTrailingBackslash(), "ZusiStart").EnsureTrailingBackslash();

      TimeTableFile? CurrentTimeTableFile = timeTable.Parent as TimeTableFile;

      string selectedTrainFilename = Selected_Train.GetDocument().Filename;
      string selectedTimeTableFilename = timeTable.GetDocument().Filename;


      if (Directory.Exists(tmpBaseFolder))
      {
        try
        {
          DirectoryInfo di = new(tmpBaseFolder);
          di.Clear();
        }
        catch (Exception ex)
        {
          _log.Error(ex.ToString());
        }
      }

      string tmpTimeTableName = "ZusiStartTempTimeTable"; // Guid.NewGuid().ToString("N");
      string tmpTrainsFolder = string.Format(@"{0}{1}\", tmpBaseFolder, tmpTimeTableName);
      try
      {
        Directory.CreateDirectory(tmpTrainsFolder);
      }
      catch (Exception ex)
      {
        _log.Error(ex.ToString());
        throw;
      }

      // temporary timetable name
      DataPathType dtp = DataPathType.Unknown;
      tmpTimeTableName = string.Format("{0}{1}.fpn", tmpBaseFolder, tmpTimeTableName);
      string tmpStrippedTimeTableName = Zusi.GetRelativePathOf(tmpTimeTableName, ref dtp);

      // clone temporary timetable from source
      TimeTable testTimeTable = new(null, CurrentTimeTableFile.Root, false);
      TimeTable tmpTimeTable = new(null, timeTable, false);

      // Fahrplandatei im Buchfahrplan
      dtp = DataPathType.Unknown;
      string orgRelativeTimetableName = Zusi.GetRelativePathOf(selectedTimeTableFilename, ref dtp);

      _allTimeTableTrains.Clear();
      //foreach (var g in AllTrains
      //        .Where(z => z.BelongsToTimeTable == Selected_Train.BelongsToTimeTable))
      //{
      //  _allTimeTableTrains.Add(g);
      //}
      var q = from t in timeTable.Trains
              where t.Link != null | t.Train != null
              select t;

      foreach (var train in q)
      {
        TimeTableState tts = new TimeTableState() { TimeTable = timeTable, Reference = train };

        if (tts.Reference.Link != null)
        {
          ZugDatei zd = new(tts.Reference.Link, tts.Reference.Link.Datei.FullPath);
          zd.Parse();
          Zug z = zd.Root;
          if (zd.Root != null)
          {
            z.BelongsToTimeTable = tts.TimeTable.ID;
            _allTimeTableTrains.Add(z);
          }
        }
        else
        {
          Zug z = tts.Reference.Train;
          if (z != null)
          {
            z.BelongsToTimeTable = tts.TimeTable.ID;
            _allTimeTableTrains.Add(z);
          }
        }
      }

      //foreach (Zug zug in _timeTableTrains)
      foreach (Zug zug in _allTimeTableTrains)
      {
        try
        {
          bool isImportant = CheckImportance(Selected_Train, zug);

          if (isImportant)
          {
            ZugDatei zd;
            string p;

            Zug tmpZug = new(null, zug);

            Datei d = tmpZug.FahrplanDatei;
            d.Dateiname = tmpStrippedTimeTableName;
            string zugFilename = zug.GetDocument().Filename;

            if (tmpZug.BuchfahrplanBMPDatei != null && zug.BuchfahrplanBMPDatei.Exists)
            {
              string s = System.IO.Path.GetFileName(tmpZug.BuchfahrplanBMPDatei.Dateiname);
              p = tmpTrainsFolder + s;
              dtp = DataPathType.Unknown;
              tmpZug.BuchfahrplanBMPDatei.Dateiname = Zusi.GetRelativePathOf(p, ref dtp);
              if (!System.IO.File.Exists(p))
              {
                System.IO.File.Copy(zug.BuchfahrplanBMPDatei.FullPath, p);
              }
            }

            string tmpTrainfileName;
            ZugDatei zugDatei = timeTable.GetDocument() as ZugDatei;
            if (zugDatei == null)
            {
              string s = tmpZug.Gattung ?? "";
              s += tmpZug.Nummer ?? "";
              if (string.IsNullOrEmpty(s))
              {
                s = Guid.NewGuid().ToString("N");
              }
              tmpTrainfileName = tmpTrainsFolder + s + ".trn";
              zd = new ZugDatei(tmpTrainfileName, tmpZug);
              tmpZug.NodeName = "Zug";
            }
            else
            {
              zd = new ZugDatei(zugDatei, tmpZug);
              string s = System.IO.Path.GetFileName(zugDatei.Filename);
              tmpTrainfileName = tmpTrainsFolder + s;
            }
            dtp = DataPathType.Unknown;
            string tmpRelativeTrainfileName = Zusi.GetRelativePathOf(tmpTrainfileName, ref dtp);

            if (tmpZug.BuchfahrplanRohDatei != null && zug.BuchfahrplanRohDatei.Exists)
            {
              string s = System.IO.Path.GetFileName(tmpZug.BuchfahrplanRohDatei.Dateiname);
              p = tmpTrainsFolder + s;
              dtp = DataPathType.Unknown;
              tmpZug.BuchfahrplanRohDatei.Dateiname = Zusi.GetRelativePathOf(p, ref dtp);

              if (true) //(zd == null)
              {
                System.IO.File.Copy(zug.BuchfahrplanRohDatei.FullPath, p, true);
              }
              else
              {
                dtp = DataPathType.Unknown;
                string orgRelativeTrainfileName = Zusi.GetRelativePathOf(zugFilename, ref dtp);

                using FileStream fs = new(zug.BuchfahrplanRohDatei.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using StreamReader sr = new(fs);
                string content = sr.ReadToEnd();
                content = content.Replace(orgRelativeTimetableName, tmpStrippedTimeTableName).Replace(orgRelativeTrainfileName, tmpRelativeTrainfileName);
                // Define the BOM for UTF-8
                byte[] utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };

                using FileStream wfs = new(p, FileMode.Create, FileAccess.Write, FileShare.Read);
                // Write the BOM to the file
                wfs.Write(utf8Bom, 0, utf8Bom.Length);

                // Write the XML content
                using StreamWriter sw = new(wfs, Encoding.UTF8);
                sw.Write(content);
                sw.Flush();
              }
            }

            //if (ti.IsLocoReplaced)
            //{
            //  // search for FZGVerbandAktion in all Fahrplaneintrag and replace 2 with 1
            //  foreach (FahrplanEintrag fpe in zd.Root.FahrplanEintraege)
            //  {
            //    if (fpe != null)
            //    {
            //      TrainSetActionType FZGVA = fpe.FzgVerbandAktion;
            //      if (FZGVA == TrainSetActionType.CabChange && ti.IsLocoInFront)
            //      {
            //        fpe.FzgVerbandAktion = TrainSetActionType.TurnTrain;
            //        FZGVA = TrainSetActionType.TurnTrain;
            //      }
            //    }
            //  }
            //  zd.Root.ReplaceTrain(ti.Reihung);
            //}
            //

            if (Selected_Train.Nummer == zd.Root.Nummer) // check if zd is selectedtrain
            {
              TrainItem ti = CurrentTrainItem;

              if (ti != null && ti.IsTrainReplaced)
              {
                if (ti.IsTrainTurned)
                {
                  //search for FZGVerbandAktion in all Fahrplaneintrag and replace 2 with 1
                  foreach (FahrplanEintrag fpe in zd.Root.FahrplanEintraege)
                  {
                    if (fpe != null)
                    {
                      TrainSetActionType FZGVA = fpe.FzgVerbandAktion;
                      if (FZGVA == TrainSetActionType.CabChange && ti.IsTrainTurned)
                      {
                        fpe.FzgVerbandAktion = TrainSetActionType.TurnTrain;
                        FZGVA = TrainSetActionType.TurnTrain;
                      }
                    }
                  }
                }

                zd.Root.ReplaceTrain(ti.ReplaceReihung);
              }
            }

            zd.SaveAs(tmpTrainfileName);

            if (zug.BuchfahrplanRohDatei?.Dateiname == Selected_Train.BuchfahrplanRohDatei?.Dateiname)
            {
              result = tmpTrainfileName;
            }

            TrainLink tl = ZusiObject.CreateNew<TrainLink>(tmpTimeTable, "Zug");
            tl.Datei = Datei.CreateNew(tl, tmpRelativeTrainfileName, false);
            tmpTimeTable.Trains.Add(new TrainReference(tl));

            if (tmpZug.StartTime is DateTime startTime)
            {
              startTime -= TimeSpan.FromMinutes(10);

              if (newStartTime == null)
              {
                newStartTime = startTime;
              }
              else if (startTime < newStartTime)
              {
                newStartTime = startTime;
              }
            }
            _log.Info(string.Format("BuildTempTimeTable: {0} erstellt ", zug.Gattung + zug.Nummer));

          }
        }
        catch (Exception ex)
        {
          _log.Error(string.Format("BuildTempTimeTable: {0} \nERROR {1} ", zug.Gattung + zug.Nummer, ex.ToString()));

        }

      }
      //ZusiDisplay anpassen
      string zusiDisplayFolder = Path.Combine(CurrentTimeTableFile.Path, "ZusiDisplay");
      if (Directory.Exists(zusiDisplayFolder))
      {
        string newZusiDisplayFolder = Path.Combine(tmpBaseFolder, "ZusiDisplay");

        dtp = DataPathType.Unknown;
        string currentRelativeTimetableName = Zusi.GetRelativePathOf(CurrentTimeTableFile.Filename, ref dtp).EnsureLeadingBackslash();
        dtp = DataPathType.Unknown;
        string tmpRelativeTimetableName = Zusi.GetRelativePathOf(tmpTimeTableName, ref dtp).EnsureLeadingBackslash();

        bool needsOtherFiles = false;
        foreach (string zda in Directory.EnumerateFiles(zusiDisplayFolder, "*.zda", SearchOption.AllDirectories))
        {
          try
          {
            ZusiDisplayFile zdaFile = new(zda);
            zdaFile.Parse(true);
            ZDLine line = zdaFile.Root;
            string infrastructurdaten_str = @"..\..\Infrastrukturdaten";
            string replace_str = @"\Timetables\Deutschland\Infrastrukturdaten";

            bool dirty = false;

            line.AnnounceBasicData = line.AnnounceBasicData?.Replace(infrastructurdaten_str, replace_str);
            line.AnnounceGenericGreetingsFile = line.AnnounceGenericGreetingsFile?.Replace(infrastructurdaten_str, replace_str);
            foreach (ZDTrack track in line.Tracks)
            {
              foreach (ZDTrains trains in track.Trains)
              {
                foreach (ZDTrain train in trains.Trains)
                {
                  train.IsNeeded = true; //_trains.Find(ti => ti.IsImportant && ti.Zug.Gattung == train.Type && ti.Zug.Nummer == train.Number) != null;
                  string train_type = train.Type.Replace(" ", "_");
                  if (Selected_Train.Gattung.Contains(train_type) && Selected_Train.Nummer.Contains(train.Number))
                    FIS_available = true;
                }

                if (trains.Timetable == currentRelativeTimetableName)
                {
                  trains.Timetable = tmpRelativeTimetableName;

                  dirty = true;
                }
                trains.AnnounceGreetings = trains.AnnounceGreetings?.Replace(infrastructurdaten_str, replace_str);
                trains.AnnounceNextStopFile = trains.AnnounceNextStopFile?.Replace(infrastructurdaten_str, replace_str);
                trains.AnnounceTrainType = trains.AnnounceTrainType?.Replace(infrastructurdaten_str, replace_str);
              }

              if (dirty)
              {
                if (!Directory.Exists(newZusiDisplayFolder))
                {
                  Directory.CreateDirectory(newZusiDisplayFolder);
                }

                string newZdaName = $"{newZusiDisplayFolder.EnsureTrailingBackslash()}{Path.GetFileNameWithoutExtension(tmpTimeTableName)}.zda";
                zdaFile.SaveAs(newZdaName);

                needsOtherFiles = true;
              }
            }
          }
          catch (Exception ex)
          {
            _log.Error(ex.ToString());
          }
        }

        if (needsOtherFiles)
        {
          DirectoryInfo dir = new(zusiDisplayFolder);
          FileInfo[] files = dir.GetFiles();
          foreach (FileInfo file in files)
          {
            if (file.Extension.ToLower() == ".zda")
            {
              continue;
            }

            string target = Path.Combine(newZusiDisplayFolder, file.Name);
            file.CopyTo(target, false);
          }
        }
      }

      //set new start time
      tmpTimeTable.StartTime = newStartTime;

      // save temporary timetable file
      //TimeTableFile CurrentTimeTableFile = timeTable.Parent as TimeTableFile;
      TimeTableFile tmpTimeTableFile = new(CurrentTimeTableFile, tmpTimeTable);
      //tmpTimeTableFile.Encoding = Encoding.UTF8;
      tmpTimeTableFile.Encoding = new UTF8Encoding(true);
      try
      {
        tmpTimeTableFile.SaveAs(tmpTimeTableName);
      }
      catch (Exception ex)
      {
        _log.Error(ex.ToString());
        result = "";
        Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, "Die Timetable kann nicht gespeichert werden", MessageBoxButton.OK, MessageBoxImage.Error);
      }

      // mit Fahrplan im Ganzen starten, wenn es sich um einen integrierten Fahrplan handelt
      if (result == null)
      {
        result = tmpTimeTableName;
      }

      //TransferServer.Instance.TransferDirectory(tmpBaseFolder, ZDStartParameter);
      tempTimeTableFilename = tmpTimeTableName;
      return result;
    }

  }
}
