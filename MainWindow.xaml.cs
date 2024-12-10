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

namespace ZusiStart
{
  /// <summary>
  /// Interaktionslogik für MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, IDisposable
  {
    #region private fields

    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private readonly HttpMiniServer _miniServer = new();
    private DataLoaderWindow _dataLoaderWindow;
    //private readonly Simulator _simulator = new Simulator();
    private readonly Fahrpult _fahrpult = new();

    public static readonly RoutedUICommand CommandAbout = new RoutedUICommand("Über _ZusiStart", nameof(CommandAbout), typeof(MainWindow));
    public static readonly RoutedUICommand CommandHelp = new RoutedUICommand("_Dokumentation", nameof(CommandHelp), typeof(MainWindow));
    public static readonly RoutedUICommand CommandExtDocu = new RoutedUICommand("Dokumentation mit _externem Programm öffnen", nameof(CommandExtDocu), typeof(MainWindow));
    public static readonly RoutedUICommand CommandOptions = new RoutedUICommand("Options", nameof(CommandOptions), typeof(MainWindow));

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

    #endregion

    #region commands

    public static readonly RoutedUICommand MinimizeCommand = new("", "MinimizeCommand", typeof(MainWindow));
    public static readonly RoutedUICommand ExitGameCommand = new("Zusi•Zugauswahl beenden", "ExitGameCommand", typeof(MainWindow));
    public static readonly RoutedUICommand StartTrainCommand = new("Ausgewählten Zug fahren", "StartTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand TimeTablesPageCommand = new("Fahrplanauswahl", "TimeTablesPageCommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchPageCommand = new("Zug suchen", "SearchPageCommand", typeof(MainWindow));
    public static readonly RoutedUICommand RecentTrainsPageCommand = new("Lieblingszüge", "RecentTrainsPageCommand", typeof(MainWindow));
    public static readonly RoutedUICommand SearchTrainCommand = new("Zug suchen", "SearchTrainCommand", typeof(MainWindow));
    public static readonly RoutedUICommand TrainStartSettingsCommand = new("Einstellungen", "TrainStartSettingsCommand", typeof(MainWindow));
    public static readonly RoutedUICommand FrictionSettingsCommand = new("Gleisbedingungen", "FrictionSettingsCommand", typeof(MainWindow));
    //--
    public static readonly RoutedUICommand ResetDataCommand = new("ResetData", "ResetDataCommand", typeof(MainWindow), new InputGestureCollection(new KeyGesture[] { new KeyGesture(Key.F12) }));

    #endregion

    private async void LoadLocalHtml()
    {
      string filePath = @"C:\path\to\your\file.html"; // Update with your file path
      string userDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\ZusiStart\WebView2";
      var options = new CoreWebView2EnvironmentOptions();

      try
      {
        // Await the CreateAsync method to get the CoreWebView2Environment instance
        var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);

        // Ensure CoreWebView2 is initialized with the environment
        await webView.EnsureCoreWebView2Async(environment);

        // Handle the initialization completed event
        webView.CoreWebView2InitializationCompleted += (sender, args) =>
        {
          if (args.IsSuccess)
          {
            webView.Source = new Uri($"file:///{filePath.Replace("\\", "/")}");
          }
          else
          {
            MessageBox.Show($"WebView2 initialization failed: {args.InitializationException}");
          }
        };
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

      EngageScaling();

      LoadWindowSettings();

      Background = Application.Current.TryFindResource(string.Format("bkgnd{0}", DateTime.Now.Second & 3)) as Brush;

      Loaded += MainWindow_Loaded;
      SortModeChanged += MainWindow_SortModeChanged;

      ZusiSim.Terminated += ZusiSim_Terminated;

      CommandBindings.Add(new CommandBinding(MinimizeCommand, OnMinimize, OnCanMinimize));
      CommandBindings.Add(new CommandBinding(ExitGameCommand, OnExitGame));
      CommandBindings.Add(new CommandBinding(StartTrainCommand, OnStartTrain, OnCanStartTrain));
      CommandBindings.Add(new CommandBinding(TimeTablesPageCommand, OnTimeTablePage));
      CommandBindings.Add(new CommandBinding(SearchPageCommand, OnSearchPage));
      CommandBindings.Add(new CommandBinding(RecentTrainsPageCommand, OnRecentTrainsPage, OnCanRecentTrainsPage));
      CommandBindings.Add(new CommandBinding(SearchTrainCommand, OnSearchTrain, OnCanSearchTrain));
      CommandBindings.Add(new CommandBinding(TrainStartSettingsCommand, OnTrainStartSettings));
      CommandBindings.Add(new CommandBinding(FrictionSettingsCommand, OnFrictionSettings, OnCanFrictionSettings));
      CommandBindings.Add(new CommandBinding(CommandAbout, OnAbout));
      CommandBindings.Add(new CommandBinding(CommandHelp, OnHelp));
      CommandBindings.Add(new CommandBinding(CommandExtDocu, OnExtDocu));
      CommandBindings.Add(new CommandBinding(CommandOptions, OnTrainStartSettings));
      //--
      CommandBindings.Add(new CommandBinding(ResetDataCommand, OnResetData, (s, e) => e.CanExecute = IsLoaded));

      _miniServer.Run();

      DataManager? dataManager = DataContext as DataManager;
      dataManager.DecoTrainsAllowed += DataManager_DecoTrainsAllowed;
      dataManager.SelectedTimeTableChanged += DataManager_SelectedTimeTableChanged;
      dataManager.NotifyDataLoadStarted += DataManager_NotifyDataLoadStarted;
      dataManager.NotifyDataLoadCompleted += DataManager_NotifyDataLoadCompleted;
      dataManager.webview = webView;

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

    //---------------------------------------------------------------------
    protected override void OnClosing(CancelEventArgs e)
    {
      SaveWindowSettings();
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

    private void OnAbout(object sender, ExecutedRoutedEventArgs e)
    {
      AboutDlg aboutDlg = new AboutDlg();
      aboutDlg.Owner = (Window)this;
      aboutDlg.ShowDialog();
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
      //OptionsDlg optionsDlg = new OptionsDlg(this);
      //optionsDlg.Owner = (Window)this;


      //optionsDlg.OptionsListBox.SelectedValue = _selectedOptions;
      //bool? result = optionsDlg.ShowDialog();
      //if (result == true)
      //{
      //  _selectedOptions = optionsDlg.OptionsListBox.SelectedValue;
      //  Properties.Settings.Default.OptionListSelectedItems = _selectedOptions;
      //  Properties.Settings.Default.Save();
      //  ReadOptions();
      //  DataManager.Instance.RefreshLayouts();
      //}
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
      _dataLoaderWindow.ShowDialog();

      VersionVisibility = Visibility.Visible;
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
    private void OnCanRecentTrainsPage(object sender, CanExecuteRoutedEventArgs e)
    {
      int n = (DataManager.Instance.RecentTrains?.Count ?? 0);
      e.CanExecute = n > 0;
    }

    //---------------------------------------------------------------------
    private void OnRecentTrainsPage(object sender, ExecutedRoutedEventArgs e)
    {
      DataManager.Instance.CurrentTrain = null;
      DataManager.Instance.SelectedRecentTrain = null;
      MainBorderVisibility = Visibility.Collapsed;
      SearchBorderVisibility = Visibility.Collapsed;
      RecentTrainsBorderVisibility = Visibility.Visible;
    }

    //---------------------------------------------------------------------
    private void OnSearchPage(object sender, ExecutedRoutedEventArgs e)
    {
      DataManager.Instance.CurrentTrain = null;
      DataManager.Instance.SelectedRecentTrain = null;
      MainBorderVisibility = Visibility.Collapsed;
      SearchBorderVisibility = Visibility.Visible;
      RecentTrainsBorderVisibility = Visibility.Collapsed;
    }

    //---------------------------------------------------------------------
    private void OnTimeTablePage(object sender, ExecutedRoutedEventArgs e)
    {
      DataManager.Instance.CurrentTrain = null;
      DataManager.Instance.SelectedRecentTrain = null;
      MainBorderVisibility = Visibility.Visible;
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
    private void OnStartTrain(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {
        HideWindow();
        Zug zug = DataManager.Instance.CurrentTrain ?? DataManager.Instance.SelectedRecentTrain.Train;
        TimeTable timeTable = DataManager.Instance.GetTimeTableOfTrain(zug);
        ZusiDocumentBase? doc = timeTable?.GetDocument();
        // start train
        if (Properties.Settings.Default.TrainStartMode == 0)
        {
          // ... via TCP interface
          TimeTable tt = DataManager.Instance.GetTimeTableOfTrain(zug);
          TrainStartInfo tsi = new() { TimetableFile = tt.GetDocument().Filename, TrainNumber = zug.Nummer };
          _fahrpult.TryStartTrain(tsi);
        }
        else
        {
          // ... commandline
          ZusiSim.Start(zug.GetDocument().Filename);
        }
        // out train into recent used trains
        string timeTableName = System.IO.Path.GetFileNameWithoutExtension(doc.Filename);
        DataManager.Instance.RecentTrains.Add(new RecentTrain(zug, timeTableName));
        DataManager.Instance.RecentTrains.Save();
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
        Properties.Settings.Default.OptimiseScheduleCriteria = dlg.OptimiseScheduleCriteria ? 0 : 1;
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
  }
}
