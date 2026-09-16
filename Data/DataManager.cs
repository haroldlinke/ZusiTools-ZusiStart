#define NO_CACHE

//using CefSharp.DevTools.Page;
//using ZusiCLIProject.FileLibrary.Zusi3;
using AvalonDock.Layout.Serialization;
using CommunityToolkit.Mvvm.Input;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using log4net;
using Makaretu.Dns;
using Microsoft.VisualBasic.Logging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json.Linq;
using Sovoma;
using Sovoma.WPF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using Xceed.Wpf.Toolkit.Primitives;
using ZusiCLIProject.FileLibrary.Zusi3.ZusiFdl;
using ZusiCLIProject.Routegraph2;
using ZusiDisplayLib;

//using ZusiKlassenLib.TimeTable;
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
using ZusiStart.KI;
using ZusiStart.KlLib2;
//using ZusiPicLib;
using ZusiStart.Miscellaneous;
using ZusiStart.ViewModels;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static ZusiStart.Data.DataManager;

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

  public record UtmBounds(double MinE, double MinN, double MaxE, double MaxN);
  public record CanvasBounds(double MinX, double MinY, double MaxX, double MaxY);

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
    public bool New_RenderEngine { get; set; }
    public string Blickwinkel_value { get; set; }
    public bool RemoteZusi { get; set; }
    public string RemoteZusiIP { get; set; }
    public bool RemoteTrackingSupport { get; set; }
    public bool DecoTrain_Separate { get; set; }
    public bool DonotHideZusiStart { get; set; }
    public bool StartOnlySelectedTrain { get; set; }
    public bool Show_ZusiMeter_Data { get; set; }
    public string ZusiMeter_Standard_Layoutfile { get; set; }
    public string CurrentTrainCat { get; set; }
    public bool IsDecoTrainsAllowed { get; set; }
    public bool IsPersonTrainsAllowed { get; set; }
    public bool IsFreightTrainsAllowed { get; set; }
    public bool RO_starttime_no_decotrains { get; set; }
    public bool RO_trainselectioncriteria_Stations { get; set; }
    public bool RO_trainselection_Streckenmodule { get; set; }
    public bool RO_use_vorlaufzeit { get; set; }
    public int RO_vorlaufzeit { get; set; } = 0;
    public bool RO_use_nachlaufzeit { get; set; }
    public int RO_nachlaufzeit { get; set; } = 0;
    public string Buchfahrplanlayout { get; set; }

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
      New_RenderEngine = false;
      Blickwinkel_value = "45";
      RemoteZusi = false;
      RemoteZusiIP = "";
      DecoTrain_Separate = false;
      DonotHideZusiStart = false;
      StartOnlySelectedTrain = false;
      Show_ZusiMeter_Data = false;
      ZusiMeter_Standard_Layoutfile = "";
      CurrentTrainCat = "all";
      IsDecoTrainsAllowed = true;
      IsFreightTrainsAllowed = true;
      IsPersonTrainsAllowed = true;
      RO_starttime_no_decotrains = false;
      RO_trainselectioncriteria_Stations = false;
      RO_trainselection_Streckenmodule = false;
      Buchfahrplanlayout = "Automatisch aus TRN-Datei";
    }
  }

  //=========================================================================
  public class DataManager : DependencyObject, IDisposable, INotifyPropertyChanged
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

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private static DataManager? __instance = null;
    //private static readonly TrainGroupConverter _trainGroupConverter = new TrainGroupConverter();
    public DataLoaderWindow dataLoaderWindow { get; set; }
    public ZusiMeter.ZusiMeterControl zusiMeterControl;
    private ZusiLocalKiEngine _ki = null;

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
            @"ZusiStart"
    };

    public double ScreenScaleFactor = 1.0;

    public static readonly string localfoldername = "zusistart";
    private static readonly string _locosPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).EnsureTrailingBackslash() + @"ZusiStart\ReplacementLoco.xml";
    private static readonly string _trainsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).EnsureTrailingBackslash() + @"ZusiStart\ReplacementTrain.xml";
    public static string Filter_ZugNr = "";
    private readonly ObservableCollection<ReplacementTrain> _replacementTrains = new();
    private readonly ObservableCollection<ReplacementLoco> _replacementLocos = new();
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
    public ObservableCollection<ReplacementLoco> ReplacementLocos => _replacementLocos;
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
    public bool RO_show_optimised_Streckenmodule = false;

    // routegraph related
    public int utm_dx = 0;
    public int utm_dy = 0;
    public UtmBounds utmBounds = new UtmBounds(0, 0, 0, 0);
    public CanvasBounds canvasBounds = new CanvasBounds(0, 0, 0, 0);
    public string routeGraphOpenFile = "";


    public int SkipTimeTableSelectionUpdate = 0;
    public bool ZSK_Fadenkreuz_active = false;
    //public List<string> LaNumberList = new List<string>();
    public double spMax = 0;
    public string FilterZugNummer = "";
    public static string CurrentLanguage { get; set; } = "de";
    public static bool CheckLanguage = false;

    public static VehicleGroup? SearchVehicleGroupValue = null;
    public static string? SearchTrainValue = null;
    public HashSet<ulong> FoundTimeTableIds { get; set; } = null;

    public MainWindow main_window;
    public OptionsDlg optionsDlg_window;

    public RouteGraph2Control routeGraph2Control;
    //public GMapControl gmap { get; set; }
    public Controls.OSMGmapControl osmGmapControl;

    private string[] _foldersToExclude = Array.Empty<string>();

    /// <summary>
    /// Vom Anwender frei konfigurierbare, von der Suche auszuschließende Ordner.
    /// Wird dauerhaft in <see cref="ExcludedFoldersFilePath"/> als JSON gespeichert.
    /// </summary>
    public ObservableCollection<string> ExcludedFolders { get; } = new ObservableCollection<string>();
    public string ExcludedFoldersFilePath = "";

    private string[] _blackListedClasses = Array.Empty<string>();
    public readonly ObservableCollection<TimeTable> _allTimeTables = new();

    public List<string> usedStreckenElemente = new List<string>();
    private readonly ObservableCollection<TimeTableGroup> _groupedTimeTables = new();


#if SEQ
        private readonly ObservableCollection<Zug> _allTrains = new();
#else
    public readonly ConcurrentBag<Zug> _allTrains = new();
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
    public readonly ObservableCollection<FoundTimeTableViewModel> _foundTimeTables = new();
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

    public int original_traincount = 0;
    public int optimised_traincount = 0;
    public DateTime? original_starttime = DateTime.MinValue;
    public DateTime? optimized_starttime = DateTime.MinValue;
    public int original_modulecount = 0;
    public int optimized_modulecount = 0;

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
        new PropertyMetadata(true, OnIsDecoTrainsAllowedChanged));
    public bool IsDecoTrainsAllowed
    {
      get => (bool)GetValue(IsDecoTrainsAllowedProperty);
      set => SetValue(IsDecoTrainsAllowedProperty, value);
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty IsPersonTrainsAllowedProperty = DependencyProperty.Register(
        "IsPersonTrainsAllowed",
        typeof(bool),
        typeof(DataManager),
        new PropertyMetadata(true, OnIsDecoTrainsAllowedChanged));
    public bool IsPersonTrainsAllowed
    {
      get => (bool)GetValue(IsPersonTrainsAllowedProperty);
      set => SetValue(IsPersonTrainsAllowedProperty, value);
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty IsTrainListExpandedProperty = DependencyProperty.Register(
        "IsTrainListExpanded",
        typeof(bool),
        typeof(DataManager),
        new PropertyMetadata(true, OnIsTrainListExpandedChanged));
    public bool IsTrainListExpanded
    {
      get => (bool)GetValue(IsTrainListExpandedProperty);
      set => SetValue(IsTrainListExpandedProperty, value);
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty IsFreightTrainsAllowedProperty = DependencyProperty.Register(
        "IsFreightTrainsAllowed",
        typeof(bool),
        typeof(DataManager),
        new PropertyMetadata(true, OnIsDecoTrainsAllowedChanged));
    public bool IsFreightTrainsAllowed
    {
      get => (bool)GetValue(IsFreightTrainsAllowedProperty);
      set => SetValue(IsFreightTrainsAllowedProperty, value);
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty AllVehicles_loadedProperty = DependencyProperty.Register(
        "AllVehicles_loaded",
        typeof(bool),
        typeof(DataManager),
        new PropertyMetadata(false, OnAllVehicles_loadedChanged));
    public bool AllVehicles_loaded
    {
      get => (bool)GetValue(AllVehicles_loadedProperty);
      set => SetValue(AllVehicles_loadedProperty, value);
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
    public static readonly DependencyProperty SelectedZugProperty = DependencyProperty.Register(
        "SelectedZug",
        typeof(Zug),
        typeof(DataManager),
        new PropertyMetadata(null));
    public Zug? SelectedZug
    {
      get { return (Zug)GetValue(SelectedZugProperty); }
      set { SetValue(SelectedZugProperty, value); }
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
    public static readonly DependencyProperty TrainkindDecoProperty = DependencyProperty.Register(
        "TrainkindDeco",
        typeof(uint),
        typeof(DataManager),
        new PropertyMetadata(0u, OnFilterChanged));
    public uint TrainkindDeco
    {
      get { return (uint)GetValue(TrainkindDecoProperty); }
      set { SetValue(TrainkindDecoProperty, value); }
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

    ////---------------------------------------------------------------------
    //public static readonly DependencyProperty LoadingStatusProperty = DependencyProperty.Register(
    //    "LoadingStatus",
    //    typeof(string),
    //    typeof(DataManager),
    //    new PropertyMetadata(null));
    //public string LoadingStatus
    //{
    //  get { return (string)GetValue(LoadingStatusProperty); }
    //  set { SetValue(LoadingStatusProperty, value); }
    //}
    private string _loadingStatus;

    public string LoadingStatus
    {
      get => _loadingStatus;
      set
      {
        if (_loadingStatus != value)
        {
          _loadingStatus = value;
          OnPropertyChanged(nameof(LoadingStatus));
        }
      }
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


    public static readonly DependencyProperty SelectedLocoProperty = DependencyProperty.Register(
            "SelectedLoco",
            typeof(ReplacementLoco),
            typeof(DataManager),
            new PropertyMetadata(null));
    public ReplacementLoco? SelectedLoco
    {
      get => (ReplacementLoco)GetValue(SelectedLocoProperty);
      set => SetValue(SelectedLocoProperty, value);
    }

    public static readonly DependencyProperty SelectedReplacementLocoProperty = DependencyProperty.Register(
        "SelectedReplacementLoco",
        typeof(ReplacementLoco),
        typeof(DataManager),
        new PropertyMetadata(null, OnSelectedReplacementLocoChanged));
    public ReplacementLoco? SelectedReplacementLoco
    {
      get => (ReplacementLoco)GetValue(SelectedReplacementLocoProperty);
      set => SetValue(SelectedReplacementLocoProperty, value);
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
    public ConcurrentBag<Zug> AllTimeTableTrains { get { return _allTimeTableTrains; } }
    public ObservableCollection<TimeTableGroup> GroupedTimeTables { get { return _groupedTimeTables; } }
    public ObservableCollection<TimeTableRelation> Relations { get { return _relations; } }
    public ObservableCollection<TrainsViewModel> FoundTrains { get { return _foundTrains; } }
    public ObservableCollection<Notch> TrainKindNotches => _trainKindNotches;
    public ObservableCollection<FoundTimeTableViewModel> FoundTimeTables { get => _foundTimeTables; }
    public ObservableCollection<TrainsViewModel> TimeTableTrains { get => _timeTableTrains; }
    public RecentTrainsCollection RecentTrains { get => _recentTrains; }

    public ObservableCollection<string> LaNumberList { get { return _laNumberList; } }
    public bool WindowIsHidden = false;
    public Options options { get; set; }
    // Publicly accessible dictionary
    public Dictionary<string, string> Fpn2zsklinkDictionary { get; private set; }
    public bool FIS_available { get; set; }
    public string OptionsFilePath = "";
    public string AvalonDockLayoutFilePath = "";
    public string AvalonDockLayoutStandardFilePath = "";

    public string VehiclesFilePath = "";
    public string Fpn2zsklinkFilePath = "";

    public List<FahrzeugVariante> AllVariants1 => _allVariants;

    public event EventHandler DecoTrainsAllowed;
    public event EventHandler TrainListExpanded;
    public event EventHandler AllVehicles_loaded_eh;
    public event EventHandler SelectedTimeTableChanged;
    public event NotifyDataLoadStartedEventHandler NotifyDataLoadStarted;
    public event EventHandler NotifyDataLoadCompleted;
    private List<string> favorite_timetables;
    //public event ProgressChangedEventHandler ProgressChanged;

    public string tab_title_tracking = LocalizationManager.Translate("Tracking-OSM");
    public string tab_title_routegraph = LocalizationManager.Translate("Streckenplan");
    public string tab_title_bildfahrplan = LocalizationManager.Translate("Bildfahrplan");
    public string tab_title_docu = LocalizationManager.Translate("FPL-Docu");
    public string tab_title_Streckenkarte = LocalizationManager.Translate("Streckenkarte");
    public string tab_title_Zusi_DB = LocalizationManager.Translate("Zusi-DB");
    public string tab_title_La_Hdb = LocalizationManager.Translate("LA Handbuch");
    public string tab_title_Ersatzfahrplan = LocalizationManager.Translate("Ersatzfahrplan");
    public string tab_title_StreBu = LocalizationManager.Translate("Streckenbuch");
    public string tab_title_buchfahrplan = LocalizationManager.Translate("Buchfahrplan");
    public string tab_title_OeRilSK = LocalizationManager.Translate("ÖRil-SK");
    public string tab_title_favorites = LocalizationManager.Translate("Favoriten");
    public string tab_title_ZusiStartHdb = LocalizationManager.Translate("ZusiStart-Handbuch");
    public string FilterPlatzhalterText = LocalizationManager.Translate("Zugnummer oder Stationsname");
    //public string [] LODZugList = ["0","1","2","3"];
    public string LODZugList = "0123";

    public TrackingItem rg_trackingItem = null;
    public ICommand TTcopyPathCommand { get; }


    //---------------------------------------------------------------------
    public static void UpdateTimeTableRelations(List<TimeTable> timeTables, int preferredSelection)
    {
      _log.Debug("UpdateTimeTableRelations called with " + timeTables.Count + " timeTables and preferredSelection=" + preferredSelection);
      Instance?.UpdateTimeTableRelations_impl(timeTables, preferredSelection);
    }

    public class BetriebsstellenManager
    {
      public static BetriebsstellenManager Instance { get; } = new();

      public ObservableCollection<string> Betriebsstellen { get; set; } = new();

      public static bool betriebstelle_in_rot = false;
    }

    //-----------------------------------------------------------
    // Tabs List
    public ObservableCollection<TabViewModel> Tabs { get; set; }
    public List<string> used_streckenmodule { get; set; }

    //---------------------------------------------------------------------
    private DataManager()
    {
      _log.Debug("DataManager constructor called");
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

      LoadExcludedFolders();
      ReadReplacementTrains();
      ReadReplacementLocos();

      string zusistartdocu_relativePath = "help/" + LocalizationManager.Translate("ZusiStart_Docu_Deutsch.pdf");
      string zusistartdoc_absolutePath = Path.GetFullPath(zusistartdocu_relativePath);

      string zusistartquickdocu_relativePath = "Assets/" + LocalizationManager.Translate("ZusiStart_QuickDocu_de.html");
      string zusistartquickdoc_absolutePath = Path.GetFullPath(zusistartquickdocu_relativePath);

      //BetriebsstellenManager.Instance.Betriebsstellen.Add("Hagen Hbf");

      Tabs = new ObservableCollection<TabViewModel>
      {
          new TabViewModel(tab_title_docu, zusistartquickdoc_absolutePath,tooltip:LocalizationManager.Translate("Fahrplan Dokumentation")),
          new TabViewModel(tab_title_Streckenkarte, "https://www.zusi-sk.eu/#10.5/50.96983/6/",tooltip:LocalizationManager.Translate("Die Zusi Streckenkarte - Streckenauswahl durch <Stern>")),
          new TabViewModel(tab_title_Zusi_DB, "http://zusidatenbank.de/?zusistart",tooltip:LocalizationManager.Translate("Zusi Datenbank für komplexe Zugsuchen")),
          new TabViewModel(tab_title_buchfahrplan, "", isImageTab: true, tooltip:LocalizationManager.Translate("Buchfahrplan des ausgewählten Zuges")),
          new TabViewModel(tab_title_La_Hdb, @"Timetables\Deutschland\Infrastrukturdaten\Zusi_La.pdf", true, tooltip:LocalizationManager.Translate("Handbuch der Langsamfahrstellen")),
          new TabViewModel(tab_title_Ersatzfahrplan, @"Timetables\Deutschland\Infrastrukturdaten\Zusi_Ersatzfahrplan_999.pdf,Timetables\Deutschland\Infrastrukturdaten\Zusi_Ersatzfahrplan_996.pdf,Timetables\Deutschland\Infrastrukturdaten\Zusi_Ersatzfahrplan_993.pdf", true, tooltip:LocalizationManager.Translate("Ersatzfahrplanhefte dienen beim Vorbild als Rückfallebene für EBuLa ") ),
          new TabViewModel(tab_title_StreBu, @"Timetables\Deutschland\Infrastrukturdaten\Zusi_Oeril.pdf", true, tooltip:LocalizationManager.Translate("Örtliche Richtlinien / Angaben für das Streckenbuch für Zusi 3")),
          //new TabViewModel(tab_title_OeRilSK, @"Timetables\Deutschland\Infrastrukturdaten\Oeril_Sk-Signale.pdf", true),
          new TabViewModel(tab_title_ZusiStartHdb, zusistartdoc_absolutePath, true, tooltip:LocalizationManager.Translate("Das Zusi-Start Handbuch")),
          //new TabViewModel(tab_title_favorites, "", tooltip:LocalizationManager.Translate("Liste der Favoriten zum direkten Start in Zusi")),
          //new TabViewModel(tab_title_tracking, "", isIntro: true),
          //new TabViewModel(tab_title_tracking, "", isIntro: true, isroutegraph: false, isgmap: true),
          //new TabViewModel(tab_title_routegraph, "", isroutegraph: true),
          //new TabViewModel(tab_title_bildfahrplan, "", isbildfahrplan: true)
      };

      TTcopyPathCommand = new RelayCommand<TimeTableRelation>(TTcopyPath);

      //gmap = new GMapControl();


    }

    private void TTcopyPath(TimeTableRelation rel)
    {
      TimeTableFile timeTableFile = rel.TimeTable.Parent as TimeTableFile;
      string filename = timeTableFile.Filename;
      //Debug.WriteLine("CALLED: " + rel);
      AddToClipBoard(filename);
    }

    public async void set_websource(string tabtitle, string url)
    {
      _log.Debug($"set_websource called with tabtitle='{tabtitle}' and url='{url}'");
      // adapted to new concept
      var zdbTab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == tabtitle);
      if (zdbTab?.WebViewInstance?.CoreWebView2 != null)
      {
        //zdbTab.WebViewInstance.CoreWebView2.Navigate(value);
        zdbTab.WebViewInstance.Source = new Uri(url);
      }
    }

    //---------------------------------------------------------------------
    public void AcceptNewLoco()
    {
      _log.Debug("AcceptNewLoco called with SelectedLoco=" + (SelectedLoco != null ? SelectedLoco.Name : "null"));
      _replacementLocos.Add(SelectedLoco);
      SelectedReplacementLoco = SelectedLoco;
    }

    //---------------------------------------------------------------------
    public void ApplyChangesLoco()
    {
      _log.Debug("ApplyChangesLoco called with SelectedLoco=" + (SelectedLoco != null ? SelectedLoco.Name : "null") + " and SelectedReplacementLoco=" + (SelectedReplacementLoco != null ? SelectedReplacementLoco.Name : "null"));
      int ix = _replacementLocos.IndexOf(SelectedReplacementLoco);
      ReplacementLoco rl = new(SelectedLoco);
      _replacementLocos.RemoveAt(ix);
      _replacementLocos.Insert(ix, rl);
      SelectedReplacementLoco = rl;
    }

    //---------------------------------------------------------------------
    public void AcceptNewTrain()
    {
      _log.Debug("AcceptNewTrain called with SelectedTrainR=" + (SelectedTrainR != null ? SelectedTrainR.Name : "null"));
      _replacementTrains.Add(SelectedTrainR);
      SelectedReplacementTrain = SelectedTrainR;
    }

    //---------------------------------------------------------------------
    public void ApplyChangesTrain()
    {
      _log.Debug("ApplyChangesTrain called with SelectedTrainR=" + (SelectedTrainR != null ? SelectedTrainR.Name : "null") + " and SelectedReplacementTrain=" + (SelectedReplacementTrain != null ? SelectedReplacementTrain.Name : "null"));
      int ix = _replacementTrains.IndexOf(SelectedReplacementTrain);
      ReplacementTrain rt = new(SelectedTrainR);
      _replacementTrains.RemoveAt(ix);
      _replacementTrains.Insert(ix, rt);
      SelectedReplacementTrain = rt;
    }

    //---------------------------------------------------------------------
    public void RemoveSelectedLoco()
    {
      _log.Debug("RemoveSelectedLoco called with SelectedReplacementLoco=" + (SelectedReplacementLoco != null ? SelectedReplacementLoco.Name : "null"));
      if (SelectedReplacementLoco != null)
      {
        _replacementLocos.Remove(SelectedReplacementLoco);
      }
    }

    //---------------------------------------------------------------------
    private void ReadReplacementLocos()
    {
      _log.Debug("ReadReplacementLocos called with _locosPath='" + _locosPath + "'");
      if (System.IO.File.Exists(_locosPath))
      {
        try
        {
          ReplacementLocoFile file = new(_locosPath);
          file.ParseDocument += File_ParseDocument;
          file.Parse();
        }
        catch (Exception ex)
        {
          _log.Error(ex.ToString());
        }
      }
    }

    //---------------------------------------------------------------------
    public void SaveReplacementLocos()
    {
      _log.Debug("SaveReplacementLocos called with _locosPath='" + _locosPath + "'");
      ReplacementLocoFile file = new(_locosPath);
      file.SaveDocument += File_SaveDocument;
      file.Save();

      UpdateContextMenu();
    }

    //---------------------------------------------------------------------
    public void RemoveSelectedTrain()
    {
      _log.Debug("RemoveSelectedTrain called with SelectedReplacementTrain=" + (SelectedReplacementTrain != null ? SelectedReplacementTrain.Name : "null"));
      if (SelectedReplacementTrain != null)
      {
        _replacementTrains.Remove(SelectedReplacementTrain);
      }
    }

    //---------------------------------------------------------------------
    public void SaveReplacementTrains()
    {
      _log.Debug("SaveReplacementTrains called with _trainsPath='" + _trainsPath + "'");
      ReplacementTrainFile file = new(_trainsPath);
      file.SaveDocument += File_SaveDocument_Train;
      file.Save();

      UpdateContextMenu();
    }

    //---------------------------------------------------------------------
    private void ReadReplacementTrains()
    {
      _log.Debug("ReadReplacementTrains called with _trainsPath='" + _trainsPath + "'");
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

    public void SaveReplacedTrain()
    {
      _log.Debug("SaveReplacedTrain called with _trainsPath='" + _trainsPath + "'");
      Zug zug = DataManager.Instance.CurrentTrain;
      TimeTable timeTable = DataManager.Instance.GetTimeTableOfTrain(zug);
      if (timeTable == null)
      {
        TimeTableFile timetablefile = new TimeTableFile(zug.FahrplanDatei.FullPath);
        timetablefile.Parse();
        timeTable = timetablefile.Root;
      }

      ZusiDocumentBase? doc = timeTable?.GetDocument();
      BuildTempTimeTable3(zug, timeTable);
    }

    public void UndoReplacedTrain()
    {
      _log.Debug("UndoReplacedTrain called with _trainsPath='" + _trainsPath + "'");
      Zug zug = DataManager.Instance.CurrentTrain;
      TimeTable timeTable = DataManager.Instance.GetTimeTableOfTrain(zug);
      if (timeTable != null)
      {
        //TimeTableFile timetablefile = new TimeTableFile(zug.FahrplanDatei.FullPath);
        ZugDatei zd = zug.Parent as ZugDatei;
        string filename = zd.Filename;
        if (filename.StartsWith(Zusi.DataPath[4]))
        {
          if (System.IO.File.Exists(filename))
          {
            System.IO.File.Delete(filename);
          }
        }
      }
    }

    //---------------------------------------------------------------------
    private void UpdateContextMenu()
    {
      _log.Debug("UpdateContextMenu called");
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
    private void File_ParseDocument(object sender, ReadLocoFileEventArgs e)
    {
      _log.Debug("File_ParseDocument called");
      foreach (XElement x in e.Locos.Elements("loco"))
      {
        try
        {
          _replacementLocos.Add(new ReplacementLoco(x));
        }
        catch (ReplacementLocoException ex)
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
    private void File_SaveDocument(object sender, SaveLocoFileEventArgs e)
    {
      _log.Debug("File_SaveDocument called with " + _replacementLocos.Count + " replacement locos to save");
      _replacementLocos.ForEach(l => l.Save(e.Writer));
    }

    //---------------------------------------------------------------------
    private void File_ParseDocument_Train(object sender, ReadTrainFileEventArgs e)
    {
      _log.Debug("File_ParseDocument_Train called");
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
      _log.Debug("File_SaveDocument_Train called with " + _replacementTrains.Count + " replacement trains to save");
      _replacementTrains.ForEach(t => t.Save(e.Writer));
    }



    //---------------------------------------------------------------------
    private static void OnSelectedReplacementTrainChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      _log.Debug("OnSelectedReplacementTrainChanged called with new value=" + (e.NewValue != null ? ((ReplacementTrain)e.NewValue).Name : "null"));
      (d as DataManager)?.OnSelectedReplacementTrainChanged((ReplacementTrain)e.NewValue);
    }

    //---------------------------------------------------------------------
    private void OnSelectedReplacementTrainChanged(ReplacementTrain value)
    {
      _log.Debug("OnSelectedReplacementTrainChanged instance method called with value=" + (value != null ? value.Name : "null"));
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
    private static void OnSelectedReplacementLocoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      _log.Debug("OnSelectedReplacementLocoChanged called with new value=" + (e.NewValue != null ? ((ReplacementLoco)e.NewValue).Name : "null"));
      (d as DataManager)?.OnSelectedReplacementLocoChanged((ReplacementLoco)e.NewValue);
    }

    //---------------------------------------------------------------------
    private void OnSelectedReplacementLocoChanged(ReplacementLoco value)
    {
      _log.Debug("OnSelectedReplacementLocoChanged instance method called with value=" + (value != null ? value.Name : "null"));
      SelectedLoco = value != null ? new ReplacementLoco(value) : null;
    }


    //---------------------------------------------------------------------
    private static void OnCurrentTrainItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      _log.Debug("OnCurrentTrainItemChanged called with new value=" + (e.NewValue != null ? (((TrainItem)e.NewValue).Zug.Gattung + ((TrainItem)e.NewValue).Zug.Nummer) : "null"));
      (d as DataManager)?.OnCurrentTrainItemChanged((TrainItem)e.NewValue);
    }

    //---------------------------------------------------------------------
    private void OnCurrentTrainItemChanged(TrainItem value)
    {
      _log.Debug("OnCurrentTrainItemChanged instance method called with value=" + (value != null ? (value.Zug.Gattung + value.Zug.Nummer) : "null"));
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
      _log.Debug("Dispose called");
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    //---------------------------------------------------------------------
    public void InitializeData()
    {
#if true || !DEBUG
      _log.Debug("InitializeData called - starting data loading sequence");
      _allTimeTables.Clear();
      _allTrains.Clear();
      _allTimeTableTrains.Clear();
      _allVariants.Clear();
      _allVehicles.Clear();


      LoadTimeTableDataAsync();
      LoadVehicleDataAsync();
      FinishLoadingAsync();
      LoadGeneralOptions();
      LoadAvalonDockOptions();


#else
            OnNotifyDataLoadCompleted();
#endif
    }

    public void LoadAvalonDockOptions()
    {
      _log.Debug("LoadAvalonDockOptions called with AvalonDockOptionsFilePath='" + AvalonDockLayoutFilePath + "'");
      if (string.IsNullOrEmpty(AvalonDockLayoutFilePath))
      {
        //OptionsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ZusiStart\\config.json";
        AvalonDockLayoutFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DataManager.localfoldername, "AvalonDockLayout");
      }
      if (string.IsNullOrEmpty(AvalonDockLayoutStandardFilePath))
      {
        string zusistart_language_AvalonDockLayoutStandard_relativePath = "Assets/AvalonDockLayout_Std";
        AvalonDockLayoutStandardFilePath = Path.GetFullPath(zusistart_language_AvalonDockLayoutStandard_relativePath);
      }
      LoadDockLayout("A");
    }

    public void SaveDockLayout(string layouttype)
    {
      string filename = AvalonDockLayoutFilePath + "_" + layouttype + ".xml";

      var serializer = new XmlLayoutSerializer(DataManager.Instance.main_window.DockManager);
      serializer.Serialize(filename);
    }

    public void LoadDockLayout(string layouttype)
    {
      if (layouttype.StartsWith("Std_"))
      {
        LoadDockStandardLayout(layouttype);

      }
      else
      {
        string filename = AvalonDockLayoutFilePath + "_" + layouttype + ".xml";

        if (System.IO.File.Exists(filename))
        {
          var serializer = new XmlLayoutSerializer(DataManager.Instance.main_window.DockManager);
          serializer.Deserialize(filename);
        }
        else
        {
          LoadDockStandardLayout("Std_" + layouttype);
        }
      }
    }

    public void LoadDockStandardLayout(string layouttype)
    {
      layouttype = layouttype.Substring(4);
      string filename = AvalonDockLayoutStandardFilePath + "_" + layouttype + ".xml";
      if (System.IO.File.Exists(filename))
      {
        var serializer = new XmlLayoutSerializer(DataManager.Instance.main_window.DockManager);
        serializer.Deserialize(filename);
      }
    }

    public void LoadGeneralOptions()
    {
      _log.Debug("LoadGeneralOptions called with OptionsFilePath='" + OptionsFilePath + "'");
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
      main_window.SetCurrentTrainCat(options.CurrentTrainCat);

      IsDecoTrainsAllowed = options.IsDecoTrainsAllowed;
      IsPersonTrainsAllowed = options.IsPersonTrainsAllowed;
      IsFreightTrainsAllowed = options.IsFreightTrainsAllowed;

      main_window.ZSKButtonVisibility = options.Show_ZSK ? Visibility.Visible : Visibility.Collapsed;
      main_window.ZDBButtonVisibility = options.Show_ZDB ? Visibility.Visible : Visibility.Collapsed;
      main_window.BfpButtonVisibility = options.Show_Bfpl ? Visibility.Visible : Visibility.Collapsed;

      if (options.ZDB_Url == "https://www.zusidatenbank.de")
        options.ZDB_Url = "http://zusidatenbank.de/?zusistart";

      if (options.RemoteZusi)
      {
        ((RoutedUICommand)MainWindow.StartTrainCommand).Text = LocalizationManager.Translate("Remote Zug monitoren");
        DataManager.Instance.main_window.BtnStartTrain.Content = ((RoutedUICommand)MainWindow.StartTrainCommand).Text;
      }
      else
      {
        ((RoutedUICommand)MainWindow.StartTrainCommand).Text = LocalizationManager.Translate("Ausgewählten Zug fahren");
        DataManager.Instance.main_window.BtnStartTrain.Content = ((RoutedUICommand)MainWindow.StartTrainCommand).Text;
      }
    }

    public void SaveGeneralOptions()
    {
      _log.Debug("SaveGeneralOptions called with OptionsFilePath='" + OptionsFilePath + "'");
      optionsDlg_window.SaveOptions(programoptions: false);

      if (options != null)
      {
        options.CurrentTrainCat = main_window.GetCurrentTrainCat();
        options.IsDecoTrainsAllowed = IsDecoTrainsAllowed;
        options.IsFreightTrainsAllowed = IsFreightTrainsAllowed;
        options.IsPersonTrainsAllowed = IsPersonTrainsAllowed;

        var json = JsonSerializer.Serialize(options);
        System.IO.File.WriteAllText(OptionsFilePath, json);
      }
    }

    public void Loadfpn2zsk_link_json()
    {
      _log.Debug("Loadfpn2zsk_link_json called with Fpn2zsklinkFilePath='" + Fpn2zsklinkFilePath + "'");
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
      _log.Debug("Savefpn2zsk_link_json called with Fpn2zsklinkFilePath='" + Fpn2zsklinkFilePath + "'");
      if (options != null)
      {
        var json = JsonSerializer.Serialize(Fpn2zsklinkDictionary);
        System.IO.File.WriteAllText(Fpn2zsklinkFilePath, json);
      }
    }

    //---------------------------------------------------------------------
    public void ClearRecentTrains()
    {
      _log.Debug("ClearRecentTrains called");
      _recentTrains.Clear();
    }

    //---------------------------------------------------------------------
    public void SetFoundTimeTable(FoundTimeTable? value)
    {
      _log.Debug("SetFoundTimeTable called with value=" + (value != null ? value.TimeTable.Name : "null"));
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

    public static bool IsValidName(string name)
    {
      if (!string.IsNullOrEmpty(name))
      {
        //string s = name.ToLower();
        //if (s.StartsWith("- zbf") ||
        //    s.StartsWith("- zf") ||
        //    s.StartsWith("- kein ") ||
        //    s.StartsWith("sbk") ||
        //    s.StartsWith("va") ||
        //    s.StartsWith("ve") ||
        //    s.StartsWith("abzw") ||
        //    s.StartsWith("esig") ||
        //    s.StartsWith("asig") ||
        //    s.StartsWith("avsig") ||
        //    s.StartsWith("bksig") ||
        //    s.StartsWith("zsig") ||
        //    s.StartsWith("zvsig") ||
        //    s.StartsWith("bü") ||
        //    s.StartsWith("üs") ||
        //    s.StartsWith("lzb") ||
        //    s.StartsWith("- eingl") ||
        //    s.StartsWith("betriebs") ||
        //    s.StartsWith("strende") ||
        //    s.StartsWith("streckenende") ||
        //    s.StartsWith("ende") ||
        //    s.StartsWith("ri.") ||
        //    s.StartsWith("von ") ||
        //    s.StartsWith("nach ") ||
        //    s.StartsWith("aufgl"))
        //{
        //  return false;
        //}

        return true;
      }

      return false;
    }

    //---------------------------------------------------------------------
    public bool SearchBetriebsstelleTrain(string number)
    {
      _log.Debug("SearchBetriebsstelleTrain called with number='" + number + "'");
      List<TimeTable> foundTimeTables = new();


      foundTimeTables.Clear();
      CurrentTrain = null;
      CurrentTrainItem = null;



      FoundTimeTableViewModel vm = null;

      string n = number.Replace(" ", "").Trim().ToLower();
      var trains = _allTrains.Where(z =>
      {
        //return z.FahrplanEintraege.Any(f => f.Bestrst != null && string.Compare(n, f.Bestrst.Replace(" ", "").Trim(), true) == 0);
        return z.FahrplanEintraege.Any(f => f.Bestrst != null && DataManager.IsValidName(f.Bestrst) && f.Bestrst.Replace(" ", "").Trim().Contains(n, StringComparison.OrdinalIgnoreCase));
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
      _log.Debug("SearchTrain called with number='" + number + "'");
      _foundTimeTables.Clear();
      CurrentTrain = null;
      CurrentTrainItem = null;

      FoundTimeTableViewModel vm = null;



      bool show_decotrains = true; // IsDecoTrainsAllowed;

      if (number.StartsWith("BR") && number.Length > 3) // Suche nach Baureihe
      {
        number = number.Substring(2);

        string n = number.Replace(" ", "").Trim(); //.ToLower();
        var trains = _allTrains.Where(z =>
        {
          return (z.Fahrzeuge.ContainsVehicle(n));
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
      else
      {

        if (vm == null || vm.Children.Count == 0)  // suche Zugnummer
        {
          string n = number.Replace(" ", "").Trim().ToLower();
          var trains = _allTrains.Where(z =>
          {
            //return (string.Compare(n, (z.Nummer).Replace(" ", "").Trim(), true) == 0 ||
            //          string.Compare(n, (z.Gattung + z.Nummer).Replace(" ", "").Trim(), true) == 0 ||
            //          string.Compare(n, (z.Gattung).Replace(" ", "").Trim(), true) == 0
            //          );
            return (string.Equals(n, (z.Nummer).Replace(" ", "").Trim(), StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(n, (z.Gattung + z.Nummer).Replace(" ", "").Trim(), StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(n, (z.Gattung).Replace(" ", "").Trim(), StringComparison.OrdinalIgnoreCase)
                      );
            //return (
            //          (z.Gattung + z.Nummer).Replace(" ", "").Trim().Contains(n, StringComparison.OrdinalIgnoreCase)
            //          );
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
            return z.FahrplanEintraege.Any(f => f.Bestrst != null && DataManager.IsValidName(f.Bestrst) && f.Bestrst.Replace(" ", "").Trim().Contains(n, StringComparison.OrdinalIgnoreCase));
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

    public void UpdateFoundTimetables(IEnumerable<ZusiStart.Data.FoundTimeTable>? trains)
    {
      FoundTimeTableViewModel vm = null;
      
      _foundTimeTables.Clear();
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
      // FoundTimeTables nach Relations kopieren
      DataManager.Instance.Relations.Clear();
      int i = 1;
      foreach (var fts in DataManager.Instance.FoundTimeTables)
      {
        if (fts.Children.Count > 0)
        {
          foreach (FoundTimeTableViewModel ftm in fts.Children)
          {
            FoundTimeTable ft = ftm.Object;
            if (ft != null)
            {
              DataManager.Instance.Relations.Add(new TimeTableRelation(i++, ft.TimeTable));
            }
          }
        }
      }

      object sender = null;
      EventArgs e = null;
      main_window.DataManager_RefreshFilter(sender, e);

      //setgroupboxcolor("ExpFpl", "GrpFpl", System.Windows.Media.Brushes.Red, newtitle: DataManager.SearchTrainValue);
    }

    //---------------------------------------------------------------------
    private void Dispose(bool disposing)
    {
      _log.Debug("Dispose called with disposing=" + disposing);
      if (disposing)
      {
        Disposable.Dispose(ref _queueFinished);
        Disposable.Dispose(ref _waitTimeTables);
        Disposable.Dispose(ref _waitVehicleData);
        Disposable.Dispose(ref _waitVehicleFilter);

        __instance = null;
      }
    }

    /**************************/
    /* Search with KI support */
    /**************************/

    public class DataRoot
    {
      public List<FahrplanData> Fahrplaene { get; set; } = new();
      //public List<StreckenData> Strecken { get; set; } = new();
      //public List<FahrzeugData> Fahrzeuge { get; set; } = new();
    }

    public class FahrplanData
    {
      public string FahrplanName { get; set; }
      public string FahrplanDatei { get; set; }
      public List<ZugData> Zuege { get; set; } = new();
    }

    public class ZugData
    {
      public string Name { get; set; }
      public string Id { get; set; }
      public string FahrzeugDatei { get; set; }
      public List<string> Strecke { get; set; } = new();
    }

    public class StreckenData
    {
      public string StreckenName { get; set; }
      public string StreckenDatei { get; set; }
      public List<BetriebsstelleData> Betriebsstellen { get; set; } = new();
    }

    public class BetriebsstelleData
    {
      public string Name { get; set; }
      public double Km { get; set; }
      public string Stellwerk { get; set; }
    }

    public class FahrzeugData
    {
      public string FahrzeugDatei { get; set; }
      public string Baureihe { get; set; }
      public string Typ { get; set; }
      public int Vmax { get; set; }
      public int LeistungKW { get; set; }
      public string Bremse { get; set; }
    }

    public class KIRequest
    {
      public string Query { get; set; }
    }

    public class StellwerkInfo
    {
      public string Name { get; set; }
      public string Typ { get; set; }
    }


    public class Zusatzinfo
    {
      public List<StellwerkInfo> Stellwerke { get; set; } = new();
    }


    //public class KIResult
    //{
    //  public string Fahrplan_Name { get; set; }
    //  public string Fahrplan_Datei { get; set; }
    //  public string Zug_Name { get; set; }
    //  public string Zug_Id { get; set; }
    //  public string Fahrzeug_Baureihe { get; set; }
    //  public string? Fahrzeug_Datei { get; set; }
    //  public List<string> Strecke { get; set; } = new();
    //  public Zusatzinfo Zusatzinfo { get; set; } = new();
    //}


    public class KIResponse
    {
      public string Query { get; set; }
      public List<KIResult> Results { get; set; } = new();
      public string Summary { get; set; }
    }



    //public static FahrplanData ExtractFahrplan(string dateiPfad)
    //{
    //  var doc = XDocument.Load(dateiPfad);

    //  var fp = new FahrplanData
    //  {
    //    FahrplanName = Path.GetFileNameWithoutExtension(dateiPfad),
    //    FahrplanDatei = dateiPfad
    //  };

    //  foreach (var zugElem in doc.Descendants("Zug"))
    //  {
    //    var zug = new ZugData
    //    {
    //      Name = (string)zugElem.Attribute("Name"),
    //      Id = (string)zugElem.Attribute("ID"),
    //      FahrzeugDatei = (string)zugElem.Element("Fahrzeug")?.Attribute("Datei")
    //    };

    //    foreach (var bst in zugElem.Descendants("Betriebsstelle"))
    //    {
    //      var name = (string)bst.Attribute("Name");
    //      if (!string.IsNullOrEmpty(name))
    //        zug.Strecke.Add(name);
    //    }

    //    fp.Zuege.Add(zug);
    //  }

    //  return fp;
    //}

    public static StreckenData ExtractStrecke(string dateiPfad)
    {
      var doc = XDocument.Load(dateiPfad);

      var strecke = new StreckenData
      {
        StreckenName = Path.GetFileNameWithoutExtension(dateiPfad),
        StreckenDatei = dateiPfad
      };

      foreach (var bst in doc.Descendants("Betriebsstelle"))
      {
        strecke.Betriebsstellen.Add(new BetriebsstelleData
        {
          Name = (string)bst.Attribute("Name"),
          Km = double.TryParse((string)bst.Attribute("Km"), out var km) ? km : 0.0,
          Stellwerk = (string)bst.Attribute("Stellwerk")
        });
      }

      return strecke;
    }

    public static FahrzeugData ExtractFahrzeug(string dateiPfad)
    {
      var doc = XDocument.Load(dateiPfad);

      return new FahrzeugData
      {
        FahrzeugDatei = dateiPfad,
        Baureihe = (string)doc.Root.Attribute("Name"),
        Typ = (string)doc.Root.Attribute("Typ"),
        Vmax = int.TryParse((string)doc.Root.Attribute("Vmax"), out var vmax) ? vmax : 0,
        LeistungKW = int.TryParse((string)doc.Root.Attribute("Leistung"), out var kw) ? kw : 0,
        Bremse = (string)doc.Root.Attribute("Bremse")
      };
    }

    public static void BuildDataJson(string zusiRoot, string outputFile)
    {
      var data = new DataRoot();

      // Fahrpläne
      //foreach (var file in Directory.GetFiles(zusiRoot, "*.fpn", SearchOption.AllDirectories))
      //{
      //  data.Fahrplaene.Add(ExtractFahrplan(file));
      //}

      foreach (var timetable in DataManager.Instance._allTimeTables)
      {
        try
        {
          var fp = new FahrplanData
          {
            FahrplanName = timetable.Name,
            FahrplanDatei = timetable.FindParent<TimeTableFile>().Filename
          };
          foreach (var zug in timetable.Trains)
          {
            Zug train = zug.Train;

            if (train == null)
            {

              _log.Warn($"Train is null in timetable {timetable.Name}");
              continue;
            }

            var zugData = new ZugData
            {
              Name = train.Gattung + train.Nummer,
              Id = train.Nummer,
              //FahrzeugDatei = train.Fahrzeuge.Datei?.Dateiname
            };
            foreach (var bst in train.FahrplanEintraege)
            {
              if (!string.IsNullOrEmpty(bst.Bestrst))
                zugData.Strecke.Add(bst.Bestrst);
            }
            fp.Zuege.Add(zugData);
          }
          data.Fahrplaene.Add(fp);
        }
        catch (Exception ex)
        {
          _log.Error("Error while building FahrplanData: " + ex.ToString());
        }
      }


      //// Strecken
      //foreach (var file in Directory.GetFiles(zusiRoot, "*.ls3", SearchOption.AllDirectories))
      //{
      //  data.Strecken.Add(ExtractStrecke(file));
      //}
      //foreach (var file in Directory.GetFiles(zusiRoot, "*.str", SearchOption.AllDirectories))
      //{
      //  data.Strecken.Add(ExtractStrecke(file));
      //}

      //// Fahrzeuge
      //foreach (var file in Directory.GetFiles(zusiRoot, "*.fzp", SearchOption.AllDirectories))
      //{
      //  data.Fahrzeuge.Add(ExtractFahrzeug(file));
      //}

      // JSON schreiben
      var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
      System.IO.File.WriteAllText(outputFile, json);
    }

    public static void AnalyseZusiData4KI()
    {
      try
      {
        string zusiRoot = @"C:\Zusi3\KI";
        string KIDataFile = @"C:\Zusi3\KI\ZusiData4KI.json";
        BuildDataJson(zusiRoot, KIDataFile);
      }
      catch (Exception ex)
      {
        _log.Error("Error in AnalyseZusiData4KI: " + ex.ToString());
      }
    }

    private string BuildPrompt(string userQuery, string dataJson)
    {
      return $@"
Du bist eine Offline-KI für ZusiStart.
Du bekommst eine JSON-Datenbasis mit Fahrplänen, Strecken und Fahrzeugen.
Analysiere die Daten vollständig.
Beantworte die Nutzerfrage ausschließlich basierend auf diesen Daten.

Nutzerfrage:
{userQuery}

Datenbasis:
{dataJson}

Gib IMMER eine JSON-Antwort im Format:

{{
  ""query"": ""..."",
  ""results"": [...],
  ""summary"": ""...""
}}

Die results enthalten alle relevanten Treffer mit Quellenangaben.
Die summary ist kurz und präzise.
Keine Erklärungen außerhalb des JSON.
";

    }

    public class OllamaResponse
    {
      public string model { get; set; }
      public string created_at { get; set; }
      public string response { get; set; }
      public bool done { get; set; }
    }


    private KIResponse ParseLlmResponse(string llmOutput)
    {
      //int start = llmOutput.IndexOf("{");
      //int end = llmOutput.LastIndexOf("}");
      //string json = llmOutput.Substring(start, end - start + 1);
      //var kiResponse = JsonSerializer.Deserialize<KIResponse>(json);
      // 1. Ollama-Response parsen
      var ollama = JsonSerializer.Deserialize<OllamaResponse>(llmOutput);

      // 2. Der eigentliche KI-JSON-Block steckt HIER:
      string json = ollama.response;

      // 3. Jetzt erst KIResponse parsen
      var kiResponse = JsonSerializer.Deserialize<KIResponse>(json);
      return kiResponse;

    }

    public async System.Threading.Tasks.Task OnRequestReceived()
    {
      await ProcessKiRequest();
    }

    public async System.Threading.Tasks.Task ProcessKiRequest()
    {
      string KIDataFile = @"C:\Zusi3\KI\ZusiData4KI.json";
      string dataJson = System.IO.File.ReadAllText(KIDataFile);

      //string reqJson = System.IO.File.ReadAllText(@"C:\Zusi3\KI/request.json");
      //var req = JsonSerializer.Deserialize<KIRequest>(reqJson);

      KIRequest req = new KIRequest { Query = "Ich suche einen Zug, der nach Recklinghausen Hbf fährt" };

      string prompt = BuildPrompt(req.Query, dataJson);

      string llmOutput = await CallLlmAsync(prompt);

      var response = ParseLlmResponse(llmOutput);

      System.IO.File.WriteAllText(@"C:\Zusi3\KI/response.json",
          JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
    }

    private async Task<string> CallLlmAsync(string prompt)
    {
      using var client = new HttpClient();

      var request = new
      {
        model = "llama3:8b",
        prompt = prompt,
        stream = false
      };

      var response = await client.PostAsync(
          "http://localhost:11434/api/generate",
          new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
      );

      return await response.Content.ReadAsStringAsync();
    }

    public class OllamaTool
    {
      public string name { get; set; }
      public string description { get; set; }
      public object parameters { get; set; }
    }

    public class ZusiToolResult
    {
      public string Query { get; set; }

      // Liste der Treffer (Züge, Fahrzeuge, etc.)
      public List<KIResult> Results { get; set; } = new();

      // Zusammenfassung für die KI
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

    public void AddKIResultMessage(string message)
    {
      Application.Current.Dispatcher.InvokeAsync(() =>
      {
        main_window.tbxKIResult.AppendText("\n" + message);
        main_window.tbxKIResult.ScrollToEnd();
        _log.Debug("AddKIResultMessage: " + message);
      });
    }

    public async void OnSearchKI(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {
        main_window.tbxKIResult.Text = "...Suche gestartet...";

        string question = main_window.tbxKISearchText.Text;

        if (string.IsNullOrWhiteSpace(question))
        {
          MessageBox.Show("Bitte eine Frage eingeben.");
          return;
        }

        // KI starten
        //var answer = await System.Threading.Tasks.Task.Run(() => _ki.ProcessQuestionAsync(question));

        var answer = await System.Threading.Tasks.Task.Run(() => _ki.ProcessQuestionAsync2(question));

        // Antwort anzeigen
        //main_window.tbxKIResult.Text = answer;
        AddKIResultMessage("\n\n****************\n"+answer);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Fehler bei der KI-Abfrage: " + ex.Message);
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
      _log.Debug("OnFrictionPresetChanged called with value=" + value);
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
    private static void OnIsTrainListExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as DataManager)?.OnIsTrainListExpandedChanged((bool)e.NewValue);
    }

    //---------------------------------------------------------------------
    private void OnIsTrainListExpandedChanged(bool value)
    {
      ToggleExpansion(DataManager.Instance.TimeTableTrains, value);
      TrainListExpanded?.Invoke(this, EventArgs.Empty);
    }

    private void ToggleExpansion(IEnumerable<TrainsViewModel> items, bool expand)
    {
      foreach (var item in items)
      {
        item.IsExpanded = expand;
        ToggleExpansion(item.Children, expand);
      }
    }

    //---------------------------------------------------------------------
    private static void OnAllVehicles_loadedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as DataManager)?.OnAllVehicles_loadedChanged(/*(bool)e.NewValue*/);
    }

    //---------------------------------------------------------------------
    private void OnAllVehicles_loadedChanged(/*bool value*/)
    {
      AllVehicles_loaded_eh?.Invoke(this, EventArgs.Empty);
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
      _log.Debug("OnSelectedTimeTableChanged called with value=" + (value != null ? value.TimeTable.Name : "null"));
      _timeTableTrains.Clear();
      SearchTrainValue = null;

      //_allTimeTableTrains.Clear();
      if (value != null)
      {
        foreach (TrainsViewModel tvm in TrainsViewModel.BuildViewModel(value.TimeTable).Children)
        {
          _timeTableTrains.Add(tvm);
        }

        if (value.TimeTable != null && (value.TimeTable.Name.Contains("Augsburg")))
        {
          DataManager.Instance.main_window.add_oeril_sk_tab();
        }
        else
        {
          DataManager.Instance.main_window.remove_oeril_sk_tab();
        }

        //foreach (var g in AllTrains
        //       .Where(z => z.BelongsToTimeTable == value.TimeTable.ID))
        //{
        //  _allTimeTableTrains.Add(g);
        //}
      }
      // check for isexpandedall
      OnIsTrainListExpandedChanged(IsTrainListExpanded);
      SelectedTimeTableChanged?.Invoke(this, EventArgs.Empty);
    }

    //---------------------------------------------------------------------
    private static void OnSelectedLaNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      _log.Debug("OnSelectedLaNumberChanged called with new value=" + (e.NewValue != null ? ((TimeTableRelation)e.NewValue).TimeTable.Name : "null"));
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
      _log.Debug("OnSelectedRecentTrainChanged called with new value=" + (e.NewValue != null ? ((RecentTrain)e.NewValue).TimeTableName : "null"));
      (d as DataManager)?.OnSelectedRecentTrainChanged((RecentTrain)e.NewValue);
    }

    //---------------------------------------------------------------------
    private void OnSelectedRecentTrainChanged(RecentTrain value)
    {
      _log.Debug("OnSelectedRecentTrainChanged instance method called with value=" + (value != null ? value.TimeTableName : "null"));
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
      _log.Debug("OnNotifyDataLoadStarted called with loaderType=" + loaderType);
      Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action<LoaderType>((t) =>
      {
        NotifyDataLoadStarted?.Invoke(this, new NotifyDataLoadStartedEventArgs(t));
      }), loaderType);
    }

    //---------------------------------------------------------------------
    public void OnNotifyDataLoadCompleted()
    {
      _log.Debug("OnNotifyDataLoadCompleted called");
      NotifyDataLoadCompleted?.Invoke(this, EventArgs.Empty);
    }

    //---------------------------------------------------------------------
    private async void FinishLoadingAsync()
    {
      _log.Debug("FinishLoadingAsync called - waiting for time tables and vehicle data to be loaded");
      DataLoaderResult result = await System.Threading.Tasks.Task.Run(() =>
      {
        return WaitHandle.WaitAll(new WaitHandle[] { _waitTimeTables, _waitVehicleData }) ? RebuildFilter() : null;
      });

      //DummyWindow dummywindow=new DummyWindow();
      //dummywindow.Show();
      //dataLoaderWindow.SetLoaderType(LoaderType.LoadVehicleGroups);
      //dataLoaderWindow.UpdateLayout();



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

      //_log.Debug("loading data completed");
    }


    struct TimeTableState
    {
      public TimeTable TimeTable { get; set; }
      public TrainReference Reference { get; set; }
    }

    //---------------------------------------------------------------------
    private async void LoadTimeTableDataAsync()
    {
      _log.Debug("LoadTimeTableDataAsync called");
      await System.Threading.Tasks.Task.Run(() =>
      {
        _log.Debug("load time tables asynch 1");

        Application.Current.Dispatcher.Invoke(() =>
        {
          LoadingStatus = "Loading...";
        });

        DateTime dtStart = DateTime.Now;
        _nQueuedItems = 0;

        OnNotifyDataLoadStarted(LoaderType.LoadComplete);

        using CancellationTokenSource cts = new();
        TimeTables.init_timetabledata();
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
        _log.Debug($"loading time tables asynch 1 completed ({duration.TotalMilliseconds} ms)");
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
      _log.Debug("LoadTimeTableDataAsync2 called");
      await System.Threading.Tasks.Task.Run(() =>
      {
        DataLoaderResult result;
        _log.Debug("load time tables asynch 2");

        favorite_timetables = _recentTrains.LoadTimeTableListFromFile();

        DateTime dtStart = DateTime.Now;
        _nQueuedItems = 0;

        OnNotifyDataLoadStarted(LoaderType.LoadComplete);

        using CancellationTokenSource cts = new();
        //TimeTables.EnumerateTimeTables(_foldersToExclude, cts.Token).ForEach(t => _allTimeTables.Add(t));
        _log.Debug("load time tables asynch 2: Load timetables");
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
                    _log.Debug($"load time tables asynch 2: {tt.Name}");
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

        _log.Debug($"loading time tables  asynch 2 completed ({duration.TotalMilliseconds} ms)");
      });

      // continue with vehicle data, if not switched off
      if (true)
      {
        LoadVehicleDataAsync2();
      }

      if (false) // continue with KI-data, if not switched off
      {
        _ki = new ZusiLocalKiEngine();
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
            if (!string.IsNullOrEmpty(tts.Reference.Link.Datei.FullPath))
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
              _log.Error("tts.Reference.Link.Datei.FullPath is empty:" + tts.TimeTable.Name);
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
      _log.Debug("LoadVehicleDataAsync called");
      await System.Threading.Tasks.Task.Run(() =>
      {
        _log.Debug("load vehicle data 1");

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
        _log.Debug($"loading vehicle data 1 completed ({duration.TotalMilliseconds} ms)");
      });

      _waitVehicleData.Set();
    }

    private async void LoadVehicleDataAsync2()
    {
      _log.Debug("LoadVehicleDataAsync2 called");
      await System.Threading.Tasks.Task.Run(async () =>
      {
        Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
        _log.Debug("load vehicle data 2 asynch");

        DateTime dtStart = DateTime.Now;

        EnsureBlacklistedClasses();

        List<Fahrzeug> vehicles = Fahrzeuge.EnumerateVehicles(null, new CancellationTokenSource().Token);
        List<FahrzeugVariante> variants = new();

        foreach (Fahrzeug f in vehicles
                  .Where(v => (v.Kind & VehicleKind.IsPowered) != 0 && !IsBlacklistedClass(v)))
        //foreach (Fahrzeug f in vehicles
        //          .Where(v => (v.Kind & VehicleKind.IsPowered) != 0 && v.Name == "611" && !IsBlacklistedClass(v)))
        {
          _log.Debug("load vehicle data 2 asynch - Vehicle:" + f.Name);
          f.Varianten.ForEach(fv =>
                {
                  if (/*!fv.Dekozug &&*/ !fv.DateiFuehrerstand[0].IsEmpty)
                  {
                    variants.Add(fv);
                  }
                  AllVariants1.Add(fv);
                });
        }
        // Yield control to UI thread
        await System.Threading.Tasks.Task.Delay(10);

        //ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(12));

        _log.DebugFormat("{0} vehicle variants loaded", AllVariants1.Count);

        _log.Debug("grouping vehicles by class");

        _allVehicles = variants.GroupBy(v =>
              {
                ClassFamily cf = ClassFamilies.First(v.BR);
                return cf != null ? cf.Name : v.BR;
              }).Select(g => new VehicleGroup(g.Key, g.ToList())).ToList();

        //ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(12));

        DateTime dtEnd = DateTime.Now;
        TimeSpan duration = dtEnd - dtStart;
        _log.Debug($"loading vehicle data 2 asynch completed ({duration.TotalMilliseconds} ms)");
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
    /// <summary>
    /// Lädt die vom Anwender ausgeschlossenen Ordner aus der JSON-Konfigurationsdatei
    /// und baut anschließend die effektive Ausschlussliste (<see cref="_foldersToExclude"/>) neu auf.
    /// </summary>
    public void LoadExcludedFolders()
    {
      if (string.IsNullOrEmpty(ExcludedFoldersFilePath))
      {
        ExcludedFoldersFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DataManager.localfoldername, "excludedFolders.json");
      }

      _log.Debug("LoadExcludedFolders called with ExcludedFoldersFilePath='" + ExcludedFoldersFilePath + "'");

      ExcludedFolders.Clear();

      try
      {
        if (System.IO.File.Exists(ExcludedFoldersFilePath))
        {
          string json = System.IO.File.ReadAllText(ExcludedFoldersFilePath);
          List<string> loaded = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
          foreach (string folder in loaded)
          {
            if (!ExcludedFolders.Contains(folder))
            {
              ExcludedFolders.Add(folder);
            }
          }
        }
      }
      catch (Exception ex)
      {
        _log.Error("Fehler beim Laden der ausgeschlossenen Ordner: " + ex.Message);
      }

      RebuildFoldersToExclude();
    }

    //---------------------------------------------------------------------
    /// <summary>
    /// Speichert die aktuell in <see cref="ExcludedFolders"/> enthaltenen Ordner dauerhaft als JSON
    /// und aktualisiert die effektive Ausschlussliste.
    /// </summary>
    public void SaveExcludedFolders()
    {
      if (string.IsNullOrEmpty(ExcludedFoldersFilePath))
      {
        ExcludedFoldersFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DataManager.localfoldername, "excludedFolders.json");
      }

      _log.Debug("SaveExcludedFolders called with ExcludedFoldersFilePath='" + ExcludedFoldersFilePath + "'");

      try
      {
        string directory = Path.GetDirectoryName(ExcludedFoldersFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
          Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(ExcludedFolders.ToList(), new JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(ExcludedFoldersFilePath, json);
      }
      catch (Exception ex)
      {
        _log.Error("Fehler beim Speichern der ausgeschlossenen Ordner: " + ex.Message);
      }

      RebuildFoldersToExclude();
    }

    //---------------------------------------------------------------------
    /// <summary>
    /// Fügt einen Ordner zur Ausschlussliste hinzu (sofern noch nicht vorhanden) und speichert die Liste.
    /// </summary>
    public void AddExcludedFolder(string folder)
    {
      if (string.IsNullOrWhiteSpace(folder))
      {
        return;
      }

      if (!ExcludedFolders.Any(f => string.Equals(f, folder, StringComparison.OrdinalIgnoreCase)))
      {
        ExcludedFolders.Add(folder);
        SaveExcludedFolders();
      }
    }

    //---------------------------------------------------------------------
    /// <summary>
    /// Entfernt einen Ordner aus der Ausschlussliste und speichert die Liste.
    /// </summary>
    public void RemoveExcludedFolder(string folder)
    {
      if (ExcludedFolders.Remove(folder))
      {
        SaveExcludedFolders();
      }
    }

    //---------------------------------------------------------------------
    /// <summary>
    /// Baut die intern für die Suche verwendete Ausschlussliste aus den fest einprogrammierten
    /// Ordnern (<see cref="__foldersToExclude"/>) und den vom Anwender konfigurierten
    /// Ordnern (<see cref="ExcludedFolders"/>) neu auf.
    /// </summary>
    private void RebuildFoldersToExclude()
    {
      _foldersToExclude = ArrayEx.SafeConcat(__foldersToExclude, ExcludedFolders.ToArray());
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
      _log.Debug("RebuildFilter called");
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
      _log.Debug("UpdateTimeTableRelations_impl called with " + (timeTables != null ? timeTables.Count.ToString() : "null") + " time tables and preferred selection=" + preferredSelection);
      SelectedTimeTableRelation = null;

      DataManager.Instance.main_window.setgroupboxcolor("ExpFpl", "GrpFpl", System.Windows.Media.Brushes.White);
      SearchVehicleGroupValue = null;
      SearchTrainValue = null;

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
          DataManager.Instance.set_websource(DataManager.Instance.tab_title_docu, SelectedTimeTableRelation.Begruessungsdatei);
        }
        if (!string.IsNullOrEmpty(SelectedTimeTableRelation.TimeTable.GetDocument().Filename))
        {

          DataPathType dtp = DataPathType.Unknown;
          string orgRelativeTimetableName = Zusi.GetRelativePathOf(SelectedTimeTableRelation.TimeTable.GetDocument().Filename, ref dtp);
          orgRelativeTimetableName = orgRelativeTimetableName.Replace("\\", "%5C");
          string url = "http://zusidatenbank.de/fahrplan/" + orgRelativeTimetableName;
          DataManager.Instance.set_websource("Zusi-DB", url);
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
      _log.Debug("FilterVehicles called");
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

            switch (TrainkindDeco)
            {
              case 1:
                res &= !vg.Variant.Dekozug;
                break;
              case 2:
                res &= vg.Variant.Dekozug;
                break;
              default:
                //_log.Warn("Vehicle has no train kind:" + v.ToString());
                break;
            }

            return res;
          })
          .Select(vg => new VehicleContainer(this, vg))
          .OrderBy(vc => vc.DisplayName);
    }

    private bool Save_allvehicles()
    {
      _log.Debug("Save_allvehicles called");
      var json = JsonSerializer.Serialize(_allVehicles);
      System.IO.File.WriteAllText(VehiclesFilePath, json);
      return true;
    }

    public void Load_allVehicles()
    {
      _log.Debug("Load_allVehicles called");
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
      _log.Debug("OnCurrentVehicleContainerChanged called with value=" + (value != null ? value.DisplayName : "null"));
      _foundTimeTables.Clear();

      if (value != null)
      {
        SearchVehicleGroupValue = value.Group;
        SearchTrainValue = null;
        main_window.setgroupboxcolor("ExpFpl", "GrpFpl", System.Windows.Media.Brushes.Red, newtitle: value.DisplayName);

        var trains = _allTrains.Where(z => z.Fahrzeuge.ContainsVehicle(SearchVehicleGroupValue.AllVariants))
            .GroupBy(g => g.BelongsToTimeTable)
            .Select(g => new FoundTimeTable()
            {
              TimeTable = _allTimeTables.First(tt => tt.ID == g.Key),
              Trains = g.ToList()
            });

        FoundTimeTableViewModel vm = FoundTimeTableViewModel.BuildViewModel(trains);
        vm.Children.ForEach(c => _foundTimeTables.Add(c));

        SearchResultTitle = value.DisplayName;

        // FoundTimeTables nach Relations kopieren
        DataManager.Instance.Relations.Clear();
        int i = 1;
        foreach (var fts in _foundTimeTables)
        {
          if (fts.Children.Count > 0)
          {
            foreach (FoundTimeTableViewModel ftm in fts.Children)
            {
              FoundTimeTable ft = ftm.Object;
              if (ft != null)
              {
                DataManager.Instance.Relations.Add(new TimeTableRelation(i++, ft.TimeTable));
              }
            }
          }
        }

      }
    }

    //---------------------------------------------------------------------
    public void SearchBR(string value)
    {
      _log.Debug("OnSearchBR called with value=" + value);
      _foundTimeTables.Clear();

      DataManager.SearchTrainValue = "br" + value;
      main_window.setgroupboxcolor("ExpFpl", "GrpFpl", System.Windows.Media.Brushes.Red, newtitle: DataManager.SearchTrainValue);

      if (value != "")
      {
        var trains = _allTrains.Where(z => z.Fahrzeuge.ContainsVehicle(value))
            .GroupBy(g => g.BelongsToTimeTable)
            .Select(g => new FoundTimeTable()
            {
              TimeTable = _allTimeTables.First(tt => tt.ID == g.Key),
              Trains = g.ToList()
            });

        FoundTimeTableViewModel vm = FoundTimeTableViewModel.BuildViewModel(trains);
        vm.Children.ForEach(c => _foundTimeTables.Add(c));

        SearchResultTitle = value;

        // FoundTimeTables nach Relations kopieren
        DataManager.Instance.Relations.Clear();
        int i = 1;
        foreach (var fts in _foundTimeTables)
        {
          if (fts.Children.Count > 0)
          {
            foreach (FoundTimeTableViewModel ftm in fts.Children)
            {
              FoundTimeTable ft = ftm.Object;
              if (ft != null)
              {
                DataManager.Instance.Relations.Add(new TimeTableRelation(i++, ft.TimeTable));
              }
            }
          }
        }

      }
    }

    public bool CheckImportance(Zug Selected_Train, Zug zug)
    {
      _log.Debug("CheckImportance called for Selected_Train=" + (Selected_Train != null ? Selected_Train.Nummer : "null") + " and zug=" + (zug != null ? zug.Nummer : "null"));
      string LODZug;
      if (string.IsNullOrEmpty(zug.LODZug))
      {
        LODZug = "0";
      }
      else
      {
        LODZug = zug.LODZug;
      }
      if (!DataManager.Instance.LODZugList.Contains(LODZug))
      {
        return false;
      }

      DateTime? zug_Aufgleiszeit = zug.Aufgleiszeit;
      DateTime? zug_Abgleiszeit = zug.Abgleiszeit;
      DateTime? Selected_Aufgleiszeit = Selected_Train.Aufgleiszeit.Value;
      DateTime? Selected_Abgleiszeit = Selected_Train.Abgleiszeit.Value;

      bool IsImportant;
      if (Selected_Aufgleiszeit == null || Selected_Abgleiszeit == null || zug_Aufgleiszeit == null || zug_Abgleiszeit == null)
      {
        IsImportant = true;
      }
      else
      {
        //if (Selected_Aufgleiszeit < Selected_Abgleiszeit)
        //// Zug fährt über Mitternacht, daher Zeiten vor der Aufgleiszeit um einen Tag erhöhen
        //{
        //  if (zug_Aufgleiszeit < Selected_Aufgleiszeit)
        //  {
        //    zug_Aufgleiszeit = zug_Aufgleiszeit.Value.AddDays(1);
        //  }
        //  if (zug_Abgleiszeit < Selected_Aufgleiszeit)
        //  {
        //    zug_Abgleiszeit = zug_Abgleiszeit.Value.AddDays(1);
        //  }
        //  Selected_Abgleiszeit = Selected_Abgleiszeit.Value.AddDays(1);
        //}

        IsImportant = !(Selected_Aufgleiszeit > zug_Abgleiszeit || Selected_Train.Abgleiszeit.Value < zug_Aufgleiszeit);
      }

      //if (IsImportant && DataManager.Instance.options.RO_trainselectioncriteria_Stations)
      //{
      //  List<string> found_bs = zug.checkBetriebstellen(BetriebsstellenManager.Instance.Betriebsstellen.ToList());
      //  if (found_bs.Count == 0)
      //  {
      //    IsImportant = false;
      //  }
      //}
      return IsImportant;
    }

    public void add_used_streckenmodule(List<string> usedmodules)
    {
      foreach (string m in usedmodules)
      {
        if (!DataManager.Instance.used_streckenmodule.Contains(m))
        {
          DataManager.Instance.used_streckenmodule.Add(m);
        }
      }
    }

    public bool CheckImportance2(ZusiCLIProject.FileLibrary.Zusi3.Zug Selected_Train, ZusiCLIProject.FileLibrary.Zusi3.Zug zug, ZusiFdl2 fdl2, List<string> seltrain_usedmodules)
    {
      _log.Debug("CheckImportance2 called for Selected_Train=" + (Selected_Train != null ? Selected_Train.Nummer : "null") + " and zug=" + (zug != null ? zug.Nummer : "null"));
      string LODZug = "0";
      try
      {
        LODZug = zug.LODzug.ToString();
      }
      catch (Exception ex)
      {
        _log.Error("Error accessing LODZug of Zug " + zug.Nummer.ToString() + ": " + ex.ToString());
        LODZug = "0";
      }

      if (!DataManager.Instance.LODZugList.Contains(LODZug))
      {
        return false;
      }

      DateTime? zug_Aufgleiszeit = zug.GetAufgleiszeit();
      DateTime? zug_Abgleiszeit = zug.GetAbgleiszeit();
      DateTime? Selected_Aufgleiszeit = Selected_Train.GetAufgleiszeit();
      DateTime? Selected_Abgleiszeit = Selected_Train.GetAbgleiszeit();
      if (Selected_Aufgleiszeit != null)
        Selected_Aufgleiszeit = Selected_Aufgleiszeit.Value.AddMinutes(-DataManager.Instance.options.RO_vorlaufzeit);
      if (Selected_Abgleiszeit != null)
        Selected_Abgleiszeit = Selected_Abgleiszeit.Value.AddMinutes(DataManager.Instance.options.RO_nachlaufzeit);

      bool IsImportant = false;
      if (Selected_Aufgleiszeit == null || Selected_Abgleiszeit == null || zug_Aufgleiszeit == null || zug_Abgleiszeit == null)
      {
        IsImportant = true;
      }
      else
      {

        IsImportant = !(Selected_Aufgleiszeit > zug_Abgleiszeit || Selected_Abgleiszeit < zug_Aufgleiszeit);
      }

      if (IsImportant && DataManager.Instance.options.RO_trainselectioncriteria_Stations)
      {
        //List<string> found_bs = zug.checkBetriebstellen(BetriebsstellenManager.Instance.Betriebsstellen.ToList());
        //if (found_bs.Count == 0)
        //{
        //  IsImportant = false;
        //}
        if (seltrain_usedmodules.Count() > 0 && fdl2 != null)
        {
          List<string> all_used_modules;
          List<string> usedmodules = zug.getusedmodules(fdl2, Selected_Aufgleiszeit, Selected_Abgleiszeit, out all_used_modules);
          if (usedmodules.Intersect(seltrain_usedmodules).Count() == 0)
          {
            IsImportant = false;
          }
          else
          {
            add_used_streckenmodule(all_used_modules);
            IsImportant = true;
          }
        }
        else
        {
          IsImportant = true;
        }
      }

      return IsImportant;
    }

    public bool check_for_FIS(ZusiKlassenLib2.Fahrplan.Zug zug)
    {
      _log.Debug("check_for_FIS called for zug=" + (zug != null ? zug.Nummer : "null"));
      FIS_available = false;
      try
      {
        ZusiKlassenLib2.Fahrplan.ZugDatei? CurrentZugFile = zug.Parent as ZusiKlassenLib2.Fahrplan.ZugDatei;

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
    public string BuildTempTimeTable(ZusiKlassenLib2.Fahrplan.Zug Selected_Train, TimeTable timeTable, out string tempTimeTableFilename)
    {
      _log.Debug("BuildTempTimeTable called for Selected_Train=" + (Selected_Train != null ? Selected_Train.Nummer : "null") + " and timeTable=" + (timeTable != null ? timeTable.Name : "null"));
      tempTimeTableFilename = "";
      if ((Properties.Settings.Default.TrainStartMode == 0) && (CurrentTrainItem == null || (!CurrentTrainItem.IsLocoReplaced && !CurrentTrainItem.IsTrainReplaced)) && (DataManager.Instance.options.RO_trainselectioncriteria_Stations))
      {

        try
        {
          string tmpzugfilename = BuildTempTimeTable2(Selected_Train, timeTable, out tempTimeTableFilename);
          if (!string.IsNullOrEmpty(tempTimeTableFilename))
          {
            return tmpzugfilename;
          }
        }
        catch (Exception ex)
        {
          _log.Error("Error in BuildTempTimeTable2: " + ex.ToString());
          _log.Error("Continue with BuildTempTimeTable");
        }
      }

      string result = null;
      DateTime? newStartTime = null;
      _log.Debug(string.Format("BuildTempTimeTable for Train: {0} getstartet", Selected_Train.ToString()));
      // create temporary timetable folder


      string tmpBaseFolder = ZusiKlassenLib2.Zusi.DataPath[2] + @"Temp\ZusiStart\TempTimetable\";

      TimeTableFile? CurrentTimeTableFile = timeTable.Parent as TimeTableFile;

      string selectedTrainFilename = Selected_Train.GetDocument().Filename;
      string selectedTimeTableFilename = timeTable.GetDocument().Filename;
      string tmpTimeTableName = "ZusiStartTempTimeTable";
      if (DataManager.Instance.options.RemoteTrackingSupport)
      {
        string timetable_pathfilename = CurrentTimeTableFile.Filename;
        string timetable_filename = Path.GetFileNameWithoutExtension(timetable_pathfilename);
        DataPathType dtp1 = DataPathType.Unknown;
        string zusi_timetablepathfilename = ZusiKlassenLib2.Zusi.GetRelativePathOf(timetable_pathfilename, ref dtp1);
        string zusi_timetablepath = Path.GetDirectoryName(zusi_timetablepathfilename);
        string zusi_temp_timetablepath = zusi_timetablepath.Replace("Timetables", "Temp");
        tmpBaseFolder = ZusiKlassenLib2.Zusi.DataPath[2] + zusi_temp_timetablepath + "\\";
        tmpTimeTableName = timetable_filename;
      }

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
      string tmpStrippedTimeTableName = ZusiKlassenLib2.Zusi.GetRelativePathOf(tmpTimeTableName, ref dtp);

      // clone temporary timetable from source
      //TimeTable testTimeTable = new(null, CurrentTimeTableFile.Root, false);
      TimeTable tmpTimeTable = new(null, timeTable, false);

      // Fahrplandatei im Buchfahrplan
      dtp = DataPathType.Unknown;
      string orgRelativeTimetableName = ZusiKlassenLib2.Zusi.GetRelativePathOf(selectedTimeTableFilename, ref dtp);

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
          bool isImportant;

          if (Selected_Train.Nummer == zug.Nummer)
          {
            isImportant = true;
          }
          else
          {
            if (DataManager.Instance.options.StartOnlySelectedTrain)
            {
              isImportant = false;
            }
            else
            {
              isImportant = CheckImportance(Selected_Train, zug);
              //isImportant = true;
            }
          }

          //isImportant = Selected_Train.Nummer == zug.Nummer; // always true for selected train

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




            if (Selected_Train.Nummer == zd.Root.Nummer) // check if zd is selectedtrain
            {
              TrainItem ti = CurrentTrainItem;
              if (ti != null && ti.IsLocoReplaced)
              {
                // search for FZGVerbandAktion in all Fahrplaneintrag and replace 2 with 1
                foreach (FahrplanEintrag fpe in zd.Root.FahrplanEintraege)
                {
                  if (fpe != null)
                  {
                    TrainSetActionType FZGVA = fpe.FzgVerbandAktion;
                    if (FZGVA == TrainSetActionType.CabChange && ti.IsLocoInFront)
                    {
                      fpe.FzgVerbandAktion = TrainSetActionType.TurnTrain;
                      FZGVA = TrainSetActionType.TurnTrain;
                    }
                  }
                }
                zd.Root.ReplaceTrain(ti.Reihung);
              }

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
                if (ti.ReplaceReihung != null) // Stored changed train composition, no need to change composition in the train file
                {
                  zd.Root.ReplaceTrain(ti.ReplaceReihung);
                }
              }

              if (Properties.Settings.Default.StartimStillstand == 0)
              {
                zd.Root.StartSpeed = 0;
              }
            }
            _log.Debug(string.Format("BuildTempTimeTable: {0} wird gespeichert unter {1}", zug.Gattung + zug.Nummer, tmpTrainfileName));
            zd.SaveAs(tmpTrainfileName);

            //if (zug.BuchfahrplanRohDatei?.Dateiname == Selected_Train.BuchfahrplanRohDatei?.Dateiname)
            if (zug.Nummer == Selected_Train.Nummer)
            {
              result = tmpTrainfileName;
            }

            TrainLink tl = ZusiObject.CreateNew<TrainLink>(tmpTimeTable, "Zug");
            tl.Datei = Datei.CreateNew(tl, tmpRelativeTrainfileName, false);
            tmpTimeTable.Trains.Add(new TrainReference(tl));

            if (tmpZug.StartTime is DateTime startTime)
            {
              //startTime -= TimeSpan.FromMinutes(10);

              if (newStartTime == null)
              {
                newStartTime = startTime;
              }
              else if (startTime < newStartTime)
              {
                newStartTime = startTime;
              }
            }

            original_traincount = _allTimeTableTrains.Count;
            optimised_traincount = tmpTimeTable.Trains.Count;
            original_starttime = timeTable.StartTime;
            optimized_starttime = newStartTime;
            _log.Debug(string.Format("BuildTempTimeTable: {0} erstellt ", zug.Gattung + zug.Nummer));

          }
        }
        catch (Exception ex)
        {
          _log.Error(string.Format("BuildTempTimeTable: {0} \nERROR {1} ", zug.Gattung + zug.Nummer, ex.ToString()));

        }

      }
      //ZusiDisplay anpassen
      _log.Debug("BuildTempTimeTable: Anpassung ZusiDisplay wird gestartet");
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
      _log.Debug("BuildTempTimeTable: Anpassung ZusiDisplay wurde abgeschlossen");
      //set new start time
      _log.Debug(string.Format("BuildTempTimeTable: Set new start time {0} ", newStartTime.HasValue ? newStartTime.Value.ToString("yyyy-MM-dd HH:mm") : "null"));
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
        MessageBox.Show(ex.Message, LocalizationManager.Translate("Die Timetable kann nicht gespeichert werden"), MessageBoxButton.OK, MessageBoxImage.Error);
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


    public string BuildTempTrain(ZusiCLIProject.FileLibrary.Zusi3.Zug Selected_Train, ZusiCLIProject.FileLibrary.Zusi3.Zug zug, string tmpStrippedTimeTableName, string tmpTrainsFolder, ZusiFdl2 fdl2, List<string> seltrain_usedmodules)
    {
      _log.Debug("BuildTempTrain called for Selected_Train=" + (Selected_Train != null ? Selected_Train.Nummer : "null") + " and zug=" + (zug != null ? zug.Nummer : "null"));
      try
      {
        DataPathType dtp;
        bool isImportant;
        string tmpTrainfileName = "";

        if (Selected_Train.Nummer == zug.Nummer)
        {
          isImportant = true;
        }
        else
        {
          if (DataManager.Instance.options.StartOnlySelectedTrain)
          {
            isImportant = false;
          }
          else
          {
            isImportant = CheckImportance2(Selected_Train, zug, fdl2, seltrain_usedmodules);
          }
        }

        if (isImportant)
        {
          string destinationfilename;

          zug.Datei.Dateiname = tmpStrippedTimeTableName; // Filename of fpn file the train file belongs to


          /*********************************************************
           * kopiere Buchfahrplandatei, wenn vorhanden
           *********************************************************/

          if (zug.BuchfahrplanRohDatei != null && zug.BuchfahrplanRohDatei.Exists())
          {
            dtp = ZusiKlassenLib2.DataPathType.Unknown;
            string sourcefilename = ZusiKlassenLib2.Zusi.GetAbsolutePathOf(zug.BuchfahrplanRohDatei.Dateiname, ref dtp);
            destinationfilename = tmpTrainsFolder + zug.BuchfahrplanRohDatei.NameOnly;

            try
            {
              System.IO.File.Copy(sourcefilename, destinationfilename, true);
            }
            catch (Exception ex)
            {
              _log.Error(string.Format("Error copying BuchfahrplanRohDatei from {0} to {1}: {2}", sourcefilename, destinationfilename, ex.ToString()));
            }

            dtp = DataPathType.Unknown;
            zug.BuchfahrplanRohDatei.Dateiname = Zusi.GetRelativePathOf(destinationfilename, ref dtp);
          }

          string s = zug.Gattung ?? "";
          s += zug.Nummer ?? "";
          if (string.IsNullOrEmpty(s))
          {
            s = Guid.NewGuid().ToString("N");
          }
          tmpTrainfileName = tmpTrainsFolder + s + ".trn";

          /****************************************************************
           *  Anpassungen am Zug, wenn es sich um den ausgewählten Zug handelt
           *  (z.B. Ersatzlok, Ersatzzug, Start im Stillstand)
           ****************************************************************/


          if (Selected_Train.Nummer == zug.Nummer) // check if zd is selectedtrain
          {
            TrainItem ti = CurrentTrainItem;
            if (ti != null && ti.IsLocoReplaced)
            {
              // search for FZGVerbandAktion in all Fahrplaneintrag and replace 2 with 1
              foreach (ZusiCLIProject.FileLibrary.Zusi3.Zug.FahrplanEintrag fpe in zug.Eintraege)
              {
                if (fpe != null)
                {
                  int FZGVA = fpe.FzgVerbandAktion;
                  if (FZGVA == 2 /*TrainSetActionType.CabChange*/ && ti.IsLocoInFront)
                  {
                    fpe.FzgVerbandAktion = 1; // TrainSetActionType.TurnTrain;
                    FZGVA = 1; // TrainSetActionType.TurnTrain;
                  }
                }
              }
              _log.Debug(string.Format("BuildTempTrain: Train {0} is loco replaced. Replacing loco in train file.", zug.Gattung + zug.Nummer));
              zug.ReplaceTrain(ti.Reihung);
            }

            if (ti != null && ti.IsTrainReplaced)
            {
              if (ti.IsTrainTurned)
              {
                //search for FZGVerbandAktion in all Fahrplaneintrag and replace 2 with 1
                foreach (ZusiCLIProject.FileLibrary.Zusi3.Zug.FahrplanEintrag fpe in zug.Eintraege)
                {
                  if (fpe != null)
                  {
                    int FZGVA = fpe.FzgVerbandAktion;
                    if (FZGVA == 2 /*TrainSetActionType.CabChange*/ && ti.IsTrainTurned)
                    {
                      fpe.FzgVerbandAktion = 1; // TrainSetActionType.TurnTrain;
                      FZGVA = 1; // TrainSetActionType.TurnTrain;
                    }
                  }
                }
              }
              _log.Debug(string.Format("BuildTempTrain: Train {0} is train replaced. Replacing train in train file.", zug.Gattung + zug.Nummer));
              zug.ReplaceTrain(ti.ReplaceReihung);
            }
            if (Properties.Settings.Default.StartimStillstand == 0)
            {
              //zug.spAnfang = 0; //zd.Root.StartSpeed = 0;
              //foreach (XmlAttribute xmlAttribute in zug.Attributes)
              //{
              //  if (xmlAttribute is XmlAttribute attr && attr.Name == "spAnfang")
              //  {
              //    xmlAttribute.Value = "0";
              //  }
              //}

              var spAttr = zug.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == "spAnfang");
              if (spAttr is not null)
              {
                spAttr.Value = "0";
              }
            }
          }
        }
        return tmpTrainfileName;
      }
      catch (Exception ex)
      {
        _log.Error(string.Format("BuildTempTrain: {0} \nERROR {1} ", zug.Gattung + zug.Nummer, ex.ToString()));
        return "";
      }
    }

    public class LoadingFrameworkSystemWpfImpl : ILoadingFrameworkHelperMethods<int>
    {
      public int LinesTotal { get; set; }

      public int ItemAdder(string s)
      {
        ++this.LinesTotal;
        Console.WriteLine(s);
        return this.LinesTotal - 1;
      }

      public int ItemUpdater(string nw, string ol, int oi)
      {
        //Console.CursorTop -= this.LinesTotal - oi;
        //Console.CursorLeft = 0;
        //while (nw.Length < ol.Length)
        //  nw += " ";
        //Console.Write(nw);
        //Console.CursorLeft = 0;
        //Console.CursorTop += this.LinesTotal - oi;
        return oi;
      }

      public void ItemFinished(string nw, int oi)
      {
        //Console.CursorTop -= this.LinesTotal - oi;
        //Console.CursorLeft = nw.Length;
        //Console.Write(" OK");
        //Console.CursorLeft = 0;
        //Console.CursorTop += this.LinesTotal - oi;
      }
    }

    //---------------------------------------------------------------------
    // Version using the F.Schn. xml-parser
    public string BuildTempTimeTable2(ZusiKlassenLib2.Fahrplan.Zug Selected_Train, TimeTable timeTable, out string tempTimeTableFilename)
    {
      _log.Debug("BuildTempTimeTable2 called for Selected_Train=" + (Selected_Train != null ? Selected_Train.Nummer : "null") + " and timeTable=" + (timeTable != null ? timeTable.Name : "null"));
      string result = null;
      DateTime? newStartTime = null;
      _log.Debug(string.Format("BuildTempTimeTable for Train: {0} started", Selected_Train.ToString()));
      // create temporary timetable folder

      string tmpBaseFolder = Zusi.DataPath[2] + @"Temp\ZusiStart\TempTimetable\";

      TimeTableFile? CurrentTimeTableFile = timeTable.Parent as TimeTableFile;
      string timetable_pathfilename = CurrentTimeTableFile.Filename;
      DataPathType dtp1 = DataPathType.Unknown;
      string zusi_timetablepathfilename = Zusi.GetRelativePathOf(timetable_pathfilename, ref dtp1);

      string selectedTrainFilename = Selected_Train.GetDocument().Filename;
      string selectedTimeTableFilename = timeTable.GetDocument().Filename;
      string tmpTimeTableName = "ZusiStartTempTimeTable";
      if (DataManager.Instance.options.RemoteTrackingSupport)
      {
        string timetable_filename = Path.GetFileNameWithoutExtension(timetable_pathfilename);
        string zusi_timetablepath = Path.GetDirectoryName(zusi_timetablepathfilename);
        string zusi_temp_timetablepath = zusi_timetablepath.Replace("Timetables", "Temp");
        tmpBaseFolder = Zusi.DataPath[2] + zusi_temp_timetablepath + "\\";
        tmpTimeTableName = timetable_filename;
      }

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

      /***************************************************
       * Test Loading Framework
       * ************************************************/
      _log.Debug("BuildTempTimeTable2: Test Loading Framework");
      string relPfad = zusi_timetablepathfilename;
      var lFr = new ZusiCLIProject.FileLibrary.Zusi3.ZusiFdl.LoadingFramework<int>();
      lFr.DataDirs = ZusiCLIProject.FileLibrary.Zusi3.Datei.GetZusiDataDirs();
      //lFr.FahrplanPfade = new string[] { relPfad };
      lFr.FahrplanPfade = [relPfad];
      var intHelper = new LoadingFrameworkSystemWpfImpl();
      ZusiCLIProject.FileLibrary.Zusi3.ZusiFdl.ZusiFdl2 fdl2 = null;
      try
      {
        lFr.InitItems(intHelper.ItemAdder);
        lFr.StartLoading(intHelper.ItemUpdater, intHelper.ItemFinished);
        fdl2 = lFr.Fdl2;
        _log.Debug(string.Format("BuildTempTimeTable2: Test Loading Framework finished. Loaded {0} lines.", intHelper.LinesTotal));
      }
      catch (Exception ex)
      {
        fdl2 = null;
        _log.Error("BuildTempTimeTable2: Test Loading Framework failed." + ex.ToString());
        if (ex.InnerException != null)
        {
          _log.Fatal("Inner Exception:");
          _log.Fatal(ex.InnerException.ToString);
          _log.Fatal(ex.InnerException.StackTrace);
        }
      }


      // clone temporary timetable from source
      //TimeTable testTimeTable = new(null, CurrentTimeTableFile.Root, false);
      //TimeTable tmpTimeTable = new(null, timeTable, false);
      var buffer = new Dictionary<string, ZusiCLIProject.FileLibrary.Zusi3.Zusi>(System.StringComparer.InvariantCulture);
      var fpnDatei = ZusiCLIProject.FileLibrary.Zusi3.Datei.CreateAndLoad(selectedTimeTableFilename, ZusiCLIProject.FileLibrary.Zusi3.Datei.GetZusiDataDirs(), buffer);
      var fpnDateiContent = fpnDatei.Content;

      // Fahrplandatei im Buchfahrplan
      dtp = DataPathType.Unknown;
      string orgRelativeTimetableName = Zusi.GetRelativePathOf(selectedTimeTableFilename, ref dtp);


      //****************************************
      // get all trains that are included in the timetable: selectedTimeTableFilename
      //****************************************

      ZusiCLIProject.FileLibrary.Zusi3.Fahrplan selectedtimetable = fpnDatei.Content.Fahrplaene.FirstOrDefault();
      ZusiCLIProject.FileLibrary.Zusi3.Fahrplan.Zugdateieintrag[] train_list = selectedtimetable.Zugdateien;
      ZusiCLIProject.FileLibrary.Zusi3.Zug[] zug_list = fpnDateiContent.Fahrplaene.FirstOrDefault()?.ZugdateienDirekt;

      var new_traineintrag_list = new List<ZusiCLIProject.FileLibrary.Zusi3.Fahrplan.Zugdateieintrag>();
      var new_zug_list = new List<ZusiCLIProject.FileLibrary.Zusi3.Zug>();

      /****************************************
       * loop through all trains of the timetable and check importance
       * if important: create temp train file and add to temp timetable
       * if not important: remove from timetable
       *****************************************/
      _log.Debug("BuildTempTimeTable2: Loop through all trains of the timetable and check importance");
      string selectedTrainfile = Selected_Train.GetDocument().Filename;
      ZusiCLIProject.FileLibrary.Zusi3.Datei selectedzugDatei = ZusiCLIProject.FileLibrary.Zusi3.Datei.CreateAndLoad(selectedTrainfile, ZusiCLIProject.FileLibrary.Zusi3.Datei.GetZusiDataDirs(), buffer);
      ZusiCLIProject.FileLibrary.Zusi3.Zug selectedzug = selectedzugDatei.Content.Zuege.FirstOrDefault();

      if (selectedzug == null)
      {
        // Selected train not found - integrated timetable
        // search train in zug_list
        foreach (ZusiCLIProject.FileLibrary.Zusi3.Zug zug in zug_list)
        {
          if (zug.Nummer == Selected_Train.Nummer)
          {
            selectedzug = zug;
            break;
          }
        }
      }
      //DateTime? selectedzug_starttime = selectedzug.GetStartTime();
      //DateTime? selectedzug_endtime = selectedzug.GetEndTime();

      DateTime? Selected_Aufgleiszeit = selectedzug.GetAufgleiszeit();
      DateTime? Selected_Abgleiszeit = selectedzug.GetAbgleiszeit();
      if (Selected_Aufgleiszeit != null)
        Selected_Aufgleiszeit = Selected_Aufgleiszeit.Value.AddMinutes(-DataManager.Instance.options.RO_vorlaufzeit);
      if (Selected_Abgleiszeit != null)
        Selected_Abgleiszeit = Selected_Abgleiszeit.Value.AddMinutes(DataManager.Instance.options.RO_nachlaufzeit);


      DateTime? Selected_StartTime = selectedzug.GetStartTime();
      List<string> all_used_modules;
      List<string> seltrain_usedmodules = selectedzug.getusedmodules(fdl2, Selected_Aufgleiszeit, Selected_Abgleiszeit, out all_used_modules, includeneigboringmodules: true);

      used_streckenmodule = seltrain_usedmodules; // all_used_modules;

      foreach (ZusiCLIProject.FileLibrary.Zusi3.Fahrplan.Zugdateieintrag train in train_list)
      {
        string trainfile = train.Datei.Dateiname;

        ZusiCLIProject.FileLibrary.Zusi3.Datei zugDatei = ZusiCLIProject.FileLibrary.Zusi3.Datei.CreateAndLoad(trainfile, ZusiCLIProject.FileLibrary.Zusi3.Datei.GetZusiDataDirs(), buffer);
        //var z = zugDatei.Content;
        ZusiCLIProject.FileLibrary.Zusi3.Zug zug = zugDatei.Content.Zuege.FirstOrDefault();

        try
        {
          string tmpTrainfileName = BuildTempTrain(selectedzug, zug, tmpStrippedTimeTableName, tmpTrainsFolder, fdl2, seltrain_usedmodules);
          if (!string.IsNullOrEmpty(tmpTrainfileName))
          {
            train.Datei.Dateiname = tmpTrainfileName;

            if (zug.Nummer == Selected_Train.Nummer)
            {
              result = tmpTrainfileName;
            }

            if (!zug.IsDecoTrain() || !DataManager.Instance.options.RO_starttime_no_decotrains)
            {
              if (zug.GetStartTime() is DateTime startTime)
              {
                //startTime -= TimeSpan.FromMinutes(10);

                if (newStartTime == null)
                {
                  newStartTime = startTime;
                }
                else if (startTime < newStartTime)
                {
                  newStartTime = startTime;
                }
              }
            }
            new_zug_list.Add(zug);

            _log.Debug(string.Format("BuildTempTimeTable: {0} erstellt ", zug.Gattung + zug.Nummer));

          }
        }
        catch (Exception ex)
        {
          _log.Error(string.Format("BuildTempTimeTable: {0} \nERROR {1} ", zug.Gattung + zug.Nummer, ex.ToString()));
        }
      }

      /****************************************
     * loop through all integrated trains of the timetable and check importance
     * if important: update train data and add to temp timetable
     * if not important: remove from timetable
     *****************************************/
      _log.Debug("BuildTempTimeTable2: Loop through all integrated trains of the timetable and check importance");


      foreach (ZusiCLIProject.FileLibrary.Zusi3.Zug zug in zug_list)
      {
        try
        {
          string tmpTrainfileName = BuildTempTrain(selectedzug, zug, tmpStrippedTimeTableName, tmpTrainsFolder, lFr.Fdl2, seltrain_usedmodules);
          if (!string.IsNullOrEmpty(tmpTrainfileName))
          {
            if (zug.Nummer == Selected_Train.Nummer)
            {
              result = tmpTrainfileName;
            }
            if (!zug.IsDecoTrain() || !DataManager.Instance.options.RO_starttime_no_decotrains)
            {
              if (zug.GetStartTime() is DateTime startTime)
              {
                //startTime -= TimeSpan.FromMinutes(10);

                if (newStartTime == null)
                {
                  newStartTime = startTime;
                }
                else if (startTime < newStartTime)
                {
                  newStartTime = startTime;
                }
              }
            }
            _log.Debug(string.Format("BuildTempTimeTable: {0} erstellt ", zug.Gattung + zug.Nummer));
            new_zug_list.Add(zug);
          }
        }
        catch (Exception ex)
        {
          _log.Error(string.Format("BuildTempTimeTable: {0} \nERROR {1} ", zug.Gattung + zug.Nummer, ex.ToString()));
        }
      }

      original_traincount = train_list.Length + zug_list.Length;
      optimised_traincount = new_zug_list.Count;
      original_starttime = timeTable.StartTime;
      optimized_starttime = newStartTime;

      // Delete all zugdatei entries. The temp train files will be added as new ZugdateiDirekt entries, because the Zugdatei entries would point to the original train files which is not wanted.
      _log.Debug("BuildTempTimeTable2: Update timetable with new train files");
      selectedtimetable.Zugdateien = Array.Empty<ZusiCLIProject.FileLibrary.Zusi3.Fahrplan.Zugdateieintrag>();
      selectedtimetable.ZugdateienDirekt = new_zug_list.ToArray();

      /****************************************
       * loop through all streckendateien of the timetable and check if they are used by the important trains
       * if used: keep in timetable
       * if not used: remove from timetable
       *****************************************/
      _log.Debug("BuildTempTimeTable2: Loop through all streckendateien of the timetable and check if they are used by the important trains");
      original_modulecount = selectedtimetable.Streckendateien.Count();

      if (DataManager.Instance.options.RO_trainselectioncriteria_Stations && DataManager.Instance.options.RO_trainselection_Streckenmodule)
      {
        if (used_streckenmodule.Count() > 0)
        {
          List<ZusiCLIProject.FileLibrary.Zusi3.Fahrplan.Streckendateieintrag> new_streckendatei_list = [];
          foreach (ZusiCLIProject.FileLibrary.Zusi3.Fahrplan.Streckendateieintrag sde in selectedtimetable.Streckendateien)
          {
            if (used_streckenmodule.Contains(sde.Datei.NameOnly))
            {
              new_streckendatei_list.Add(sde);
            }
          }
          selectedtimetable.Streckendateien = new_streckendatei_list.ToArray();
        }
      }
      optimized_modulecount = selectedtimetable.Streckendateien.Count();

      //ZusiDisplay anpassen
      _log.Debug("BuildTempTimeTable2: Anpassung ZusiDisplay wird gestartet");
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
                foreach (ZDTrain train1 in trains.Trains)
                {
                  train1.IsNeeded = true; //_trains.Find(ti => ti.IsImportant && ti.Zug.Gattung == train.Type && ti.Zug.Nummer == train.Number) != null;
                  string train_type = train1.Type.Replace(" ", "_");
                  if (Selected_Train.Gattung.Contains(train_type) && Selected_Train.Nummer.Contains(train1.Number))
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
      //var attributes = selectedtimetable.Attributes;
      _log.Debug(string.Format("BuildTempTimeTable2: Set new start time {0} ", newStartTime.HasValue ? newStartTime.Value.ToString("yyyy-MM-dd HH:mm") : "null"));
      var Attr_AnfangsZeit = selectedtimetable.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == "AnfangsZeit");
      if (Attr_AnfangsZeit is not null)
      {
        Attr_AnfangsZeit.Value = newStartTime?.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
      }

      fpnDatei.SaveAs(tmpTimeTableName);

      // mit Fahrplan im Ganzen starten, wenn es sich um einen integrierten Fahrplan handelt
      if (result == null)
      {
        result = tmpTimeTableName;
      }

      //TransferServer.Instance.TransferDirectory(tmpBaseFolder, ZDStartParameter);
      tempTimeTableFilename = tmpTimeTableName;
      return result;
    }

    //---------------------------------------------------------------------
    // used to create and save optimised timetable (e.g. if train or loco is replaced)

    public void BuildTempTimeTable3(ZusiKlassenLib2.Fahrplan.Zug Selected_Train, TimeTable timeTable)
    {
      _log.Debug("BuildTempTimeTable3 called for Selected_Train=" + (Selected_Train != null ? Selected_Train.Nummer : "null") + " and timeTable=" + (timeTable != null ? timeTable.Name : "null"));

      string result = null;
      DateTime? newStartTime = null;
      _log.Debug(string.Format("BuildTempTimeTable3 for Train: {0} gestartet", Selected_Train.ToString()));
      // create temporary timetable folder
      string tmpBaseFolder = "";
      string tmpTrainsFolder = "";

      string[] array2 = Selected_Train.FahrplanDatei.Dateiname.Split('\\');
      if (array2.Length >= 2)
      {
        tmpBaseFolder = ZusiKlassenLib2.Zusi.DataPath[4] + array2[0] + @"\" + array2[1] + @"\" + array2[2] + @"\";
        //tmpBaseFolder = Zusi.DataPath[2] + @"Temp\ZusiStart\Replacementrains\";
        tmpTrainsFolder = tmpBaseFolder + array2[3].Remove(array2[3].Length - 4) + @"\";
      }



      //string tmpBaseFolder = ZusiKlassenLib2.Zusi.DataPath[2] + Selected_Train.FahrplanDatei.Dateiname.Remove(Selected_Train.FahrplanDatei.Dateiname.Length - 4) + @"\"; // @"Timetables\myTimeTables\"+ timeTable.Name + @"\";

      TimeTableFile? CurrentTimeTableFile = timeTable.Parent as TimeTableFile;

      string selectedTrainFilename = Selected_Train.GetDocument().Filename;
      string selectedTimeTableFilename = timeTable.GetDocument().Filename;
      string tmpTimeTableName = timeTable.Name;


      //if (Directory.Exists(tmpBaseFolder))
      //{
      //  try
      //  {
      //    DirectoryInfo di = new(tmpBaseFolder);
      //    di.Clear();
      //  }
      //  catch (Exception ex)
      //  {
      //    _log.Error(ex.ToString());
      //  }
      //}

      //string tmpTrainsFolder = string.Format(@"{0}{1}\", tmpBaseFolder, tmpTimeTableName);
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
      string tmpStrippedTimeTableName = ZusiKlassenLib2.Zusi.GetRelativePathOf(tmpTimeTableName, ref dtp);

      // clone temporary timetable from source
      //TimeTable testTimeTable = new(null, CurrentTimeTableFile.Root, false);
      TimeTable tmpTimeTable = new(null, timeTable, false);

      // Fahrplandatei im Buchfahrplan
      dtp = DataPathType.Unknown;
      string orgRelativeTimetableName = ZusiKlassenLib2.Zusi.GetRelativePathOf(selectedTimeTableFilename, ref dtp);

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
          bool isImportant;

          if (Selected_Train.Nummer == zug.Nummer)
          {
            isImportant = true;
          }
          else
          {
            isImportant = false;
          }

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


              //System.IO.File.Copy(zug.BuchfahrplanRohDatei.FullPath, p, true);
            }

            if (Selected_Train.Nummer == zd.Root.Nummer) // check if zd is selectedtrain
            {
              TrainItem ti = CurrentTrainItem;
              if (ti != null && ti.IsLocoReplaced)
              {
                // search for FZGVerbandAktion in all Fahrplaneintrag and replace 2 with 1
                foreach (FahrplanEintrag fpe in zd.Root.FahrplanEintraege)
                {
                  if (fpe != null)
                  {
                    TrainSetActionType FZGVA = fpe.FzgVerbandAktion;
                    if (FZGVA == TrainSetActionType.CabChange && ti.IsLocoInFront)
                    {
                      fpe.FzgVerbandAktion = TrainSetActionType.TurnTrain;
                      FZGVA = TrainSetActionType.TurnTrain;
                    }
                  }
                }
                zd.Root.ReplaceTrain(ti.Reihung);
              }

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

              if (Properties.Settings.Default.StartimStillstand == 0)
              {
                zd.Root.StartSpeed = 0;
              }
            }
            _log.Debug(string.Format("BuildTempTimeTable: {0} wird gespeichert unter {1}", zug.Gattung + zug.Nummer, tmpTrainfileName));
            zd.SaveAs(tmpTrainfileName);

            //if (zug.BuchfahrplanRohDatei?.Dateiname == Selected_Train.BuchfahrplanRohDatei?.Dateiname)
            if (zug.Nummer == Selected_Train.Nummer)
            {
              result = tmpTrainfileName;
            }

          }
        }
        catch (Exception ex)
        {
          _log.Error(string.Format("BuildTempTimeTable: {0} \nERROR {1} ", zug.Gattung + zug.Nummer, ex.ToString()));

        }

      }

    }


    public void AddToClipBoard(string text)
    {
      try
      {
        System.Windows.Forms.Clipboard.SetText(text);
      }
      catch (Exception ex)
      {
        _log.Error("Error copying to clipboard: " + ex.ToString());
        //Clipboard.SetText(text, System.Windows.TextDataFormat.Text);
      }

    }

    public void show_gleisbelegung(string station)
    {
      try
      {
        if (true)
        {
          show_gleisbelegung2(station);
          return;
        }

        //int num = (int)System.Windows.MessageBox.Show("Gleisbelegung: " + station, "Not implemented yet", MessageBoxButton.OK);

        /***************************************************
        * Test Loading Framework
        * ************************************************/
        //Zug zug = DataManager.Instance.CurrentTrain;
        TimeTableRelation timetablerelation = DataManager.Instance.SelectedTimeTableRelation;
        TimeTable timeTable = timetablerelation?.TimeTable;
        TimeTableFile? CurrentTimeTableFile = timeTable.Parent as TimeTableFile;
        string timetable_pathfilename = CurrentTimeTableFile.Filename;
        DataPathType dtp1 = DataPathType.Unknown;
        string zusi_timetablepathfilename = Zusi.GetRelativePathOf(timetable_pathfilename, ref dtp1);
        string selectedTimeTableFilename = timeTable.GetDocument().Filename;

        _log.Debug("BuildTempTimeTable2: Test Loading Framework");
        string relPfad = zusi_timetablepathfilename;
        var lFr = new ZusiCLIProject.FileLibrary.Zusi3.ZusiFdl.LoadingFramework<int>();
        lFr.DataDirs = ZusiCLIProject.FileLibrary.Zusi3.Datei.GetZusiDataDirs();
        //lFr.FahrplanPfade = new string[] { relPfad };
        lFr.FahrplanPfade = [relPfad];
        var intHelper = new LoadingFrameworkSystemWpfImpl();
        ZusiCLIProject.FileLibrary.Zusi3.ZusiFdl.ZusiFdl2 fdl2 = null;
        try
        {
          lFr.InitItems(intHelper.ItemAdder);
          lFr.StartLoading(intHelper.ItemUpdater, intHelper.ItemFinished);
          fdl2 = lFr.Fdl2;
          _log.Debug(string.Format("BuildTempTimeTable2: Test Loading Framework finished. Loaded {0} lines.", intHelper.LinesTotal));
        }
        catch (Exception ex)
        {
          fdl2 = null;
          _log.Error("BuildTempTimeTable2: Test Loading Framework failed." + ex.ToString());
          if (ex.InnerException != null)
          {
            _log.Fatal("Inner Exception:");
            _log.Fatal(ex.InnerException.ToString);
            _log.Fatal(ex.InnerException.StackTrace);
          }
        }


        // clone temporary timetable from source
        //TimeTable testTimeTable = new(null, CurrentTimeTableFile.Root, false);
        //TimeTable tmpTimeTable = new(null, timeTable, false);
        var buffer = new Dictionary<string, ZusiCLIProject.FileLibrary.Zusi3.Zusi>(System.StringComparer.InvariantCulture);
        var fpnDatei = ZusiCLIProject.FileLibrary.Zusi3.Datei.CreateAndLoad(selectedTimeTableFilename, ZusiCLIProject.FileLibrary.Zusi3.Datei.GetZusiDataDirs(), buffer);
        var fpnDateiContent = fpnDatei.Content;

        // Fahrplandatei im Buchfahrplan
        DataPathType dtp;
        dtp = DataPathType.Unknown;
        string orgRelativeTimetableName = Zusi.GetRelativePathOf(selectedTimeTableFilename, ref dtp);


        //****************************************
        // get all trains that are included in the timetable: selectedTimeTableFilename
        //****************************************

        ZusiCLIProject.FileLibrary.Zusi3.Fahrplan Fahrplan = fpnDatei.Content.Fahrplaene.FirstOrDefault();
        ZusiCLIProject.FileLibrary.Zusi3.Fahrplan.Zugdateieintrag[] train_list = Fahrplan.Zugdateien;
        ZusiCLIProject.FileLibrary.Zusi3.Zug[] zug_list = fpnDateiContent.Fahrplaene.FirstOrDefault()?.ZugdateienDirekt;

        var new_traineintrag_list = new List<ZusiCLIProject.FileLibrary.Zusi3.Fahrplan.Zugdateieintrag>();
        var new_zug_list = new List<ZusiCLIProject.FileLibrary.Zusi3.Zug>();

        foreach (ZusiCLIProject.FileLibrary.Zusi3.Fahrplan.Zugdateieintrag train in train_list)
        {
          string trainfile = train.Datei.Dateiname;

          ZusiCLIProject.FileLibrary.Zusi3.Datei zugDatei = ZusiCLIProject.FileLibrary.Zusi3.Datei.CreateAndLoad(trainfile, ZusiCLIProject.FileLibrary.Zusi3.Datei.GetZusiDataDirs(), buffer);
          //var z = zugDatei.Content;
          ZusiCLIProject.FileLibrary.Zusi3.Zug zug1 = zugDatei.Content.Zuege.FirstOrDefault();

          try
          {
            new_zug_list.Add(zug1);
          }
          catch (Exception ex)
          {
            _log.Error(string.Format("BuildTempTimeTable: \nERROR {0} ", ex.ToString()));
          }
        }
        foreach (ZusiCLIProject.FileLibrary.Zusi3.Zug zug1 in zug_list)
        {
          try
          {
            new_zug_list.Add(zug1);
          }
          catch (Exception ex)
          {
            _log.Error(string.Format("BuildTempTimeTable: \nERROR {0} ", ex.ToString()));
          }
        }

        /****************************************
       * loop through all integrated trains of the timetable and check importance
       * if important: update train data and add to temp timetable
       * if not important: remove from timetable
       *****************************************/
        _log.Debug("BuildTempTimeTable2: Loop through all integrated trains of the timetable and check importance");
        new GleisbelegungWindow(new_zug_list.ToArray(), station).Show();




      }
      catch (Exception ex)
      {
        _log.Error(string.Format("BuildTempTimeTable: \nERROR {0} ", ex.ToString()));
      }
    }

    public void show_gleisbelegung2(string station)
    {
      try
      {
        //int num = (int)System.Windows.MessageBox.Show("Gleisbelegung: " + station, "Not implemented yet", MessageBoxButton.OK);

        /***************************************************
        * Test Loading Framework
        * ************************************************/

        TimeTableRelation timetablerelation = DataManager.Instance.SelectedTimeTableRelation;
        TimeTable timeTable = timetablerelation?.TimeTable;
        TimeTableFile? CurrentTimeTableFile = timeTable.Parent as TimeTableFile;
        string timetable_pathfilename = CurrentTimeTableFile.Filename;
        DataPathType dtp1 = DataPathType.Unknown;
        string zusi_timetablepathfilename = Zusi.GetRelativePathOf(timetable_pathfilename, ref dtp1);
        string selectedTimeTableFilename = timeTable.GetDocument().Filename;

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

        new GleisbelegungWindow(new_zug_list.ToArray(), station).Show();

      }
      catch (Exception ex)
      {
        _log.Error(string.Format("BuildTempTimeTable: \nERROR {0} ", ex.ToString()));
      }
    }

  }

}

