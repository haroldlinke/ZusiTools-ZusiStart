using log4net;
using Sovoma;
using Sovoma.WPF;
using Sovoma.ControlsKit;
using ZusiFahrpultLib;
using ZusiMeterGaugesLib;
using ZusiPicLib;

using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
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


namespace ZusiStart
{
  /// <summary>
  /// Interaktionslogik für MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, IDisposable
  {
    #region private fields

    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private static string _url_zusi_strecken_karte = "https://www.zusi-sk.eu/";
    private static string _url_zusi_datenbank = "https://www.zusidatenbank.de/?zusistart";
    private static string _zusibildfahrplan_mode = "FPL";


    private readonly HttpMiniServer _miniServer = new();
    private DataLoaderWindow _dataLoaderWindow;
    private WebView_Window _webViewWindow_ZDB;
    private WebView_Window _webViewWindow_ZSK;
    //private readonly Simulator _simulator = new Simulator();
    private readonly Fahrpult _fahrpult = new();

    public static readonly RoutedUICommand CommandAbout = new RoutedUICommand("Über _ZusiStart", nameof(CommandAbout), typeof(MainWindow));
    public static readonly RoutedUICommand CommandHelp = new RoutedUICommand("_Dokumentation", nameof(CommandHelp), typeof(MainWindow));
    public static readonly RoutedUICommand CommandExtDocu = new RoutedUICommand("Dokumentation mit _externem Programm öffnen", nameof(CommandExtDocu), typeof(MainWindow));
    public static readonly RoutedUICommand CommandOptions = new RoutedUICommand("Optionen", nameof(CommandOptions), typeof(MainWindow));
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
    public static readonly RoutedUICommand ExitGameCommand = new("Zusi•Zugauswahl beenden", "ExitGameCommand", typeof(MainWindow));
    public static readonly RoutedUICommand StartTrainCommand = new("Ausgewählten Zug fahren", "StartTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand TimeTablesPageCommand = new("Fahrplanauswahl", "TimeTablesPageCommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchPageCommand = new("Zug suchen", "SearchPageCommand", typeof(MainWindow));
    public static readonly RoutedUICommand FavoriteTrainsPageCommand = new("Zug Favoriten", "FavoriteTrainsPageCommand", typeof(MainWindow));
    public static readonly RoutedUICommand AddFavoriteTrainCommand = new("Fav +", "AddFavoriteTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand DelFavoriteTrainCommand = new("Fav -", "DelFavoriteTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchTrainCommand = new("Zug suchen", "SearchTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchTrainZusiDBCommand = new("ZUSI Datenbank", "SearchTrainZusiDBCommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchTrainZusiSKCommand = new("ZUSI Streckenkarte", "SearchTrainZusiSKCommand", typeof(MainWindow));
    public static readonly RoutedUICommand ZusiBildFahrplanCommand = new("ZUSI Bildfahrplan", "ZusiBildFahrplanCommand", typeof(MainWindow));

    public static readonly RoutedUICommand TrainStartSettingsCommand = new("Einstellungen", "TrainStartSettingsCommand", typeof(MainWindow));
    public static readonly RoutedUICommand FrictionSettingsCommand = new("Gleisbedingungen", "FrictionSettingsCommand", typeof(MainWindow));
    //--
    public static readonly RoutedUICommand ResetDataCommand = new("ResetData", "ResetDataCommand", typeof(MainWindow), new InputGestureCollection(new KeyGesture[] { new KeyGesture(Key.F12) }));

    #endregion

    private async void InitializeWebView2Instances()
    {
      string userDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ZusiStart\WebView2";
      var options = new CoreWebView2EnvironmentOptions();

      // Await the CreateAsync method to get the CoreWebView2Environment instance
      DataManager.Instance.webview_environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);

      // Ensure CoreWebView2 is initialized with the environment
      await webView.EnsureCoreWebView2Async(DataManager.Instance.webview_environment);
      await webView.EnsureCoreWebView2Async(null);
      webView.CoreWebView2.Navigate("https://www.hlinke.de/ZUSItools/zusistart_dummy_page.htmlm");

      await webView_ZDB.EnsureCoreWebView2Async(DataManager.Instance.webview_environment);
      await webView_ZDB.EnsureCoreWebView2Async(null);
      webView_ZDB.CoreWebView2.Navigate("https://www.zusidatenbank.de?zusistart");
      //webView_ZDB.CoreWebView2.Navigate("https://www.zusidatenbank.de");
      webView_ZDB.NavigationCompleted += OnWebView_NavigationCompleted;

      _webViewWindow_ZDB = new WebView_Window
      {
        Owner = this
      };
      await _webViewWindow_ZDB.webViewWin.EnsureCoreWebView2Async(DataManager.Instance.webview_environment);
      await _webViewWindow_ZDB.webViewWin.EnsureCoreWebView2Async(null);
    }

    private async void LoadLocalHtml()
    {
      //string filePath = @"C:\path\to\your\file.html"; // Update with your file path
      //string userDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ZusiStart\WebView2";
      //var options = new CoreWebView2EnvironmentOptions();
      //var environment = CoreWebView2Environment.CreateAsync(null, userDataFolder, options);

      try
      {
        InitializeWebView2Instances();
        //// Await the CreateAsync method to get the CoreWebView2Environment instance
        //var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);

        //// Ensure CoreWebView2 is initialized with the environment
        //await webView.EnsureCoreWebView2Async(environment);

        //// Handle the initialization completed event
        //webView.CoreWebView2InitializationCompleted += (sender, args) =>
        //{
        //  if (args.IsSuccess)
        //  {
        //    webView.Source = new Uri($"file:///{filePath.Replace("\\", "/")}");
        //  }
        //  else
        //  {
        //    MessageBox.Show($"WebView2 initialization failed: {args.InitializationException}");
        //  }
        //};
      }
      catch (Exception ex)
      {
        MessageBox.Show($"WebView2 initialization failed: {ex.Message}");
      }
    }

    //---------------------------------------------------------------------
    public MainWindow()
    {
      InitializeComponent();

      Log.Debug("ZusiStart started - Version:" + AsmInfo.Version.ToString());

      EngageScaling();

      LoadWindowSettings();

      Background = Application.Current.TryFindResource(string.Format("bkgnd{0}", DateTime.Now.Second & 3)) as Brush;

      Loaded += MainWindow_Loaded;
      SortModeChanged += MainWindow_SortModeChanged;

      ZusiSim.Terminated += ZusiSim_Terminated;

      this.Closing += MainWindow_Closing;

      CommandBindings.Add(new CommandBinding(MinimizeCommand, OnMinimize, OnCanMinimize));
      CommandBindings.Add(new CommandBinding(ExitGameCommand, OnExitGame));
      CommandBindings.Add(new CommandBinding(StartTrainCommand, OnStartTrain, OnCanStartTrain));
      CommandBindings.Add(new CommandBinding(TimeTablesPageCommand, OnTimeTablePage));
      CommandBindings.Add(new CommandBinding(SearchPageCommand, OnSearchPage));
      CommandBindings.Add(new CommandBinding(FavoriteTrainsPageCommand, OnFavoriteTrainsPage, OnCanFavoriteTrainsPage));
      CommandBindings.Add(new CommandBinding(AddFavoriteTrainCommand, OnAddFavoriteTrain, OnCanAddFavoriteTrain));
      CommandBindings.Add(new CommandBinding(DelFavoriteTrainCommand, OnDelFavoriteTrain, OnCanDelFavoriteTrain));
      CommandBindings.Add(new CommandBinding(SearchTrainCommand, OnSearchTrain, OnCanSearchTrain));
      CommandBindings.Add(new CommandBinding(SearchTrainZusiDBCommand, OnSearchTrainwithZusiDB));
      CommandBindings.Add(new CommandBinding(SearchTrainZusiSKCommand, OnSearchTrainwithZusiSK, OnCanSearchTrainwithZusiSK));
      CommandBindings.Add(new CommandBinding(ZusiBildFahrplanCommand, OnZusiBildFahrplan, OnCanZusiBildFahrplan));
      CommandBindings.Add(new CommandBinding(TrainStartSettingsCommand, OnTrainStartSettings));
      CommandBindings.Add(new CommandBinding(FrictionSettingsCommand, OnFrictionSettings, OnCanFrictionSettings));
      CommandBindings.Add(new CommandBinding(CommandAbout, OnAbout));
      CommandBindings.Add(new CommandBinding(CommandHelp, OnHelp));
      CommandBindings.Add(new CommandBinding(CommandExtDocu, OnExtDocu));
      CommandBindings.Add(new CommandBinding(CommandOptions, OnOptions));
      CommandBindings.Add(new CommandBinding(CommandCreatePictures, OnCreatePictures));
      //CommandBindings.Add(new CommandBinding(ManageReplLocosCommand, OnManageReplLocos, OnCanManageReplLocos));
      //CommandBindings.Add(new CommandBinding(ReplaceLocoCommand, OnReplaceLoco, OnCanReplaceLoco));
      //CommandBindings.Add(new CommandBinding(UndoReplLocoCommand, OnUndoReplLoco, OnCanUndoReplLoco));
      //--
      CommandBindings.Add(new CommandBinding(ResetDataCommand, OnResetData, (s, e) => e.CanExecute = IsLoaded));

      //_miniServer.Run();

      DataManager? dataManager = DataContext as DataManager;
      dataManager.DecoTrainsAllowed += DataManager_DecoTrainsAllowed;
      dataManager.SelectedTimeTableChanged += DataManager_SelectedTimeTableChanged;
      dataManager.NotifyDataLoadStarted += DataManager_NotifyDataLoadStarted;
      dataManager.NotifyDataLoadCompleted += DataManager_NotifyDataLoadCompleted;
      dataManager.webview = webView;

      dataManager.webview_ZDB = webView_ZDB;
      dataManager.main_window = this;

      //string userDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ZusiStart\WebView2";
      //var options = new CoreWebView2EnvironmentOptions();
      //var environment = CoreWebView2Environment.CreateAsync(null, userDataFolder, options);

      //webView.CoreWebView2InitializationCompleted += (sender, args) =>
      //{
      //  if (!args.IsSuccess)
      //  { 
      //    MessageBox.Show($"WebView2 initialization failed: {args.InitializationException}");
      //    Log.Debug($"WebView2 initialization failed: {args.InitializationException}");
      //  }
      //};
      //dataManager.ProgressChanged += DataManager_ProgressChanged;

      LoadLocalHtml();
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
      if (_webViewWindow_ZDB != null)
      {
        _webViewWindow_ZDB.Close();
      }
      if (_webViewWindow_ZSK != null)
      {
        _webViewWindow_ZSK.Close();
      }
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

    public double GetScreenScaleFactor(Window window)
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
      aboutDlg.Owner = (Window)this;
      aboutDlg.ShowDialog();
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
      CreatePicturesDlg.Owner = (Window)this;
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
        string absolutePath = Path.GetFullPath(relativePath);
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
      optionsDlg.Owner = (Window)this;
      optionsDlg.ShowDialog();
    }

    //---------------------------------------------------------------------
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      Activate();

      _dataLoaderWindow = new DataLoaderWindow
      {
        Owner = this
      };
      DataManager.Instance.InitializeData();
      DataManager.Instance.dataLoaderWindow = _dataLoaderWindow;
      DataManager.Instance.ScreenScaleFactor = GetScreenScaleFactor(this);
      _dataLoaderWindow.ShowDialog();
      get_bildfahrplanpath();
      get_zusidisplaypath();
      get_zusimeterpath();
      VersionVisibility = Visibility.Visible;
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
        string zusiexe_path = Path.GetDirectoryName(Zusi.Executable);
        string zusidisplay_path = Path.Combine(zusiexe_path, @"_Tools\ZusiDisplay\");
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
      CollectionViewSource.GetDefaultView(tvFreightTrains.ItemsSource).Refresh();
      CollectionViewSource.GetDefaultView(tvFoundPassengerTrains.ItemsSource).Refresh();
      CollectionViewSource.GetDefaultView(tvFoundFreightTrains.ItemsSource).Refresh();
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
      FadeOut(rctMask, 600, null);
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
      CollectionViewSource.GetDefaultView(tvFreightTrains.ItemsSource).Refresh();
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
      Zug zug = DataManager.Instance.CurrentTrain ?? DataManager.Instance.SelectedRecentTrain.Train;
      string timeTablefilename = zug.FahrplanDatei.FullPath;

      string timeTableName = System.IO.Path.GetFileNameWithoutExtension(timeTablefilename);
      DataManager.Instance.RecentTrains.Add(new RecentTrain(zug, timeTableName));
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
      Zug zug = DataManager.Instance.CurrentTrain ?? DataManager.Instance.SelectedRecentTrain.Train;
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
      MainBorderVisibility = Visibility.Collapsed;
      MainWebBorderVisibility = Visibility.Collapsed;
      SearchBorderVisibility = Visibility.Visible;
      RecentTrainsBorderVisibility = Visibility.Collapsed;
    }

    //---------------------------------------------------------------------
    private void OnTimeTablePage(object sender, ExecutedRoutedEventArgs e)
    {
      DataManager.Instance.CurrentTrain = null;
      DataManager.Instance.SelectedRecentTrain = null;
      MainBorderVisibility = Visibility.Visible;
      MainWebBorderVisibility = Visibility.Collapsed;
      SearchBorderVisibility = Visibility.Collapsed;
      RecentTrainsBorderVisibility = Visibility.Collapsed;
    }

    //---------------------------------------------------------------------
    private void OnCanStartTrain(object sender, CanExecuteRoutedEventArgs e)
    {
      bool canExecute = Properties.Settings.Default.TrainStartMode != 1 || ZusiSim.CanStart;
      e.CanExecute = canExecute && DataManager.Instance.CurrentTrain != null || DataManager.Instance.SelectedRecentTrain != null;
    }

    //---------------------------------------------------------------------
    public void OnStartTrain(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {
        Zug zug = DataManager.Instance.CurrentTrain ?? DataManager.Instance.SelectedRecentTrain.Train;
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
            HideWindow();
            if (Properties.Settings.Default.TrainStartMode == 0)
            {
              // ... via TCP interface
              TrainStartInfo tsi = new() { TimetableFile = tmpTimeTableFileName, TrainNumber = zug.Nummer };
              _fahrpult.TryStartTrain(tsi);
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
          HideWindow();
          if (Properties.Settings.Default.TrainStartMode == 0)
          {
            // ... via TCP interface
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
        // out train into recent used trains
        //string timeTableName = System.IO.Path.GetFileNameWithoutExtension(doc.Filename);
        //DataManager.Instance.RecentTrains.Add(new RecentTrain(zug, timeTableName));
        //DataManager.Instance.RecentTrains.Save();
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

    private async void StartZusiWebBrowser(string url = "https://www.zusidatenbank.de")
    {
      bool int_window = false;

      if (int_window)
      {
        DataManager.Instance.webview_ZDB.CoreWebView2.SourceChanged += CoreWebView2SourceChanged;
        MainBorderVisibility = Visibility.Collapsed;
        MainWebBorderVisibility = Visibility.Visible;
        SearchBorderVisibility = Visibility.Collapsed;
        RecentTrainsBorderVisibility = Visibility.Collapsed;
        await webView_ZDB.EnsureCoreWebView2Async(null);
        webView_ZDB.CoreWebView2.Navigate(url);
      }
      else
      {
        //MainBorderVisibility = Visibility.Collapsed;
        //MainWebBorderVisibility = Visibility.Collapsed;
        //SearchBorderVisibility = Visibility.Visible;
        //RecentTrainsBorderVisibility = Visibility.Collapsed;

        if (_webViewWindow_ZDB == null || !_webViewWindow_ZDB.IsLoaded)
        {
          _webViewWindow_ZDB = new WebView_Window();
          _webViewWindow_ZDB.set_websource(url);
          _webViewWindow_ZDB.Show();
          await _webViewWindow_ZDB.webViewWin.EnsureCoreWebView2Async(DataManager.Instance.webview_environment);
          
        }
        else
        {
          _webViewWindow_ZDB.Close();
          _webViewWindow_ZDB = new WebView_Window();
          _webViewWindow_ZDB.set_websource(url);
          _webViewWindow_ZDB.Show();
          await _webViewWindow_ZDB.webViewWin.EnsureCoreWebView2Async(DataManager.Instance.webview_environment);
          //_webViewWindow_ZDB.set_websource(url);
          //_webViewWindow_ZDB.Activate();
        }

        //var _webview_window = new WebView_Window();
        //_webViewWindow_ZDB.webViewWin.CoreWebView2.Navigate(url);
       

        //if (_webViewWindow_ZDB.IsLoaded)
        //{
        //  _webViewWindow_ZDB.Show();
        //  _webViewWindow_ZDB.Closed += _webViewWindow_ZDB_Closed;
        //}
      }
    }

    private void _webViewWindow_ZDB_Closed(object sender, EventArgs e)
    {
      //_webViewWindow_ZDB = null;
    }

    private void _webViewWindow_ZSK_Closed(object sender, EventArgs e)
    {
      _webViewWindow_ZSK = null;
    }


    private void OnWebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
      urlTextBlock.Text = webView_ZDB.Source.ToString();
    }

    private void CoreWebView2SourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
    {

      string newSource = webView_ZDB.Source.ToString();
      DataManager.Instance.webview_ZDB_source = newSource;
      urlTextBlock.Text = newSource;
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

    }

    private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
      string url_requested = e.Uri;
      if (url_requested.StartsWith("https://forum.zusi.de/viewtopic.php?f=70&t=17386"))
      {
        e.Handled = true; // Verhindert das Öffnen eines neuen Fensters
        // handle link to "Strecke"
        webView_ZDB.CoreWebView2.Navigate("https://www.zusidatenbank.de/streckenmodule/Routes%5CDeutschland%5C32U_0006_0058%5C000566_005780_Hildesheim_Hbf%5CHildesheim_Hbf_2001.st3");
      }
      if (url_requested.EndsWith(".st3"))
      {
        string train_number = "";
        string[] pathitems = url_requested.Split("%5C");
        if (pathitems != null && pathitems.Count() == 5)
        {
          train_number = pathitems[4];

          bool train_found = ((DataManager)DataContext).SearchTrain(train_number);

          if (train_found)
          {
            e.Handled = true; // Verhindert das Öffnen eines neuen Fensters
            DataManager.Instance.CurrentTrain = null;
            DataManager.Instance.SelectedRecentTrain = null;
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

    }

    private void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
      bool test = true;
    }

    private void OnSearchTrainwithZusiDB(object sender, ExecutedRoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(DataManager.Instance.options.ZDB_Url))
        StartZusiWebBrowser(url: _url_zusi_datenbank);
      else
        StartZusiWebBrowser(url: DataManager.Instance.options.ZDB_Url);
    }

    //---------------------------------------------------------------------
    private void OnCanSearchTrainwithZusiSK(object sender, CanExecuteRoutedEventArgs e)
    {
      bool canExecute = DataManager.Instance.options.Show_ZSK;
      e.CanExecute = canExecute;
    }

    //private (double, double) ConvertUtmToLatLon(double easting, double northing, int zone)
    //{
    //  var utm = ProjectedCoordinateSystem.WGS84_UTM(zone, northing >= 0);
    //  var wgs84 = GeographicCoordinateSystem.WGS84;
    //  var transform = new CoordinateTransformationFactory().CreateFromCoordinateSystems(utm, wgs84);
    //  var point = transform.MathTransform.Transform(new double[] { easting, northing });
    //  return (point[0], point[1]);
    //}

    private void OnSearchTrainwithZusiSK(object sender, ExecutedRoutedEventArgs e)
    {

      string zsk_url = "";
      if (true || DataManager.Instance.Fpn2zsklinkDictionary == null)
      {
        DataManager.Instance.Loadfpn2zsk_link_json();
      }

      if (DataManager.Instance.SelectedTimeTableRelation.TimeTable != null)
      {
        Point point = DataManager.Instance.SelectedTimeTableRelation.TimeTable.Utm.ToLatLon();

        //point.X += 0.33435;
        //point.Y -= 0.1704;

        zsk_url = DataManager.Instance.options.ZSK_Url + "#" + point.ToString() + "/10";
        zsk_url = zsk_url.Replace(";", "/");
        zsk_url = zsk_url.Replace(",", ".");
      }


      //if (DataManager.Instance.Fpn2zsklinkDictionary != null)
      //{
      //  try
      //  {
      //    string timetablename = DataManager.Instance.SelectedTimeTableRelation.TimeTableName;

      //    if (DataManager.Instance.Fpn2zsklinkDictionary.ContainsKey(timetablename))
      //      zsk_url = DataManager.Instance.Fpn2zsklinkDictionary[timetablename];
      //  }
      //  catch
      //  {
      //    zsk_url = "";
      //  }
      //}
      //StartZusiStreckenkarte();
      if (!string.IsNullOrEmpty(zsk_url))
        StartZusiWebBrowser(url: zsk_url);
      else
      {
        if (string.IsNullOrEmpty(DataManager.Instance.options.ZSK_Url))
          StartZusiWebBrowser(url: _url_zusi_strecken_karte);
        else
          StartZusiWebBrowser(url: DataManager.Instance.options.ZSK_Url);
      }
    }

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
          try {
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
      DataManager.Instance.CurrentTrain = tvm?.Object;
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
          File.Delete(RecentTrainsCollection.FileName);
        }
        catch { }
      }

      return res;
    }

    #endregion

    private void ToggleButton_Checked(object sender, RoutedEventArgs e)
    {

    }

    private void GoBackButton_Click(object sender, RoutedEventArgs e)
    {
      // Assuming you have a WebView2 control named 'webView'
      if (webView_ZDB.CanGoBack)
      {
        webView_ZDB.GoBack();
      }
    }
  }
}
