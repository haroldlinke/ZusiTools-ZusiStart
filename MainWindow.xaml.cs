using log4net;
using Sovoma;
using Sovoma.WPF;
using Sovoma.ControlsKit;
using ZusiFahrpultLib;
using ZusiMeterGaugesLib;

using System;
using System.Drawing;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Shapes;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using ZusiKlassenLib;
using ZusiKlassenLib.Common;
using ZusiKlassenLib.Fahrplan;
using ZusiKlassenLib.TimeTable;
using ZusiStart.About;
using ZusiStart.Connection;
using ZusiStart.Controls;
using ZusiStart.Data;
using ZusiStart.Dialogs;
using ZusiStart.Miscellaneous;
using System.Diagnostics;
using Microsoft.Web.WebView2.Core;
using ZusiStart.CreatePictures;
using Microsoft.AspNetCore.Builder;
using System.Xml;
using ZusiKlassenLib.Cab;
using ZusiDisplayLib;
using Microsoft.Win32;
using System.Xml.Linq;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using ZusiBuchfahrplanlib;
using System.Windows.Media.Imaging;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic.Logging;
using System.Globalization;
using System.IO;
using ZusiStart.ViewModels;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ZusiMeterGaugesLib.Interfaces;
using System.ComponentModel;
using System.Reflection.Metadata;
//using System.Windows.Forms;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Web.WebView2.WinForms;
//using System.Windows.Forms;
//using System.Windows.Forms;
//using System.Windows.Forms;


namespace ZusiStart
{
  /// <summary>
  /// Interaktionslogik für MainWindow.xaml
  /// </summary>
  /// 

  public class TabContentTemplateSelector : DataTemplateSelector
  {
    public DataTemplate WebViewTemplate { get; set; }
    public DataTemplate IntroTemplate { get; set; }
    public DataTemplate ImageTemplate { get; set; }
    public DataTemplate ListTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      var tab = item as TabViewModel;
      if (tab == null) return base.SelectTemplate(item, container);
      if (tab.Title == DataManager.Instance.tab_title_favorites) return ListTemplate;
      if (tab.IsIntro) return IntroTemplate;
      if (tab.IsImageTab) return ImageTemplate;
      return WebViewTemplate;
    }
  }

  public partial class MainWindow : System.Windows.Window, IDisposable
  {
    #region private fields

    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public static string _url_zusi_strecken_karte = "https://www.zusi-sk.eu/#10.5/50.96983/6/";
    public static string _url_zusi_datenbank = "https://www.zusidatenbank.de/?zusistart";
    private static string _zusibildfahrplan_mode = "FPL";


    private readonly HttpMiniServer _miniServer = new();
    private DataLoaderWindow _dataLoaderWindow;
    //private WebView_Window _webViewWindow_ZDB;
    //private WebView_Window _webViewWindow_ZSK;
    //private readonly Simulator _simulator = new Simulator();
    private readonly Fahrpult _fahrpult = new();
    private int _activetab = 1;
    private GMapMarker marker;
    private OverlayWindow overlay;

    public static CoreWebView2Environment SharedEnvironment;

    public static readonly RoutedUICommand CommandAbout = new RoutedUICommand("Über _ZusiStart", nameof(CommandAbout), typeof(MainWindow));
    //public static readonly RoutedUICommand CommandAbout = new RoutedUICommand(Properties.Resources.AboutZusiStart, nameof(CommandAbout), typeof(MainWindow));
    //public static readonly RoutedUICommand CommandHelp = new RoutedUICommand(Properties.Resource1.MenuDocumentation, nameof(CommandHelp), typeof(MainWindow));
    //public static readonly RoutedUICommand CommandExtDocu = new RoutedUICommand("Dokumentation mit _externem Programm öffnen", nameof(CommandExtDocu), typeof(MainWindow));
    //public static readonly RoutedUICommand CommandOptions = new RoutedUICommand("System-Optionen", nameof(CommandOptions), typeof(MainWindow));
    //public static readonly RoutedUICommand CommandQuit = new RoutedUICommand("Beenden", nameof(CommandOptions), typeof(MainWindow));
    //public static readonly RoutedUICommand CommandCreatePictures = new RoutedUICommand("Erzeuge alle Fahrzeugbilder", nameof(CommandCreatePictures), typeof(MainWindow));
    public static readonly RoutedUICommand CommandHelp = new RoutedUICommand("_Dokumentation", nameof(CommandHelp), typeof(MainWindow));
    public static readonly RoutedUICommand CommandExtDocu = new RoutedUICommand("Dokumentation mit _externem Programm öffnen", nameof(CommandExtDocu), typeof(MainWindow));
    public static readonly RoutedUICommand CommandOptions = new RoutedUICommand("System-Optionen", nameof(CommandOptions), typeof(MainWindow));
    public static readonly RoutedUICommand CommandQuit = new RoutedUICommand("Beenden", nameof(CommandOptions), typeof(MainWindow));
    public static readonly RoutedUICommand CommandCreatePictures = new RoutedUICommand("Erzeuge alle Fahrzeugbilder", nameof(CommandCreatePictures), typeof(MainWindow));
    //public static readonly RoutedUICommand UndoReplLocoCommand = new("_Rückgängig Loktausch", "UndoReplLocoCommand", typeof(MainWindow),
    //        new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.Z, ModifierKeys.Control) }));
    //public static readonly RoutedUICommand ReplaceLocoCommand = new("_Lok tauschen", "ReplaceLocoCommand", typeof(MainWindow),
    //    new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.F5) }));
    //public static readonly RoutedUICommand ManageReplLocosCommand = new("_Austauschloks verwalten...", "ManageReplLocosCommand", typeof(MainWindow),
    //    new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.F6) }));
    #endregion

    #region dependency properties

    //---------------------------------------------------------------------
    public static readonly DependencyProperty FrictionSettingsPopupVisibleProperty = DependencyProperty.Register(
            "FrictionSettingsPopupVisible",
            typeof(bool),
            typeof(MainWindow),
            new PropertyMetadata(false));
    public bool FrictionSettingsPopupVisible
    {
      get => (bool)GetValue(FrictionSettingsPopupVisibleProperty);
      set => SetValue(FrictionSettingsPopupVisibleProperty, value);
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _mainBorderVisibilityKey = DependencyProperty.RegisterReadOnly(
        "MainBorderVisibility",
        typeof(Visibility),
        typeof(MainWindow),
        new PropertyMetadata(Visibility.Collapsed));
    public static readonly DependencyProperty MainBorderVisibilityProperty = _mainBorderVisibilityKey.DependencyProperty;
    public Visibility MainBorderVisibility
    {
      get => (Visibility)GetValue(MainBorderVisibilityProperty);
      private set => SetValue(_mainBorderVisibilityKey, value);
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _mainWebBorderVisibilityKey = DependencyProperty.RegisterReadOnly(
        "MainWebBorderVisibility",
        typeof(Visibility),
        typeof(MainWindow),
        new PropertyMetadata(Visibility.Collapsed));
    public static readonly DependencyProperty MainWebBorderVisibilityProperty = _mainWebBorderVisibilityKey.DependencyProperty;
    public Visibility MainWebBorderVisibility
    {
      get => (Visibility)GetValue(MainWebBorderVisibilityProperty);
      private set => SetValue(_mainWebBorderVisibilityKey, value);
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _recentTrainsBorderVisibilityKey = DependencyProperty.RegisterReadOnly(
        "RecentTrainsBorderVisibility",
        typeof(Visibility),
        typeof(MainWindow),
        new PropertyMetadata(Visibility.Collapsed));
    public static readonly DependencyProperty RecentTrainsBorderVisibilityProperty = _recentTrainsBorderVisibilityKey.DependencyProperty;
    public Visibility RecentTrainsBorderVisibility
    {
      get => (Visibility)GetValue(RecentTrainsBorderVisibilityProperty);
      private set => SetValue(_recentTrainsBorderVisibilityKey, value);
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _searchBorderVisibilityKey = DependencyProperty.RegisterReadOnly(
        "SearchBorderVisibility",
        typeof(Visibility),
        typeof(MainWindow),
        new PropertyMetadata(Visibility.Collapsed));
    public static readonly DependencyProperty SearchBorderVisibilityProperty = _searchBorderVisibilityKey.DependencyProperty;
    public Visibility SearchBorderVisibility
    {
      get => (Visibility)GetValue(SearchBorderVisibilityProperty);
      private set => SetValue(_searchBorderVisibilityKey, value);
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _statusBarVisibilityKey = DependencyProperty.RegisterReadOnly(
        "StatusBarVisibility",
        typeof(Visibility),
        typeof(MainWindow),
        new PropertyMetadata(Visibility.Collapsed));
    public static readonly DependencyProperty StatusBarVisibilityProperty = _statusBarVisibilityKey.DependencyProperty;
    public Visibility StatusBarVisibility
    {
      get => (Visibility)GetValue(StatusBarVisibilityProperty);
      private set => SetValue(_statusBarVisibilityKey, value);
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _versionVisibilityKey = DependencyProperty.RegisterReadOnly(
        "VersionVisibility",
        typeof(Visibility),
        typeof(MainWindow),
        new PropertyMetadata(Visibility.Collapsed));
    public static readonly DependencyProperty VersionVisibilityProperty = _versionVisibilityKey.DependencyProperty;
    public Visibility VersionVisibility
    {
      get => (Visibility)GetValue(VersionVisibilityProperty);
      private set => SetValue(_versionVisibilityKey, value);
    }



    //---------------------------------------------------------------------
    public static readonly DependencyPropertyKey _zskButtonVisibilityKey = DependencyProperty.RegisterReadOnly(
        "ZSKButtonVisibility",
        typeof(Visibility),
        typeof(MainWindow),
        new PropertyMetadata(Visibility.Collapsed));
    public static readonly DependencyProperty ZSKButtonVisibilityProperty = _zskButtonVisibilityKey.DependencyProperty;
    public Visibility ZSKButtonVisibility
    {
      get => (Visibility)GetValue(ZSKButtonVisibilityProperty);
      set => SetValue(_zskButtonVisibilityKey, value);
    }

    //---------------------------------------------------------------------
    public static readonly DependencyPropertyKey _zdbButtonVisibilityKey = DependencyProperty.RegisterReadOnly(
        "ZDBButtonVisibility",
        typeof(Visibility),
        typeof(MainWindow),
        new PropertyMetadata(Visibility.Collapsed));
    public static readonly DependencyProperty ZDBButtonVisibilityProperty = _zdbButtonVisibilityKey.DependencyProperty;
    public Visibility ZDBButtonVisibility
    {
      get => (Visibility)GetValue(ZDBButtonVisibilityProperty);
      set => SetValue(_zdbButtonVisibilityKey, value);
    }

    //---------------------------------------------------------------------
    public static readonly DependencyPropertyKey _bfpButtonVisibilityKey = DependencyProperty.RegisterReadOnly(
        "BfpButtonVisibility",
        typeof(Visibility),
        typeof(MainWindow),
        new PropertyMetadata(Visibility.Collapsed));
    public static readonly DependencyProperty BfpButtonVisibilityProperty = _bfpButtonVisibilityKey.DependencyProperty;
    public Visibility BfpButtonVisibility
    {
      get => (Visibility)GetValue(BfpButtonVisibilityProperty);
      set => SetValue(_bfpButtonVisibilityKey, value);
    }

    #endregion

    #region commands

    public static readonly RoutedUICommand MinimizeCommand = new("", "MinimizeCommand", typeof(MainWindow));
    public static readonly RoutedUICommand ExitGameCommand = new("ZusiStart beenden", "ExitGameCommand", typeof(MainWindow));
    public static readonly RoutedUICommand StartTrainCommand = new("Ausgewählten Zug fahren", "StartTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand StartFavTrainCommand = new("Ausgewählten Zug fahren", "StartFavTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand TimeTablesPageCommand = new("Fahrplanauswahl", "TimeTablesPageCommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchPageCommand = new("Zug suchen", "SearchPageCommand", typeof(MainWindow));
    public static readonly RoutedUICommand FavoriteTrainsPageCommand = new("Zug Favoriten", "FavoriteTrainsPageCommand", typeof(MainWindow));
    public static readonly RoutedUICommand AddFavoriteTrainCommand = new("Zug hinzufügen", "AddFavoriteTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand EditFavoriteCommentCommand = new("Kommentar bearbeiten", "EditFavoriteCommentCommand", typeof(MainWindow));
    public static readonly RoutedUICommand DelFavoriteTrainCommand = new("Zug löschen", "DelFavoriteTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchTrainCommand = new("Zug suchen", "SearchTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchTrainZusiDBCommand = new("ZUSI Datenbank", "SearchTrainZusiDBCommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchTrainZusiSKCommand = new("ZUSI Streckenkarte", "SearchTrainZusiSKCommand", typeof(MainWindow));
    public static readonly RoutedUICommand ZSK_PositionOverlayCommand = new("Fadenkreuz Positionieren", "ZSK_PositionOverlayCommand", typeof(MainWindow));
    public static readonly RoutedUICommand ZSK_FadenkreuzActiveCommand = new("Fadenkreuz anzeigen", "ZSK_FadenkreuzActiveCommand", typeof(MainWindow));
    public static readonly RoutedUICommand ZSK_FindStationCommand = new("Fadenkreuz Positionieren", "ZSK_FindStationCommand", typeof(MainWindow));
    public static readonly RoutedUICommand TrackTrainCommand = new("Zug tracken", "TrackTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand ZusiBildFahrplanCommand = new("ZUSI Bildfahrplan", "ZusiBildFahrplanCommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchBuchfahrplanCommand = new("Buchfahrplan", "SearchBuchfahrplanCommand", typeof(MainWindow));
    public static readonly RoutedUICommand CreateVehicleListCommand = new("Zug suchen", "CreateVehicleListCommand", typeof(MainWindow));
    public static readonly RoutedUICommand FilterRefreshCommand = new("Anwenden", "FilterRefreshCommand", typeof(MainWindow));
    public static readonly RoutedUICommand FilterResetCommand = new("Zurücksetzen", "FilterResetCommand", typeof(MainWindow));
    public static readonly RoutedUICommand TrainStartSettingsCommand = new("Start-Optionen", "TrainStartSettingsCommand", typeof(MainWindow));
    public static readonly RoutedUICommand FrictionSettingsCommand = new("Gleisbedingungen", "FrictionSettingsCommand", typeof(MainWindow));
    //--
    public static readonly RoutedUICommand ResetDataCommand = new("ResetData", "ResetDataCommand", typeof(MainWindow), new InputGestureCollection(new KeyGesture[] { new KeyGesture(Key.F12) }));
    public static readonly RoutedUICommand UndoReplTrainCommand = new("_Rückgängig Zugtausch", "UndoReplTrainCommand", typeof(MainWindow),
           new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.F5, ModifierKeys.Alt) }));
    public static readonly RoutedUICommand ReplaceTrainCommand = new("_Zug tauschen", "ReplaceTrainCommand", typeof(MainWindow),
        new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.F7, ModifierKeys.Alt) }));
    public static readonly RoutedUICommand ManageReplTrainsCommand = new("_Austauschzüge auswählen/verwalten", "ManageReplTrainsCommand", typeof(MainWindow),
        new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.F6, ModifierKeys.Alt) }));

    #endregion

    //private async void InitializeWebView2Instances()
    //{
    //  //string userDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ZusiStart\WebView2";
    //  //string userDataFolder_ZDB = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ZusiStart\WebView2_ZDB";
    //  //string userDataFolder_ZSK= Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ZusiStart\WebView2_ZSK";

    //  string userDataFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DataManager.localfoldername, "WebView2");
    //  string userDataFolder_ZDB = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DataManager.localfoldername, "WebView2_ZDB");
    //  string userDataFolder_ZSK = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DataManager.localfoldername, "WebView2_ZSK");

    //  var options = new CoreWebView2EnvironmentOptions();
    //  var options_ZSK = new CoreWebView2EnvironmentOptions();
    //  var options_ZDB = new CoreWebView2EnvironmentOptions();

    //  // Await the CreateAsync method to get the CoreWebView2Environment instance
    //  DataManager.Instance.webview_environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);
    //  DataManager.Instance.webview_environment_ZSK = await CoreWebView2Environment.CreateAsync(null, userDataFolder_ZSK, options_ZSK);
    //  DataManager.Instance.webview_environment_ZDB = await CoreWebView2Environment.CreateAsync(null, userDataFolder_ZDB, options_ZDB);
    //  try
    //  {
    //    // Ensure CoreWebView2 is initialized with the environment
    //    await webView.EnsureCoreWebView2Async(DataManager.Instance.webview_environment);
    //  }
    //  catch { }
    //  await webView.EnsureCoreWebView2Async(null);
    //  webView.CoreWebView2.Navigate("https://www.hlinke.de/ZUSItools/zusistart_dummy_page.html");

    //  try
    //  {
    //    await webView_ZSK.EnsureCoreWebView2Async(DataManager.Instance.webview_environment_ZSK);
    //  }
    //  catch { }
    //  await webView_ZSK.EnsureCoreWebView2Async(null);
    //  webView_ZSK.CoreWebView2.Navigate(_url_zusi_strecken_karte);

    //  webView_ZSK.NavigationCompleted += OnWebView_NavigationCompleted_ZSK;
    //  webView_ZSK.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested_ZDB;
    //  webView_ZSK.CoreWebView2.SourceChanged += CoreWebView2SourceChanged_ZSK;
    //  webView_ZSK.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;

    //  try
    //  {
    //    await webView_ZDB.EnsureCoreWebView2Async(DataManager.Instance.webview_environment_ZDB);
    //  }
    //  catch { }
    //  await webView_ZDB.EnsureCoreWebView2Async(null);
    //  webView_ZDB.CoreWebView2.Navigate("https://www.zusidatenbank.de?zusistart");
    //  //webView_ZDB.CoreWebView2.Navigate("https://www.zusidatenbank.de");
    //  webView_ZDB.NavigationCompleted += OnWebView_NavigationCompleted_ZDB;
    //  webView_ZDB.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested_ZDB;
    //  webView_ZDB.CoreWebView2.SourceChanged += CoreWebView2SourceChanged_ZDB;
    //  webView_ZDB.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;

    //  _webViewWindow_ZDB = new WebView_Window
    //  {
    //    Owner = this
    //  };
    //  await _webViewWindow_ZDB.webViewWin.EnsureCoreWebView2Async(DataManager.Instance.webview_environment);
    //  await _webViewWindow_ZDB.webViewWin.EnsureCoreWebView2Async(null);

    //}

    private async void InitializeWebViewEnvironment()
    {
      string userDataFolder = System.IO.Path.Combine(
                 Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                 "ZusiStart", "WebView2");

      SharedEnvironment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
    }

    private async void TabControl_SelectionChanged2(object sender, SelectionChangedEventArgs e)
    {
      foreach (var item in e.AddedItems)
      {
        var tabItem = (sender as TabControl)?.ItemContainerGenerator.ContainerFromItem(item) as TabItem;

        if (tabItem != null)
        {

          TabViewModel tvm = tabItem.Header as TabViewModel;

          if (!tvm.IsInitialised)
          {

            await tvm.InitializeWebViewAsync(SharedEnvironment);

            if (tvm.Title == DataManager.Instance.tab_title_Streckenkarte)
            {
              await System.Threading.Tasks.Task.Delay(1500); // for Streckenkarte wait until loaded before going to fullscreen
            }
          }

          if (tvm.Title == DataManager.Instance.tab_title_Streckenkarte) // set Streckenkarte always to fullscreen
          {
            tvm.WebViewInstance?.CoreWebView2?.ExecuteScriptAsync("if (!document.fullscreenElement){[...document.querySelectorAll('button, div')].find(el => el.title?.includes('Vollbild'))?.click();}");
          }
        }

      }
    }

    //private void ZoomIn_Click(object sender, RoutedEventArgs e)
    //{
    //  var tab = (DataContext as DataManager)?.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_Streckenkarte);
    //  tab?.WebViewInstance?.CoreWebView2?.ExecuteScriptAsync("document.body.style.zoom='120%';");
    //}

    //private void ZoomOut_Click(object sender, RoutedEventArgs e)
    //{
    //  var tab = (DataContext as DataManager)?.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_Streckenkarte);
    //  tab?.WebViewInstance?.CoreWebView2?.ExecuteScriptAsync("document.body.style.zoom='80%';");
    //}

    //private void ResetView_Click(object sender, RoutedEventArgs e)
    //{
    //  var tab = (DataContext as DataManager)?.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_Streckenkarte);
    //  tab?.WebViewInstance?.CoreWebView2?.ExecuteScriptAsync("[...document.querySelectorAll('button, div')].find(el => el.title?.includes('Vollbild'))?.click();");

    //}

    //private void LoadLocalHtml()
    //  {
    //    try
    //    {
    //      InitializeWebView2Instances();

    //    }
    //    catch (Exception ex)
    //    {
    //      MessageBox.Show($"WebView2 initialization failed: {ex.Message}");
    //    }
    //  }

    //---------------------------------------------------------------------
    public MainWindow()
    {
      InitializeComponent();
      InitializeWebViewEnvironment();
      //InitializeMap();

      Log.Debug("ZusiStart started - Version:" + AsmInfo.Version.ToString());

      EngageScaling();

      LoadWindowSettings();

      Background = Application.Current.TryFindResource(string.Format("bkgnd{0}", DateTime.Now.Second & 3)) as System.Windows.Media.Brush;

      Loaded += MainWindow_Loaded;
      SortModeChanged += MainWindow_SortModeChanged;

      ZusiSim.Terminated += ZusiSim_Terminated;

      this.Closing += MainWindow_Closing;

      CommandBindings.Add(new CommandBinding(MinimizeCommand, OnMinimize, OnCanMinimize));
      CommandBindings.Add(new CommandBinding(ExitGameCommand, OnExitGame));
      CommandBindings.Add(new CommandBinding(StartTrainCommand, OnStartTrain, OnCanStartTrain));
      CommandBindings.Add(new CommandBinding(StartFavTrainCommand, OnStartFavTrain, OnCanStartFavTrain));
      CommandBindings.Add(new CommandBinding(TimeTablesPageCommand, OnTimeTablePage));
      CommandBindings.Add(new CommandBinding(SearchPageCommand, OnSearchPage, OnCanSearchPage));
      CommandBindings.Add(new CommandBinding(FavoriteTrainsPageCommand, OnFavoriteTrainsPage, OnCanFavoriteTrainsPage));
      CommandBindings.Add(new CommandBinding(AddFavoriteTrainCommand, OnAddFavoriteTrain, OnCanAddFavoriteTrain));
      CommandBindings.Add(new CommandBinding(EditFavoriteCommentCommand, OnEditFavoriteComment, OnCanDelFavoriteTrain));

      CommandBindings.Add(new CommandBinding(DelFavoriteTrainCommand, OnDelFavoriteTrain, OnCanDelFavoriteTrain));
      CommandBindings.Add(new CommandBinding(SearchTrainCommand, OnSearchTrain, OnCanSearchTrain));
      CommandBindings.Add(new CommandBinding(CreateVehicleListCommand, OnCreateVehicleList, OnCanCreateVehicleList));
      //CommandBindings.Add(new CommandBinding(SearchTrainZusiDBCommand, OnSearchTrainwithZusiDB));
      //CommandBindings.Add(new CommandBinding(SearchBuchfahrplanCommand, OnSearchBuchfahrplan));

      //CommandBindings.Add(new CommandBinding(SearchTrainZusiSKCommand, OnSearchTrainwithZusiSK, OnCanSearchTrainwithZusiSK));
      //CommandBindings.Add(new CommandBinding(ZSK_PositionOverlayCommand, OnZSK_PositionOverlayCommand, OnCanZSK_PositionOverlayCommand));
      //CommandBindings.Add(new CommandBinding(ZSK_FadenkreuzActiveCommand, OnZSK_FadenkreuzActiveCommand));
      //CommandBindings.Add(new CommandBinding(ZSK_FindStationCommand, OnZSK_FindStationCommand, OnCanZSK_PositionOverlayCommand));
      CommandBindings.Add(new CommandBinding(TrackTrainCommand, OnTrackTrain));

      CommandBindings.Add(new CommandBinding(ZusiBildFahrplanCommand, OnZusiBildFahrplan, OnCanZusiBildFahrplan));
      CommandBindings.Add(new CommandBinding(TrainStartSettingsCommand, OnTrainStartSettings));
      CommandBindings.Add(new CommandBinding(FrictionSettingsCommand, OnFrictionSettings, OnCanFrictionSettings));
      CommandBindings.Add(new CommandBinding(CommandAbout, OnAbout));
      CommandBindings.Add(new CommandBinding(CommandHelp, OnHelp));
      CommandBindings.Add(new CommandBinding(CommandExtDocu, OnExtDocu));
      CommandBindings.Add(new CommandBinding(CommandOptions, OnOptions));
      CommandBindings.Add(new CommandBinding(CommandQuit, OnQuit));

      CommandBindings.Add(new CommandBinding(CommandCreatePictures, OnCreatePictures));

      CommandBindings.Add(new CommandBinding(FilterRefreshCommand, OnFilterRefresh, OnCanFilterRefresh));
      CommandBindings.Add(new CommandBinding(FilterResetCommand, OnFilterReset, OnCanFilterReset));


      //CommandBindings.Add(new CommandBinding(ManageReplLocosCommand, OnManageReplLocos, OnCanManageReplLocos));
      //CommandBindings.Add(new CommandBinding(ReplaceLocoCommand, OnReplaceLoco, OnCanReplaceLoco));
      //CommandBindings.Add(new CommandBinding(UndoReplLocoCommand, OnUndoReplLoco, OnCanUndoReplLoco));
      //--
      CommandBindings.Add(new CommandBinding(ResetDataCommand, OnResetData, (s, e) => e.CanExecute = IsLoaded));
      CommandBindings.Add(new CommandBinding(ManageReplTrainsCommand, OnManageReplTrains, OnCanManageReplTrains));
      CommandBindings.Add(new CommandBinding(ReplaceTrainCommand, OnReplaceTrain, OnCanReplaceTrain));
      CommandBindings.Add(new CommandBinding(UndoReplTrainCommand, OnUndoReplTrain, OnCanUndoRepTrain));

      // *** add keyboard bindings

      //var focusListTimeTableGrps = new RoutedCommand();
      //CommandBindings.Add(new CommandBinding(focusListTimeTableGrps, (s, e) => ListTimeTableGrp.Focus()));
      //InputBindings.Add(new KeyBinding(focusListTimeTableGrps, Key.A, ModifierKeys.Control));

      //var focusListTimeTables = new RoutedCommand();
      //CommandBindings.Add(new CommandBinding(focusListTimeTables, (s, e) => ListTimeTables.Focus()));
      //InputBindings.Add(new KeyBinding(focusListTimeTables, Key.B, ModifierKeys.Control));

      //// *** add keyboard bindings

      //var FocusListtvPassengerTrains = new RoutedCommand();
      //CommandBindings.Add(new CommandBinding(FocusListtvPassengerTrains, (s, e) => tvPassengerTrains.Focus()));
      //InputBindings.Add(new KeyBinding(FocusListtvPassengerTrains, Key.C, ModifierKeys.Control));

      //var focusListFilter = new RoutedCommand();
      //CommandBindings.Add(new CommandBinding(focusListFilter, (s, e) => BrdFilter.Focus()));
      //InputBindings.Add(new KeyBinding(focusListFilter, Key.D, ModifierKeys.Control));


      //_miniServer.Run();

      DataManager? dataManager = DataContext as DataManager;
      dataManager.DecoTrainsAllowed += DataManager_DecoTrainsAllowed;
      dataManager.SelectedTimeTableChanged += DataManager_SelectedTimeTableChanged;
      dataManager.NotifyDataLoadStarted += DataManager_NotifyDataLoadStarted;
      dataManager.NotifyDataLoadCompleted += DataManager_NotifyDataLoadCompleted;
      //dataManager.webview = webView;

      //dataManager.webview_ZDB = webView_ZDB;
      //dataManager.webview_ZSK = webView_ZSK;
      dataManager.main_window = this;

      //dataManager.ProgressChanged += DataManager_ProgressChanged;
      //LoadLocalHtml();
    }

    //private void InitializeMap()
    //{
    //  gmap.MapProvider = GMap.NET.MapProviders.GMapProviders.OpenStreetMap;
    //  gmap.Position = new PointLatLng(50.0, 8.0); // Startposition
    //  gmap.MinZoom = 5;
    //  gmap.MaxZoom = 18;
    //  gmap.Zoom = 12;
    //  gmap.ShowCenter = false;

    //  // Marker direkt zur Markers-Sammlung hinzufügen
    //  marker = new GMapMarker(new PointLatLng(50.0, 8.0))
    //  {
    //    Shape = new Ellipse
    //    {
    //      Width = 24,
    //      Height = 24,
    //      Stroke = System.Windows.Media.Brushes.Red,
    //      StrokeThickness = 10
    //    }
    //  };
    //  gmap.Markers.Add(marker);

    //}

    private void Gmap_Loaded(object sender, RoutedEventArgs e)
    {
      //InitializeMap();
    }

    //---------------------------------------------------------------------
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    //---------------------------------------------------------------------
    private void Dispose(bool disposing)
    {
      if (disposing)
      {
        _miniServer?.Dispose();
        //ZusiPictureManager.Destroy();
        DataManager.Instance.Dispose();
        _fahrpult?.Dispose();
      }
    }

    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      //if (_webViewWindow_ZDB != null)
      //{
      //  _webViewWindow_ZDB.Close();
      //}
      //if (_webViewWindow_ZSK != null)
      //{
      //  _webViewWindow_ZSK.Close();
      //}
    }

    //---------------------------------------------------------------------
    protected override void OnClosing(CancelEventArgs e)
    {
      SaveWindowSettings();
      DataManager.Instance.SaveGeneralOptions();
      base.OnClosing(e);
      Dispose(true);
    }

    private void LoadWindowSettings()
    {
      this.Top = Properties.Settings.Default.WindowTop;
      this.Left = Properties.Settings.Default.WindowLeft;
      this.Height = Properties.Settings.Default.WindowHeight;
      this.Width = Properties.Settings.Default.WindowWidth;
      this.WindowState = Properties.Settings.Default.WindowState;
      AdjustWindowPosition();
    }

    private void SaveWindowSettings()
    {
      Properties.Settings.Default.WindowTop = this.Top;
      Properties.Settings.Default.WindowLeft = this.Left;
      Properties.Settings.Default.WindowHeight = this.Height;
      Properties.Settings.Default.WindowWidth = this.Width;
      Properties.Settings.Default.WindowState = this.WindowState;
      Properties.Settings.Default.Save();
    }

    private void AdjustWindowPosition()
    {
      if (this.Top < 0) this.Top = 0;
      //if (this.Left < 0) this.Left = 0;
      if (this.Top + this.Height > SystemParameters.VirtualScreenHeight)
        this.Top = SystemParameters.VirtualScreenHeight - this.Height;
      if (this.Left + this.Width > SystemParameters.VirtualScreenWidth)
        this.Left = SystemParameters.VirtualScreenWidth - this.Width;
    }

    public double GetScreenScaleFactor(System.Windows.Window window)
    {
      PresentationSource source = PresentationSource.FromVisual(window);
      if (source != null)
      {
        Matrix transformToDevice = source.CompositionTarget.TransformToDevice;
        return transformToDevice.M11; // This gives you the scale factor for the X dimension
      }
      return 1.0; // Default scale factor if source is null
    }

    private void OnAbout(object sender, ExecutedRoutedEventArgs e)
    {
      AboutDlg aboutDlg = new AboutDlg();
      aboutDlg.Owner = (System.Windows.Window)this;
      aboutDlg.ShowDialog();
    }

    private void OnQuit(object sender, ExecutedRoutedEventArgs e)
    {
      window.Close();
    }

    ////---------------------------------------------------------------------
    //private void OnCanManageReplLocos(object sender, CanExecuteRoutedEventArgs e)
    //{
    //  TrainItem ti = DataManager.Instance.SelectedTrain;
    //  e.CanExecute = ti == null || !ti.IsLocoReplaced;
    //}

    ////---------------------------------------------------------------------
    //private void OnManageReplLocos(object sender, ExecutedRoutedEventArgs e)
    //{
    //  DataManager dm = DataManager.Instance;
    //  dm.SelectedLoco = null;
    //  dm.SelectedReplacementLoco = null;
    //  ManageReplLocosDialog dlg = new()
    //  {
    //    Owner = this
    //  };
    //  if (dlg.ShowDialog() == true)
    //  {
    //    dm.SaveReplacementLocos();
    //  }
    //}

    ////---------------------------------------------------------------------
    //private void OnCanReplaceLoco(object sender, CanExecuteRoutedEventArgs e)
    //{
    //  TrainItem ti = DataManager.Instance.SelectedTrain;
    //  e.CanExecute = ti != null && !ti.IsLocoReplaced;
    //}

    ////---------------------------------------------------------------------
    //private void OnReplaceLoco(object sender, ExecutedRoutedEventArgs e)
    //{
    //  DataManager.Instance.SelectedTrain.ReplaceLoco(this);
    //}

    ////---------------------------------------------------------------------
    //private void OnCanUndoReplLoco(object sender, CanExecuteRoutedEventArgs e)
    //{
    //  TrainItem ti = DataManager.Instance.SelectedTrain;
    //  e.CanExecute = ti != null && ti.IsLocoReplaced;
    //}

    ////---------------------------------------------------------------------
    //private void OnUndoReplLoco(object sender, ExecutedRoutedEventArgs e)
    //{
    //  DataManager.Instance.SelectedTrain.UndoReplaceLoco();
    //}

    private void OnCreatePictures(object sender, ExecutedRoutedEventArgs e)
    {
      CreatePicturesDlg CreatePicturesDlg = new CreatePicturesDlg();
      CreatePicturesDlg.Owner = (System.Windows.Window)this;
      CreatePicturesDlg.ShowDialog();
    }

    private void OnHelp(object sender, ExecutedRoutedEventArgs e)
    {
      HelpDlg helpDlg = new HelpDlg();
      helpDlg.ShowDialog();
    }

    private void OnExtDocu(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {
        string relativePath = "help/ZusiStart_Docu_Deutsch.pdf";
        string absolutePath = System.IO.Path.GetFullPath(relativePath);
        Process.Start("cmd.exe", $"/c start \"\" \"{absolutePath}\"");
        //Process.Start(absolutePath);
      }
      catch (Exception ex)
      {
        int num = (int)System.Windows.MessageBox.Show(ex.ToString(), "Fehler beim Öffnen der Dokumentation", MessageBoxButton.OK);

      }
    }

    private void OnOptions(object sender, ExecutedRoutedEventArgs e)
    {
      OptionsDlg optionsDlg = new OptionsDlg();
      optionsDlg.Owner = (System.Windows.Window)this;
      optionsDlg.ShowDialog();
    }

    //---------------------------------------------------------------------
    private void OnCanManageReplTrains(object sender, CanExecuteRoutedEventArgs e)
    {
      TrainItem ti = DataManager.Instance.CurrentTrainItem;
      e.CanExecute = ti == null || !ti.IsTrainReplaced;
    }

    //---------------------------------------------------------------------
    private void OnManageReplTrains(object sender, ExecutedRoutedEventArgs e)
    {
      DataManager dm = DataManager.Instance;
      dm.SelectedTrainR = null;
      dm.SelectedReplacementTrain = null;
      ManageReplTrainsDialog dlg = new()
      {
        Owner = this
      };
      if (dlg.ShowDialog() == true)
      {
        dm.SaveReplacementTrains();
      }
    }


    //---------------------------------------------------------------------
    private void OnCanReplaceTrain(object sender, CanExecuteRoutedEventArgs e)
    {
      Zug zg = DataManager.Instance.CurrentTrain;
      TrainItem ti = DataManager.Instance.CurrentTrainItem;
      e.CanExecute = zg != null && !(ti != null && ti.IsTrainReplaced);
    }

    //---------------------------------------------------------------------
    private void OnReplaceTrain(object sender, ExecutedRoutedEventArgs e)
    {
      if (DataManager.Instance.CurrentTrainItem != null)
      {
        DataManager.Instance.CurrentTrainItem.ReplaceTrain(this);
      }
      else
      {
        DataManager.Instance.CurrentTrainItem = new TrainItem(DataManager.Instance.CurrentTrain);
        DataManager.Instance.CurrentTrainItem.ReplaceTrain(this);
      }
    }

    //---------------------------------------------------------------------
    private void OnCanUndoRepTrain(object sender, CanExecuteRoutedEventArgs e)
    {
      TrainItem ti = DataManager.Instance.CurrentTrainItem;
      e.CanExecute = ti != null && ti.IsTrainReplaced;
    }

    //---------------------------------------------------------------------
    private void OnFilterRefresh(object sender, ExecutedRoutedEventArgs e)
    {
      DataManager_RefreshFilter(sender, e);
    }

    //---------------------------------------------------------------------
    private void OnCanFilterRefresh(object sender, CanExecuteRoutedEventArgs e)
    {
      //TrainItem ti = DataManager.Instance.CurrentTrainItem;
      //e.CanExecute = ti != null && ti.IsTrainReplaced;
      e.CanExecute = true;
    }

    //---------------------------------------------------------------------
    private void OnFilterReset(object sender, ExecutedRoutedEventArgs e)
    {
      tblFilterZugNummer.Text = "";
      Checkbox_Dekozüge.IsChecked = false;
      Checkbox_Personenzüge.IsChecked = true;
      Checkbox_Güterzüge.IsChecked = true;
      DataManager_RefreshFilter(sender, e);
    }

    //---------------------------------------------------------------------
    private void OnCanFilterReset(object sender, CanExecuteRoutedEventArgs e)
    {
      //TrainItem ti = DataManager.Instance.CurrentTrainItem;
      //e.CanExecute = ti != null && ti.IsTrainReplaced;
      e.CanExecute = true;
    }

    //---------------------------------------------------------------------
    private void OnUndoReplTrain(object sender, ExecutedRoutedEventArgs e)
    {
      DataManager.Instance.CurrentTrainItem.UndoReplaceTrain();
    }

    // **Overlay **
    //public void PositionOverlay()
    //{
    //  // Sicherstellen, dass WebView2 korrekt gerendert wurde
    //  if (PresentationSource.FromVisual(webView_ZSK) != null)
    //  {
    //    // Hole die Position des WebView2 im Hauptfenster
    //    System.Windows.Point position = webView_ZSK.PointToScreen(new System.Windows.Point(0, 0));

    //    // Setze die Position des Overlay-Fensters passend
    //    overlay.Left = position.X;
    //    overlay.Top = position.Y;
    //    overlay.Width = webView_ZSK.ActualWidth;
    //    overlay.Height = webView_ZSK.ActualHeight;
    //  }
    //}
    // **Overlay **
    //private void UpdateOverlay()
    //{
    //  if (PresentationSource.FromVisual(this) != null && overlay != null)
    //  {
    //    // Get updated screen position
    //    System.Windows.Point position = webView_ZSK.PointToScreen(new System.Windows.Point(0, 0));

    //    // Adjust overlay window position and size dynamically
    //    overlay.Left = position.X;
    //    overlay.Top = position.Y;
    //    overlay.Width = webView_ZSK.ActualWidth;
    //    overlay.Height = webView_ZSK.ActualHeight;
    //  }
    //}

    //---------------------------------------------------------------------
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      Activate();

      _dataLoaderWindow = new DataLoaderWindow
      {
        Owner = this
      };

      overlay = new OverlayWindow();
      overlay.Owner = this; // Verknüpft die Fenster miteinander
                            //overlay.Show();
                            // Listen for Window size and position changes
                            // **Overlay **
                            //this.SizeChanged += (s, e) => UpdateOverlay();
                            //this.LocationChanged += (s, e) => UpdateOverlay();

      // Detect when the MainWindow gets or loses focus
      //this.Activated += (s, e) => overlay.Show();
      //this.Deactivated += (s, e) => overlay.Hide();
      //PositionOverlay();


      DataManager.Instance.InitializeData();
      DataManager.Instance.dataLoaderWindow = _dataLoaderWindow;
      DataManager.Instance.ScreenScaleFactor = GetScreenScaleFactor(this);
      _dataLoaderWindow.ShowDialog();
      get_bildfahrplanpath();
      get_zusidisplaypath();
      get_zusimeterpath();
      createZSK_locationGroupId_dict();
      VersionVisibility = Visibility.Visible;
      if ((Startup.commandlineargs.Length == 3 && Startup.commandlineargs[1] == "--start"))
      {
        Log.Debug("Commandline option --start");

        string train = Startup.commandlineargs[2];

        try
        {
          train = Zusi.DataPath[0] + "Timetables\\" + train;
          ZugDatei zd = new ZugDatei(null, train);
          if (zd.Root == null)
          {
            zd.Parse();
          }
          DataManager.Instance.CurrentTrain = zd.Root;
          if (zd.Root == null)
            Log.Debug($"Message: {train} not found");
          else
          {
            //starte aktuellen Zug:
            object _sender = null;
            ExecutedRoutedEventArgs _e = null;
            OnStartTrain(_sender, _e);
          }

        }
        catch (Exception ex)
        {
          Log.Debug(ex.Message);
        }
      }
    }

    private void get_bildfahrplanpath()
    {
      string registryPath = @"SOFTWARE\Zusi3\Fahrsim\Einstellungen\MenuBildfahrplanFPL";
      string registryPathSteam = @"SOFTWARE\Zusi3\Fahrsimsteam\Einstellungen\MenuBildfahrplanFPL";
      string attributeName = "Datei";
      bool isRegistered = false;
      try
      {

        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath))
        {
          if (key != null)
          {
            object value = key.GetValue(attributeName);
            if (value != null)
            {
              DataManager.Instance.BildfahrplanExePath = value as string;
              isRegistered = true;
            }
          }
        }
        if (!isRegistered)
        {
          using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPathSteam))
          {
            if (key != null)
            {
              object value = key.GetValue(attributeName);
              if (value != null)
              {
                DataManager.Instance.BildfahrplanExePath = value as string;
                isRegistered = true;
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error accessing registry: {ex.Message}");
      }
    }

    private void get_zusimeterpath()
    {
      string registryPath = @"SOFTWARE\Zusi3\Fahrsim\Einstellungen\MenuZusiMeter";
      string registryPathSteam = @"SOFTWARE\Zusi3\Fahrsimsteam\Einstellungen\MenuZusiMeter";
      string attributeName = "Datei";
      bool isRegistered = false;
      try
      {

        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath))
        {
          if (key != null)
          {
            object value = key.GetValue(attributeName);
            if (value != null)
            {
              DataManager.Instance.ZusiMeterStartCmd = value as string;
              isRegistered = true;
            }
          }
        }
        if (!isRegistered)
        {
          using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPathSteam))
          {
            if (key != null)
            {
              object value = key.GetValue(attributeName);
              if (value != null)
              {
                DataManager.Instance.ZusiMeterStartCmd = value as string;
                isRegistered = true;
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error accessing registry: {ex.Message}");
      }
    }

    public string get_zusidisplaypath()
    {
      try
      {
        string zusiexe_path = System.IO.Path.GetDirectoryName(Zusi.Executable);
        string zusidisplay_path = System.IO.Path.Combine(zusiexe_path, @"_Tools\ZusiDisplay\");
        string zusidisplay_startcmd = zusidisplay_path + @"ZusiDisplay.64.exe";
        DataManager.Instance.ZusiDisplayStartCmd = zusidisplay_startcmd;
        return zusidisplay_startcmd;


      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error accessing registry: {ex.Message}");
        return "";
      }
    }

    //---------------------------------------------------------------------
    private void DataManager_DecoTrainsAllowed(object? sender, EventArgs e)
    {
      CollectionViewSource.GetDefaultView(tvPassengerTrains.ItemsSource).Refresh();
      //CollectionViewSource.GetDefaultView(tvFreightTrains.ItemsSource).Refresh();
      CollectionViewSource.GetDefaultView(tvFoundPassengerTrains.ItemsSource).Refresh();
      //CollectionViewSource.GetDefaultView(tvFoundFreightTrains.ItemsSource).Refresh();
    }


    //---------------------------------------------------------------------
    private void DataManager_RefreshFilter(object? sender, EventArgs e)
    {
      CollectionViewSource.GetDefaultView(tvPassengerTrains.ItemsSource).Refresh();
    }

    //---------------------------------------------------------------------
    private void DataManager_NotifyDataLoadStarted(object sender, NotifyDataLoadStartedEventArgs e)
    {
      _dataLoaderWindow.SetLoaderType(e.LoaderType);
    }

    //---------------------------------------------------------------------
    private void DataManager_NotifyDataLoadCompleted(object sender, EventArgs e)
    {
      _dataLoaderWindow.Close();
      //FadeOut(rctMask, 600, null);
      FadeIn(brdMain, 600);
      MainBorderVisibility = Visibility.Visible;
      StatusBarVisibility = Visibility.Visible;
    }

#if false
        //---------------------------------------------------------------------
        private void DataManager_ProgressChanged(object sender, ZusiStart.Data.ProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(() => _dataLoaderWindow.Progress += e.Step);
        }
#endif

    //---------------------------------------------------------------------
    private void DataManager_SelectedTimeTableChanged(object sender, EventArgs e)
    {
      CollectionViewSource.GetDefaultView(tvPassengerTrains.ItemsSource).Refresh();
      //CollectionViewSource.GetDefaultView(tvFreightTrains.ItemsSource).Refresh();
    }

    //---------------------------------------------------------------------
    private void OnExitGame(object sender, ExecutedRoutedEventArgs e)
    {
      Close();
    }

    //---------------------------------------------------------------------
    private void OnCanMinimize(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = WindowState != WindowState.Minimized;
    }

    //---------------------------------------------------------------------
    private void OnMinimize(object sender, ExecutedRoutedEventArgs e)
    {
      WindowState = WindowState.Minimized;
    }

    public void NotifyCanExecuteChanged()
    {
      CommandManager.InvalidateRequerySuggested();
    }

    //---------------------------------------------------------------------
    private void OnCanFavoriteTrainsPage(object sender, CanExecuteRoutedEventArgs e)
    {
      int n = (DataManager.Instance.RecentTrains?.Count ?? 0);
      e.CanExecute = n > 0;
    }

    //---------------------------------------------------------------------
    private void OnFavoriteTrainsPage(object sender, ExecutedRoutedEventArgs e)
    {
      DataManager.Instance.CurrentTrain = null;
      DataManager.Instance.SelectedRecentTrain = null;
      MainBorderVisibility = Visibility.Collapsed;
      SearchBorderVisibility = Visibility.Collapsed;
      RecentTrainsBorderVisibility = Visibility.Visible;
      MainWebBorderVisibility = Visibility.Collapsed;
    }

    //---------------------------------------------------------------------
    private void OnCanAddFavoriteTrain(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = DataManager.Instance.CurrentTrain != null;
    }

    //---------------------------------------------------------------------
    private void OnAddFavoriteTrain(object sender, ExecutedRoutedEventArgs e)
    {
      Zug zug = DataManager.Instance.CurrentTrain;
      string timeTablefilename = zug.FahrplanDatei.FullPath;

      string timeTableName = System.IO.Path.GetFileNameWithoutExtension(timeTablefilename);
      DataManager.Instance.RecentTrains.Add(new RecentTrain(zug, timeTableName));
      DataManager.Instance.RecentTrains.Save();
    }

    //---------------------------------------------------------------------
    private void OnEditFavoriteComment(object sender, ExecutedRoutedEventArgs e)
    {
      RecentTrain rt = DataManager.Instance.SelectedRecentTrain;

      string comment = rt.Comment;

      if (comment =="")
      {
        rt.Comment = rt.Train.Gattung+rt.Train.Nummer;
      }

      rt.CommentEdit = !rt.CommentEdit;

      DataManager.Instance.RecentTrains.Save();
    }

    //---------------------------------------------------------------------
    private void OnCanDelFavoriteTrain(object sender, CanExecuteRoutedEventArgs e)
    {
      if (DataManager.Instance.SelectedRecentTrain != null)
      {
        e.CanExecute = DataManager.Instance.SelectedRecentTrain.Train != null;
      }
      else
      {
        e.CanExecute = false;
      }
    }

    //---------------------------------------------------------------------
    private void OnDelFavoriteTrain(object sender, ExecutedRoutedEventArgs e)
    {
      Zug zug = DataManager.Instance.SelectedRecentTrain.Train;
      string timeTablefilename = zug.FahrplanDatei.FullPath;

      string timeTableName = System.IO.Path.GetFileNameWithoutExtension(timeTablefilename);
      int index = 0;
      foreach (RecentTrain train in DataManager.Instance.RecentTrains)
      {
        if (train.TimeTableName == timeTableName)
        {
          break;
        }
        index++;
      }
      DataManager.Instance.RecentTrains.RemoveAt(index);
      DataManager.Instance.RecentTrains.Save();
    }

    //---------------------------------------------------------------------
    private void OnSearchPage(object sender, ExecutedRoutedEventArgs e)
    {
      DataManager.Instance.CurrentTrain = null;
      DataManager.Instance.SelectedRecentTrain = null;
      DataManager.Instance.CurrentTrainItem = null;
      MainBorderVisibility = Visibility.Collapsed;
      MainWebBorderVisibility = Visibility.Collapsed;
      SearchBorderVisibility = Visibility.Visible;
      RecentTrainsBorderVisibility = Visibility.Collapsed;

      //if (DataManager.Instance.FilteredVehicles == null)
      //{
      //  //DataManager.Instance.OnNotifyDataLoadStarted(LoaderType.LoadComplete);
      //  DataManager.Instance.FilteredVehicles = DataManager.Instance.FilterVehicles();

      //  DataManager.Instance.CountFilteredVehicles = DataManager.Instance.FilteredVehicles.Count();
      //  //DataManager.Instance.OnNotifyDataLoadCompleted();
      //}

    }

    //---------------------------------------------------------------------
    private void OnTimeTablePage(object sender, ExecutedRoutedEventArgs e)
    {
      DataManager.Instance.CurrentTrain = null;
      DataManager.Instance.SelectedRecentTrain = null;
      DataManager.Instance.CurrentTrainItem = null;
      MainBorderVisibility = Visibility.Visible;
      MainWebBorderVisibility = Visibility.Collapsed;
      SearchBorderVisibility = Visibility.Collapsed;
      RecentTrainsBorderVisibility = Visibility.Collapsed;
    }

    //---------------------------------------------------------------------
    private void OnCanStartFavTrain(object sender, CanExecuteRoutedEventArgs e)
    {
      bool canExecute = Properties.Settings.Default.TrainStartMode != 1 || ZusiSim.CanStart;
      e.CanExecute = canExecute && DataManager.Instance.SelectedRecentTrain != null;
    }

    //---------------------------------------------------------------------
    private void OnCanStartTrain(object sender, CanExecuteRoutedEventArgs e)
    {
      bool canExecute = Properties.Settings.Default.TrainStartMode != 1 || ZusiSim.CanStart;
      e.CanExecute = canExecute && DataManager.Instance.CurrentTrain != null;
    }

    //---------------------------------------------------------------------
    private void OnCanSearchPage(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = DataManager.Instance.AllVehicles_loaded;
    }

    private bool? ZusiTCPServerenabled()
    {

      bool flag = false;
      bool flag2 = false;
      bool flag3 = false;
      string text = "Software\\Zusi3\\Fahrsim\\Einstellungen";

      try
      {
        using (Registry.CurrentUser.OpenSubKey(text, writable: true))
        {
          Debug.WriteLine("ZusiTCPServerenabled key " + text + " found");
          flag3 = false;
          flag2 = false;
        }
      }
      catch
      {
        flag3 = true;
      }

      if (flag3)
      {
        try
        {
          text = "Software\\Zusi3\\Fahrsimsteam\\Einstellungen";
          using (Registry.CurrentUser.OpenSubKey(text, writable: true))
          {
            Debug.WriteLine("ZusiTCPServerenabled key " + text + " found");
            flag2 = true;
          }
        }
        catch
        {
          flag2 = false;
          flag = true;
        }
      }

      if (flag)
      {
        Debug.WriteLine("ZusiTCPServerenabled no ZUSI entry found");
        return false;
      }

      bool flag4 = true;
      string keyVal = ((!flag2) ? ("Software\\Zusi3\\Fahrsim\\Einstellungen") : ("Software\\Zusi3\\Fahrsimsteam\\Einstellungen"));
      RegistryKey registryKey = null;
      int NetzwerkServerAutom_value = 0;

      try
      {
        registryKey = Registry.CurrentUser.OpenSubKey(keyVal, writable: true);
        if (registryKey != null)
        {
          Debug.WriteLine("ZusiTCPServerenabled key " + keyVal + " found");
          NetzwerkServerAutom_value = (int)registryKey.GetValue("NetzwerkServerAutom");
          Debug.WriteLine("ZusiTCPServerenabled keydata " + NetzwerkServerAutom_value);
        }
      }
      catch
      {
        return null;
      }
      if (NetzwerkServerAutom_value != 1)
      {
        MessageBoxResult result = System.Windows.MessageBox.Show("Der ZUSI TCP Netzwerkserver ist NICHT eingeschaltet. Soll der ZUSI TCP Netwerkserver eingeschaltet werden?\n\nJa: TCP Server wird eingeschaltet \nNein: ZUSI wird mit Kommandozeile gestartet \nAbbrechen: Start wird abgebrochen", "Fehler beim Starten von Zug mit TCP Fahrpultschnittstelle", MessageBoxButton.YesNoCancel);
        switch (result)
        {
          case MessageBoxResult.Yes:
            if (registryKey != null)
            {
              registryKey.SetValue("NetzwerkServerAutom", 1, RegistryValueKind.DWord);
              return true;
            }

            break;
          case MessageBoxResult.No:
            return false;
            break;
          case MessageBoxResult.Cancel:
            return null;
            break;
        }
        return null;
      }
      else
      {
        return true;
      }


    }

    //---------------------------------------------------------------------
    public void OnStartFavTrain(object sender, ExecutedRoutedEventArgs e)
    {
      Zug zug = DataManager.Instance.SelectedRecentTrain.Train;
      OnStartTrain(zug);
    }

    //---------------------------------------------------------------------
    public void OnStartTrain(object sender, ExecutedRoutedEventArgs e)
    {
      Zug zug = DataManager.Instance.CurrentTrain;
      OnStartTrain(zug);
    }

    //---------------------------------------------------------------------
    public void OnStartTrain(Zug zug)
    {
      try
      {
        TimeTable timeTable = DataManager.Instance.GetTimeTableOfTrain(zug);
        if (timeTable == null)
        {
          TimeTableFile timetablefile = new TimeTableFile(zug.FahrplanDatei.FullPath);
          timetablefile.Parse();
          timeTable = timetablefile.Root;
        }

        ZusiDocumentBase? doc = timeTable?.GetDocument();

        if (Properties.Settings.Default.OptimiseSchedule == 0)
        {
          string tmpTimeTableFileName = "";
          string tempzugfilename = DataManager.Instance.BuildTempTimeTable(zug, timeTable, out tmpTimeTableFileName);
          if (!string.IsNullOrEmpty(tempzugfilename))
          {
            // start train
            FrictionSettingsPopupVisible = false;
            HideWindow();
            if (Properties.Settings.Default.TrainStartMode == 0)
            {
              // ... via TCP interface
              bool? TCPserver = ZusiTCPServerenabled();
              if (TCPserver == null)
                return;
              if (TCPserver == true)
              {
                TrainStartInfo tsi = new() { TimetableFile = tmpTimeTableFileName, TrainNumber = zug.Nummer };
                _fahrpult.TryStartTrain(tsi);
              }
              else
              {
                // ... commandline
                ZusiSim.Start(tempzugfilename);
              }
            }
            else
            {
              // ... commandline
              ZusiSim.Start(tempzugfilename);
            }
            if (Properties.Settings.Default.StartBildfahrplan == 0)
            {
              StartZusiBildFahrplan(tempzugfilename, tmpTimeTableFileName);
            }
            if (Properties.Settings.Default.StartFIS == 0 && DataManager.Instance.FIS_available)
            {
              StartZusiDisplay();
            }
            if (Properties.Settings.Default.StartZusiMeter == 0)
            {
              StartZusiMeter();
            }
          }
        }
        else
        {

          // start train
          //HideWindow();
          if (Properties.Settings.Default.TrainStartMode == 0)
          {
            // ... via TCP interface
            bool? TCPserver = ZusiTCPServerenabled();
            if (TCPserver == null)
              return;
            if (TCPserver == true)
            {
              TimeTable tt = DataManager.Instance.GetTimeTableOfTrain(zug);
              string timetablefilename = "";
              if (tt != null)
              {
                timetablefilename = tt.GetDocument().Filename;
              }
              else
              {
                timetablefilename = zug.FahrplanDatei.FullPath;
              }
              TrainStartInfo tsi = new() { TimetableFile = timetablefilename, TrainNumber = zug.Nummer };
              _fahrpult.TryStartTrain(tsi);
            }
            else
            {
              // ... commandline
              ZusiSim.Start(zug.GetDocument().Filename);
            }
          }
          else
          {
            // ... commandline
            ZusiSim.Start(zug.GetDocument().Filename);
          }
          if (Properties.Settings.Default.StartBildfahrplan == 0)
          {
            StartZusiBildFahrplan();
          }
          if (Properties.Settings.Default.StartFIS == 0 && DataManager.Instance.FIS_available)
          {
            StartZusiDisplay();
          }
          if (Properties.Settings.Default.StartZusiMeter == 0)
          {
            StartZusiMeter();
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
      }
    }

    //---------------------------------------------------------------------
    private void OnResetData(object sender, ExecutedRoutedEventArgs e)
    {
      ResetDataDialog dlg = new()
      {
        Owner = this
      };
      if (dlg.ShowDialog() == true)
      {
        if (ResetData(dlg.DeleteRecentTrains))
        {
          Close();
        }
      }
    }

    //---------------------------------------------------------------------
    private void OnCanFrictionSettings(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = true;
    }

    //---------------------------------------------------------------------
    private void OnFrictionSettings(object sender, ExecutedRoutedEventArgs e)
    {
      FrictionSettingsPopupVisible = !FrictionSettingsPopupVisible;
    }

    //---------------------------------------------------------------------
    private void ZusiSim_Terminated(object sender, EventArgs e)
    {
      Dispatcher.BeginInvoke(new Action(() => ShowWindow()));
      ZusiStart.Connection.ZusiMeter.Quit();
      ZusiStart.Connection.ZusiDisplay.Quit();
      ZusiStart.Connection.ZusiBildFahrplan.Quit();
    }

    //---------------------------------------------------------------------
    private void OnCanCreateVehicleList(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = DataManager.Instance.FilteredVehicles == null;
    }

    //---------------------------------------------------------------------
    private void OnCreateVehicleList(object sender, ExecutedRoutedEventArgs e)
    {
      if (DataManager.Instance.FilteredVehicles == null)
      {
        //DataManager.Instance.OnNotifyDataLoadStarted(LoaderType.LoadComplete);
        DataManager.Instance.FilteredVehicles = DataManager.Instance.FilterVehicles();

        DataManager.Instance.CountFilteredVehicles = DataManager.Instance.FilteredVehicles.Count();
        //DataManager.Instance.OnNotifyDataLoadCompleted();
      }
    }

    //---------------------------------------------------------------------
    private void OnCanSearchTrain(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = !string.IsNullOrEmpty(tbxTrainNumber.Text);
    }

    //---------------------------------------------------------------------
    private void OnSearchTrain(object sender, ExecutedRoutedEventArgs e)
    {
      if (((DataManager)DataContext).SearchTrain(tbxTrainNumber.Text))
      {
        tblkMessage.Visibility = Visibility.Hidden;
      }
      else
      {
        tblkMessage.Visibility = Visibility.Visible;
      }
    }

    private void createZSK_locationGroupId_dict()
    {
      string dateipfad = "Resources/ZSK_locationGroupId.csv"; // Pfad zur CSV-Datei

      string absolutePath = System.IO.Path.GetFullPath(dateipfad);
      Dictionary<string, string> ZSK_locationGroupId_dict = new Dictionary<string, string>();

      foreach (var line in System.IO.File.ReadLines(absolutePath))
      {
        var teile = line.Split(','); // Annahme: CSV ist komma-separiert
        if (teile.Length == 2) // Sicherstellen, dass genau zwei Werte vorhanden sind
        {
          DataManager.Instance.ZSK_locationGroupId_dict[teile[0].Trim()] = teile[1].Trim();
        }
      }
    }


    //private async void StartZusiWebBrowser(string url = "https://www.zusidatenbank.de")
    //{
    //  bool int_window = false;

    //  if (int_window)
    //  {
    //    DataManager.Instance.webview_ZDB.CoreWebView2.SourceChanged += CoreWebView2SourceChanged_ZDB;
    //    DataManager.Instance.webview_ZSK.CoreWebView2.SourceChanged += CoreWebView2SourceChanged_ZSK;
    //    //DataManager.Instance.webview_ZSK.CoreWebView2.FrameNavigationStarting += (sender, args) =>
    //    //{
    //    //  Console.WriteLine($"Frame navigating to: {args.Uri}");

    //    //  // Example: Cancel navigation if the URL contains "blockedsite.com"
    //    //  if (args.Uri.Contains("blockedsite.com"))
    //    //  {
    //    //    args.Cancel = true;
    //    //    Console.WriteLine("Navigation blocked.");
    //    //  }
    //    //};


    //    MainBorderVisibility = Visibility.Collapsed;
    //    MainWebBorderVisibility = Visibility.Visible;
    //    SearchBorderVisibility = Visibility.Collapsed;
    //    RecentTrainsBorderVisibility = Visibility.Collapsed;
    //    await webView_ZDB.EnsureCoreWebView2Async(null);
    //    webView_ZDB.CoreWebView2.Navigate(url);
    //  }
    //  else
    //  {
    //    //MainBorderVisibility = Visibility.Collapsed;
    //    //MainWebBorderVisibility = Visibility.Collapsed;
    //    //SearchBorderVisibility = Visibility.Visible;
    //    //RecentTrainsBorderVisibility = Visibility.Collapsed;

    //    if (_webViewWindow_ZDB == null || !_webViewWindow_ZDB.IsLoaded)
    //    {
    //      _webViewWindow_ZDB = new WebView_Window();
    //      _webViewWindow_ZDB.set_websource(url);
    //      _webViewWindow_ZDB.Show();
    //      await _webViewWindow_ZDB.webViewWin.EnsureCoreWebView2Async(DataManager.Instance.webview_environment);

    //    }
    //    else
    //    {
    //      _webViewWindow_ZDB.Close();
    //      _webViewWindow_ZDB = new WebView_Window();
    //      _webViewWindow_ZDB.set_websource(url);
    //      _webViewWindow_ZDB.Show();
    //      await _webViewWindow_ZDB.webViewWin.EnsureCoreWebView2Async(DataManager.Instance.webview_environment);
    //      //_webViewWindow_ZDB.set_websource(url);
    //      //_webViewWindow_ZDB.Activate();
    //    }

    //    //var _webview_window = new WebView_Window();
    //    //_webViewWindow_ZDB.webViewWin.CoreWebView2.Navigate(url);


    //    //if (_webViewWindow_ZDB.IsLoaded)
    //    //{
    //    //  _webViewWindow_ZDB.Show();
    //    //  _webViewWindow_ZDB.Closed += _webViewWindow_ZDB_Closed;
    //    //}
    //  }
    //}

    //private void _webViewWindow_ZDB_Closed(object sender, EventArgs e)
    //{
    //  //_webViewWindow_ZDB = null;
    //}

    //private void _webViewWindow_ZSK_Closed(object sender, EventArgs e)
    //{
    //  _webViewWindow_ZSK = null;
    //}


    //private void OnWebView_NavigationCompleted_ZDB(object sender, CoreWebView2NavigationCompletedEventArgs e)
    //{
    //  //urlTextBlock_ZDB.Text = webView_ZDB.Source.ToString();
    //}

    //private void OnWebView_NavigationCompleted_ZSK(object sender, CoreWebView2NavigationCompletedEventArgs e)
    //{
    //  //urlTextBlock_ZDB.Text = webView_ZDB.Source.ToString();
    //}

    //public async System.Threading.Tasks.Task NavigateToUrlAsync_ZDB(string url)
    //{
    //  var tcs = new TaskCompletionSource<bool>();

    //  EventHandler<CoreWebView2NavigationCompletedEventArgs> handler = null;
    //  handler = (sender, e) =>
    //  {
    //    webView_ZDB.NavigationCompleted -= handler;
    //    tcs.SetResult(true);
    //  };

    //  webView_ZDB.NavigationCompleted += handler;
    //  try
    //  {

    //    webView_ZDB.CoreWebView2.Navigate(url);
    //  }
    //  catch (Exception ex)
    //  {
    //    Xceed.Wpf.Toolkit.MessageBox.Show("URL: < " + url + " > \n" + ex.Message + "\nBitte in den Optionen korrigieren", "Fehler beim Öffnen der URL", MessageBoxButton.OK, MessageBoxImage.Error);
    //  }
    //  await tcs.Task;
    //}

    //public async void set_websource_ZDB(string value)
    //{
    // // adapted to new concept
    //  var zdbTab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == "Zusi-DB");
    //  if (zdbTab?.WebViewInstance?.CoreWebView2 != null)
    //  {
    //    zdbTab.WebViewInstance.CoreWebView2.Navigate(value);
    //  }

    //}

    //public async System.Threading.Tasks.Task NavigateToUrlAsync_ZSK(string url)
    //{
    //  var tcs = new TaskCompletionSource<bool>();

    //  EventHandler<CoreWebView2NavigationCompletedEventArgs> handler = null;
    //  handler = (sender, e) =>
    //  {
    //    webView_ZSK.NavigationCompleted -= handler;
    //    tcs.SetResult(true);
    //  };

    //  webView_ZSK.NavigationCompleted += handler;
    //  try
    //  {
    //    webView_ZSK.CoreWebView2.Navigate(url);
    //  }
    //  catch (Exception ex)
    //  {
    //    Xceed.Wpf.Toolkit.MessageBox.Show("URL: < " + url + " > \n" + ex.Message + "\nBitte in den Optionen korrigieren", "Fehler beim Öffnen der URL", MessageBoxButton.OK, MessageBoxImage.Error);
    //  }
    //  await tcs.Task;
    //}

    //public async void set_websource_ZSK(string value)
    //{
    //  // adapted to new concept
    //  var zdbTab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_Streckenkarte);
    //  if (zdbTab?.WebViewInstance?.CoreWebView2 != null)
    //  {
    //    zdbTab.WebViewInstance.CoreWebView2.Navigate(value);
    //  }
    //}

    public void CoreWebView2SourceChanged_ZDB(object? sender, CoreWebView2SourceChangedEventArgs e)
    {
      string newSource = ((CoreWebView2)sender).Source.ToString();
      DataManager.Instance.webview_ZDB_source = newSource;
      //urlTextBlock_ZDB.Text = newSource;
      //string[] source_parts = newSource.Split('?'); // ZUSIDatabase Start-Button
      //newSource = source_parts[0];

      if (newSource.EndsWith(".trn/"))
      {
        string train_number = "";
        string[] pathitems = newSource.Split("%5C");
        if (pathitems != null && pathitems.Count() == 5)
        {
          train_number = Zusi.DataPath[0] + "Timetables\\" + pathitems[1] + "\\" + pathitems[2] + "\\" + pathitems[3] + "\\" + pathitems[4];
          train_number = train_number.Replace(".trn/", "");
          train_number = train_number + ".trn";
          //bool train_found = ((DataManager)DataContext).SearchTrain(train_number);
          ZugDatei zd = new ZugDatei(null, train_number);
          if (zd.Root == null)
          {
            zd.Parse();

          }
          DataManager.Instance.CurrentTrain = zd.Root;

          //if (source_parts.Count() > 1)
          //{
          //  object dummy_sender = null;
          //  ExecutedRoutedEventArgs dummy_e = null;
          //  OnStartTrain(dummy_sender, dummy_e);
          //}
        }
      }
      else if (newSource.EndsWith(".st3"))
      {
        string train_number = "";
        string[] pathitems = newSource.Split("%5C");
        if (pathitems != null && pathitems.Count() == 5)
        {
          train_number = pathitems[4];

          bool train_found = ((DataManager)DataContext).SearchTrain(train_number);

          if (train_found)
          {
            DataManager.Instance.CurrentTrain = null;
            DataManager.Instance.SelectedRecentTrain = null;
            DataManager.Instance.CurrentTrainItem = null;
            MainBorderVisibility = Visibility.Collapsed;
            MainWebBorderVisibility = Visibility.Collapsed;
            SearchBorderVisibility = Visibility.Visible;
            RecentTrainsBorderVisibility = Visibility.Collapsed;
          }

          //if (source_parts.Count() > 1)
          //{
          //  object dummy_sender = null;
          //  ExecutedRoutedEventArgs dummy_e = null;
          //  OnStartTrain(dummy_sender, dummy_e);
          //}
        }
      }
      else if (newSource.Contains("?zugstart="))
      {
        //starte aktuellen Zug:
        object _sender = null;
        ExecutedRoutedEventArgs _e = null;
        DataManager.Instance.main_window.OnStartTrain(_sender, _e);
      }

    }

    private Dictionary<string, string> locationFiles = new Dictionary<string, string>
        {
            { "11.892356985347789/52.047453270489115", "Deutschland\\Magdeburg_Dessau\\" },
            { "10.377657338289906/52.863821938369256","Deutschland\\Hamburg_Kassel\\"},
            { "Tokyo", "Tokyo_report.pdf" }
        };

    public void CoreWebView2SourceChanged_ZSK(object? sender, CoreWebView2SourceChangedEventArgs e)
    {
      string newSource = ((CoreWebView2)sender).Source.ToString();
      Log.Debug("ZSK-Webview_Source:" + newSource);
      System.Diagnostics.Debug.WriteLine($"***** CoreWebView2SourceChanged_ZSK: Source {newSource} *****");
      DataManager.Instance.webview_ZSK_source = newSource;

      if (newSource.EndsWith(".trn/"))
      {
        string train_number = "";
        string[] pathitems = newSource.Split("%5C");
        if (pathitems != null && pathitems.Count() == 5)
        {
          train_number = Zusi.DataPath[0] + "Timetables\\" + pathitems[1] + "\\" + pathitems[2] + "\\" + pathitems[3] + "\\" + pathitems[4];
          train_number = train_number.Replace(".trn/", "");
          train_number = train_number + ".trn";
          //bool train_found = ((DataManager)DataContext).SearchTrain(train_number);
          ZugDatei zd = new ZugDatei(null, train_number);
          if (zd.Root == null)
          {
            zd.Parse();

          }
          DataManager.Instance.CurrentTrain = zd.Root;

          //if (source_parts.Count() > 1)
          //{
          //  object dummy_sender = null;
          //  ExecutedRoutedEventArgs dummy_e = null;
          //  OnStartTrain(dummy_sender, dummy_e);
          //}
        }
      }
      else if (newSource.EndsWith(".st3"))
      {
        string train_number = "";
        string[] pathitems = newSource.Split("%5C");
        if (pathitems != null && pathitems.Count() == 5)
        {
          train_number = pathitems[4];

          bool train_found = ((DataManager)DataContext).SearchTrain(train_number);

          if (train_found)
          {
            DataManager.Instance.CurrentTrain = null;
            DataManager.Instance.SelectedRecentTrain = null;
            DataManager.Instance.CurrentTrainItem = null;
            MainBorderVisibility = Visibility.Collapsed;
            MainWebBorderVisibility = Visibility.Collapsed;
            SearchBorderVisibility = Visibility.Visible;
            RecentTrainsBorderVisibility = Visibility.Collapsed;
          }

          //if (source_parts.Count() > 1)
          //{
          //  object dummy_sender = null;
          //  ExecutedRoutedEventArgs dummy_e = null;
          //  OnStartTrain(dummy_sender, dummy_e);
          //}
        }
      }
      else if (newSource.Contains("zusi-sk.eu/#"))
      {
        Log.Debug("ZSK-Webview_Source: Startswith Zusi_Sk" + newSource);
        string[] newSource_parts = newSource.Split("#");
        string location_str = newSource_parts[1];
        string[] location_parts = location_str.Split("/");
        string location = location_parts[0] + "/" + location_parts[1];
        if (DataManager.Instance.ZSK_locationGroupId_dict.TryGetValue(location, out string filenames))
        {

          System.Diagnostics.Debug.WriteLine($"***** Filename for {location}: {filenames} *****");

          string tt_group_id = filenames;
          DataManager.Instance.FoundTimeTableGroupTitle = tt_group_id;

          TimeTableGroup? found_tt_group = null;

          foreach (TimeTableGroup tt_group in DataManager.Instance.GroupedTimeTables)
          {
            if (tt_group.GroupID == tt_group_id)
            {
              found_tt_group = tt_group;
              break;
            }
          }
          if (found_tt_group != null)
          {
            List<TimeTable> timetable_list = found_tt_group.Members;

            if (timetable_list.Count != 0)
            {
              DataManager.Instance.FoundTimeTableGroupTitle = tt_group_id;
              DataManager.UpdateTimeTableRelations(timetable_list, 0);
            }
          }
        }
        else
        {
          DataManager.Instance.FoundTimeTableGroupTitle = "";
          // Englische Kultur für die korrekte Erkennung von '.'
          CultureInfo englishCulture = CultureInfo.InvariantCulture;

          if (location_parts.Length >= 2 && double.TryParse(location_parts[0], NumberStyles.Float, englishCulture, out double longitude) &&
                                double.TryParse(location_parts[1], NumberStyles.Float, englishCulture, out double latitude))
          {

            longitude = Math.Round(longitude, 2);
            latitude = Math.Round(latitude, 2);

            // Wieder in einen String umwandeln
            string roundedCoordinates = $"{longitude.ToString("F2", englishCulture)}/{latitude.ToString("F2", englishCulture)}";
            System.Diagnostics.Debug.WriteLine($"***** Search Module for {roundedCoordinates}:  *****");
            if (DataManager.Instance.ZSK_locationGroupId_dict.TryGetValue(roundedCoordinates, out string modules))
            {
              System.Diagnostics.Debug.WriteLine($"***** Filename for {roundedCoordinates}: {modules} *****");
              DataManager.Instance.FoundStationTitle = modules;
            }
            else
            {
              DataManager.Instance.FoundStationTitle = "";
            }
          }
        }
      }
    }

    //private void CoreWebView2_NewWindowRequested_ZDB(object sender, CoreWebView2NewWindowRequestedEventArgs e)
    //{
    //  string url_requested = e.Uri;
    //  string[] url_requested_parts;
    //  string betriebsstelle = "";
    //  string strecke = "";

    //  if (url_requested.Contains("?Betriebsstelle="))
    //  {
    //    url_requested_parts = url_requested.Split("?");
    //    if (url_requested_parts.Count() == 2)
    //    {
    //      if (url_requested_parts[0].EndsWith(".st3"))
    //      {
    //        string searchString = "Betriebsstelle=";
    //        string url = url_requested_parts[1];

    //        int startIndex = url.IndexOf(searchString) + searchString.Length;
    //        betriebsstelle = url.Substring(startIndex);
    //        betriebsstelle = betriebsstelle.Replace("+", " ");
    //        //DataManager.Instance.main_window.tbxTrainNumber.Text = betriebsstelle;
    //        bool betriebsstelle_found = DataManager.Instance.SearchTrain(betriebsstelle);

    //        if (betriebsstelle_found)
    //        {
    //          e.Handled = true; // Verhindert das Öffnen eines neuen Fensters
    //          DataManager.Instance.CurrentTrain = null;
    //          DataManager.Instance.SelectedRecentTrain = null;
    //          DataManager.Instance.CurrentTrainItem = null;
    //          //DataManager.Instance.main_window.tbxTrainNumber.Text = betriebsstelle;
    //          return;
    //        }
    //      }
    //    }
    //  }

    //  if (url_requested.Contains("?Strecke="))
    //  {
    //    url_requested_parts = url_requested.Split("?");
    //    if (url_requested_parts.Count() == 2)
    //    {
    //      if (url_requested_parts[0].EndsWith(".st3"))
    //      {
    //        string searchString = "Strecke=";
    //        string url = url_requested_parts[1];

    //        int startIndex = url.IndexOf(searchString) + searchString.Length;
    //        strecke = url.Substring(startIndex);
    //        strecke = strecke.Replace("+", " ");
    //        url_requested = url_requested_parts[0];
    //      }
    //    }
    //  }

    //  if (url_requested.EndsWith(".st3"))
    //  {
    //    e.Handled = true; // Verhindert das Öffnen eines neuen Fensters
    //    string train_number = "";
    //    string[] pathitems = url_requested.Split("%5C");
    //    if (pathitems != null && pathitems.Count() == 5)
    //    {
    //      train_number = pathitems[4];

    //      bool train_found = DataManager.Instance.SearchTrain(train_number);

    //      if (train_found)
    //      {
    //        e.Handled = true; // Verhindert das Öffnen eines neuen Fensters
    //        DataManager.Instance.CurrentTrain = null;
    //        DataManager.Instance.SelectedRecentTrain = null;
    //        //DataManager.Instance.main_window.tbxTrainNumber.Text = train_number;

    //        if (strecke != "")
    //        {
    //          DataManager.Instance.SearchResultTitle = strecke;
    //        }
    //      }
    //    }
    //  }
    //}

    //private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
    //{
    //  // Check if the URL is the one you want to intercept
    //  if (e.Uri.Contains("?zugstart="))
    //  {
    //    //starte aktuellen Zug:
    //    object _sender = null;
    //    ExecutedRoutedEventArgs _e = null;
    //    e.Cancel = true;
    //    DataManager.Instance.main_window.OnStartTrain(_sender, _e);
    //    // Navigate to the new URL
    //    //webView2.CoreWebView2.Navigate("https://newurl.com");
    //  }
    //  if (e.Uri.Contains("fpndatei="))
    //  {
    //    string uri = e.Uri;

    //    string[] pathitems = uri.Split("=");
    //    if (pathitems != null && pathitems.Count() == 2)
    //    {
    //      e.Cancel = true;
    //      string fpnname = pathitems[1];
    //      fpnname = fpnname.Replace("?", "\\");
    //      TimeTable timeTable = DataManager.Instance.GetTimeTableOfFpnName(fpnname);

    //      if (timeTable != null)
    //      {
    //        TimeTableRelation value = new(0, timeTable);

    //        DataManager.Instance.OnSelectedTimeTableChanged(value);
    //      }
    //    }
    //  }
    //}

    //private void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    //{
    //  bool test = true;
    //}

    //private void OnSearchTrainwithZusiDB(object sender, ExecutedRoutedEventArgs e)
    //{
    //  if (string.IsNullOrEmpty(DataManager.Instance.options.ZDB_Url))
    //    StartZusiWebBrowser(url: _url_zusi_datenbank);
    //  else
    //    StartZusiWebBrowser(url: DataManager.Instance.options.ZDB_Url);
    //}

    //private void OnSearchBuchfahrplan(object sender, ExecutedRoutedEventArgs e)
    //{
    //  SearchBuchfahrplan();
    //}
    private async void SearchPDFjs(Microsoft.Web.WebView2.Wpf.WebView2 EfpTab, string searchstring)
    {

      await EfpTab.CoreWebView2.ExecuteScriptAsync(@"
PDFViewerApplication.page = 1; // Gehe zur ersten Seite  
PDFViewerApplication.findController.executeCommand('find', {
    query: '" + searchstring + @"',
    phraseSearch: true,
    caseSensitive: false,
    highlightAll: true
  });
setTimeout(() => {
    const matches = PDFViewerApplication.findController._pageMatches;
    const totalMatches = matches.reduce((sum, pageMatches) => sum + pageMatches.length, 0);

    if (totalMatches === 0) {
      alert('Kein Eintrag für " + searchstring + @" gefunden.');
    }
  }, 1000); // Warte kurz, bis die Suche abgeschlossen ist
");

    }


    public async void SetLaNummer(string lanr)
    {
      if (lanr != null)
      {
        // handle La-PDF
        var LaTab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_La_Hdb);
        if (LaTab != null)
        {
          if (LaTab.PDFViewerUrl != null)
          {
            //string page1string = LaTab.PDFViewerUrl + "#page=" + 1;
            //LaTab.WebViewInstance.Source = new Uri(page1string);
            string searchstring = lanr + " ";
            SearchPDFjs(LaTab.WebViewInstance, searchstring);
          }
        }
        //handle Ersatzfahrpläne
        var EfpTab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_Ersatzfahrplan);
        string Efpnr = "999.pdf";
        if (EfpTab != null)
        {
          if (EfpTab.PDFViewerUrl != null)
          {
            if (DataManager.Instance.spMax >= 30 && DataManager.Instance.spMax <= 60)
            {
              Efpnr = "993.pdf";
            }
            if (DataManager.Instance.spMax >= 70 && DataManager.Instance.spMax <= 100)
            {
              Efpnr = "996.pdf";
            }
            string PDFViewerURL = EfpTab.PDFViewerUrl;
            if (PDFViewerURL.Length >= 7)
            {
              PDFViewerURL = PDFViewerURL.Substring(0, PDFViewerURL.Length - 7) + Efpnr;
            }

            //string searchstring = EfpTab.PDFViewerUrl + "#search=Strecke " + lanr;
            string searchstring = "Strecke " + lanr;
            try
            {
              if (PDFViewerURL != EfpTab.PDFViewerUrl)
              {
                EfpTab.WebViewInstance.Source = new Uri(PDFViewerURL);
                EfpTab.PDFViewerUrl = PDFViewerURL;
              }
              SearchPDFjs(EfpTab.WebViewInstance, searchstring);

            }
            catch
            {

            }
          }
        }
      }
    }

    public void SearchBuchfahrplan()
    {
      ZT_Buchfahrplan bfpl = new ZT_Buchfahrplan();

      string filename; // = @"C:\Program Files\Zusi3\_ZusiData\Timetables\Deutschland\Hamburg_Kassel\Guntershausen-Edesheim_2020_06Uhr-10Uhr\RB14202_14009.timetable.xml";
      DataPathType dtp = DataPathType.Unknown;

      Zug zug = DataManager.Instance.CurrentTrain ?? DataManager.Instance.SelectedRecentTrain?.Train;
      if (zug != null)
      {
        try
        {
          ZugDatei trn_file = zug.Parent as ZugDatei;
          filename = Zusi.GetRelativePathOf(trn_file.Filename, ref dtp);
          if (zug.BuchfahrplanRohDatei != null)
          {
            string bfpl_filename = zug.BuchfahrplanRohDatei.FullPath;
            filename = System.IO.Path.Combine(bfpl_filename, "");
            Bitmap bitmap = bfpl.show_Buchfahrplan(filename);
            if (bitmap != null)
            {
              //buchfahrplanImage.Source = BitmapToImageSource(bitmap);

              var imageTab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_buchfahrplan);
              if (imageTab != null)
              {
                imageTab.ImageSource = BitmapToImageSource(bitmap);  // @"D:\Pfad\zum\Fahrplanbild.png"; // oder .jpg
              }
            }
            DataManager.Instance.LaNumberList.Clear();

            DataManager.Instance.LaNumberList.AddRange(bfpl.LaNumberList);

            string lanr = bfpl.LaNumberList?.FirstOrDefault();
            SetLaNummer(lanr);

            //if (lanr != null)
            //{
            //  var LaTab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_La_Hdb);
            //  if (LaTab != null)
            //  {
            //    if (LaTab.PDFViewerUrl != null)
            //    {
            //      string searchstring = LaTab.PDFViewerUrl+"#search=" + lanr +" ";
            //      LaTab.WebViewInstance.Source = new Uri(searchstring);
            //    }
            //  }
            //}
          }
          else
          {
            //buchfahrplanImage.Visibility = Visibility.Collapsed;
          }
        }
        catch
        {
          //buchfahrplanImage.Visibility = Visibility.Collapsed;
        }
      }

    }
    // Convert Bitmap to BitmapImage
    private BitmapImage BitmapToImageSource(Bitmap bitmap)
    {
      using (MemoryStream memory = new MemoryStream())
      {
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
        memory.Position = 0;
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = memory;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        return bitmapImage;
      }
    }


    //public async System.Threading.Tasks.Task NavigateToUrlAsync(string url)
    //{
    //  var tcs = new TaskCompletionSource<bool>();

    //  EventHandler<CoreWebView2NavigationCompletedEventArgs> handler = null;
    //  handler = (sender, e) =>
    //  {
    //    webView_ZSK.NavigationCompleted -= handler;
    //    tcs.SetResult(true);
    //  };

    //  webView_ZSK.NavigationCompleted += handler;
    //  try
    //  {

    //    webView_ZSK.CoreWebView2.Navigate(url);
    //  }
    //  catch (Exception ex)
    //  {
    //    Xceed.Wpf.Toolkit.MessageBox.Show("URL: < " + url + " > \n" + ex.Message + "\nBitte in den Optionen korrigieren", "Fehler beim Öffnen der URL", MessageBoxButton.OK, MessageBoxImage.Error);


    //  }
    //  // webViewWin.CoreWebView2.Reload();

    //  await tcs.Task;
    //  //webViewWin.CoreWebView2.Reload();
    //}

    //public async void set_websource(string value)
    //{
    //  //_websource = value;
    //  await webView_ZSK.EnsureCoreWebView2Async(null);
    //  if (webView_ZSK.CoreWebView2 != null)
    //  {
    //    //_navigation_completed = false;
    //    NavigateToUrlAsync(value);
    //    //webViewWin.CoreWebView2.Navigate(value);
    //  }
    //  //webViewWin.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
    //}

    //---------------------------------------------------------------------
    //private void OnCanSearchTrainwithZusiSK(object sender, CanExecuteRoutedEventArgs e)
    //{
    //  bool canExecute = DataManager.Instance.options.Show_ZSK;
    //  e.CanExecute = canExecute;
    //}

    //private void OnZSK_PositionOverlayCommand(object sender, ExecutedRoutedEventArgs e)
    //{
    //  // **Overlay **
    //  //PositionOverlay();
    //  //overlay.Show();
    //}

    //private void OnCanZSK_PositionOverlayCommand(object sender, CanExecuteRoutedEventArgs e)
    //{
    //  bool canExecute = DataManager.Instance.ZSK_Fadenkreuz_active;
    //  e.CanExecute = canExecute;
    //}

    //private void OnZSK_FadenkreuzActiveCommand(object sender, ExecutedRoutedEventArgs e)
    //{
    //  PositionOverlay();
    //  if (DataManager.Instance.ZSK_Fadenkreuz_active)
    //  {
    //    overlay.Hide();
    //    DataManager.Instance.ZSK_Fadenkreuz_active = false;
    //    ZSK_FadenkreuzActiveButton.Content = "Fadenkreuz Anzeigen";
    //  }
    //  else
    //  {
    //    overlay.Show();
    //    DataManager.Instance.ZSK_Fadenkreuz_active = true;
    //    ZSK_FadenkreuzActiveButton.Content = "Fadenkreuz Löschen";
    //  }

    //}

    //private void OnZSK_FindStationCommand(object sender, ExecutedRoutedEventArgs e)
    //{
    //  if (DataManager.Instance.ZSK_Fadenkreuz_active)
    //  {
    //    if (DataManager.Instance.FoundStationTitle != "")
    //    {
    //      bool found = DataManager.Instance.SearchBetriebsstelleTrain(DataManager.Instance.FoundStationTitle);
    //    }

    //  }
    //}

    //private void ZSK_Border_SizeChanged(object sender, SizeChangedEventArgs e)
    //{
    //  PositionOverlay();
    //}


    //private void OnSearchTrainwithZusiSK(object sender, ExecutedRoutedEventArgs e)
    //{

    //  string zsk_url = "";
    //  if (DataManager.Instance.SelectedTimeTableRelation.TimeTable != null)
    //  {
    //    System.Windows.Point point = DataManager.Instance.SelectedTimeTableRelation.TimeTable.Utm.ToLatLon();

    //    zsk_url = DataManager.Instance.options.ZSK_Url + "#" + point.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/10";
    //    zsk_url = zsk_url.Replace(",", "/");
    //  }
    //  if (!string.IsNullOrEmpty(zsk_url))
    //    StartZusiWebBrowser(url: zsk_url);
    //  else
    //  {
    //    if (string.IsNullOrEmpty(DataManager.Instance.options.ZSK_Url))
    //      StartZusiWebBrowser(url: _url_zusi_strecken_karte);
    //    else
    //      StartZusiWebBrowser(url: DataManager.Instance.options.ZSK_Url + "#10.5/50.96983/6");
    //  }
    //}

    private void OnTrackTrain(object sender, ExecutedRoutedEventArgs e)
    {
    }

    //private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //{
    //  if (e.Source is TabControl) // Ensures the event is from TabControl
    //  {
    //    var tabControl = sender as TabControl;
    //    var selectedTab = tabControl.SelectedItem as TabItem;


    //    if (selectedTab != null)
    //    {
    //      switch (selectedTab.Header)
    //      {
    //        case "Doku":
    //          overlay.Hide();
    //          FunctionForTab1();
    //          break;
    //        case "Zusi-Streckenkarte":
    //          FunctionForTab2();
    //          break;
    //        case "Zusi-Datenbank":
    //          overlay.Hide();
    //          FunctionForTab3();
    //          break;
    //        case "Buchfahrplan":
    //          overlay.Hide();
    //          FunctionForTab4();
    //          break;
    //        case "Tracking":
    //          overlay.Hide();
    //          FunctionForTab5();
    //          break;
    //      }
    //    }
    //  }
    //}

    //private void FunctionForTab1()
    //{
    //  _activetab = 1;
    //}

    //private void FunctionForTab2()
    //{
    //  _activetab = 2;
    //  try
    //  {
    //    if (string.IsNullOrEmpty(DataManager.Instance.options.ZSK_Url))
    //      ReloadPageWithDummyNavigation(_url_zusi_strecken_karte);
    //    else
    //      ReloadPageWithDummyNavigation(DataManager.Instance.options.ZSK_Url + "#10.5/50.96983/6");

    //    //overlay.Show();
    //    PositionOverlay();
    //  }
    //  catch
    //  {

    //  }
    //}

    //private void FunctionForTab3()
    //{
    //  _activetab = 3;
    //}

    //private void FunctionForTab4()
    //{
    //  _activetab = 4;
    //  SearchBuchfahrplan();
    //}

    //private void FunctionForTab5()
    //{
    //  _activetab = 5;
    //}

    //---------------------------------------------------------------------
    private void OnCanZusiBildFahrplan(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = DataManager.Instance.BildfahrplanExePath != "";
    }

    private void OnZusiBildFahrplan(object sender, ExecutedRoutedEventArgs e)
    {
      StartZusiBildFahrplan();
    }

    private void StartZusiBildFahrplan(string trn_filename = "", string timetablefilename = "")
    {
      // ... commandline
      DataPathType dtp = DataPathType.Unknown;
      //string Bildfahrplanfilename = @"C:\Program Files\Zusi3\_Tools\ZUSIBildfahrplan\TimetableGraphProject.exe";
      string Bildfahrplanfilename = "";
      //string BildfahrplanParameter = "-fpn _Timetables\\Deutschland\\Ruhrtalbahn\\Hagen-Kassel_Fahrplan1981_04Uhr-12Uhr.fpn -trn _Timetables\\Deutschland\\Ruhrtalbahn\\Hagen-Kassel_Fahrplan1981_04Uhr-12Uhr\\D17441.trn -zn _17441 -mode FPL";
      string BildfahrplanParameter = "";

      if (DataManager.Instance.BildfahrplanExePath != "")
      {
        Bildfahrplanfilename = DataManager.Instance.BildfahrplanExePath;
      }
      if (Bildfahrplanfilename != "")
      {

        Zug zug = DataManager.Instance.CurrentTrain ?? DataManager.Instance.SelectedRecentTrain?.Train;
        if (zug != null)
        {
          try
          {
            ZugDatei trn_file = zug.Parent as ZugDatei;
            if (string.IsNullOrEmpty(trn_filename))
            {
              trn_filename = Zusi.GetRelativePathOf(trn_file.Filename, ref dtp);
              timetablefilename = Zusi.GetRelativePathOf(zug.FahrplanDatei.FullPath, ref dtp);
            }

            string zugnummer = zug.Nummer;

            BildfahrplanParameter = "-fpn _" + timetablefilename + " -trn _" + trn_filename + " -zn _" + zugnummer + " -mode " + _zusibildfahrplan_mode;
          }
          catch { }
        }

        else
        {
          BildfahrplanParameter = "";
        }

        ZusiBildFahrplan.Start(Bildfahrplanfilename, BildfahrplanParameter);
      }
    }

    private void StartZusiDisplay()
    {
      string startcmd = "";

      if (DataManager.Instance.options.ZusiDisplay_Exe != "")
      {
        startcmd = DataManager.Instance.options.ZusiDisplay_Exe;
      }
      if (startcmd != "")
      {
        ZusiStart.Connection.ZusiDisplay.Start(startcmd, (DataManager.Instance.options.ZusiDisplay_Param));
      }
    }

    private void StartZusiMeter()
    {
      string startcmd = "";

      if (DataManager.Instance.options.ZusiMeter_Exe != "")
      {
        startcmd = DataManager.Instance.options.ZusiMeter_Exe;
      }
      if (startcmd != "")
      {
        ZusiStart.Connection.ZusiMeter.Start(startcmd, (DataManager.Instance.options.ZusiMeter_Param));
      }
    }

    //private void UpdateMarkerPosition(double latitude, double longitude)
    //{
    //  marker.Position = new PointLatLng(latitude, longitude);
    //  gmap.Position = marker.Position; // Karte auf neue Position zentrieren
    //}


    //---------------------------------------------------------------------
    private void OnTrainStartSettings(object sender, ExecutedRoutedEventArgs e)
    {
      TrainStartSettingsDialog dlg = new()
      {
        Owner = this
      };
      if (dlg.ShowDialog() == true)
      {
        //if (dlg.ForbidAlternativePicLibSources)
        //{
        //  ZusiPictureManager.AllowAlternativeSources();
        //}
        //else
        //{
        //  ZusiPictureManager.ForbidAlternativeSources();
        //}

        Properties.Settings.Default.ForbidAlternativePicLibSources = dlg.ForbidAlternativePicLibSources;
        Properties.Settings.Default.TrainStartMode = dlg.StartViaThrottle ? 0 : 1;
        Properties.Settings.Default.OptimiseSchedule = dlg.OptimiseSchedule ? 0 : 1;
        Properties.Settings.Default.Use_LS3_Renderer_DLL = dlg.Use_LS3_Renderer_DLL ? 0 : 1;
        Properties.Settings.Default.StartBildfahrplan = dlg.StartBildfahrplan ? 0 : 1;
        Properties.Settings.Default.StartFIS = dlg.StartFIS ? 0 : 1;
        Properties.Settings.Default.StartZusiMeter = dlg.StartZusiMeter ? 0 : 1;

        Properties.Settings.Default.Save();
      }
    }

    //---------------------------------------------------------------------
    public event RoutedEventHandler SortModeChanged
    {
      add { AddHandler(SortButton.SortModeChangedEvent, value); }
      remove { RemoveHandler(SortButton.SortModeChangedEvent, value); }
    }

    //---------------------------------------------------------------------
    private void MainWindow_SortModeChanged(object sender, RoutedEventArgs e)
    {
      if (e.OriginalSource is SortButton sb)
      {
        if (sb.GetVisualAncestor<TreeViewItem>() is TreeViewItem tvi)
        {
          tvi.Items.SortDescriptions.Clear();
          switch (sb.SortMode)
          {
            case SortMode.None:
              tvi.Items.SortDescriptions.Add(new SortDescription("StartTime", ListSortDirection.Ascending));
              break;
            case SortMode.Ascending:
              tvi.Items.SortDescriptions.Add(new SortDescription("JourneyTime", ListSortDirection.Ascending));
              tvi.Items.SortDescriptions.Add(new SortDescription("StartTime", ListSortDirection.Ascending));
              break;
            case SortMode.Descending:
              tvi.Items.SortDescriptions.Add(new SortDescription("JourneyTime", ListSortDirection.Descending));
              tvi.Items.SortDescriptions.Add(new SortDescription("StartTime", ListSortDirection.Descending));
              break;
          }
          e.Handled = true;
        }
      }
    }

    //---------------------------------------------------------------------
    private void EngageScaling()
    {
      WpfScreen wpfScreen = WpfScreen.GetScreenFrom(this);
      System.Windows.Rect bounds = wpfScreen.DeviceBounds;
      uint dpi = wpfScreen.Dpi;
      if (dpi != 96 || bounds.Width < 1920 || bounds.Height < 1080)
      {
        double scale = Math.Min(bounds.Width / 1920, bounds.Height / 1080) * 96.0 / dpi;
        if (scale != 1)
        {
          //mainGrid.LayoutTransform = new ScaleTransform(scale, scale, 0, 0);
          //Log.DebugFormat("added scaling matrix with factor {0}", scale);
        }
      }
    }

    //---------------------------------------------------------------------
    private static void FadeIn(UIElement what, double duration)
    {
      Duration d = new(TimeSpan.FromMilliseconds(duration));
      DoubleAnimation da = new(1.0, d);
      what.BeginAnimation(OpacityProperty, da);
    }

    //---------------------------------------------------------------------
    private static void FadeOut(UIElement what, double duration, Action? completed)
    {
      Duration d = new(TimeSpan.FromMilliseconds(duration));
      DoubleAnimation da = new(0.0, d);
      da.Completed += (s, e) => completed?.Invoke();
      what.BeginAnimation(OpacityProperty, da);
    }

    //---------------------------------------------------------------------
    private void HideWindow()
    {
      FadeOut(this, 1000, () => Hide());
    }

    //---------------------------------------------------------------------
    private void ShowWindow()
    {
      Show();
      FadeIn(this, 600);
    }

    //---------------------------------------------------------------------
    private void FreightTrains_Filter(object sender, FilterEventArgs e)
    {
      TrainsFilter("Güterzüge", e);
    }

    //---------------------------------------------------------------------
    private void PassengerTrains_Filter(object sender, FilterEventArgs e)
    {
      TrainsFilter("Personenzüge", e);
    }

    //---------------------------------------------------------------------
    private void GeneralTrains_Filter(object sender, FilterEventArgs e)
    {
      if (e.Item is TrainsViewModel tvm)
      {
        e.Accepted = false;
        if ((bool)Checkbox_Personenzüge.IsChecked)
        {
          e.Accepted = tvm.DisplayName == "Personenzüge";
        }
        if ((bool)Checkbox_Güterzüge.IsChecked)
        {
          e.Accepted = tvm.DisplayName == "Güterzüge" || e.Accepted;
        }

        DataManager.Instance.FilterZugNummer = tblFilterZugNummer.Text;

        //string number = tblFilterZugNummer.Text;

        //string n = number.Replace(" ", "").Trim().ToLower();
        //bool train_found = (string.Compare(n, (z.Nummer).Replace(" ", "").Trim(), true) == 0 ||
        //            string.Compare(n, (z.Gattung + z.Nummer).Replace(" ", "").Trim(), true) == 0);
      }
    }

    //---------------------------------------------------------------------
    private void TrainsFilter(string criteria, FilterEventArgs e)
    {
      if (e.Item is TrainsViewModel tvm)
      {
        e.Accepted = tvm.DisplayName == criteria;
      }
    }

    //---------------------------------------------------------------------
    private void ListBox_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
      if (e.NewFocus is DependencyObject d)
      {
        ListBoxItem lbi = d.GetVisualAncestor<ListBoxItem>();
        if (lbi != null && !lbi.IsSelected)
        {
          lbi.IsSelected = true;

          TimeTableOverviewControl ttoc = d.GetVisualAncestor<TimeTableOverviewControl>();
          if (ttoc != null)
          {
            ttoc.RaiseSelectedTimeTableChanged();
          }
        }
      }
    }

    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      ScrollViewer scrollViewer = sender as ScrollViewer;
      if (scrollViewer != null)
      {
        if (e.Delta > 0)
        {
          scrollViewer.LineUp();
        }
        else
        {
          scrollViewer.LineDown();
        }

        e.Handled = true;
      }
    }

    //---------------------------------------------------------------------
    private void TimeTableOverviewControl_SelectedTimeTableChanged(object sender, SelectedTimeTableChangedEventArgs e)
    {
      DataManager.UpdateTimeTableRelations(e.TimeTables, e.PreferredSelection);
    }

    //---------------------------------------------------------------------
    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      TrainsViewModel? tvm = e.NewValue as TrainsViewModel;
      DataManager.Instance.CurrentTrainItem = null;
      DataManager.Instance.CurrentTrain = tvm?.Object;

    }

    //private async void ReloadPageWithDummyNavigation(string originalUrl)
    //{
    //  try
    //  {
    //    // Navigate to a dummy page
    //    //DataManager.Instance.webview_ZSK.Source = new Uri("about:blank");

    //    // Wait for the dummy page to load completely
    //    //await System.Threading.Tasks.Task.Delay(500); // Adjust delay as needed

    //    // Navigate back to the original page
    //    DataManager.Instance.webview_ZSK.Source = new Uri(originalUrl);

    //    DataManager.Instance.webview_ZSK.Reload();
    //    PositionOverlay();
    //  }

    //  catch
    //  {

    //  }
    //}

    private void SelectedTimeTableChanged(object sender, SelectionChangedEventArgs e)
    {
      //Ensure the sender is a ListBox
      if (sender is ListBox listBox)
      {

        TimeTableGroup? selectedItem2 = listBox.SelectedItem as TimeTableGroup;
        TimeTableRelation? selectedItem = listBox.SelectedItem as TimeTableRelation;
        TimeTable selectedTT;
        if (selectedItem2 != null) // Timetablegroup
        {

          if (DataManager.Instance.SkipTimeTableSelectionUpdate > 0)
          {
            selectedItem = null;
            DataManager.Instance.SkipTimeTableSelectionUpdate -= 1;
          }
          else
          {
            //DataManager.Instance.SkipTimeTableSelectionUpdate = true;
            List<TimeTable> timeTables = selectedItem2.Members;
            //DataManager.Instance.UpdateTimeTableRelations_impl(selectedItem2.Members, 0);
            DataManager.Instance.SelectedTimeTableRelation = null;

            int n = timeTables.Count;

            DataManager.Instance.Relations.Clear();
            for (int i = 0; i < n; i++)
            {
              TimeTableRelation ttr = new(i + 1, timeTables[i]);
              DataManager.Instance.Relations.Add(ttr);
            }

            if (DataManager.Instance.Relations.Count >= 1)
            {

              DataManager.Instance.SelectedTimeTableRelation = DataManager.Instance.Relations[0];

              //if (!string.IsNullOrEmpty(SelectedTimeTableRelation.Begruessungsdatei))
              //{
              //  DataManager.Instance.webview.Source = new Uri(SelectedTimeTableRelation.Begruessungsdatei);
              //}
              //if (!string.IsNullOrEmpty(SelectedTimeTableRelation.TimeTable.GetDocument().Filename))
              //{

              //  DataPathType dtp = DataPathType.Unknown;
              //  string orgRelativeTimetableName = Zusi.GetRelativePathOf(SelectedTimeTableRelation.TimeTable.GetDocument().Filename, ref dtp);
              //  orgRelativeTimetableName = orgRelativeTimetableName.Replace("\\", "%5C");
              //  string url = "https://www.zusidatenbank.de/fahrplan/" + orgRelativeTimetableName;


              //  DataManager.Instance.webview_ZDB.Source = new Uri(url);
              //}
            }
            // **Overlay **
            //PositionOverlay();
          }
        }

        // Call your desired function or logic
        if (selectedItem != null)
        {
          if (!string.IsNullOrEmpty(selectedItem.Begruessungsdatei))
          {
            //DataManager.Instance.webview.Source = new Uri(selectedItem.Begruessungsdatei);
            DataManager.Instance.set_websource(DataManager.Instance.tab_title_docu, selectedItem.Begruessungsdatei);
          }
          //if (false) //_activetab == 2)
          //{
          //  System.Windows.Point point = selectedItem.TimeTable.Utm.ToLatLon();
          //  string zsk_url = DataManager.Instance.options.ZSK_Url + "#" + point.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/10";
          //  zsk_url = zsk_url.Replace(",", "/");
          //  ReloadPageWithDummyNavigation(zsk_url);
          //}
          //  //listBox.UnselectAll();
          //  //listBox.UpdateLayout();
        }

      }
    }


    private void SelectedTimeTableRelationChanged(object sender, SelectionChangedEventArgs e)
    {
      // Ensure the sender is a ListBox
      if (sender is ListBox listBox)
      {
        // Get the selected item
        TimeTableRelation? selectedItem = listBox.SelectedItem as TimeTableRelation;

        // Call your desired function or logic
        if (selectedItem != null)
        {
          if (!string.IsNullOrEmpty(selectedItem.Begruessungsdatei))
          {
            //DataManager.Instance.webview.Source = new Uri(selectedItem.Begruessungsdatei);
            DataManager.Instance.set_websource(DataManager.Instance.tab_title_docu, selectedItem.Begruessungsdatei);
          }
          //if (false) // _activetab == 2)
          //{
          //  System.Windows.Point point = selectedItem.TimeTable.Utm.ToLatLon();
          //  string zsk_url = DataManager.Instance.options.ZSK_Url + "#" + point.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/10";
          //  zsk_url = zsk_url.Replace(",", "/");
          //  ReloadPageWithDummyNavigation(zsk_url);
          //}
          //if (_activetab == 5)
          //{
          //  System.Windows.Point point = selectedItem.TimeTable.Utm.ToLatLon();
          //  UpdateMarkerPosition(point.Y, point.X);
          //}
        }
        //listBox.UnselectAll();
        //listBox.UpdateLayout();
      }
    }

    private void SelectedTimeTableRelationChanged2(object sender, SelectionChangedEventArgs e)
    {
      // Ensure the sender is a ListBox
      if (sender is ListBox listBox)
      {
        // Get the selected item
        TimeTableRelation? selectedItem = listBox.SelectedItem as TimeTableRelation;

        // Call your desired function or logic
        if (selectedItem != null)
        {
          if (!string.IsNullOrEmpty(selectedItem.Begruessungsdatei))
          {
            //DataManager.Instance.webview.Source = new Uri(selectedItem.Begruessungsdatei);
            DataManager.Instance.set_websource(DataManager.Instance.tab_title_docu, selectedItem.Begruessungsdatei);
          }
          DataManager.Instance.LaNumberList.Clear();
          //if (false) //_activetab == 2)
          //{
          //  System.Windows.Point point = selectedItem.TimeTable.Utm.ToLatLon();
          //  string zsk_url = DataManager.Instance.options.ZSK_Url + "#" + point.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/10";
          //  zsk_url = zsk_url.Replace(",", "/");
          //  ReloadPageWithDummyNavigation(zsk_url);
          //}
          //if (_activetab == 5)
          //{
          //  System.Windows.Point point = selectedItem.TimeTable.Utm.ToLatLon();
          //  UpdateMarkerPosition(point.Y, point.X);
          //}
        }
        //listBox.UnselectAll();
        //listBox.UpdateLayout();
      }
    }

    //---------------------------------------------------------------------
    private void FoundTimeTableTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      FoundTimeTableViewModel? stt = e.NewValue as FoundTimeTableViewModel;
      DataManager.Instance.SetFoundTimeTable(stt?.Object);
      if (stt != null)
      {
        DataManager.Instance.FoundTimeTableTitle = stt.DisplayName;
      }
    }

    //---------------------------------------------------------------------
    private void FoundTrainTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      TrainsViewModel? tvm = e.NewValue as TrainsViewModel;
      DataManager.Instance.CurrentTrain = tvm?.Object;
    }

    //---------------------------------------------------------------------
    private void TbxTrainNumber_GotFocus(object sender, RoutedEventArgs e)
    {
      tblkMessage.Visibility = Visibility.Hidden;
    }

    //---------------------------------------------------------------------
    private void TbxTrainNumber_TextChanged(object sender, EventArgs e)
    {
      tblkMessage.Visibility = Visibility.Hidden;
    }

    //---------------------------------------------------------------------
    private void VersionPanel_MouseEnter(object sender, MouseEventArgs e)
    {
      if (MainBorderVisibility == Visibility.Visible)
      {
        AboutDlg dlg = new()
        {
          Owner = this
        };
        dlg.ShowDialog();
      }
    }

    #region prerequisites

    //---------------------------------------------------------------------
    private bool ResetData(bool deleteRecentTrains)
    {
      bool res = false;

      if (deleteRecentTrains)
      {
        try
        {
          OnTimeTablePage(this, null);
          DataManager.Instance.ClearRecentTrains();
          System.IO.File.Delete(RecentTrainsCollection.FileName);
        }
        catch { }
      }

      return res;
    }

    #endregion

    private void ToggleButton_Checked(object sender, RoutedEventArgs e)
    {

    }

    private void SelectedLaNumberChanged(object sender, SelectionChangedEventArgs e)
    {
      string lanr = e.ToString();
      if (sender is ListBox listBox)
      {

        string? selectedItem = listBox.SelectedItem as string;

        SetLaNummer(selectedItem);
      }
    }

    //private void GoBackButton_Click(object sender, RoutedEventArgs e)
    //{
    //  // Assuming you have a WebView2 control named 'webView'
    //  if (webView_ZDB.CanGoBack)
    //  {
    //    webView_ZDB.GoBack();
    //    PositionOverlay();
    //  }
    //}
  }
}
