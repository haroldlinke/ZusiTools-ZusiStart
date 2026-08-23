using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using CommunityToolkit.Mvvm.Input;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.VisualBasic.Logging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Win32;
using Sovoma;
using Sovoma.ControlsKit;
using Sovoma.WPF;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;


//using System.Windows.Forms;

//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using Xceed.Wpf.Toolkit.Primitives;
using Z8Routegraph2n;
using ZusiBuchfahrplanlib;
using ZusiCLIProject.Routegraph2;
using ZusiCLIProject.Zusi3Fahrschule2;
using ZusiDisplayLib;
using ZusiFahrpultLib;
using ZusiKlassenLib2;
using ZusiKlassenLib2.Buchfahrplan;
using ZusiKlassenLib2.Cab;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Fahrplan;
using ZusiKlassenLib2.TimeTable;
using ZusiMeterGaugesLib;
using ZusiMeterGaugesLib.Gauges;
using ZusiMeterGaugesLib.Interfaces;
using ZusiStart.About;
using ZusiStart.Connection;
using ZusiStart.Controls;
using ZusiStart.CreatePictures;
using ZusiStart.Data;
using ZusiStart.Dialogs;
using ZusiStart.Miscellaneous;
using ZusiStart.ViewModels;
using static GMap.NET.Entity.OpenStreetMapGraphHopperGeocodeEntity;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using static System.Net.Mime.MediaTypeNames;

//using System.Windows.Forms;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
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
    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public DataTemplate WebViewTemplate { get; set; }
    public DataTemplate TrackingTemplate { get; set; }
    public DataTemplate RouteGraphTemplate { get; set; }
    public DataTemplate BildfahrplanTemplate { get; set; }
    public DataTemplate ImageTemplate { get; set; }
    public DataTemplate ListTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      var tab = item as TabViewModel;
      _log.Debug($"Selecting template for tab: {tab?.Title}");
      if (tab == null) return base.SelectTemplate(item, container);
      if (tab.Title == DataManager.Instance.tab_title_favorites) return ListTemplate;
      if (tab.IsIntro) return TrackingTemplate;
      if (tab.IsImageTab) return ImageTemplate;
      if (tab.Title == DataManager.Instance.tab_title_routegraph || tab.Title == DataManager.Instance.tab_title_routegraph) return RouteGraphTemplate;
      if (tab.Title == DataManager.Instance.tab_title_bildfahrplan || tab.Title == DataManager.Instance.tab_title_bildfahrplan) return BildfahrplanTemplate;
      return WebViewTemplate;
    }
  }

  public partial class MainWindow : System.Windows.Window, IDisposable
  {
    #region private fields

    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public static string _url_zusi_strecken_karte = "https://www.zusi-sk.eu/#10.5/50.96983/6/";
    public static string _url_zusi_datenbank = "http://zusidatenbank.de/?zusistart";
    private static string _zusibildfahrplan_mode = "FPL";


    //private readonly HttpMiniServer _miniServer = new();
    private DataLoaderWindow _dataLoaderWindow;
    //private WebView_Window _webViewWindow_ZDB;
    //private WebView_Window _webViewWindow_ZSK;
    //private readonly Simulator _simulator = new Simulator();
    private readonly Fahrpult _fahrpult = new();
    private int _activetab = 1;
    private GMapMarker marker;
    //private OverlayWindow overlay;
    private readonly ConcurrentQueue<EventArgs> _dataQueue = new ConcurrentQueue<EventArgs>();
    private readonly AutoResetEvent _dataWakeUp = new AutoResetEvent(false);
    private readonly bool _dataCancellation = false;

    //UTM Daten
    int _utmX = 0;
    int _utmY = 0;
    int _zone = 0;
    char _zoneField = ' ';
    string _zone2 = "";
    int _xkoordinate = 0;
    int _ykoordinate = 0;
    double _drehwinkel_z = 0;
    string _zugDatei = "";

    TabViewModel _oeril_sk_tab;

    public static CoreWebView2Environment SharedEnvironment;

    public static readonly RoutedUICommand CommandAbout = new RoutedUICommand("Über _ZusiStart", nameof(CommandAbout), typeof(MainWindow));
    public static readonly RoutedUICommand CommandHelp = new RoutedUICommand("_Dokumentation", nameof(CommandHelp), typeof(MainWindow));
    public static readonly RoutedUICommand CommandExtDocu = new RoutedUICommand("Dokumentation mit _externem Programm öffnen", nameof(CommandExtDocu), typeof(MainWindow));
    public static readonly RoutedUICommand CommandOptions = new RoutedUICommand("Programmeinstellungen", nameof(CommandOptions), typeof(MainWindow));
    public static readonly RoutedUICommand CommandQuit = new RoutedUICommand("Beenden", nameof(CommandQuit), typeof(MainWindow));
    public static readonly RoutedUICommand CommandCreatePictures = new RoutedUICommand("Erzeuge alle Fahrzeugbilder neu", nameof(CommandCreatePictures), typeof(MainWindow));
    public static readonly RoutedUICommand UndoReplLocoCommand = new("_Rückgängig Loktausch", "UndoReplLocoCommand", typeof(MainWindow),
        new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.Z, ModifierKeys.Control) }));
    public static readonly RoutedUICommand ReplaceLocoCommand = new("_Lok tauschen", "ReplaceLocoCommand", typeof(MainWindow),
      new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.F5) }));
    public static readonly RoutedUICommand ManageReplLocosCommand = new("_Austauschloks verwalten...", "ManageReplLocosCommand", typeof(MainWindow),
      new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.F6) }));
    public static readonly RoutedUICommand SaveRevisedTrainCommand = new RoutedUICommand("_Zugänderung permanent speichern", "SaveRevisedTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand UndoRevisedTrainCommand = new RoutedUICommand("_permanente Zugänderung rückgängig machen (nach Neustart)", "UndoRevisedTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand CommandSaveLayout_A = new RoutedUICommand("Layout A speichern", nameof(CommandSaveLayout_A), typeof(MainWindow));
    public static readonly RoutedUICommand CommandSaveLayout_B = new RoutedUICommand("Layout B speichern", nameof(CommandSaveLayout_B), typeof(MainWindow));
    public static readonly RoutedUICommand CommandLoadLayout_A = new RoutedUICommand("Layout A laden", nameof(CommandLoadLayout_A), typeof(MainWindow));
    public static readonly RoutedUICommand CommandLoadLayout_B = new RoutedUICommand("Layout B laden", nameof(CommandLoadLayout_B), typeof(MainWindow));
    public static readonly RoutedUICommand CommandLoadLayout_Standard_A = new RoutedUICommand("Standard Layout A laden", nameof(CommandLoadLayout_Standard_A), typeof(MainWindow));
    public static readonly RoutedUICommand CommandLoadLayout_Standard_B = new RoutedUICommand("Standard Layout B laden", nameof(CommandLoadLayout_Standard_B), typeof(MainWindow));
    public static readonly RoutedUICommand CommandLayout_Showall = new RoutedUICommand("Alle Fenster öffnen", nameof(CommandLayout_Showall), typeof(MainWindow));

    //public ICommand TTcopyPathCommand { get; }
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

    //---------------------------------------------------------------------
    public static readonly DependencyProperty EditCommentButtonTextProperty =
    DependencyProperty.Register(
      nameof(EditCommentButtonText),
      typeof(string),
      typeof(MainWindow),
      new PropertyMetadata("Kommentar bearbeiten"));

    public string EditCommentButtonText
    {
      get => (string)GetValue(EditCommentButtonTextProperty);
      set => SetValue(EditCommentButtonTextProperty, value);
    }

    #endregion

    #region commands

    public static readonly RoutedUICommand MinimizeCommand = new("", "MinimizeCommand", typeof(MainWindow));
    public static readonly RoutedUICommand ExitGameCommand = new("ZusiStart beenden", "ExitGameCommand", typeof(MainWindow));
    public static readonly RoutedUICommand StartTrainCommand = new("Ausgewählten Zug fahren", "StartTrainCommand", typeof(MainWindow), new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.F5, ModifierKeys.Alt) }));
    public static readonly RoutedUICommand StartFavTrainCommand = new("Ausgewählten Zug fahren", "StartFavTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand TimeTablesPageCommand = new("Fahrplanauswahl", "TimeTablesPageCommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchPageCommand = new("Zug suchen", "SearchPageCommand", typeof(MainWindow));
    public static readonly RoutedUICommand FavoriteTrainsPageCommand = new("Zug Favoriten", "FavoriteTrainsPageCommand", typeof(MainWindow));
    public static readonly RoutedUICommand AddFavoriteTrainCommand = new("Zug hinzufügen", "AddFavoriteTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand EditFavoriteCommentCommand = new("Kommentar bearbeiten", "EditFavoriteCommentCommand", typeof(MainWindow));
    public static readonly RoutedUICommand DelFavoriteTrainCommand = new("Zug löschen", "DelFavoriteTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchTrainCommand = new("Zug suchen", "SearchTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand AnalyseKICommand = new("KI analysieren", "AnalyseKICommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchKICommand = new("Mit KI suchen", "SearchKICommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchBRCommand = new("Baureihe suchen", "SearchBRCommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchTrainZusiDBCommand = new("ZUSI Datenbank", "SearchTrainZusiDBCommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchTrainZusiSKCommand = new("ZUSI Streckenkarte", "SearchTrainZusiSKCommand", typeof(MainWindow));
    public static readonly RoutedUICommand ZSK_PositionOverlayCommand = new("Fadenkreuz Positionieren", "ZSK_PositionOverlayCommand", typeof(MainWindow));
    public static readonly RoutedUICommand ZSK_FadenkreuzActiveCommand = new("Fadenkreuz anzeigen", "ZSK_FadenkreuzActiveCommand", typeof(MainWindow));
    public static readonly RoutedUICommand ZSK_FindStationCommand = new("Fadenkreuz Positionieren", "ZSK_FindStationCommand", typeof(MainWindow));
    public static readonly RoutedUICommand TrackTrainCommand = new("Zug tracken", "TrackTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand ZusiBildFahrplanCommand = new("ZUSI Bildfahrplan", "ZusiBildFahrplanCommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchBuchfahrplanCommand = new("Buchfahrplan", "SearchBuchfahrplanCommand", typeof(MainWindow));
    public static readonly RoutedUICommand CreateVehicleListCommand = new("Zug suchen", "CreateVehicleListCommand", typeof(MainWindow));
    public static readonly RoutedUICommand FilterRefreshCommand = new("Züge filtern", "FilterRefreshCommand", typeof(MainWindow));
    public static readonly RoutedUICommand FilterFplRefreshCommand = new("Fahrpläne suchen", "FilterRefreshCommand", typeof(MainWindow));
    public static readonly RoutedUICommand FilterResetCommand = new("Zurücksetzen", "FilterResetCommand", typeof(MainWindow));
    public static readonly RoutedUICommand TrainStartSettingsCommand = new("Start-Optionen", "TrainStartSettingsCommand", typeof(MainWindow));
    public static readonly RoutedUICommand ResetViewCommand = new("Ansicht zurücksetzen", "ResetViewCommand", typeof(MainWindow));
    public static readonly RoutedUICommand ReloadCommand = new("Fahrpläne neu laden", "ReloadCommand", typeof(MainWindow));

    public static readonly RoutedUICommand FrictionSettingsCommand = new("Gleisbedingungen", "FrictionSettingsCommand", typeof(MainWindow));
    public static readonly RoutedUICommand StartFahrschuleCommand = new("Fahrschule", "StartFahrschuleCommand", typeof(MainWindow));
    //--
    public static readonly RoutedUICommand ResetDataCommand = new("ResetData", "ResetDataCommand", typeof(MainWindow), new InputGestureCollection(new KeyGesture[] { new KeyGesture(Key.F12) }));
    public static readonly RoutedUICommand UndoReplTrainCommand = new("_Rückgängig Zugtausch", "UndoReplTrainCommand", typeof(MainWindow),
         new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.F8, ModifierKeys.Alt) }));
    public static readonly RoutedUICommand ReplaceTrainCommand = new("_Zug tauschen", "ReplaceTrainCommand", typeof(MainWindow),
      new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.F7, ModifierKeys.Alt) }));
    public static readonly RoutedUICommand ManageReplTrainsCommand = new("_Austauschzüge auswählen/verwalten", "ManageReplTrainsCommand", typeof(MainWindow),
      new InputGestureCollection(new InputGesture[] { new KeyGesture(Key.F6, ModifierKeys.Alt) }));

    #endregion

    private void UpdateCommandTexts()
    {
      _log.Debug("Updating command texts for localization");
      AddFavoriteTrainCommand.Text = LocalizationManager.Translate("Zug hinzufügen");

      DelFavoriteTrainCommand.Text = LocalizationManager.Translate("Zug löschen");
      EditFavoriteCommentCommand.Text = LocalizationManager.Translate("Kommentar bearbeiten");
      ExitGameCommand.Text = LocalizationManager.Translate("ZusiStart beenden");
      if (DataManager.Instance.options != null)
      {

        if (DataManager.Instance.options.RemoteZusi)
        {
          StartTrainCommand.Text = LocalizationManager.Translate("Remote Zug monitoren");
          BtnStartTrain.Content = StartTrainCommand.Text;
        }
        else
        {
          StartTrainCommand.Text = LocalizationManager.Translate("Ausgewählten Zug fahren");
          BtnStartTrain.Content = StartTrainCommand.Text;
        }
      }
      StartFavTrainCommand.Text = LocalizationManager.Translate("Ausgewählten Zug fahren");
      FilterRefreshCommand.Text = LocalizationManager.Translate("Züge filtern");
      //BtnFilterRefresh.Content = FilterRefreshCommand.Text; 
      //FilterFplRefreshCommand.Text = LocalizationManager.Translate("Fahrpläne suchen");
      //BtnFilterFplRefresh.Content = FilterFplRefreshCommand.Text;
      FilterResetCommand.Text = LocalizationManager.Translate("Zurücksetzen");
      //BtnFilterReset.Content = FilterResetCommand.Text;
      TrainStartSettingsCommand.Text = LocalizationManager.Translate("Start-Optionen");
      ResetViewCommand.Text = LocalizationManager.Translate("Ansicht zurücksetzen");
      ReloadCommand.Text = LocalizationManager.Translate("Fahrpläne neu laden");
      BtnTrainStartSettings.Content = TrainStartSettingsCommand.Text;
      FrictionSettingsCommand.Text = LocalizationManager.Translate("Gleisbedingungen");
      frictionSettingsButton.Content = FrictionSettingsCommand.Text;
      StartFahrschuleCommand.Text = LocalizationManager.Translate("Fahrschule");
      BtnStartFahrschule.Content = StartFahrschuleCommand.Text;

      UndoReplTrainCommand.Text = LocalizationManager.Translate("_Rückgängig Zugtausch");
      ReplaceTrainCommand.Text = LocalizationManager.Translate("_Zug tauschen");
      ManageReplTrainsCommand.Text = LocalizationManager.Translate("_Austauschzüge auswählen/verwalten");
      CommandOptions.Text = LocalizationManager.Translate("Programmeinstellungen");
      CommandQuit.Text = LocalizationManager.Translate("Beenden");
      CommandCreatePictures.Text = LocalizationManager.Translate("Erzeuge alle Fahrzeugbilder neu");
      MenuItemOptions.Header = CommandOptions.Text;
      MenuItemQuit.Header = CommandQuit.Text;
      //MenuItemCreatePictures.Header = CommandCreatePictures.Text;
      TrainStartSettings.Header = TrainStartSettingsCommand.Text;
      CommandSaveLayout_A.Text = LocalizationManager.Translate("Layout A speichern");
      CommandSaveLayout_B.Text = LocalizationManager.Translate("Layout B speichern");
      CommandLoadLayout_A.Text = LocalizationManager.Translate("Layout A laden");
      CommandLoadLayout_B.Text = LocalizationManager.Translate("Layout B laden");
      CommandLoadLayout_Standard_A.Text = LocalizationManager.Translate("Standard Layout A laden");
      CommandLoadLayout_Standard_B.Text = LocalizationManager.Translate("Standard Layout B laden");

      EditCommentButtonText = LocalizationManager.Translate("Kommentar bearbeiten");


      //ZSK_PositionOverlayCommand = new("Fadenkreuz Positionieren", "ZSK_PositionOverlayCommand", typeof(MainWindow));
      //ZSK_FadenkreuzActiveCommand = new("Fadenkreuz anzeigen", "ZSK_FadenkreuzActiveCommand", typeof(MainWindow));
      //ZSK_FindStationCommand = new("Fadenkreuz Positionieren", "ZSK_FindStationCommand", typeof(MainWindow));
      //TrackTrainCommand = new("Zug tracken", "TrackTrainCommand", typeof(MainWindow));
      //ZusiBildFahrplanCommand = new("ZUSI Bildfahrplan", "ZusiBildFahrplanCommand", typeof(MainWindow));
      //SearchBuchfahrplanCommand = new("Buchfahrplan", "SearchBuchfahrplanCommand", typeof(MainWindow));
      //CreateVehicleListCommand = new("Zug suchen", "CreateVehicleListCommand", typeof(MainWindow));
    }


    private async void InitializeWebViewEnvironment()
    {
      _log.Debug("Initializing WebView2 environment");
      try
      {
        string userDataFolder = System.IO.Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
          "ZusiStart", "WebView2");

        SharedEnvironment = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
      }
      catch (Exception ex)
      {
        _log.Error("Fehler bei der Initialisierung der WebView2-Umgebung: " + ex.Message);
        MessageBox.Show(LocalizationManager.Translate("WebView2 konnte nicht initialisiert werden."), LocalizationManager.Translate("Fehler"), MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      _log.Debug($"Hyperlink clicked: {e.Uri.AbsoluteUri}");
      try
      {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Unable to open link: {ex.Message}");
      }
      e.Handled = true;
    }

    private async void TabControl_SelectionChanged2(object sender, SelectionChangedEventArgs e)
    {
      _log.Debug("Tab selection changed");
      foreach (var item in e.AddedItems)
      {
        var tabItem = (sender as TabControl)?.ItemContainerGenerator.ContainerFromItem(item) as TabItem;

        if (tabItem != null)
        {

          TabViewModel tvm = tabItem.Header as TabViewModel;
          _log.Debug($"Selected tab: {tvm?.Title}");
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
            _log.Debug("Setting Streckenkarte to fullscreen mode");
            tvm.WebViewInstance?.CoreWebView2?.ExecuteScriptAsync("if (!document.fullscreenElement){[...document.querySelectorAll('button, div')].find(el => el.title?.includes('Vollbild'))?.click();}");

            //            string script = @"
            //  const map = window.c4gMaps[498];
            //  const center = map.getCenter();

            //  const style = document.createElement('style');
            //  style.innerHTML = `
            //    .custom-cross-icon .cross {
            //      font-size: 20px;
            //      color: red;
            //      font-weight: bold;
            //      line-height: 20px;
            //      text-align: center;
            //    }
            //  `;
            //  document.head.appendChild(style);

            //  const crossIcon = L.divIcon({
            //    className: 'custom-cross-icon',
            //    html: '<div class=""cross"">+</div>',
            //    iconSize: [20, 20],
            //    iconAnchor: [10, 10]
            //  });
            //  const marker = L.marker(center, { icon: crossIcon }).addTo(map);
            //";

            string script = @"
  const map = window.c4gMaps[498];
  const center = map.getCenter();

  const crossIcon = L.divIcon({
    className: 'icon',
    html: '<div style=""color:red;font-size:30px;font-weight:bold;text-align:center;"">+</div>',
    iconSize: [30, 30],
    iconAnchor: [10, 10]
  });

          const marker = L.marker(center, { icon: crossIcon }).addTo(map);
          ";

            await System.Threading.Tasks.Task.Delay(1500);
            tvm.WebViewInstance?.CoreWebView2.ExecuteScriptAsync(script);

          }
        }

      }
    }

    //private void LanguageSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //{
    //  if (LanguageSelector.SelectedItem is ComboBoxItem item && item.Tag is string lang)
    //  {
    //    DataManager.CurrentLanguage = lang;
    //    // UI neu laden/aktualisieren:
    //    // 1. DataContext neu setzen (triggert Bindings)
    //    DataContext = null;
    //    DataContext = Data.DataManager.Instance;
    //    // 2. Optional: Einzelne Controls manuell aktualisieren, falls nötig
    //    UpdateCommandTexts();
    //  }
    //}

    //---------------------------------------------------------------------
    public MainWindow()
    {
      //FeatureManager.initFeatures([FeatureManager.Features.Tracking, FeatureManager.Features.StartLocation]);
      _log.Debug("Initializing MainWindow");
      LocalizationManager.Load();
      string lang = Properties.Settings.Default.Language;
      if (!string.IsNullOrEmpty(lang) && !lang.Contains("auto"))
      {
        DataManager.CurrentLanguage = lang;
      }
      else
      {
        // Automatische Erkennung, z. B. anhand CultureInfo
        DataManager.CurrentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        DataManager.CurrentLanguage = DataManager.CurrentLanguage switch
        {
          "de" => "de",
          "en" => "en",
          "fr" => "fr",
          _ => "en",
        };
      }
      InitializeComponent();
      InitializeWebViewEnvironment();
      UpdateCommandTexts();
      // Bildschirmgröße ermitteln


      //InitializeMap();

      _log.Debug("ZusiStart started - Version:" + AsmInfo.Version.ToString());

      EngageScaling();

      LoadWindowSettings();

      Background = System.Windows.Application.Current.TryFindResource(string.Format("bkgnd{0}", DateTime.Now.Second & 3)) as System.Windows.Media.Brush;

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
      CommandBindings.Add(new CommandBinding(SearchKICommand, OnSearchKI, OnCanSearchKI));
      CommandBindings.Add(new CommandBinding(AnalyseKICommand, OnAnalyseKI, OnCanAnalyseKI));
      CommandBindings.Add(new CommandBinding(SearchBRCommand, OnSearchBR, OnCanSearchBR));
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
      CommandBindings.Add(new CommandBinding(ResetViewCommand, OnResetView));
      CommandBindings.Add(new CommandBinding(ReloadCommand, OnReload));
      CommandBindings.Add(new CommandBinding(FrictionSettingsCommand, OnFrictionSettings, OnCanFrictionSettings));
      CommandBindings.Add(new CommandBinding(StartFahrschuleCommand, OnStartFahrschule, OnCanStartFahrschule));

      CommandBindings.Add(new CommandBinding(CommandAbout, OnAbout));
      CommandBindings.Add(new CommandBinding(CommandHelp, OnHelp));
      CommandBindings.Add(new CommandBinding(CommandExtDocu, OnExtDocu));
      CommandBindings.Add(new CommandBinding(CommandOptions, OnOptions));
      CommandBindings.Add(new CommandBinding(CommandQuit, OnQuit));

      CommandBindings.Add(new CommandBinding(CommandCreatePictures, OnCreatePictures));

      CommandBindings.Add(new CommandBinding(FilterRefreshCommand, OnFilterRefresh, OnCanFilterRefresh));
      CommandBindings.Add(new CommandBinding(FilterFplRefreshCommand, OnFplFilterRefresh, OnCanFplFilterRefresh));

      CommandBindings.Add(new CommandBinding(FilterResetCommand, OnFilterReset, OnCanFilterReset));


      CommandBindings.Add(new CommandBinding(ManageReplLocosCommand, OnManageReplLocos, OnCanManageReplLocos));
      CommandBindings.Add(new CommandBinding(ReplaceLocoCommand, OnReplaceLoco, OnCanReplaceLoco));
      CommandBindings.Add(new CommandBinding(UndoReplLocoCommand, OnUndoReplLoco, OnCanUndoReplLoco));
      //--
      CommandBindings.Add(new CommandBinding(ResetDataCommand, OnResetData, (s, e) => e.CanExecute = IsLoaded));
      CommandBindings.Add(new CommandBinding(ManageReplTrainsCommand, OnManageReplTrains, OnCanManageReplTrains));
      CommandBindings.Add(new CommandBinding(ManageReplTrainsCommand, OnManageReplTrains, OnCanManageReplTrains));
      CommandBindings.Add(new CommandBinding(SaveRevisedTrainCommand, OnSaveRevisedTrain, OnCanSaveRevisedTrain));
      CommandBindings.Add(new CommandBinding(UndoRevisedTrainCommand, OnUndoRevisedTrain, OnCanUndoRevisedTrain));

      CommandBindings.Add(new CommandBinding(ReplaceTrainCommand, OnReplaceTrain, OnCanReplaceTrain));
      CommandBindings.Add(new CommandBinding(UndoReplTrainCommand, OnUndoReplTrain, OnCanUndoRepTrain));

      CommandBindings.Add(new CommandBinding(CommandSaveLayout_A, OnSaveLayout_A));
      CommandBindings.Add(new CommandBinding(CommandSaveLayout_B, OnSaveLayout_B));
      CommandBindings.Add(new CommandBinding(CommandLoadLayout_A, OnLoadLayout_A));
      CommandBindings.Add(new CommandBinding(CommandLoadLayout_B, OnLoadLayout_B));
      CommandBindings.Add(new CommandBinding(CommandLoadLayout_Standard_A, OnLoadLayout_Standard_A));
      CommandBindings.Add(new CommandBinding(CommandLoadLayout_Standard_B, OnLoadLayout_Standard_B));
      CommandBindings.Add(new CommandBinding(CommandLayout_Showall, OnLayout_Showall));



      // *** add keyboard bindings

      //var focusListTimeTableGrps = new RoutedCommand();
      //CommandBindings.Add(new CommandBinding(focusListTimeTableGrps, (s, e) => ListTimeTableGrp.Focus()));
      //InputBindings.Add(new KeyBinding(focusListTimeTableGrps, Key.F1, ModifierKeys.Shift));

      //var focusListTimeTables = new RoutedCommand();
      //CommandBindings.Add(new CommandBinding(focusListTimeTables, (s, e) => ListTimeTables.Focus()));
      //InputBindings.Add(new KeyBinding(focusListTimeTables, Key.F2, ModifierKeys.Shift));

      //// *** add keyboard bindings

      //var FocusListtvPassengerTrains = new RoutedCommand();
      //CommandBindings.Add(new CommandBinding(FocusListtvPassengerTrains, (s, e) => tvPassengerTrains.Focus()));
      //InputBindings.Add(new KeyBinding(FocusListtvPassengerTrains, Key.F3, ModifierKeys.Shift));

      //var focusListFilter = new RoutedCommand();
      //CommandBindings.Add(new CommandBinding(focusListFilter, (s, e) => BrdFilter.Focus()));
      //InputBindings.Add(new KeyBinding(focusListFilter, Key.F4, ModifierKeys.Shift));


      //_miniServer.Run();
      UpdateCommandTexts();
      //DataContext = null;
      DataContext = DataManager.Instance;

      DataManager? dataManager = DataContext as DataManager;
      dataManager.DecoTrainsAllowed += DataManager_DecoTrainsAllowed;
      dataManager.SelectedTimeTableChanged += DataManager_SelectedTimeTableChanged;
      dataManager.NotifyDataLoadStarted += DataManager_NotifyDataLoadStarted;
      dataManager.NotifyDataLoadCompleted += DataManager_NotifyDataLoadCompleted;

      dataManager.main_window = this;

      //DataManager.Instance.predetectDpi = VisualTreeHelper.GetDpi(this);
      //DataManager.Instance.rg_trackingItem = new TrackingItem("Zug", DataManager.Instance.predetectDpi);

      //dataManager.ProgressChanged += DataManager_ProgressChanged;
      //LoadLocalHtml();
      //TTcopyPathCommand = new RelayCommand<TimeTableRelation>(TTcopyPath);
    }

    //private void TTcopyPath(TimeTableRelation rel)
    //{
    //  string filename = rel.ToString();
    //}

    private void InitializeMap()
    {
      _log.Debug("Initializing GMap control");
      TabViewModel? zdbTab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_tracking);
      if (zdbTab != null)
      {
        // Identify your app to OSM
        GMapProvider.UserAgent = "ZusiStart (contact: support@zusi-tools.hlinke.de)";
        zdbTab.gmap.MapProvider = GMap.NET.MapProviders.GMapProviders.OpenStreetMap;
        zdbTab.gmap.Position = new PointLatLng(50.0, 8.0); // Startposition
                                                           //GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
                                                           //zdbTab.gmap.Position = new PointLatLng(49.75, 6.64);
        zdbTab.gmap.Zoom = 12;
        zdbTab.gmap.MinZoom = 5;
        zdbTab.gmap.MaxZoom = 18;
        zdbTab.gmap.Zoom = 12;
        zdbTab.gmap.ShowCenter = false;
        zdbTab.gmap.MouseWheelZoomEnabled = true;
        zdbTab.gmap.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
        zdbTab.gmap.IgnoreMarkerOnMouseWheel = true;

        GMaps.Instance.Mode = AccessMode.ServerAndCache;


        // Marker direkt zur Markers-Sammlung hinzufügen
        //marker = new GMapMarker(new PointLatLng(50.0, 8.0))
        //{
        //  Shape = new Ellipse
        //  {
        //    Width = 24,
        //    Height = 24,
        //    Stroke = System.Windows.Media.Brushes.Red,
        //    StrokeThickness = 10
        //  }
        //};
        marker = new GMapMarker(new PointLatLng(50.0, 8.0))
        {
          Shape = new System.Windows.Shapes.Path
          {
            Stroke = System.Windows.Media.Brushes.Red,
            StrokeThickness = 3,
            Opacity = 0.75,
            Data = new GeometryGroup
            {
              Children = new GeometryCollection
        {
            // Horizontale Linie
            new LineGeometry(new System.Windows.Point(-20, 0), new System.Windows.Point(20, 0)),
            // Vertikale Linie
            new LineGeometry(new System.Windows.Point(0, -20), new System.Windows.Point(0, 20)),
            // Kreis um das Zentrum (Radius = 12)
            new EllipseGeometry(new System.Windows.Point(0, 0), 12, 12)
        }
            }
          }
        };




        zdbTab.gmap.Markers.Add(marker);

        // Assuming vm is your TrackingViewModel and RouteVisual already set **Test** 
        //zdbTab.gmap.OnMapZoomChanged += () => zdbTab.UpdateRouteTransform(zdbTab.gmap, DataManager.Instance.utmBounds, zone: 32, northHemisphere: true);
        //zdbTab.gmap.OnMapDrag += () => zdbTab.UpdateRouteTransform(zdbTab.gmap, DataManager.Instance.utmBounds, zone: 32, northHemisphere: true);
      }
    }

    //private void InitializeMap2()
    //{
    //  _log.Debug("Initializing GMap control");

    //  // Identify your app to OSM
    //  GMapProvider.UserAgent = "ZusiStart (contact: support@zusi-tools.hlinke.de)";
    //  DataManager.Instance.gmap.MapProvider = GMap.NET.MapProviders.GMapProviders.OpenStreetMap;
    //  DataManager.Instance.gmap.Position = new PointLatLng(50.0, 8.0); // Startposition
    //                                                                   //GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
    //                                                                   //zdbTab.gmap.Position = new PointLatLng(49.75, 6.64);
    //  DataManager.Instance.gmap.Zoom = 12;
    //  DataManager.Instance.gmap.MinZoom = 5;
    //  DataManager.Instance.gmap.MaxZoom = 18;
    //  DataManager.Instance.gmap.Zoom = 12;
    //  DataManager.Instance.gmap.ShowCenter = false;
    //  DataManager.Instance.gmap.MouseWheelZoomEnabled = true;
    //  DataManager.Instance.gmap.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
    //  DataManager.Instance.gmap.IgnoreMarkerOnMouseWheel = true;

    //  GMaps.Instance.Mode = AccessMode.ServerAndCache;


    //  // Marker direkt zur Markers-Sammlung hinzufügen
    //  //marker = new GMapMarker(new PointLatLng(50.0, 8.0))
    //  //{
    //  //  Shape = new Ellipse
    //  //  {
    //  //    Width = 24,
    //  //    Height = 24,
    //  //    Stroke = System.Windows.Media.Brushes.Red,
    //  //    StrokeThickness = 10
    //  //  }
    //  //};
    //  marker = new GMapMarker(new PointLatLng(50.0, 8.0))
    //  {
    //    Shape = new System.Windows.Shapes.Path
    //    {
    //      Stroke = System.Windows.Media.Brushes.Red,
    //      StrokeThickness = 3,
    //      Opacity = 0.75,
    //      Data = new GeometryGroup
    //      {
    //        Children = new GeometryCollection
    //    {
    //        // Horizontale Linie
    //        new LineGeometry(new System.Windows.Point(-20, 0), new System.Windows.Point(20, 0)),
    //        // Vertikale Linie
    //        new LineGeometry(new System.Windows.Point(0, -20), new System.Windows.Point(0, 20)),
    //        // Kreis um das Zentrum (Radius = 12)
    //        new EllipseGeometry(new System.Windows.Point(0, 0), 12, 12)
    //    }
    //      }
    //    }
    //  };

    //  DataManager.Instance.gmap.Markers.Add(marker);

    //  // Assuming vm is your TrackingViewModel and RouteVisual already set **Test** 
    //  //zdbTab.gmap.OnMapZoomChanged += () => zdbTab.UpdateRouteTransform(zdbTab.gmap, DataManager.Instance.utmBounds, zone: 32, northHemisphere: true);
    //  //zdbTab.gmap.OnMapDrag += () => zdbTab.UpdateRouteTransform(zdbTab.gmap, DataManager.Instance.utmBounds, zone: 32, northHemisphere: true);
    
    //}

    //private void loadgmapoverlay()
    //{
    //  return; ; // temporär deaktiviert
    //            //TabViewModel? zdbTab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_tracking);
    //            //var loader = new RailwayPolylineOverlayWpf(zdbTab.gmap);
    //            //_ = loader.LoadRailwayOverlayAsync(49.73, 6.61, 49.77, 6.67); // Trier region
    //}

    //private void Gmap_Loaded(object sender, RoutedEventArgs e)
    //{
    //  _log.Debug("GMap control loaded");
    //  InitializeMap2();
    //}

    //---------------------------------------------------------------------
    public void Dispose()
    {
      _log.Debug("Disposing MainWindow");
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    //---------------------------------------------------------------------
    private void Dispose(bool disposing)
    {
      _log.Debug("Disposing MainWindow (disposing=" + disposing + ")");
      if (disposing)
      {
        //_miniServer?.Dispose();
        //ZusiPictureManager.Destroy();
        DataManager.Instance.Dispose();
        _fahrpult?.Dispose();
      }
    }

    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      _log.Debug("MainWindow closing");

      //if (_webViewWindow_ZDB != null)
      //{
      //  _webViewWindow_ZDB.Close();
      //}
      //if (_webViewWindow_ZSK != null)
      //{
      //  _webViewWindow_ZSK.Close();
      //}
      //DataManager.Instance.SaveDockLayout("A");
    }


    //private void LoadDockLayout()
    //{
    //  var file = DataManager.Instance.AvalonDockLayoutFilePath;

    //  if (System.IO.File.Exists(file))
    //  {
    //    var serializer = new XmlLayoutSerializer(DockManager);
    //    serializer.Deserialize(file);
    //  }
    //}

    //private void SaveDockLayout()
    //{
    //  var serializer = new XmlLayoutSerializer(DockManager);
    //  serializer.Serialize(DataManager.Instance.AvalonDockLayoutFilePath);
    //}

    //---------------------------------------------------------------------
    protected override void OnClosing(CancelEventArgs e)
    {
      _log.Debug("MainWindow OnClosing");
      SaveWindowSettings();
      DataManager.Instance.SaveGeneralOptions();
      base.OnClosing(e);
      Dispose(true);
    }

    private void LoadWindowSettings()
    {
      _log.Debug("Loading window settings");
      this.Top = Properties.Settings.Default.WindowTop;
      this.Left = Properties.Settings.Default.WindowLeft;
      this.Height = Properties.Settings.Default.WindowHeight;
      this.Width = Properties.Settings.Default.WindowWidth;
      this.WindowState = Properties.Settings.Default.WindowState;
      if (Properties.Settings.Default.WidthColumnRight.Value > 0)
        RightColumn.Width = Properties.Settings.Default.WidthColumnRight;
      if (Properties.Settings.Default.WidthColumnLeft.Value > 0)
        LeftColumn.Width = Properties.Settings.Default.WidthColumnLeft;
      if (Properties.Settings.Default.HeightRowBottom.Value > 0)
        BottomRow.Height = Properties.Settings.Default.HeightRowBottom;
      //if (Properties.Settings.Default.HeightRowTop.Value > 0)
      //  TopRow.Height = Properties.Settings.Default.HeightRowTop;
      AdjustWindowPosition();
    }

    private void SaveWindowSettings()
    {
      _log.Debug("Saving window settings");
      Properties.Settings.Default.WindowTop = this.Top;
      Properties.Settings.Default.WindowLeft = this.Left;
      Properties.Settings.Default.WindowHeight = this.Height;
      Properties.Settings.Default.WindowWidth = this.Width;
      Properties.Settings.Default.WindowState = this.WindowState;
      Properties.Settings.Default.WidthColumnRight = RightColumn.Width;
      Properties.Settings.Default.WidthColumnLeft = LeftColumn.Width;
      Properties.Settings.Default.HeightRowBottom = BottomRow.Height;
      //Properties.Settings.Default.HeightRowTop = TopRow.Height;
      Properties.Settings.Default.Save();
    }

    private void AdjustWindowPosition()
    {
      _log.Debug("Adjusting window position to ensure it's within screen bounds");
      if (this.Top < 0) this.Top = 0;
      //if (this.Left < 0) this.Left = 0;
      if (this.Top + this.Height > SystemParameters.VirtualScreenHeight)
        this.Top = SystemParameters.VirtualScreenHeight - this.Height;
      if (this.Left + this.Width > SystemParameters.VirtualScreenWidth)
        this.Left = SystemParameters.VirtualScreenWidth - this.Width;
    }

    public double GetScreenScaleFactor(System.Windows.Window window)
    {
      _log.Debug("Getting screen scale factor for DPI scaling");
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
      _log.Debug("Opening About dialog");
      AboutDlg aboutDlg = new AboutDlg();
      aboutDlg.Owner = (System.Windows.Window)this;
      aboutDlg.ShowDialog();
    }

    private void OnQuit(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Quit command executed - closing MainWindow");
      window.Close();
    }

    //---------------------------------------------------------------------
    private void OnCanManageReplLocos(object sender, CanExecuteRoutedEventArgs e)
    {
      _log.Debug("Checking if Manage Replacement Locos command can execute");
      TrainItem ti = DataManager.Instance.CurrentTrainItem;
      e.CanExecute = ti == null || !ti.IsLocoReplaced;
    }

    //---------------------------------------------------------------------
    private void OnManageReplLocos(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Manage Replacement Locos command");
      DataManager dm = DataManager.Instance;
      dm.SelectedLoco = null;
      dm.SelectedReplacementLoco = null;
      ManageReplLocosDialog dlg = new()
      {
        Owner = this
      };
      if (dlg.ShowDialog() == true)
      {
        dm.SaveReplacementLocos();
      }
    }

    //---------------------------------------------------------------------
    private void OnCanReplaceLoco(object sender, CanExecuteRoutedEventArgs e)
    {
      _log.Debug("Checking if Replace Loco command can execute");
      TrainItem ti = DataManager.Instance.CurrentTrainItem;
      //e.CanExecute = ti != null && !ti.IsLocoReplaced;
      DataManager dm = DataManager.Instance;
      e.CanExecute = DataManager.Instance.CurrentTrain != null && (dm.SelectedTrainR == null || !dm.SelectedTrainR.IsDirty) && !(ti != null && (ti.IsTrainReplaced || ti.IsTrainReplaced));
    }

    //---------------------------------------------------------------------
    private void OnReplaceLoco(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Replace Loco command");
      //DataManager.Instance.CurrentTrainItem.ReplaceLoco(this);
      if (DataManager.Instance.CurrentTrainItem != null)
      {
        DataManager.Instance.CurrentTrainItem.ReplaceLoco(this);
      }
      else
      {
        DataManager.Instance.CurrentTrainItem = new TrainItem(DataManager.Instance.CurrentTrain);
        DataManager.Instance.CurrentTrainItem.ReplaceLoco(this);
      }
      DataManager.Instance.CurrentTrainItem.OrigZug = DataManager.Instance.CurrentTrain;
      Zug replacetrain = new(DataManager.Instance.CurrentTrain.Parent, DataManager.Instance.CurrentTrain);
      replacetrain.ReplaceTrain(DataManager.Instance.CurrentTrainItem.Reihung);
      DataManager.Instance.CurrentTrain = replacetrain;
    }

    //---------------------------------------------------------------------
    private void OnCanUndoReplLoco(object sender, CanExecuteRoutedEventArgs e)
    {
      _log.Debug("Checking if Undo Replace Loco command can execute");
      TrainItem ti = DataManager.Instance.CurrentTrainItem;
      e.CanExecute = ti != null && ti.IsLocoReplaced;
    }

    //---------------------------------------------------------------------
    private void OnUndoReplLoco(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Undo Replace Loco command");
      DataManager.Instance.CurrentTrainItem.UndoReplaceLoco();
    }

    private void OnCreatePictures(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Create Pictures command");
      CreatePicturesDlg CreatePicturesDlg = new CreatePicturesDlg();
      CreatePicturesDlg.Owner = (System.Windows.Window)this;
      CreatePicturesDlg.ShowDialog();
    }

    private void OnHelp(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Help command - opening Help dialog");
      HelpDlg helpDlg = new HelpDlg();
      helpDlg.ShowDialog();
    }

    private void OnExtDocu(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing External Documentation command - opening PDF");
      try
      {
        string relativePath = "help/ZusiStart_Docu_Deutsch.pdf";
        string absolutePath = System.IO.Path.GetFullPath(relativePath);
        Process.Start("cmd.exe", $"/c start \"\" \"{absolutePath}\"");
        //Process.Start(absolutePath);
      }
      catch (Exception ex)
      {
        int num = (int)System.Windows.MessageBox.Show(ex.ToString(), LocalizationManager.Translate("Fehler beim Öffnen der Dokumentation"), MessageBoxButton.OK);

      }
    }

    private void OnOptions(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Options command - opening Options dialog");
      try
      {
        //OptionsDlg optionsDlg = new OptionsDlg();
        DataManager.Instance.optionsDlg_window.Owner = (System.Windows.Window)this;
        DataManager.Instance.optionsDlg_window.ShowDialog();
      }
      catch (Exception ex)
      {
        DataManager.Instance.optionsDlg_window = new OptionsDlg();
        DataManager.Instance.optionsDlg_window.Owner = (System.Windows.Window)this;
        DataManager.Instance.optionsDlg_window.ShowDialog();
      }

    }

    private void OnSaveLayout_A(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Save Layout A command");
      DataManager.Instance.SaveDockLayout("A");
    }

    private void OnSaveLayout_B(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Save Layout B command");
      DataManager.Instance.SaveDockLayout("B");
    }

    private void OnLoadLayout_A(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Load Layout A command");
      DataManager.Instance.LoadDockLayout("A");
    }

    private void OnLoadLayout_B(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Load Layout B command");
      DataManager.Instance.LoadDockLayout("B");
    }

    private void OnLoadLayout_Standard_A(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Load Layout A command");
      DataManager.Instance.LoadDockStandardLayout("Std_A");
    }

    private void OnLoadLayout_Standard_B(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Load Layout B command");
      DataManager.Instance.LoadDockStandardLayout("Std_B");
    }

    private void OnLayout_Showall(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {
        _log.Debug("Executing Show All Layout command");
        var layout = DockManager.Layout;

        // 1. Geschlossene Fenster wieder anzeigen
        foreach (var anchor in layout.Descendents().OfType<LayoutAnchorable>())
        {
          try
          {

            if (!anchor.IsVisible)
              anchor.Show();
          }
          catch (Exception ex)
          {
            _log.Error("Error in Show All Layout command: " + ex.ToString());
          }
        }

        // irgendein existierendes Pane suchen
        var pane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault();

        if (pane != null)
        {
          // 2. Fenster ins Pane einfügen, falls noch nicht vorhanden
          if (!pane.Children.Contains(KiSucheAnchorable))
            pane.Children.Add(KiSucheAnchorable);
          KiSucheAnchorable.Show();
        }
        //S1Z1T1.Children.Add(KiSucheAnchorable);
        //KiSucheAnchorable.Show();

      }
      catch (Exception ex)
      {
        _log.Error("Error in Show All Layout command: " + ex.ToString());
      }

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
      _log.Debug("Executing Manage Replacement Trains command");
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
    private void OnCanSaveRevisedTrain(object sender, CanExecuteRoutedEventArgs e)
    {
      TrainItem ti = DataManager.Instance.CurrentTrainItem;
      e.CanExecute = ti != null && (ti.IsLocoReplaced || ti.IsTrainReplaced);
    }

    //---------------------------------------------------------------------
    private void OnSaveRevisedTrain(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Save Replacement Trains command");
      DataManager dm = DataManager.Instance;
      dm.SaveReplacedTrain();
    }

    //---------------------------------------------------------------------
    private void OnCanUndoRevisedTrain(object sender, CanExecuteRoutedEventArgs e)
    {
      TrainItem ti = DataManager.Instance.CurrentTrainItem;
      e.CanExecute = ti != null && (ti.IsLocoReplaced || ti.IsTrainReplaced);
    }

    //---------------------------------------------------------------------
    private void OnUndoRevisedTrain(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Save Replacement Trains command");
      DataManager dm = DataManager.Instance;
      dm.UndoReplacedTrain();
    }


    //---------------------------------------------------------------------
    private void OnCanReplaceTrain(object sender, CanExecuteRoutedEventArgs e)
    {
      Zug zg = DataManager.Instance.CurrentTrain;
      TrainItem ti = DataManager.Instance.CurrentTrainItem;
      e.CanExecute = zg != null && !(ti != null && (ti.IsTrainReplaced || ti.IsLocoReplaced));
    }

    //---------------------------------------------------------------------
    private void OnReplaceTrain(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Replace Train command");
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
      _log.Debug("Executing Filter Refresh command");
      DataManager.Instance.FilterZugNummer = getFilterSearchtext(tblFilterZugNummer).Trim();
      GrpTrains.BorderBrush = System.Windows.Media.Brushes.Red;
      DataManager_RefreshFilter(sender, e);
    }

    //---------------------------------------------------------------------
    private void OnCanFilterRefresh(object sender, CanExecuteRoutedEventArgs e)
    {
      //TrainItem ti = DataManager.Instance.CurrentTrainItem;
      //e.CanExecute = ti != null && ti.IsTrainReplaced;
      e.CanExecute = true;
    }

    public void setgroupboxcolor(string expandername, string groupboxname, System.Windows.Media.Brush color, string newtitle = null)
    {
      // Schritt-für-Schritt-Plan (Pseudocode):
      // 1. Da GrpFpl im XAML als Teil eines ControlTemplates einer Expander-GroupBox deklariert ist, 
      //    ist sie nicht direkt als Feld im CodeBehind verfügbar.
      // 2. Stattdessen muss das Element zur Laufzeit über die VisualTreeHelper-Methoden gesucht werden.
      // 3. In der Methode OnFplFilterRefresh wird der VisualTree ab dem Expander durchsucht, 
      //    um die GroupBox mit dem Namen "GrpFpl" zu finden und deren BorderBrush zu setzen.

      // Expander-Instanz finden (ersetzen Sie ggf. "IhrExpanderName" durch den tatsächlichen Namen)
      _log.Debug($"Setting GroupBox color: Expander='{expandername}', GroupBox='{groupboxname}', Color='{color}'");
      var expander = FindExpanderInVisualTree(this, expandername);
      if (expander != null)
      {
        var groupBox = FindChildByName<GroupBox>(expander, groupboxname);
        if (groupBox != null)
        {
          groupBox.BorderBrush = color;
          if (!string.IsNullOrEmpty(newtitle))
          {
            groupBox.Header = newtitle;
          }
        }
      }

      //var groupBox = FindChildByName<GroupBox>(this, groupboxname);
      //if (groupBox != null)
      //{
      //  groupBox.BorderBrush = color;
      //}
    }

    //---------------------------------------------------------------------
    private void OnFplFilterRefresh(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing FPL Filter Refresh command");
      if (tblFilterZugNummer.Text.Trim().Length < 3)
      {
        MessageBox.Show(LocalizationManager.Translate("Bitte mindestens 3 Zeichen für die FPL-Suche eingeben."), LocalizationManager.Translate("Hinweis"), MessageBoxButton.OK, MessageBoxImage.Information);
        return;
      }
      else
      {
        DataManager.Instance.FilterZugNummer = tblFilterZugNummer.Text.Trim();
        bool result = DataManager.Instance.SearchTrain(tblFilterZugNummer.Text.Trim());
        if (!result)
        {
          MessageBox.Show(LocalizationManager.Translate("Kein Fahrplan gefunden."), LocalizationManager.Translate("Hinweis"), MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
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
          DataManager_RefreshFilter(sender, e);

          // Schritt-für-Schritt-Plan (Pseudocode):
          // 1. Da GrpFpl im XAML als Teil eines ControlTemplates einer Expander-GroupBox deklariert ist, 
          //    ist sie nicht direkt als Feld im CodeBehind verfügbar.
          // 2. Stattdessen muss das Element zur Laufzeit über die VisualTreeHelper-Methoden gesucht werden.
          // 3. In der Methode OnFplFilterRefresh wird der VisualTree ab dem Expander durchsucht, 
          //    um die GroupBox mit dem Namen "GrpFpl" zu finden und deren BorderBrush zu setzen.

          // Expander-Instanz finden (ersetzen Sie ggf. "IhrExpanderName" durch den tatsächlichen Namen)
          //var expander = FindExpanderInVisualTree(this, "ExpFpl");
          //if (expander != null)
          //{
          //  var groupBox = FindChildByName<GroupBox>(expander, "GrpFpl");
          //  if (groupBox != null)
          //  {
          //    groupBox.BorderBrush = System.Windows.Media.Brushes.Red;
          //  }
          //}
          setgroupboxcolor("ExpFpl", "GrpFpl", System.Windows.Media.Brushes.Red);
        }
      }
    }

    // Hilfsmethode, um ein Kind mit bestimmtem Namen und Typ im VisualTree zu finden
    private T FindChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
      if (parent == null) return null;
      int count = VisualTreeHelper.GetChildrenCount(parent);
      for (int i = 0; i < count; i++)
      {
        var child = VisualTreeHelper.GetChild(parent, i);
        if (child is T fe && fe.Name == name)
          return fe;
        var result = FindChildByName<T>(child, name);
        if (result != null)
          return result;
      }
      return null;
    }

    // Beispiel: Expander im VisualTree suchen (falls benötigt)
    private Expander FindExpanderInVisualTree(DependencyObject parent, string expanderName)
    {
      if (parent == null) return null;
      int count = VisualTreeHelper.GetChildrenCount(parent);
      for (int i = 0; i < count; i++)
      {
        var child = VisualTreeHelper.GetChild(parent, i);
        if (child is Expander exp && exp.Name == expanderName)
          return exp;
        var result = FindExpanderInVisualTree(child, expanderName);
        if (result != null)
          return result;
      }
      return null;
    }

    //---------------------------------------------------------------------
    private void OnCanFplFilterRefresh(object sender, CanExecuteRoutedEventArgs e)
    {
      //TrainItem ti = DataManager.Instance.CurrentTrainItem;
      //e.CanExecute = ti != null && ti.IsTrainReplaced;
      e.CanExecute = true;
    }

    //---------------------------------------------------------------------
    private void OnFilterReset(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Filter Reset command");
      //tblFilterZugNummer.Text = "";
      //Checkbox_Dekozüge.IsChecked = false;
      //Checkbox_Personenzüge.IsChecked = true;
      //Checkbox_Güterzüge.IsChecked = true;
      //DataManager_RefreshFilter(sender, e);
      GrpTrains.BorderBrush = System.Windows.Media.Brushes.White;
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
      _log.Debug("Executing Undo Replace Train command");
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

    static void CopyDirectory(string sourceDir, string destinationDir)
    {
      _log.Debug($"Copying directory from '{sourceDir}' to '{destinationDir}'");
      // Create destination directory if it doesn't exist
      Directory.CreateDirectory(destinationDir);

      // Copy all files
      foreach (string filePath in Directory.GetFiles(sourceDir))
      {
        string fileName = System.IO.Path.GetFileName(filePath);
        string destFile = System.IO.Path.Combine(destinationDir, fileName);
        System.IO.File.Copy(filePath, destFile, true); // true = overwrite if exists
      }

      // Copy all subdirectories recursively
      foreach (string subDir in Directory.GetDirectories(sourceDir))
      {
        string subDirName = System.IO.Path.GetFileName(subDir);
        string destSubDir = System.IO.Path.Combine(destinationDir, subDirName);
        CopyDirectory(subDir, destSubDir);
      }
    }

    public void SetCurrentTrainCat(string current_traincat)
    {
      _log.Debug($"Setting current train category to '{current_traincat}'");
      foreach (ComboBoxItem item in ComboBox_TrainCategories.Items)
      {
        if ((string)item.Tag == current_traincat)
        {
          ComboBox_TrainCategories.SelectedItem = item;
          break;
        }
      }
    }

    public string GetCurrentTrainCat()
    {
      _log.Debug("Getting current train category from ComboBox");
      if (ComboBox_TrainCategories.SelectedItem is ComboBoxItem item && item.Tag is string traincat)
      {
        return traincat;
      }
      return "all";
    }

    //---------------------------------------------------------------------
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      _log.Debug("MainWindow loaded - initializing data and UI components");
      Activate();

      _dataLoaderWindow = new DataLoaderWindow
      {
        Owner = this
      };

      //overlay = new OverlayWindow();
      //overlay.Owner = this; // Verknüpft die Fenster miteinander
      //overlay.Show();
      // Listen for Window size and position changes
      // **Overlay **
      //this.SizeChanged += (s, e) => UpdateOverlay();
      //this.LocationChanged += (s, e) => UpdateOverlay();

      // Detect when the MainWindow gets or loses focus
      //this.Activated += (s, e) => overlay.Show();
      //this.Deactivated += (s, e) => overlay.Hide();
      //PositionOverlay();

      // Check if demo timetables for driver shool are present
      string demoFplPath = System.IO.Path.Combine(Zusi.DataPath[DataPathType.DataDir], "Timetables");
      string demoFplFile = System.IO.Path.Combine(demoFplPath, "Deutschland", "Demo", "Demofahrplan_1986.fpn");
      if (!System.IO.File.Exists(demoFplFile))
      {

        string? executablePath = Process.GetCurrentProcess().MainModule?.FileName;

        string demosourcedir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(executablePath), "Fahrschule", "Timetables");
        CopyDirectory(demosourcedir, demoFplPath);
      }
      _log.Debug("Initializing DataManager and loading data");
      DataManager.Instance.InitializeData();

      DataManager.Instance.dataLoaderWindow = _dataLoaderWindow;
      _log.Debug("Setting screen scale factor for DPI scaling");
      DataManager.Instance.ScreenScaleFactor = GetScreenScaleFactor(this);
      _log.Debug("Initializing Options dialog");
      DataManager.Instance.optionsDlg_window = new OptionsDlg();
      //_dataLoaderWindow.ShowDialog();
      get_bildfahrplanpath();
      get_zusidisplaypath();
      get_zusimeterpath();
      _log.Debug("Creating ZSK location group ID dictionary");
      createZSK_locationGroupId_dict();
      VersionVisibility = Visibility.Visible;
      _log.Debug("Checking command line arguments for auto-starting a train");
      if ((Startup.commandlineargs.Length == 3 && Startup.commandlineargs[1] == "--start"))
      {
        _log.Debug("Commandline option --start");

        string train = Startup.commandlineargs[2];
        _log.Debug($"Attempting to start train '{train}' from command line argument");

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
            _log.Error($"Message: {train} not found");
          else
          {
            //starte aktuellen Zug:
            object _sender = null;
            ExecutedRoutedEventArgs _e = null;
            _log.Debug($"Starting train '{train}' from command line argument");
            OnStartTrain(_sender, _e);
          }

        }
        catch (Exception ex)
        {
          _log.Error(ex.Message);
        }
      }
      _log.Debug("Updating command texts for localization");
      UpdateCommandTexts();

    }

    private void get_bildfahrplanpath()
    {
      _log.Debug("Getting Bildfahrplan executable path from registry");
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
        _log.Debug($"Bildfahrplan executable path: '{DataManager.Instance.BildfahrplanExePath}'");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error accessing registry: {ex.Message}");
      }
    }

    private void get_zusimeterpath()
    {
      _log.Debug("Getting ZusiMeter executable path from registry");
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
        _log.Debug($"ZusiMeter executable path: '{DataManager.Instance.ZusiMeterStartCmd}'");
      }
      catch (Exception ex)
      {
        _log.Error($"Error accessing registry: {ex.Message}");
      }
    }

    public string get_zusidisplaypath()
    {
      _log.Debug("Getting ZusiDisplay executable path based on Zusi installation directory");
      try
      {
        string zusiexe_path = System.IO.Path.GetDirectoryName(Zusi.Executable);
        string zusidisplay_path = System.IO.Path.Combine(zusiexe_path, @"_Tools\ZusiDisplay\");
        string zusidisplay_startcmd = zusidisplay_path + @"ZusiDisplay.64.exe";
        DataManager.Instance.ZusiDisplayStartCmd = zusidisplay_startcmd;
        _log.Debug($"ZusiDisplay executable path: '{DataManager.Instance.ZusiDisplayStartCmd}'");
        return zusidisplay_startcmd;
      }
      catch (Exception ex)
      {
        _log.Error($"Error accessing registry: {ex.Message}");
        return "";
      }
    }

    //---------------------------------------------------------------------
    private void DataManager_DecoTrainsAllowed(object? sender, EventArgs e)
    {
      CollectionViewSource.GetDefaultView(tvPassengerTrains.ItemsSource).Refresh();
      //CollectionViewSource.GetDefaultView(tvFreightTrains.ItemsSource).Refresh();
      //CollectionViewSource.GetDefaultView(tvFoundPassengerTrains.ItemsSource).Refresh();
      //CollectionViewSource.GetDefaultView(tvFoundFreightTrains.ItemsSource).Refresh();
    }


    //---------------------------------------------------------------------
    public void DataManager_RefreshFilter(object? sender, EventArgs e)
    {
      if (tvPassengerTrains != null)
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
      //FadeIn(brdMain, 600);
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
      _log.Debug("DataManager selected timetable changed - refreshing train lists");
      CollectionViewSource.GetDefaultView(tvPassengerTrains.ItemsSource).Refresh();
      //CollectionViewSource.GetDefaultView(tvFreightTrains.ItemsSource).Refresh();
    }

    //---------------------------------------------------------------------
    private void OnExitGame(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Exit Game command - closing MainWindow");
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
      _log.Debug("Executing Minimize command - minimizing MainWindow");
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
      _log.Debug("Executing Favorite Trains Page command - showing recent trains");
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
      _log.Debug("Executing Add Favorite Train command - adding current train to recent trains");
      Zug zug = DataManager.Instance.CurrentTrain;
      string timeTablefilename = zug.FahrplanDatei.FullPath;

      string timeTableName = System.IO.Path.GetFileNameWithoutExtension(timeTablefilename);
      DataManager.Instance.RecentTrains.Add(new RecentTrain(zug, timeTableName));
      DataManager.Instance.RecentTrains.Save();
    }

    //---------------------------------------------------------------------
    private void OnEditFavoriteComment(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Edit Favorite Comment command - toggling comment edit mode for selected recent train");
      RecentTrain rt = DataManager.Instance.SelectedRecentTrain;

      rt.CommentEdit = !rt.CommentEdit;
      if (rt.CommentEdit)
      {
        EditFavoriteCommentCommand.Text = LocalizationManager.Translate("Kommentar speichern");
        EditCommentButtonText = EditFavoriteCommentCommand.Text;



        string comment = rt.Comment;
        if (string.IsNullOrEmpty(comment))
        {
          rt.Comment = rt.Train.Gattung + rt.Train.Nummer;
        }
      }
      else
      {
        EditFavoriteCommentCommand.Text = LocalizationManager.Translate("Kommentar bearbeiten");
        EditCommentButtonText = EditFavoriteCommentCommand.Text;
      }
      DataManager.Instance.RecentTrains.Save();

      // UI-Update erzwingen
      var view = CollectionViewSource.GetDefaultView(DataManager.Instance.RecentTrains);
      view.Refresh();
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
      _log.Debug("Executing Delete Favorite Train command - removing selected recent train from recent trains");
      Zug zug = DataManager.Instance.SelectedRecentTrain.Train;

      int index = 0;
      bool found = false;
      foreach (RecentTrain train in DataManager.Instance.RecentTrains)
      {
        if (train.Train.Nummer == zug.Nummer)
        {
          found = true;
          break;
        }
        index++;
      }
      if (found)
      {
        DataManager.Instance.RecentTrains.RemoveAt(index);
        DataManager.Instance.RecentTrains.Save();
      }
    }

    //---------------------------------------------------------------------
    private void OnSearchPage(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Search Page command - showing search page");
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
      _log.Debug("Executing Time Table Page command - showing main page");
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
      e.CanExecute = (canExecute && DataManager.Instance.CurrentTrain != null) || (DataManager.Instance.options != null && DataManager.Instance.options.RemoteZusi);
    }

    //---------------------------------------------------------------------
    private void OnCanSearchPage(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = DataManager.Instance.AllVehicles_loaded;
    }

    private bool? ZusiTCPServerenabled()
    {
      _log.Debug("Checking if Zusi TCP Server is enabled in registry");
      bool flag_error = false;
      bool flag_zusiSteamkey_found = false;
      bool flag_Zusikey_found = false;
      string zusikeyval = "Software\\Zusi3\\Fahrsim\\Einstellungen";
      string zusiSteamkeyval = "Software\\Zusi3\\FahrsimSteam\\Einstellungen";
      RegistryKey registryKey = null;
      RegistryKey registryKey_Steam = null;
      int NetzwerkServerAutom_value = 0;
      try
      {
        using (Registry.CurrentUser.OpenSubKey(zusikeyval, writable: true))
        {
          _log.Info("ZusiTCPServerenabled key " + zusikeyval + " found");
          flag_Zusikey_found = true;
          registryKey = Registry.CurrentUser.OpenSubKey(zusikeyval, writable: true);
          if (registryKey != null)
          {
            _log.Info("ZusiTCPServerenabled key " + zusikeyval + " found");
            NetzwerkServerAutom_value = (int)registryKey.GetValue("NetzwerkServerAutom");
            _log.Info("ZusiTCPServerenabled keydata " + NetzwerkServerAutom_value);
          }
        }
      }
      catch
      {
        flag_Zusikey_found = false;
        _log.Info("ZusiTCPServerenabled key " + zusikeyval + " Not found");
      }

      if (NetzwerkServerAutom_value != 1) // check for Steam version
      {
        try
        {
          using (Registry.CurrentUser.OpenSubKey(zusiSteamkeyval, writable: true))
          {
            _log.Info("ZusiTCPServerenabled key " + zusikeyval + " found");
            flag_zusiSteamkey_found = true;
            registryKey_Steam = Registry.CurrentUser.OpenSubKey(zusiSteamkeyval, writable: true);
            if (registryKey_Steam != null)
            {
              _log.Info("ZusiTCPServerenabled key " + zusiSteamkeyval + " found");
              NetzwerkServerAutom_value = (int)registryKey_Steam.GetValue("NetzwerkServerAutom");
              _log.Info("ZusiTCPServerenabled keydata " + NetzwerkServerAutom_value);
            }
          }
        }
        catch
        {
          flag_zusiSteamkey_found = false;
          _log.Info("ZusiTCPServerenabled key " + zusiSteamkeyval + " Not found");
        }
      }
      if (NetzwerkServerAutom_value != 1)
      {
        MessageBoxResult result = System.Windows.MessageBox.Show(LocalizationManager.Translate("Der ZUSI TCP Netzwerkserver ist NICHT eingeschaltet. Soll der ZUSI TCP Netwerkserver eingeschaltet werden?\n\nJa: TCP Server wird eingeschaltet \nNein: ZUSI wird mit Kommandozeile gestartet \nAbbrechen: Start wird abgebrochen"), LocalizationManager.Translate("Fehler beim Starten von Zug mit TCP Fahrpultschnittstelle"), MessageBoxButton.YesNoCancel);
        switch (result)
        {
          case MessageBoxResult.Yes:
            if (registryKey != null)
            {
              registryKey.SetValue("NetzwerkServerAutom", 1, RegistryValueKind.DWord);
              _log.Info("ZusiTCPServerenabled NetzwerkServerAutom set to 1");
              return true;
            }
            if (registryKey_Steam != null)
            {
              registryKey_Steam.SetValue("NetzwerkServerAutom", 1, RegistryValueKind.DWord);
              _log.Info("ZusiTCPServerenabled NetzwerkServerAutom set to 1");
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
      _log.Debug("Executing Start Favorite Train command - starting selected recent train");
      Zug zug = DataManager.Instance.SelectedRecentTrain.Train;
      OnStartTrain(zug);
    }

    //---------------------------------------------------------------------
    public void OnStartTrain(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Start Train command - starting current train");
      Zug zug = DataManager.Instance.CurrentTrain;
      OnStartTrain(zug);
    }

    public static void DoEvents()
    {
      var frame = new DispatcherFrame();
      Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
          new DispatcherOperationCallback(delegate (object f)
          {
            ((DispatcherFrame)f).Continue = false;
            return null;
          }), frame);
      Dispatcher.PushFrame(frame);
    }

    //---------------------------------------------------------------------
    public void OnStartTrain(Zug zug)
    {
      _log.Debug($"Starting train: {zug.Gattung} {zug.Nummer} - checking timetable and starting options");
      try
      {
        //TimeTable timeTable = DataManager.Instance.GetTimeTableOfTrain(zug);
        //if (timeTable == null)
        //{
        //  TimeTableFile timetablefile = new TimeTableFile(zug.FahrplanDatei.FullPath);
        //  timetablefile.Parse();
        //  timeTable = timetablefile.Root;
        //}

        //ZusiDocumentBase? doc = timeTable?.GetDocument();

        string CurrentTrainCat = (ComboBox_TrainCategories.SelectedItem as ComboBoxItem)?.Tag as string ?? "all";

        switch (CurrentTrainCat)
        {
          case "all":
            DataManager.Instance.LODZugList = "0123";
            break;
          case "many":
            DataManager.Instance.LODZugList = "123";
            break;
          case "important":
            DataManager.Instance.LODZugList = "23";
            break;
          case "minimal":
            DataManager.Instance.LODZugList = "3";
            break;
          case "none":
            DataManager.Instance.LODZugList = "";
            break;
        }


        if (DataManager.Instance.options.RemoteZusi)
        {
          // start train remote

          //HideWindow(nohide: true);

          // ... via TCP interface
          bool? TCPserver = ZusiTCPServerenabled();
          if (TCPserver == null)
            return;
          if (TCPserver == true)
          {
            //TimeTable tt = DataManager.Instance.GetTimeTableOfTrain(zug);
            //string timetablefilename = "";
            //if (tt != null)
            //{
            //  timetablefilename = tt.GetDocument().Filename;
            //}
            //else
            //{
            //  timetablefilename = zug.FahrplanDatei.FullPath;
            //}

            bool onlymonitor = true; // onlymonitor is always true

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
              onlymonitor = true;
            }

            TrainStartInfo tsi = new() { TimetableFile = "", TrainNumber = "" };
            _fahrpult.TryStartTrain(tsi, zusiremote: true, onlymonitor: onlymonitor);

          }

          if (FeatureManager.feature_enabled(FeatureManager.Features.Tracking))
          {
            // *HLI* DataManager.Instance.zusiMeterControl.assign_fahrpult(_fahrpult);
            Fahrpult_DataProcessor(); //**HLI**
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
        else
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

            //System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //{
            //  DataManager.Instance.LoadingStatus = LocalizationManager.Translate("Fahrplan wird optimiert... Dies kann je nach Fahrplangröße einige Zeit dauern.");
            //});
            Statuszeile_Fahrplaene.Visibility = Visibility.Collapsed;
            DataManager.Instance.LoadingStatus = LocalizationManager.Translate("Fahrplan wird optimiert... ") + LocalizationManager.Translate($"Dies kann je nach Fahrplangröße einige Zeit dauern. Anzahl Züge: {timeTable.Trains.Count}");
            // UI bekommt Zeit zum Rendern
            DoEvents();

            string tmpTimeTableFileName = "";
            string tempzugfilename = "";

            tempzugfilename = DataManager.Instance.BuildTempTimeTable(zug, timeTable, out tmpTimeTableFileName);

            if (!string.IsNullOrEmpty(tempzugfilename))
            {
              // start train
              FrictionSettingsPopupVisible = false;
              //System.Windows.Application.Current.Dispatcher.Invoke(() =>
              //{
              //  DataManager.Instance.LoadingStatus =
              //    LocalizationManager.Translate($"Ausgewählter Zug wird gestartet - Optimierung: {DataManager.Instance.original_traincount} -> {DataManager.Instance.optimised_traincount} - Startzeit: {DataManager.Instance.original_starttime.ToString()} -> {DataManager.Instance.optimized_starttime.ToString()}");
              //});

              if (DataManager.Instance.original_modulecount > 0)

                DataManager.Instance.LoadingStatus = LocalizationManager.Translate($"Ausgewählter Zug {zug.Gattung} {zug.Nummer} gestartet - Optimierung: Züge {DataManager.Instance.original_traincount} -> {DataManager.Instance.optimised_traincount} - Startzeit {DataManager.Instance.original_starttime.ToString()} -> {DataManager.Instance.optimized_starttime.ToString()} - Module {DataManager.Instance.original_modulecount.ToString()} -> {DataManager.Instance.optimized_modulecount.ToString()}");

              else
                DataManager.Instance.LoadingStatus = LocalizationManager.Translate($"Ausgewählter Zug {zug.Gattung} {zug.Nummer} gestartet - Optimierung: Züge {DataManager.Instance.original_traincount} -> {DataManager.Instance.optimised_traincount} - Startzeit {DataManager.Instance.original_starttime.ToString()} -> {DataManager.Instance.optimized_starttime.ToString()}");

              HideWindow();

              if (Properties.Settings.Default.TrainStartMode == 0)
              {
                // ... via TCP interface
                bool? TCPserver = ZusiTCPServerenabled();

                if (TCPserver != null && TCPserver == true)
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
              if (DataManager.Instance.RO_show_optimised_Streckenmodule)
              {
                UpdateRouteGraph2(doc.Filename);
              }

              if (FeatureManager.feature_enabled(FeatureManager.Features.Tracking))
              {
                // *HLI* DataManager.Instance.zusiMeterControl.assign_fahrpult(_fahrpult);
                Fahrpult_DataProcessor(); //**HLI**
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

            HideWindow();

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
            if (FeatureManager.feature_enabled(FeatureManager.Features.Tracking))
            {
              // *HLI* DataManager.Instance.zusiMeterControl.assign_fahrpult(_fahrpult);
              Fahrpult_DataProcessor(); //**HLI**
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
      }
      catch (Exception ex)
      {
        _log.Error(ex.ToString());
        ShowWindow();
      }
    }

    //---------------------------------------------------------------------
    private void Fahrpult_ClientConnected(object sender, ClientConnectedEventArgs e)
    {
      _log.Debug("Fahrpult client connected - processing connection status");
      //_log.Debug($"throttle connected ({e.ClientAccepted}, {e.NeededDataAccepted})");

      //if (e.ClientAccepted)
      //{
      //  Status |= 1;
      //}
      //if (e.NeededDataAccepted)
      //{
      //  Status |= 2;
      //}

      //lock (_lock)
      //{
      //  _pendingStart = false;
      //  if (_pendingTrain != null)
      //  {
      //    StartTrain(_pendingTrain.Value);
      //    _pendingTrain = null;
      //  }
      //}
    }

    public void Fahrpult_FtdDataReceived(object sender, FtdDataReceivedEventArgs e)
    {
      _dataQueue.Enqueue((EventArgs)e);
      _dataWakeUp.Set();
    }

    public void Fahrpult_ProgDataReceived(object sender, ProgDataReceivedEventArgs e)
    {
      _dataQueue.Enqueue((EventArgs)e);
      _dataWakeUp.Set();
    }

    public void Fahrpult_Disconnected(object sender, EventArgs e)
    {
      _log.Debug("Fahrpult disconnected - showing MainWindow");
      System.Windows.Application.Current.Dispatcher.Invoke(() => ShowWindow());
    }

    public void Fahrpult_Connected(object sender, EventArgs e)
    {
      _log.Debug("Fahrpult connected - hiding MainWindow");
      System.Windows.Application.Current.Dispatcher.Invoke(() => HideWindow(nohide: true));
    }

    public System.Windows.Point UTM2LatLon()
    {
      double utmX = _utmX + _xkoordinate / 1000.0;
      double utmY = _utmY + _ykoordinate / 1000.0;

      int southhemi = 0;
      //if (_zoneField < 'N')
      //  southhemi = 1;

      double lat = 0;
      double lon = 0;
      ZusiStart.Miscellaneous.UTM.UtmToLatLon(utmX * 1000, utmY * 1000, _zone, southhemi, out lat, out lon);

      return new System.Windows.Point(lon, lat);
    }

    public System.Windows.Point UTM2LatLon2()
    {
      double utmX = _utmX + _xkoordinate / 1000.0;
      double utmY = _utmY + _ykoordinate / 1000.0;
      //_utmX = x.GetAttrValue("UTM_WE", 0);
      //_utmY = x.GetAttrValue("UTM_NS", 0);
      //_zone = x.GetAttrValue("UTM_Zone", 0);
      //_zone2 = x.GetAttrValue("UTM_Zone2", "");
      _zoneField = ((!string.IsNullOrEmpty(_zone2)) ? _zone2[0] : '\0');
      bool IsNorth = (_zoneField >= 'N');

      double num = -0.00066286966871111107;
      double num2 = -0.0003868060578;
      double sm_a = 6378137.0;
      double sm_b = 6356752.314245;
      double x = Math.Pow(Math.Pow(sm_a, 2.0) - Math.Pow(sm_b, 2.0), 0.5) / sm_b;
      double num5 = Math.Pow(x, 2.0);
      double num6 = Math.Pow(sm_a, 2.0) / sm_b;
      double num7 = utmX * 1000 - 500000;
      double num8 = (IsNorth ? (utmY * 1000) : (utmY * 1000 - 10000000));
      double num9 = (double)_zone * 6.0 - 183.0;
      double num10 = num8 / (sm_a * 0.9996);
      double num11 = num6 / Math.Pow(1.0 + num5 * Math.Pow(Math.Cos(num10), 2.0), 0.5) * 0.9996;
      double num12 = num7 / num11;
      double num13 = Math.Sin(2.0 * num10);
      double num14 = num13 * Math.Pow(Math.Cos(num10), 2.0);
      double num15 = num10 + num13 / 2.0;
      double num16 = (3.0 * num15 + num14) * 0.25;
      double num17 = (5.0 * num16 + Math.Pow(num14 * Math.Cos(num10), 2.0)) / 3.0;
      double num18 = 0.75 * num5;
      double num19 = 1.6666666666666667 * Math.Pow(num18, 2.0);
      double num20 = 1.2962962962962963 * Math.Pow(num18, 3.0);
      double num21 = 0.9996 * num6 * (num10 - num18 * num15 + num19 * num16 - num20 * num17);
      double num22 = (num8 - num21) / num11;
      double num23 = num5 * Math.Pow(num12, 2.0) * 0.5 * Math.Pow(Math.Cos(num10), 2.0);
      double num24 = num12 * (1.0 - num23 / 3.0);
      double num25 = num22 * (1.0 - num23) + num10;
      double num26 = (Math.Exp(num24) - Math.Exp(0.0 - num24)) * 0.5;
      double num27 = Math.Atan(num26 / Math.Cos(num25));
      double num28 = Math.Atan(Math.Cos(num27) * Math.Tan(num25));
      return new System.Windows.Point(Sovoma.WPF.MathEx.Math2D.Degrees(num27) + num9 + num2, Sovoma.WPF.MathEx.Math2D.Degrees(num10 + (1.0 + num5 * Math.Pow(Math.Cos(num10), 2.0) - 1.5 * num5 * Math.Sin(num10) * Math.Cos(num10) * (num28 - num10)) * (num28 - num10)) + num);
    }

    private async void Fahrpult_DataProcessor()
    {
      await System.Threading.Tasks.Task.Run((Action)(() =>
      {
        while (!_dataCancellation)
        {
          this._dataWakeUp.WaitOne();
          if (_dataCancellation)
            break;
          this.Dispatcher.Invoke((Action)(() =>
        {
          EventArgs result;
          while (this._dataQueue.TryDequeue(out result))
          {
            bool koordinates_changed = false;
            //DataManager.Instance.zusiMeterControl.ZusiMeter_DataProcessor(result);
            FtdDataReceivedEventArgs f = result as FtdDataReceivedEventArgs;
            if (f != null)
            {
              //this._gauges.ForEach((Action<IGauge>)(g => g.SetFtdData(f)));
              foreach (FtdIdValuePair p in f.Values)
              {
                switch (p.ID)
                {
                  case ZFtdID.UTM_RefX:
                    _utmX = p.GetValue<int>();
                    break;
                  case ZFtdID.UTM_RefY:
                    _utmY = p.GetValue<int>();
                    break;
                  case ZFtdID.UTM_Zone:
                    _zone = p.GetValue<int>();
                    break;
                  case ZFtdID.UTM_Zone2:
                    _zone2 = "U"; // p.GetValue<string>();
                    break;
                  case ZFtdID.xKoordinate:
                    _xkoordinate = p.GetValue<int>();
                    koordinates_changed = true;
                    break;
                  case ZFtdID.yKoordinate:
                    _ykoordinate = p.GetValue<int>();
                    break;
                  case ZFtdID.Drehwinkel_z_Achse:
                    _drehwinkel_z = p.GetValue<double>();
                    break;
                }
              }

              if (koordinates_changed)
              {
                //Log.Debug("Fahrpult_DataProcessor - UTM" + _utmX.ToString() + "," + _utmY.ToString() + "," + _zone.ToString() + "," + _zone2 + "," + _xkoordinate.ToString() + "," + _ykoordinate.ToString());
                System.Windows.Point point = UTM2LatLon();
                UpdateMarkerPosition(point.Y, point.X);
                double utmX = _utmX + _xkoordinate / 1000.0;
                double utmY = _utmY + _ykoordinate / 1000.0;
                UpdateRGMarkerPosition(utmX, utmY, _drehwinkel_z);
                FahrschuleConnect();
              }
            }

            else
            {
              if (DataManager.Instance.options.RemoteZusi == false)
              {
                break;
              }
              ProgDataReceivedEventArgs pa = result as ProgDataReceivedEventArgs;
              if (pa != null)
              {
                //this._gauges.ForEach((Action<IGauge>)(g => g.SetFtdData(f)));
                foreach (ProgIdValuePair fp in pa.Values)
                {
                  switch (fp.ID)
                  {
                    case ZProgID.Zugdatei:
                      _zugDatei = fp.GetValue<string>();
                      _log.Debug("Fahrpult_DataProcessor zugDatei" + _zugDatei);

                      if (_zugDatei == @"Temp\ZusiStart\TempTimetable\ZusiStartTempTimeTable.fpn")
                      {
                        break;
                      }
                      if (_zugDatei.StartsWith("Temp"))
                      {
                        _zugDatei = _zugDatei.Replace("Temp\\", "Timetables\\");
                      }
                      //string filePath = Zusi.DataPath[DataPathType.DataDir] + _zugDatei;
                      //Log.Debug("Fahrpult_DataProcessor PrivatePath" + filePath);
                      //if (!System.IO.File.Exists(filePath))
                      //{
                      //  filePath = Zusi.DataPath[DataPathType.Official] + _zugDatei;
                      //  Log.Debug("Fahrpult_DataProcessor OfficialPath" + filePath);
                      //}
                      DataPathType dtp = DataPathType.Unknown;
                      string filePath = Zusi.GetAbsolutePathOf(_zugDatei, ref dtp);

                      // Verzeichnis, in dem die Datei liegt
                      string currentDir = System.IO.Path.GetDirectoryName(filePath);
                      var dirInfo = new DirectoryInfo(currentDir);

                      // Name des aktuellen Verzeichnisses = Name des Fahrplans
                      string dirName = dirInfo.Name;

                      // Name des übergeordneten Verzeichnisses
                      string parentDirName = dirInfo.Name;

                      // Vollständiger Pfad zum übergeordneten Verzeichnis
                      string parentDirFullPath = dirInfo.Parent.FullName;
                      DataManager.Instance.routeGraphOpenFile = parentDirFullPath + "\\" + parentDirName + ".fpn";

                      UpdateRouteGraph2(parentDirFullPath + "\\" + parentDirName + ".fpn");

                      break;
                  }
                }
              }
            }
          }
        }));
        }
      }));
    }

    //---------------------------------------------------------------------
    private void OnResetData(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Reset Data command - showing reset data confirmation dialog");
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
      _log.Debug("Executing Friction Settings command - toggling friction settings popup visibility");
      FrictionSettingsPopupVisible = !FrictionSettingsPopupVisible;
    }

    //---------------------------------------------------------------------
    private void OnCanStartFahrschule(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = true;
    }

    FahrschulWindow fahrschulWindow;

    //---------------------------------------------------------------------
    private void OnStartFahrschule(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Start Fahrschule command - showing Fahrschule window");
      fahrschulWindow = new FahrschulWindow { Owner = DataManager.Instance.main_window };

      fahrschulWindow.Show();

      try
      {
        string train = Zusi.DataPath[2] + "Timetables\\Deutschland\\Demo\\Demofahrplan_1986\\D2640.trn";
        ZugDatei zd = new ZugDatei(null, train);
        if (zd.Root == null)
        {
          zd.Parse();
        }
        DataManager.Instance.CurrentTrain = zd.Root;
        if (zd.Root == null)
          _log.Debug($"Message: {train} not found");
        else
        {
          //starte aktuellen Zug:
          object _sender = null;
          ExecutedRoutedEventArgs _e = null;
          OnStartTrain(_sender, _e);
          InfoTabControl.SelectedIndex = 10;
          DataManager.Instance.routeGraphOpenFile = Zusi.DataPath[2] + "Timetables\\Deutschland\\Demo\\Demofahrplan_1986.fpn";
          UpdateRouteGraph2(DataManager.Instance.routeGraphOpenFile);

        }

      }
      catch (Exception ex)
      {
        _log.Debug(ex.Message);
      }


    }

    bool Fahrschule_connected = false;

    public void FahrschuleConnect()
    {
      _log.Debug("Attempting to connect to Fahrschule - checking connection status and connecting if not already connected");
      if (fahrschulWindow == null)
        return;
      if (Fahrschule_connected == false)
      {
        object senderBtn = null;
        RoutedEventArgs eBtn = null;

        fahrschulWindow.BtnConnect_Click(senderBtn, eBtn);
        Fahrschule_connected = true;
      }
    }

    //---------------------------------------------------------------------
    private void ZusiSim_Terminated(object sender, EventArgs e)
    {
      _log.Debug("ZusiSim process terminated - showing MainWindow and quitting Zusi connections");
      Dispatcher.BeginInvoke(new Action(() => ShowWindow()));
      ZusiStart.Connection.ZusiMeter.Quit();
      ZusiStart.Connection.ZusiDisplay.Quit();
      ZusiStart.Connection.ZusiBildFahrplan.Quit();
      Fahrschule_connected = false;
    }

    //---------------------------------------------------------------------
    private void OnCanCreateVehicleList(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = DataManager.Instance.FilteredVehicles == null;
    }

    //---------------------------------------------------------------------
    private void OnCreateVehicleList(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Create Vehicle List command - creating filtered vehicle list if not already created");
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
      e.CanExecute = true; // !string.IsNullOrEmpty(tbxTrainNumber.Text);
    }

    //---------------------------------------------------------------------
    private void OnSearchTrain(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Search Train command - searching for train based on input train number and updating UI accordingly");
      //if (((DataManager)DataContext).SearchTrain(tbxTrainNumber.Text))
      //{
      //  tblkMessage.Visibility = Visibility.Hidden;
      //}
      //else
      //{
      //  tblkMessage.Visibility = Visibility.Visible;
      //}
      DataManager.SearchVehicleGroupValue = null;
      if (tbxTrainNumber.Text == null || tbxTrainNumber.Text.Trim().Length < 3)
      {
        MessageBox.Show(LocalizationManager.Translate("Bitte mindestens 3 Zeichen für die FPL-Suche eingeben."), LocalizationManager.Translate("Hinweis"), MessageBoxButton.OK, MessageBoxImage.Information);
        return;
      }
      else
      {
        DataManager.SearchTrainValue = getFilterSearchtext(tbxTrainNumber).Trim();

        bool result = DataManager.Instance.SearchTrain(DataManager.SearchTrainValue);
        if (!result)
        {
          MessageBox.Show(LocalizationManager.Translate("Kein Fahrplan gefunden."), LocalizationManager.Translate("Hinweis"), MessageBoxButton.OK, MessageBoxImage.Information);
          //setgroupboxcolor("ExpFpl", "GrpFpl", System.Windows.Media.Brushes.Red, newtitle: DataManager.SearchTrainValue);
        }
        else
        {
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
          DataManager_RefreshFilter(sender, e);

          setgroupboxcolor("ExpFpl", "GrpFpl", System.Windows.Media.Brushes.Red, newtitle: DataManager.SearchTrainValue);
        }
      }
    }

    private void OnSearchKI(object sender, ExecutedRoutedEventArgs e)
    {
      DataManager.Instance.OnSearchKI(sender, e);
    }

    private void OnCanSearchKI(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = true;  
    }



    private void OnAnalyseKI(object sender, ExecutedRoutedEventArgs e)
    {
      //DataManager.AnalyseZusiData4KI();
    }

    private void OnCanAnalyseKI(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = true;
    }



    //---------------------------------------------------------------------
    private void TbxBRNumber_GotFocus(object sender, RoutedEventArgs e)
    {
      //tblkMessage.Visibility = Visibility.Hidden;
    }

    //---------------------------------------------------------------------
    private void TbxBRNumber_TextChanged(object sender, EventArgs e)
    {
      //tblkMessage.Visibility = Visibility.Hidden;
    }

    //---------------------------------------------------------------------
    private void OnCanSearchBR(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = true; // !string.IsNullOrEmpty(tbxTrainNumber.Text);
    }

    //---------------------------------------------------------------------
    private void OnSearchBR(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Executing Search BR command - searching for BR based on input BR number and updating UI accordingly");

      DataManager.SearchVehicleGroupValue = null;
      if (tbxBRNumber.Text == null || tbxBRNumber.Text.Trim().Length < 2)
      {
        MessageBox.Show(LocalizationManager.Translate("Bitte mindestens 2 Zeichen für die BR-Suche eingeben."), LocalizationManager.Translate("Hinweis"), MessageBoxButton.OK, MessageBoxImage.Information);
        return;
      }
      else
      {

        DataManager.Instance.SearchBR(tbxBRNumber.Text);
      }
    }

    private void createZSK_locationGroupId_dict()
    {
      _log.Debug("Creating ZSK_locationGroupId dictionary - reading from CSV file and populating dictionary with location group ID mappings");
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

    public void CoreWebView2SourceChanged_ZDB(object? sender, CoreWebView2SourceChangedEventArgs e)
    {
      TabViewModel tabvm = InfoTabControl.SelectedValue as TabViewModel;
      string currenttabtitle = tabvm.Title;
      _log.Debug("ZDB-Webview_SourceChanged - current tab: " + currenttabtitle);
      if (currenttabtitle == DataManager.Instance.tab_title_Zusi_DB) // only execute when ZDB tab is active
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

    }

    private Dictionary<string, string> locationFiles = new Dictionary<string, string>
    {
      { "11.892356985347789/52.047453270489115", "Deutschland\\Magdeburg_Dessau\\" },
      { "10.377657338289906/52.863821938369256","Deutschland\\Hamburg_Kassel\\"},
      { "Tokyo", "Tokyo_report.pdf" }
    };

    public void CoreWebView2SourceChanged_ZSK(object? sender, CoreWebView2SourceChangedEventArgs e)
    {
      TabViewModel tabvm = InfoTabControl.SelectedValue as TabViewModel;
      string currenttabtitle = tabvm.Title;
      _log.Debug("ZSK-Webview_SourceChanged - current tab: " + currenttabtitle);

      if (currenttabtitle == DataManager.Instance.tab_title_Streckenkarte)
      {

        string newSource = ((CoreWebView2)sender).Source.ToString();
        _log.Debug("ZSK-Webview_Source:" + newSource);
        System.Diagnostics.Debug.WriteLine($"***** CoreWebView2SourceChanged_ZSK: Source {newSource} *****");
        DataManager.Instance.webview_ZSK_source = newSource;

        //if (newSource.EndsWith(".trn/"))
        //{
        //  string train_number = "";
        //  string[] pathitems = newSource.Split("%5C");
        //  if (pathitems != null && pathitems.Count() == 5)
        //  {
        //    train_number = Zusi.DataPath[0] + "Timetables\\" + pathitems[1] + "\\" + pathitems[2] + "\\" + pathitems[3] + "\\" + pathitems[4];
        //    train_number = train_number.Replace(".trn/", "");
        //    train_number = train_number + ".trn";
        //    //bool train_found = ((DataManager)DataContext).SearchTrain(train_number);
        //    ZugDatei zd = new ZugDatei(null, train_number);
        //    if (zd.Root == null)
        //    {
        //      zd.Parse();

        //    }
        //    DataManager.Instance.CurrentTrain = zd.Root;

        //    //if (source_parts.Count() > 1)
        //    //{
        //    //  object dummy_sender = null;
        //    //  ExecutedRoutedEventArgs dummy_e = null;
        //    //  OnStartTrain(dummy_sender, dummy_e);
        //    //}
        //  }
        //}
        //else if (newSource.EndsWith(".st3"))
        //{
        //  string train_number = "";
        //  string[] pathitems = newSource.Split("%5C");
        //  if (pathitems != null && pathitems.Count() == 5)
        //  {
        //    train_number = pathitems[4];

        //    bool train_found = ((DataManager)DataContext).SearchTrain(train_number);

        //    if (train_found)
        //    {
        //      DataManager.Instance.CurrentTrain = null;
        //      DataManager.Instance.SelectedRecentTrain = null;
        //      DataManager.Instance.CurrentTrainItem = null;
        //      MainBorderVisibility = Visibility.Collapsed;
        //      MainWebBorderVisibility = Visibility.Collapsed;
        //      SearchBorderVisibility = Visibility.Visible;
        //      RecentTrainsBorderVisibility = Visibility.Collapsed;
        //    }

        //    //if (source_parts.Count() > 1)
        //    //{
        //    //  object dummy_sender = null;
        //    //  ExecutedRoutedEventArgs dummy_e = null;
        //    //  OnStartTrain(dummy_sender, dummy_e);
        //    //}
        //  }
        //}
        //else 
        if (newSource.Contains("zusi-sk.eu/#"))
        {
          _log.Debug("ZSK-Webview_Source: Startswith Zusi_Sk" + newSource);
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
    }

    public void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
    {
      _log.Debug("WebView2 NavigationStarting - URL: " + e.Uri);
      // Check if the URL is the one you want to intercept
      if (e.Uri.Contains("?zugstart="))
      {
        //starte aktuellen Zug:
        object _sender = null;
        ExecutedRoutedEventArgs _e = null;
        e.Cancel = true;
        DataManager.Instance.main_window.OnStartTrain(_sender, _e);
        // Navigate to the new URL
        //webView2.CoreWebView2.Navigate("https://newurl.com");
      }
      if (e.Uri.Contains("fpndatei="))
      {
        string uri = e.Uri;

        string[] pathitems = uri.Split("=");
        if (pathitems != null && pathitems.Count() == 2)
        {
          e.Cancel = true;
          string fpnname = pathitems[1];
          fpnname = fpnname.Replace("?", "\\");
          TimeTable timeTable = DataManager.Instance.GetTimeTableOfFpnName(fpnname);

          if (timeTable != null)
          {
            TimeTableRelation value = new(0, timeTable);

            DataManager.Instance.OnSelectedTimeTableChanged(value);
          }
        }
      }
    }

    private async void SearchPDFjs(Microsoft.Web.WebView2.Wpf.WebView2 EfpTab, string searchstring)
    {
      _log.Debug("Executing SearchPDFjs - searching for string: " + searchstring);

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


    private async void SearchPDFjs_TOC(Microsoft.Web.WebView2.Wpf.WebView2 EfpTab, string searchstring)
    {
      _log.Debug("Executing SearchPDFjs_TOC - searching for string in table of contents: " + searchstring);

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
      console.log('Kein Eintrag für " + searchstring + @" gefunden.');
    }
  }, 1000); // Warte kurz, bis die Suche abgeschlossen ist
");

    }

    public class Chapter
  {
    public string title { get; set; }
    public object dest { get; set; } // dest ist ein komplexes Objekt
    }


    private async void SearchPDFjs_TOC_test(Microsoft.Web.WebView2.Wpf.WebView2 EfpTab, string searchstring)
    {
      string js = @"
(() => {
  return new Promise(resolve => {
    const waitForPDF = () => {
      if (typeof PDFViewerApplication !== 'undefined' && PDFViewerApplication.pdfDocument) {
        PDFViewerApplication.pdfDocument.getOutline().then(outline => {
          if (!outline) {
            resolve('[]');
            return;
          }

          function flattenOutline(items) {
            const result = [];
            for (const item of items) {
              if (item.title && item.dest) {
                result.push({ title: item.title, dest: item.dest });
              }
              if (item.items && item.items.length > 0) {
                result.push(...flattenOutline(item.items));
              }
            }
            return result;
          }

          const flat = flattenOutline(outline);
          resolve(JSON.stringify(flat));
        });
      } else {
        setTimeout(waitForPDF, 100);
      }
    };
    waitForPDF();
  });
})();
";




      string jsonResult = await EfpTab.CoreWebView2.ExecuteScriptAsync(js);

      string cleanedJson = System.Text.RegularExpressions.Regex.Unescape(jsonResult.Trim('"'));
      try
      {


        var chapters = System.Text.Json.JsonSerializer.Deserialize<List<Chapter>>(cleanedJson);

        var match = chapters.FirstOrDefault(c => c.title.Contains(searchstring));
        if (match != null)
        {
          string destJson = System.Text.Json.JsonSerializer.Serialize(match.dest);
          await EfpTab.CoreWebView2.ExecuteScriptAsync($@"
        PDFViewerApplication.pdfLinkService.navigateTo({destJson});
    ");
        }
      }
      catch
      {

      }



    }

    public void remove_oeril_sk_tab()
    {
      _log.Debug("Removing OeRilSK tab - checking if tab exists and removing it from the tab collection");
      //if (DataManager.Instance.Tabs.Contains(_oeril_sk_tab))
      //{
      //  _oeril_sk_tab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_OeRilSK);
      //  DataManager.Instance.Tabs.Remove(_oeril_sk_tab);
      //}
    }

    public void add_oeril_sk_tab()
    {
      //if (!DataManager.Instance.Tabs.Contains(_oeril_sk_tab))
      //{
      //  DataManager.Instance.Tabs.Add(_oeril_sk_tab);
      //}
    }


    public async void SetLaNummer(string lanr)
    {
      _log.Debug("Setting La-Nummer - searching for La-Nummer: " + lanr);
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
        var StreBuTab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_StreBu);
        if (StreBuTab != null)
        {
          if (StreBuTab.PDFViewerUrl != null)
          {
            string searchstring = lanr + " ";
            searchstring = new string(searchstring.Where(char.IsDigit).ToArray()) + " "; // remove all chars

            SearchPDFjs_TOC(StreBuTab.WebViewInstance, searchstring);
          }
        }
      }
    }

    /// <summary>
    /// Merges a list of bitmaps into one horizontal bitmap.
    /// </summary>
    static Bitmap MergeBitmapsHorizontally(List<Bitmap> bitmaps)
    {
      if (bitmaps == null || bitmaps.Count == 0)
        throw new ArgumentException("Bitmap list is empty.");

      // Calculate total width and max height
      int totalWidth = 0;
      int maxHeight = 0;
      foreach (var bmp in bitmaps)
      {
        if (bmp == null) throw new ArgumentException("One of the bitmaps is null.");
        totalWidth += bmp.Width + 10;
        if (bmp.Height > maxHeight) maxHeight = bmp.Height;
      }

      // Create result bitmap
      Bitmap result = new Bitmap(totalWidth, maxHeight);
      using (Graphics g = Graphics.FromImage(result))
      {
        g.Clear(System.Drawing.Color.Transparent); // Optional background
        int offsetX = 0;
        foreach (var bmp in bitmaps)
        {
          g.DrawImage(bmp, offsetX, 0);
          offsetX += bmp.Width + 10;
        }
      }

      return result;
    }


    public void SearchBuchfahrplan()
    {
      _log.Debug("Searching Buchfahrplan - attempting to load and display Buchfahrplan for current train");
      ZT_Buchfahrplan bfpl = new ZT_Buchfahrplan();

      string filename; // = @"C:\Program Files\Zusi3\_ZusiData\Timetables\Deutschland\Hamburg_Kassel\Guntershausen-Edesheim_2020_06Uhr-10Uhr\RB14202_14009.timetable.xml";
      DataPathType dtp = DataPathType.Unknown;

      Zug zug = DataManager.Instance.CurrentTrain ?? DataManager.Instance.SelectedRecentTrain?.Train;
      if (zug != null)
      {
        try
        {
          ZugDatei trn_file = zug.Parent as ZugDatei;
          if (trn_file != null)
          {
            if (FeatureManager.feature_enabled(FeatureManager.Features.StartLocation))
            {
              TabViewModel tabvm = InfoTabControl.SelectedValue as TabViewModel;
              string currenttabtitle = tabvm.Title;
              if (currenttabtitle == DataManager.Instance.tab_title_tracking) // only execute when tracking tab is active
              {
                string bfpl_fahrplanDatei = zug.FahrplanDatei.Dateiname;
                bfpl.TestLoadingFramework(bfpl_fahrplanDatei, zug.Nummer, out _utmX, out _utmY, out _zone, out _zone2);
                if (_utmX != 0)
                {
                  System.Windows.Point point = UTM2LatLon();
                  UpdateMarkerPosition(point.Y, point.X);
                }
              }
            }

            filename = Zusi.GetRelativePathOf(trn_file.Filename, ref dtp);
            if (zug.BuchfahrplanRohDatei != null)
            {

              string bfpl_filename = zug.BuchfahrplanRohDatei.FullPath;
              filename = System.IO.Path.Combine(bfpl_filename, "");
              List<Bitmap> bitmaps = bfpl.show_Buchfahrplan2(filename, zug.BuchfahrplanDll);
              if (bitmaps != null && bitmaps.Count > 0)
              {
                //buchfahrplanImage.Source = BitmapToImageSource(bitmap);
                Bitmap merged = MergeBitmapsHorizontally(bitmaps);
                var imageTab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_buchfahrplan);
                if (imageTab != null)
                {

                  imageTab.ImageSource = BitmapToImageSource(merged);
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
      _log.Debug("Executing OnZusiBildFahrplan - attempting to start ZusiBildfahrplan with current train data");
      StartZusiBildFahrplan();
    }

    private void StartZusiBildFahrplan(string trn_filename = "", string timetablefilename = "")
    {
      _log.Debug("Starting ZusiBildfahrplan - preparing parameters for ZusiBildfahrplan based on current train data");
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
      _log.Debug("Starting ZusiDisplay - preparing parameters for ZusiDisplay based on current train data");
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
      _log.Debug("Starting ZusiMeter - preparing parameters for ZusiMeter based on current train data");
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

    public void UpdateMarkerPosition(double latitude, double longitude)
    {
      //if (marker == null)
      //{
      //  InitializeMap();
      //}
      //_log.Debug("UpdateMarkerPosition - Pos" + latitude.ToString() + "," + longitude.ToString());

      //marker.Position = new PointLatLng(latitude, longitude);
      //TabViewModel? zdbTab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_tracking);
      //if (zdbTab != null)
      //  //zdbTab.gmap.Position = marker.Position; // Karte auf neue Position zentrieren
      //  DataManager.Instance.osmGmapControl.gmap.Position = marker.Position;
      OSMGmapControl.UpdateMarkerPosition(latitude, longitude);
    }

    public void UpdateRGMarkerPosition(double utmX, double utmY, double phi)
    {
      if (DataManager.Instance.rg_trackingItem != null)
      {
        DataManager.Instance.rg_trackingItem.UpdateMarkerPosition(utmX, utmY, phi);
      }



    }
    private void OnResetView(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Resetting view - resetting all tabs and views to their default state");
      ShowWindow();
    }

    private void OnReload(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Reloading - reloading timetable and vehicles");
      DataManager.Instance.InitializeData();
    }

    //---------------------------------------------------------------------
    private void OnTrainStartSettings(object sender, ExecutedRoutedEventArgs e)
    {
      _log.Debug("Opening Train Start Settings Dialog - allowing user to configure train start options");
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
        Properties.Settings.Default.StartimStillstand = dlg.StartimStillstand ? 0 : 1;

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
      _log.Debug("Sort mode changed - updating sorting of train list based on new sort mode");
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
      System.Windows.Duration d = new(TimeSpan.FromMilliseconds(duration));
      DoubleAnimation da = new(1.0, d);
      what.BeginAnimation(OpacityProperty, da);
    }

    //---------------------------------------------------------------------
    private static void FadeOut(UIElement what, double duration, Action? completed)
    {
      System.Windows.Duration d = new(TimeSpan.FromMilliseconds(duration));
      DoubleAnimation da = new(0.0, d);
      da.Completed += (s, e) => completed?.Invoke();
      what.BeginAnimation(OpacityProperty, da);
    }

    GridLength gridLengthLeftColumn;
    GridLength gridLengthRightColumn;
    GridLength gridLengthTopRow;
    GridLength gridLengthBottomRow;

    //---------------------------------------------------------------------
    private void HideWindow(bool nohide = false)
    {
      _log.Debug("Hiding window - hiding main window and adjusting visibility of UI elements based on hide settings");
      if (!DataManager.Instance.WindowIsHidden)
      {
        DataManager.Instance.WindowIsHidden = true;
        if (!DataManager.Instance.options.DonotHideZusiStart && !nohide)
        {
          FadeOut(this, 1000, () => Hide());
        }
        else
        {
          DataManager.Instance.LoadDockLayout("B");
          ////brdCol1.Visibility = Visibility.Collapsed;
          //brdCol1.Hide();
          ////brdTrains.Visibility = Visibility.Collapsed;
          //BrdSelTrain.Visibility = Visibility.Collapsed;
          ////TabControlCol1.Visibility = Visibility.Collapsed;
          ////brdFahrplaene.Visibility = Visibility.Collapsed;
          ////GridSplitterFahrplaene.Visibility = Visibility.Collapsed;
          ////StatusLine.Visibility = Visibility.Collapsed;
          //Statuszeile_Buttons.Visibility = Visibility.Collapsed;
          //gridLengthLeftColumn = LeftColumn.Width;
          //gridLengthRightColumn = RightColumn.Width;
          ////gridLengthTopRow = TopRow.Height;
          //gridLengthBottomRow = BottomRow.Height;

          //LeftColumn.Width = new GridLength(0, GridUnitType.Star);
          //RightColumn.Width = new GridLength(0, GridUnitType.Star);
          ////TopRow.Height = new GridLength(0, GridUnitType.Star);
          //BottomRow.Height = new GridLength(0, GridUnitType.Star);
        }
      }
    }

    //---------------------------------------------------------------------
    private void ShowWindow()
    {
      _log.Debug("Showing window - showing main window and adjusting visibility of UI elements based on show settings");
      if (DataManager.Instance.WindowIsHidden)
      {
        DataManager.Instance.WindowIsHidden = false;
        Show();
        FadeIn(this, 600);

        DataManager.Instance.LoadDockLayout("A");

        //brdCol1.Show(); // = Visibility.Visible;
        ////brdTrains.Visibility = Visibility.Visible;
        //BrdSelTrain.Visibility = Visibility.Visible;
        ////TabControlCol1.Visibility = Visibility.Visible;
        ////brdFahrplaene.Visibility = Visibility.Visible;
        ////GridSplitterFahrplaene.Visibility = Visibility.Visible;
        //DataManager.Instance.LoadingStatus = " ";
        //StatusLine.Visibility = Visibility.Visible;
        //Statuszeile_Buttons.Visibility = Visibility.Visible;


        //LeftColumn.Width = gridLengthLeftColumn;
        //RightColumn.Width = gridLengthRightColumn;
        ////TopRow.Height = gridLengthTopRow;
        //BottomRow.Height = gridLengthBottomRow;


        // make sure that ZusiStart is not covered by another app
        //this.WindowState = WindowState.Normal; // falls minimiert

        // Trick: kurz TopMost setzen
        this.Topmost = true;
        this.Topmost = false;

        // Fokus setzen
        this.Activate();
        this.Focus();
      }
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
      _log.Debug("Applying general train filter - filtering train list based on train number and other criteria");
      if (e.Item is TrainsViewModel tvm)
      {
        e.Accepted = true;
        //if (true) //((bool)Checkbox_Personenzüge.IsChecked)
        //{
        //  e.Accepted = tvm.DisplayName == "Personenzüge";
        //}
        //if (true) //((bool)Checkbox_Güterzüge.IsChecked)
        //{
        //  e.Accepted = tvm.DisplayName == "Güterzüge" || e.Accepted;
        //}
        //if (!DataManager.Instance.IsDecoTrainsAllowed) //(!(Checkbox_Personenzüge.IsChecked ?? false) && !(Checkbox_Güterzüge.IsChecked ?? false) && (Checkbox_Dekozüge.IsChecked ?? false))
        //{
        //  e.Accepted = true;
        //}

        DataManager.Instance.FilterZugNummer = getFilterSearchtext(tblFilterZugNummer).Trim();

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
      _log.Debug("Selected timetable changed - updating timetable relations and views based on new selection");
      DataManager.UpdateTimeTableRelations(e.TimeTables, e.PreferredSelection);
    }

    //---------------------------------------------------------------------
    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      _log.Debug("Selected train changed - updating current train and related views based on new selection");
      TrainsViewModel? tvm = e.NewValue as TrainsViewModel;
      DataManager.Instance.CurrentTrainItem = null;
      DataManager.Instance.CurrentTrain = tvm?.Object;

      if (DataManager.Instance.CurrentTrain?.Parent is ZugDatei zd)
      {
        if (zd.Filename.StartsWith(Zusi.DataPath[4]))
        {
          DataManager.Instance.CurrentTrainItem = new TrainItem(DataManager.Instance.CurrentTrain);
          DataManager.Instance.CurrentTrainItem.IsTrainReplaced = true;
        }
      }

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

    //---------------------------------------------------------------------
    public static bool TrainFiltered_ok(Zug m)
    {
      _log.Debug("Checking if train is accepted by filter - evaluating train number and related criteria against filter settings");
      bool train_accepted = true;

      if (DataManager.Instance.FilterZugNummer != "")
      {
        string n = DataManager.Instance.FilterZugNummer.Replace(" ", "").Trim().ToLower();
        train_accepted = string.IsNullOrEmpty(n) || (m.Gattung + m.Nummer).Replace(" ", "").Trim().ToLower().Contains(n) == true;
        //check for Betriebsstelle
        if (train_accepted == false)
        {
          train_accepted = m.FahrplanEintraege.Any(f => f.Bestrst != null && DataManager.IsValidName(f.Bestrst) && f.Bestrst.Replace(" ", "").Trim().ToLower().Contains(n, StringComparison.OrdinalIgnoreCase));
        }
      }
      return train_accepted;
    }

    //---------------------------------------------------------------------
    public bool TimeTableFiltered_ok(TimeTable m)
    {
      _log.Debug("Checking if timetable is accepted by filter - evaluating timetable name and related criteria against filter settings");
      bool timetable_accepted = true;
      try
      {
        if (m != null && DataManager.Instance.FilterZugNummer != "")
        {
          // Überprüfe, ob ein FoundTimeTable mit TimeTable == m existiert
          timetable_accepted = DataManager.Instance.FoundTimeTables.Any(ft => ft.Object?.TimeTable.Name == m.Name);
        }
        return timetable_accepted;
      }
      catch
      {
        return false;
      }
    }

    private void SelectedTimeTableChanged(object sender, SelectionChangedEventArgs e)
    {
      _log.Debug("Selected timetable relation changed - updating views and related data based on new selection");
      //Ensure the sender is a ListBox
      if (sender is ListBox listBox)
      {

        DataManager.FoundTimeTableIds = null;

        TimeTableGroup? selectedItem2 = listBox.SelectedItem as TimeTableGroup;
        TimeTableRelation? selectedItem = listBox.SelectedItem as TimeTableRelation;
        TimeTable selectedTT;
        _log.Debug("Selected item: " + (selectedItem != null ? selectedItem.TimeTable.Name : "null") + ", Selected group: " + (selectedItem2 != null ? selectedItem2.GroupID : "null"));

        DataManager.SearchVehicleGroupValue = null;
        DataManager.SearchTrainValue = null;
        if (selectedItem2 != null) // Timetablegroup
        {
          _log.Debug("Selected item is a TimeTableGroup" + selectedItem2.GroupID);
          setgroupboxcolor("ExpFpl", "GrpFpl", System.Windows.Media.Brushes.White, newtitle: selectedItem2.GroupID);
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
      _log.Debug("Selected timetable relation changed - updating views and related data based on new selection");
      // Ensure the sender is a ListBox
      if (sender is ListBox listBox)
      {
        // Get the selected item
        TimeTableRelation? selectedItem = listBox.SelectedItem as TimeTableRelation;
        _log.Debug("Selected item: " + (selectedItem != null ? selectedItem.TimeTable.Name : "null"));

        // Call your desired function or logic
        if (selectedItem != null)
        {
          if (!string.IsNullOrEmpty(selectedItem.Begruessungsdatei))
          {
            //DataManager.Instance.webview.Source = new Uri(selectedItem.Begruessungsdatei);
            _log.Debug("Selected timetable relation has a greeting file - setting web source to " + selectedItem.Begruessungsdatei);
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
      _log.Debug("Selected timetable relation changed (version 2) - updating views and related data based on new selection");
      // Ensure the sender is a ListBox
      if (sender is ListBox listBox)
      {
        // Get the selected item
        TimeTableRelation? selectedItem = listBox.SelectedItem as TimeTableRelation;
        _log.Debug("Selected item: " + (selectedItem != null ? selectedItem.TimeTable.Name : "null"));

        // Call your desired function or logic
        if (selectedItem != null)
        {
          if (!string.IsNullOrEmpty(selectedItem.Begruessungsdatei))
          {
            //DataManager.Instance.webview.Source = new Uri(selectedItem.Begruessungsdatei);
            string begruessungsdatei_filename = selectedItem.Begruessungsdatei;
            _log.Debug("Selected timetable relation has a greeting file - original filename: " + begruessungsdatei_filename);
            if (begruessungsdatei_filename.StartsWith("file:///"))
            {
              DataPathType dpt = DataPathType.Unknown;
              string bdf_relativefilename = Zusi.GetRelativePathOf(begruessungsdatei_filename.Substring("file:///".Length).Replace("/", "\\"), ref dpt);
              dpt = DataPathType.DataDir;
              string bdf_absolute_filename = Zusi.GetAbsolutePathOf(bdf_relativefilename, ref dpt);
              if (System.IO.File.Exists(bdf_absolute_filename))
                begruessungsdatei_filename = "file:///" + bdf_absolute_filename.Replace("\\", "/");
              else
              {
                dpt = DataPathType.Official;
                bdf_absolute_filename = Zusi.GetAbsolutePathOf(bdf_relativefilename, ref dpt);
                if (System.IO.File.Exists(bdf_absolute_filename))
                  begruessungsdatei_filename = "file:///" + bdf_absolute_filename.Replace("\\", "/"); ;
              }
            }

            DataManager.Instance.set_websource(DataManager.Instance.tab_title_docu, begruessungsdatei_filename);
          }
          DataManager.Instance.LaNumberList.Clear();
          //if (false) //_activetab == 2)
          //{
          //  System.Windows.Point point = selectedItem.TimeTable.Utm.ToLatLon();
          //  string zsk_url = DataManager.Instance.options.ZSK_Url + "#" + point.ToString(System.Globalization.CultureInfo.InvariantCulture) + "/10";
          //  zsk_url = zsk_url.Replace(",", "/");
          //  ReloadPageWithDummyNavigation(zsk_url);
          //}

          if (FeatureManager.feature_enabled(FeatureManager.Features.Tracking))
          {
            //Log.Debug("Fahrpult_DataProcessor - UTM" + selectedItem.TimeTable.Utm.ToString());
            _log.Debug("Selected timetable relation has tracking feature enabled - updating marker position based on timetable UTM coordinates");
            System.Windows.Point point = selectedItem.TimeTable.Utm.ToLatLon();
            UpdateMarkerPosition(point.Y, point.X);
            _log.Debug("Updated marker position to " + point.Y.ToString() + "," + point.X.ToString());
            //UpdateRailwayOverlay(point.Y, point.X);
            DataManager.Instance.used_streckenmodule = [];
            DataManager.Instance.routeGraphOpenFile = selectedItem.TimeTable.GetDocument().Filename;
            UpdateRouteGraph2(selectedItem.TimeTable.GetDocument().Filename);

          }
        }
        //listBox.UnselectAll();
        //listBox.UpdateLayout();
      }
    }

    public async void UpdateRouteGraph2(string filename)
    {
      _log.Debug("Updating route graph - attempting to update route graph view based on selected timetable's document filename: " + filename);
      try
      {
        TabViewModel? zdbTab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_routegraph);
        if (zdbTab.RouteGraphContent != null)
        {
          TabViewModel tabvm = InfoTabControl.SelectedValue as TabViewModel;
          string currenttabtitle = tabvm.Title;
          if (currenttabtitle == DataManager.Instance.tab_title_routegraph) // only execute when routegraph tab is active
          {

            RouteGraph2Control routeGraph2Control = zdbTab.RouteGraphContent as RouteGraph2Control;
            //routeGraph2Control.Set_RouteGraphVisibility(Visibility.Visible);
            routeGraph2Control.ModulOeffnen([filename]);
            //routeGraph2Control.Set_RouteGraphVisibility(Visibility.Collapsed);
            DataManager.Instance.routeGraphOpenFile = "";
          }

        }
      }
      catch (Exception ex)
      {
        LogHelper.LogException(ex, "Something went wrong updating routegraph");
      }

    }

    public async void UpdateRailwayOverlay(double latitude, double longitude)
    {
      return; ; // temporär deaktiviert
                //TabViewModel? zdbTab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_tracking);
                //var loader = new RailwayPolylineOverlayWpf(zdbTab.gmap);
                //double minLat = latitude - 0.01;
                //double minLong = longitude - 0.01;
                //double maxLat = latitude + 0.01;
                //double maxLong = longitude + 0.01;
                //await loader.LoadRailwayOverlayAsync(minLat, minLong, maxLat, maxLong);
    }

    //---------------------------------------------------------------------
    private void FoundTimeTableTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      _log.Debug("Selected found timetable changed - updating current found timetable and related views based on new selection");
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
      _log.Debug("Selected found train changed - updating current found train and related views based on new selection");
      TrainsViewModel? tvm = e.NewValue as TrainsViewModel;
      DataManager.Instance.CurrentTrain = tvm?.Object;
    }

    //---------------------------------------------------------------------
    private void TbxTrainNumber_GotFocus(object sender, RoutedEventArgs e)
    {
      //tblkMessage.Visibility = Visibility.Hidden;
    }

    //---------------------------------------------------------------------
    private void TbxTrainNumber_TextChanged(object sender, EventArgs e)
    {
      //tblkMessage.Visibility = Visibility.Hidden;
    }

    //---------------------------------------------------------------------
    private void TbxFilterTrainNumber_TextChanged(object sender, EventArgs e)
    {
      _log.Debug("Train number filter text changed - updating train filter based on new text");
      //tblkMessage.Visibility = Visibility.Hidden;
      ZusiStart.Controls.SearchTextBox searchtextbox = sender as ZusiStart.Controls.SearchTextBox;

      string searchtext = searchtextbox.Text;
      _log.Debug("Train number filter text: " + searchtext);

      if (searchtext != DataManager.Instance.FilterPlatzhalterText && searchtext != "")
      {
        DataManager.Instance.FilterZugNummer = searchtext.Trim();
        if (GrpTrains != null)
          GrpTrains.BorderBrush = System.Windows.Media.Brushes.Red;
        DataManager_RefreshFilter(sender, e);
      }
      else
      {
        DataManager.Instance.FilterZugNummer = "";
        DataManager.Instance.FoundTimeTables.Clear();
        DataManager.FoundTimeTableIds = null;
        if (GrpTrains != null)
          GrpTrains.BorderBrush = System.Windows.Media.Brushes.White;
        DataManager_RefreshFilter(sender, e);
      }


    }
    //---------------------------------------------------------------------
    private void VersionPanel_MouseEnter(object sender, MouseEventArgs e)
    {
      _log.Debug("Mouse entered version panel - showing about dialog if main border is visible");
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
      _log.Debug("Selected LA number changed - updating related views and data based on new selection");
      string lanr = e.ToString();
      if (sender is ListBox listBox)
      {

        string? selectedItem = listBox.SelectedItem as string;

        SetLaNummer(selectedItem);
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      _log.Debug("Window loaded - performing initial setup and loading of data");
      ZusiStart.Controls.SearchTextBox searchtextbox = sender as ZusiStart.Controls.SearchTextBox;
      searchtextbox.Text = DataManager.Instance.FilterPlatzhalterText;
      searchtextbox.Foreground = System.Windows.Media.Brushes.Gray;
    }

    private void TbxFilterSearchtxt_GotFocus(object sender, RoutedEventArgs e)
    {
      ZusiStart.Controls.SearchTextBox searchtextbox = sender as ZusiStart.Controls.SearchTextBox;
      if (searchtextbox.Text == DataManager.Instance.FilterPlatzhalterText)
      {
        searchtextbox.Text = "";
        searchtextbox.Foreground = System.Windows.Media.Brushes.Black;
      }
    }

    private void TbxFilterSearchtxt_LostFocus(object sender, RoutedEventArgs e)
    {
      ZusiStart.Controls.SearchTextBox searchtextbox = sender as ZusiStart.Controls.SearchTextBox;
      if (string.IsNullOrWhiteSpace(searchtextbox.Text))
      {
        searchtextbox.Text = DataManager.Instance.FilterPlatzhalterText;
        searchtextbox.Foreground = System.Windows.Media.Brushes.Gray;
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public string getFilterSearchtext(ZusiStart.Controls.SearchTextBox searchtextbox)
    {
      if (searchtextbox.Text == DataManager.Instance.FilterPlatzhalterText)
      {
        return "";
      }
      return searchtextbox.Text;
    }

    private void GoBackButton_Click(object sender, RoutedEventArgs e)
    {

      _log.Debug("Go back button clicked - navigating back in web view history if possible");
      var Tab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_Zusi_DB);
      if (Tab != null)
        if (Tab.WebViewInstance.CanGoBack)
        {
          Tab.WebViewInstance.GoBack();
        }
    }

    private void GoForwardButton_Click(object sender, RoutedEventArgs e)
    {
      _log.Debug("Go forward button clicked - navigating forward in web view history if possible");
      var Tab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_Zusi_DB);
      if (Tab != null)
        if (Tab.WebViewInstance.CanGoForward)
        {
          Tab.WebViewInstance.GoForward();
        }
    }

  }
}