using log4net;
using Microsoft.Win32;
using Sovoma;
using Sovoma.WPF.Converter;
using Sovoma.WPF;
using Sovoma.WPF.Network;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Xml.Linq;
using ZusiFahrpultLib;
//using ZusiKlassenLib2;
using ZusiMeter.Miscellaneous;
using ZusiMeter.Properties;
using ZusiMeter.About;
using ZusiMeter.Options;
using ZusiMeterGaugesLib.Common;
using ZusiMeterGaugesLib.Components;
using ZusiMeterGaugesLib.Controls;
using ZusiMeterGaugesLib.Editors;
using ZusiMeterGaugesLib.DigitalGauges;
using ZusiMeterGaugesLib.GaugeTemplates;
using ZusiMeterGaugesLib.Interfaces;
using ZusiMeterGaugesLib.Managers;
using ZusiMeterGaugesLib.TheRailRunner;
using ZusiMeterGaugesLib.Utils;
using System.Windows.Media;
using ZusiMeter.Data;
using ZusiMeter.Pages;
using Zusisuplib;
using System.Windows.Interop;
using System.Net.Sockets;
//using System.Windows.Forms;
//using System.Windows.Forms;
//using System.Windows.Forms;
using CheckListBox = Xceed.Wpf.Toolkit.CheckListBox;
using System.Runtime.CompilerServices;
using ZusiFahrpultLib.Miscellaneous;
using System.Text;
using System.Reflection.Metadata;
using Microsoft.VisualBasic.Logging;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace ZusiMeter
{
  public partial class ZusiMeterControl : UserControl, IComponentConnector  //**HLI
  {
    private static bool _generateFahrpultDump = false;
    private static readonly ILog _log = LogManager.GetLogger(typeof(ZusiMeterControl));
    private static readonly string? _defaultTitle = AsmInfo.Product;
    private static Brush? _Windowbackground = null;
    private readonly ObservableCollection<string> _layoutFiles = new ObservableCollection<string>();
    private BeaconReceiver _beaconReceiver = new();
    private readonly List<IGauge> _gauges = new();
    private readonly List<ZFtdID> _gaugeIds = new();
    private readonly List<ZProgID> _gaugeProgIds = new();
    private FahrpultClient _fahrpult;
    //private ZusiStart.Connection.Fahrpult _fahrpult;
    private readonly LayoutBackground _background;
    private readonly bool _initialized;
    private bool _mustReconnect;
    private double _colWidth;
    private double _rowHeight;
    private WindowState _oldstate;

    private readonly System.Timers.Timer _timerZusiMelderConf = new System.Timers.Timer(2000.0);
    private readonly System.Timers.Timer _timerGracePeriod = new System.Timers.Timer(3000.0);
    private bool _willIPBoardSwitchVisible;
    private bool _beaconDetected;
    private readonly ConcurrentQueue<EventArgs> _dataQueue = new ConcurrentQueue<EventArgs>();
    private readonly AutoResetEvent _dataWakeUp = new AutoResetEvent(false);
    private readonly bool _dataCancellation;
    private bool _trackWindow;
    private string? _curentlayoutfile = null;
    private static string? _selectedOptions = null;
    private static string? _currentlayoutfolder = null;
    private static string? _currentexamplelayoutfolder = null;
    private static readonly DependencyPropertyKey _keyCanActivate = DependencyProperty.RegisterReadOnly(nameof(CanActivate), typeof(bool), typeof(ZusiMeterControl), new PropertyMetadata((object)true));
    public static readonly DependencyProperty CanActivateProperty = ZusiMeterControl._keyCanActivate.DependencyProperty;
    public static readonly DependencyProperty HostProperty = DependencyProperty.Register(nameof(Host), typeof(string), typeof(ZusiMeterControl), new PropertyMetadata((object)null, new PropertyChangedCallback(ZusiMeterControl.OnHostChanged)));
    private static readonly DependencyPropertyKey _keyIsIPBoardSwitchVisible = DependencyProperty.RegisterReadOnly(nameof(IsIPBoardSwitchVisible), typeof(bool), typeof(ZusiMeterControl), new PropertyMetadata((object)false));
    public static readonly DependencyProperty IsIPBoardSwitchVisibleProperty = ZusiMeterControl._keyIsIPBoardSwitchVisible.DependencyProperty;
    public static readonly DependencyProperty IsZusiMelderConfVisibleProperty = DependencyProperty.Register(nameof(IsZusiMelderConfVisible), typeof(bool), typeof(ZusiMeterControl), new PropertyMetadata((object)false));
    private static readonly DependencyPropertyKey _missingConfigurationKey = DependencyProperty.RegisterReadOnly(nameof(MissingConfiguration), typeof(bool), typeof(ZusiMeterControl), new PropertyMetadata((object)false));
    public static readonly DependencyProperty MissingConfigurationProperty = ZusiMeterControl._missingConfigurationKey.DependencyProperty;
    public static readonly DependencyProperty PortProperty = DependencyProperty.Register(nameof(Port), typeof(int), typeof(ZusiMeterControl), new PropertyMetadata((object)0, new PropertyChangedCallback(ZusiMeterControl.OnPortChanged)));
    private static readonly DependencyPropertyKey _selectLayoutKey = DependencyProperty.RegisterReadOnly(nameof(SelectLayout), typeof(bool), typeof(ZusiMeterControl), new PropertyMetadata((object)true));
    public static readonly DependencyProperty SelectLayoutProperty = ZusiMeterControl._selectLayoutKey.DependencyProperty;
    private static readonly DependencyPropertyKey _showMainMenuKey = DependencyProperty.RegisterReadOnly(nameof(ShowMainMenu), typeof(bool), typeof(ZusiMeterControl), new PropertyMetadata((object)true));
    public static readonly DependencyProperty ShowMainMenuProperty = ZusiMeterControl._showMainMenuKey.DependencyProperty;
    private static readonly DependencyPropertyKey _showStatusBarKey = DependencyProperty.RegisterReadOnly(nameof(ShowStatusBar), typeof(bool), typeof(ZusiMeterControl), new PropertyMetadata((object)true));
    public static readonly DependencyProperty ShowStatusBarProperty = ZusiMeterControl._showStatusBarKey.DependencyProperty;
    private static readonly DependencyPropertyKey _showNoMoveCloseIconsKey = DependencyProperty.RegisterReadOnly(nameof(ShowNoMoveCloseIcons), typeof(bool), typeof(ZusiMeterControl), new PropertyMetadata((object)true));
    public static readonly DependencyProperty ShowNoMoveCloseIconsProperty = ZusiMeterControl._showNoMoveCloseIconsKey.DependencyProperty;
    private static readonly DependencyPropertyKey _zoomKey = DependencyProperty.RegisterReadOnly(nameof(Zoom), typeof(double), typeof(ZusiMeterControl), new PropertyMetadata((object)1.0));
    public static readonly DependencyProperty ZoomProperty = ZusiMeterControl._zoomKey.DependencyProperty;
    public static readonly DependencyProperty ZusiConfigurationProperty = DependencyProperty.Register(nameof(ZusiConfiguration), typeof(ZusiConfigurationMode), typeof(ZusiMeterControl), new PropertyMetadata((object)(ZusiConfigurationMode)Settings.Default.ZusiConfiguration, new PropertyChangedCallback(OnZusiConfigurationChanged)));
    private static readonly DependencyPropertyKey _keyZusiConnectionState = DependencyProperty.RegisterReadOnly(nameof(ZusiConnectionState), typeof(int), typeof(ZusiMeterControl), new PropertyMetadata((object)0, (PropertyChangedCallback)((d, e) => CommandManager.InvalidateRequerySuggested())));
    public static readonly DependencyProperty ZusiConnectionStateProperty = ZusiMeterControl._keyZusiConnectionState.DependencyProperty;
    private static readonly DependencyPropertyKey _keyZusiConnectionString = DependencyProperty.RegisterReadOnly(nameof(ZusiConnectionString), typeof(string), typeof(ZusiMeterControl), new PropertyMetadata((PropertyChangedCallback)null));

    public static readonly DependencyProperty ZusiConnectionStringProperty = ZusiMeterControl._keyZusiConnectionString.DependencyProperty;

    public static readonly DependencyProperty PrivateLayoutFolderProperty = DependencyProperty.Register(nameof(PrivateLayoutFolder), typeof(string), typeof(ZusiMeterControl), new PropertyMetadata((object)"", new PropertyChangedCallback(OnPrivateLayoutFolderChanged)));
    public static readonly DependencyProperty ExampleLayoutFolderProperty = DependencyProperty.Register(nameof(ExampleLayoutFolder), typeof(string), typeof(ZusiMeterControl), new PropertyMetadata((object)"", new PropertyChangedCallback(OnExampleLayoutFolderChanged)));

    public static readonly RoutedUICommand CommandLoadLayout = new RoutedUICommand("Aktuelles _Layout anzeigen", nameof(CommandLoadLayout), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandLoadOtherLayout = new RoutedUICommand("_Anderes Layout anzeigen", nameof(CommandLoadOtherLayout), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandQuit = new RoutedUICommand("_Beenden", nameof(CommandQuit), typeof(ZusiMeterControl), CommandKey.Alt_F(Key.F4));
    public static readonly RoutedUICommand CommandAbout = new RoutedUICommand("Über _ZusiMeter", nameof(CommandAbout), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandHelp = new RoutedUICommand("_Dokumentation", nameof(CommandHelp), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandExtDocu = new RoutedUICommand("Dokumentation mit _externem Programm öffnen", nameof(CommandExtDocu), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandOptions = new RoutedUICommand("Options", nameof(CommandOptions), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandWillNewLayout = new RoutedUICommand("_zur Layoutauswahl", nameof(CommandWillNewLayout), typeof(ZusiMeterControl), new InputGestureCollection((IList)new InputGesture[1]
        {
        (InputGesture) new KeyGesture(Key.N, ModifierKeys.Control)
        }));
    public static readonly RoutedUICommand CommandNewGraphicLayout = new RoutedUICommand("Neues _Grafiklayout anlegen", nameof(CommandNewGraphicLayout), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandNewTextLayout = new RoutedUICommand("Neues _Textlayout anlegen", nameof(CommandNewTextLayout), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandOpenLayout = new RoutedUICommand("Anderes Layout bearbeiten", nameof(CommandOpenLayout), typeof(ZusiMeterControl), new InputGestureCollection((IList)new InputGesture[1]
        {
    (InputGesture) new KeyGesture(Key.O, ModifierKeys.Control)
        }));
    public static readonly RoutedUICommand CommandLoadLayoutEdit = new RoutedUICommand("Aktuelles Layout _bearbeiten", nameof(CommandLoadLayoutEdit), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandAppHelper = new RoutedUICommand("_", nameof(CommandAppHelper), typeof(ZusiMeterControl), new InputGestureCollection((IList)new InputGesture[1]
    {
      (InputGesture) new KeyGesture(Key.Z, ModifierKeys.Control | ModifierKeys.Shift)
    }));
    public static readonly RoutedUICommand CommandPause = new RoutedUICommand("", nameof(CommandPause), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandTimejump = new RoutedUICommand("", nameof(CommandTimejump), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandTimelapse = new RoutedUICommand("", nameof(CommandTimelapse), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandBack = new RoutedUICommand("Zurück zur Layoutauswahl", nameof(CommandBack), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandExitApp = new RoutedUICommand("", nameof(CommandExitApp), typeof(ZusiMeterControl), CommandKey.Alt_F(Key.F4));
    public static readonly RoutedUICommand CommandMinimizeApp = new RoutedUICommand("", nameof(CommandMinimizeApp), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandSendToAutoStart = new RoutedUICommand("Layout zu ZUSI-Autostart hinzufügen", nameof(CommandSendToAutoStart), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandFullScreen = new RoutedUICommand("Anzeige Ohne Rand", nameof(CommandFullScreen), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandOpenIPConnConf = new RoutedUICommand("TCP/IP Verbindung einstellen", nameof(CommandOpenIPConnConf), typeof(ZusiMeterControl));
    public static readonly RoutedUICommand CommandEditFullScreen = new RoutedUICommand("EDitor in FullScreenMode", nameof(CommandEditFullScreen), typeof(ZusiMeterControl));

    // public ObservableCollection<string> OptionItems { get; }



    public ObservableCollection<OptionItem> OptionItems
    {
      get;
      set;
    }

    public class OptionItem
    {
      public string Key
      {
        get;
        set;
      }
      public string Text
      {
        get;
        set;
      }
    }

    public bool CanActivate
    {
      get => (bool)this.GetValue(ZusiMeterControl.CanActivateProperty);
      private set => this.SetValue(ZusiMeterControl._keyCanActivate, (object)value);
    }

    public string Host
    {
      get => (string)this.GetValue(ZusiMeterControl.HostProperty);
      set => this.SetValue(ZusiMeterControl.HostProperty, (object)value);
    }

    public bool IsIPBoardSwitchVisible
    {
      get => (bool)this.GetValue(ZusiMeterControl.IsIPBoardSwitchVisibleProperty);
      private set => this.SetValue(ZusiMeterControl._keyIsIPBoardSwitchVisible, (object)value);
    }

    public bool IsZusiMelderConfVisible
    {
      get => (bool)this.GetValue(ZusiMeterControl.IsZusiMelderConfVisibleProperty);
      set => this.SetValue(ZusiMeterControl.IsZusiMelderConfVisibleProperty, (object)value);
    }

    public bool MissingConfiguration
    {
      get => (bool)this.GetValue(ZusiMeterControl.MissingConfigurationProperty);
      private set => this.SetValue(ZusiMeterControl._missingConfigurationKey, (object)value);
    }

    public int Port
    {
      get => (int)this.GetValue(ZusiMeterControl.PortProperty);
      set => this.SetValue(ZusiMeterControl.PortProperty, (object)value);
    }

    public string PrivateLayoutFolder
    {
      get => (string)this.GetValue(ZusiMeterControl.PrivateLayoutFolderProperty);
      set => this.SetValue(ZusiMeterControl.PrivateLayoutFolderProperty, (object)value);
    }

    public string ExampleLayoutFolder
    {
      get => (string)this.GetValue(ZusiMeterControl.ExampleLayoutFolderProperty);
      set => this.SetValue(ZusiMeterControl.ExampleLayoutFolderProperty, (object)value);
    }

    public static string GetCurrentLayoutFolder()
    {
      return _currentlayoutfolder;
    }

    public static string GetCurrentExampleLayoutFolder()
    {
      return _currentexamplelayoutfolder;
    }

    public bool ShowMainMenu
    {
      get => (bool)this.GetValue(ZusiMeterControl.ShowMainMenuProperty);
      private set => this.SetValue(ZusiMeterControl._showMainMenuKey, (object)value);
    }

    public bool ShowStatusBar
    {
      get => (bool)this.GetValue(ZusiMeterControl.ShowStatusBarProperty);
      private set => this.SetValue(ZusiMeterControl._showStatusBarKey, (object)value);
    }

    public bool ShowNoMoveCloseIcons
    {
      get => (bool)this.GetValue(ZusiMeterControl.ShowNoMoveCloseIconsProperty);
      private set => this.SetValue(ZusiMeterControl._showNoMoveCloseIconsKey, (object)value);
    }

    public bool SelectLayout
    {
      get => (bool)this.GetValue(ZusiMeterControl.SelectLayoutProperty);
      private set => this.SetValue(ZusiMeterControl._selectLayoutKey, (object)value);
    }

    public double Zoom
    {
      get => (double)this.GetValue(ZusiMeterControl.ZoomProperty);
      private set => this.SetValue(ZusiMeterControl._zoomKey, (object)value);
    }

    public ZusiConfigurationMode ZusiConfiguration
    {
      get => (ZusiConfigurationMode)this.GetValue(ZusiMeterControl.ZusiConfigurationProperty);
      set => this.SetValue(ZusiMeterControl.ZusiConfigurationProperty, (object)value);
    }

    public int ZusiConnectionState
    {
      get => (int)this.GetValue(ZusiMeterControl.ZusiConnectionStateProperty);
      private set => this.SetValue(ZusiMeterControl._keyZusiConnectionState, (object)value);
    }

    public string ZusiConnectionString
    {
      get => (string)this.GetValue(ZusiMeterControl.ZusiConnectionStringProperty);
      private set => this.SetValue(ZusiMeterControl._keyZusiConnectionString, (object)value);
    }

    public ObservableCollection<string> LayoutFiles => this._layoutFiles;

    public LayoutBackground LayoutBackground => this._background;

    public static string GetZusiMeterLayoutFileDir()
    {
      if (string.IsNullOrEmpty(_currentlayoutfolder))
      {
        string folderpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "ZusiMeterLayouts");

        if (!Directory.Exists(folderpath))
        {
          string zusifolderpath = Zusi.DataPath[2]; // public data zusi folder
          folderpath = Path.Combine(zusifolderpath, "_Tools\\ZusiMeter\\ZusiMeterLayouts");

          if (!Directory.Exists(folderpath))
          {
            try
            {
              Directory.CreateDirectory(folderpath);
            }
            catch
            {
              folderpath = "";
            }
            if (folderpath == "" || !Directory.Exists(folderpath))
            { // Zusi Public folder does not exist ordireectory cannot be created. Create ZusiMeterLayouts folder in Personal documnets folder 
              folderpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "ZusiMeterLayouts");
              Directory.CreateDirectory(folderpath);
            }
          }
        }
        _currentlayoutfolder = folderpath;
      }
      return _currentlayoutfolder;
    }

    public static string GetZusiMeterExampleFileDir()
    {
      if (string.IsNullOrEmpty(_currentexamplelayoutfolder))
      {
        string? executablePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);

        if (executablePath != null)
        {
          string folderpath = Path.Combine(executablePath, "ZusiMeterExampleLayouts");
          if (!Directory.Exists(folderpath))
          {
            Directory.CreateDirectory(folderpath);

          }
          _currentexamplelayoutfolder = folderpath;
        }
        else
        {
          _currentexamplelayoutfolder = "";
        }

      }
      return _currentexamplelayoutfolder;
    }

    private static readonly string _defaultSavePrompt = "Soll das aktuelle Layout schnell noch gespeichert werden?";
    private static readonly string _defaultSavePrompt2 = "Das aktuelle Layout ist leer. Soll dieses Layout gelöscht werden?";
    //private static string? _currentlayoutfolder = null;

    public event RoutedEventHandler DefaultDialBackgroundSettingsChanged
    {
      add
      {
        this.AddHandler(BrushEditor.DefaultDialBackgroundSettingsChangedEvent, (Delegate)value);
      }
      remove
      {
        this.RemoveHandler(BrushEditor.DefaultDialBackgroundSettingsChangedEvent, (Delegate)value);
      }
    }

    public event RoutedEventHandler DefaultTextBackgroundSettingsChanged
    {
      add
      {
        this.AddHandler(BrushEditor.DefaultTextBackgroundSettingsChangedEvent, (Delegate)value);
      }
      remove
      {
        this.RemoveHandler(BrushEditor.DefaultTextBackgroundSettingsChangedEvent, (Delegate)value);
      }
    }


    static ZusiMeterControl()
    {
      //Window.LeftProperty.AddOwner(typeof(ZusiMeterControl), (PropertyMetadata)new FrameworkPropertyMetadata((object)double.NaN, new PropertyChangedCallback(ZusiMeterControl.OnLeftChanged)));
      //Window.TopProperty.AddOwner(typeof(ZusiMeterControl), (PropertyMetadata)new FrameworkPropertyMetadata((object)double.NaN, new PropertyChangedCallback(ZusiMeterControl.OnTopChanged)));
      //Window.TopmostProperty.AddOwner(typeof(ZusiMeterControl), (PropertyMetadata)new FrameworkPropertyMetadata((object)false, new PropertyChangedCallback(ZusiMeterControl.OnTopMostChanged)));
    }

    public ZusiMeterControl()
    {

      this.DataContext = this;

      //OptionItems = new ObservableCollection<string> { "First", "Second", "Third" };
      //if (Settings.Default.VintageBackground) // **HLI
      //  BackgroundSettings.SetVintageBackground(); // **HLI
      this._background = new LayoutBackground();
      this.OptionItems = new ObservableCollection<OptionItem>()
      {
        new OptionItem() { Key = "DoNotShowExamples", Text = "Zeige Beispiellayouts nicht an" },
        new OptionItem() { Key = "Modeframeless", Text = "Layoutanzeige immer ohne Rahmen/Kopfzeile, mit transparentem Hintergrund" },
        new OptionItem() { Key = "ShowNoMoveCloseIcons", Text = "Layoutanzeige ohne Rahmen: Verstecke Verschiebe- und Schließenicon" },
        new OptionItem() { Key = "EditorExtendedMode", Text = "Layouteditor: Erweiterungsmodus einschalten" },
      };
      this.ReadOptions();

      this.ObtainLayoutFiles();
      this.InitializeComponent();

      this._initialized = true;
      this._timerZusiMelderConf.AutoReset = false;
      this._timerZusiMelderConf.Elapsed += new ElapsedEventHandler(this.TimerZusiMelderConf_Elapsed);
      this._timerGracePeriod.AutoReset = false;
      this._timerGracePeriod.Elapsed += new ElapsedEventHandler(this.TimerGracePeriod_Elapsed);

      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandNewGraphicLayout, new ExecutedRoutedEventHandler(this.OnNewGraphicLayout)));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandNewTextLayout, new ExecutedRoutedEventHandler(this.OnNewTextLayout)));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandLoadLayoutEdit, new ExecutedRoutedEventHandler(this.OnLoadLayoutEdit), new CanExecuteRoutedEventHandler(this.OnCanLoadLayout)));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandOpenLayout, new ExecutedRoutedEventHandler(this.OnOpenLayout)));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandLoadLayout, new ExecutedRoutedEventHandler(this.OnLoadLayout), new CanExecuteRoutedEventHandler(this.OnCanLoadLayout)));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandLoadOtherLayout, new ExecutedRoutedEventHandler(this.OnLoadOtherLayout)));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandAppHelper, new ExecutedRoutedEventHandler(this.OnAppHelper)));
      //this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandPause, new ExecutedRoutedEventHandler(this.OnPause), (CanExecuteRoutedEventHandler)((s, e) => e.CanExecute = this.ZusiConnectionState == 2)));
      //this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandTimejump, new ExecutedRoutedEventHandler(this.OnTimejump), (CanExecuteRoutedEventHandler)((s, e) => e.CanExecute = this.ZusiConnectionState == 2)));
      //this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandTimelapse, new ExecutedRoutedEventHandler(this.OnTimelapse), (CanExecuteRoutedEventHandler)((s, e) => e.CanExecute = this.ZusiConnectionState == 2)));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandBack, new ExecutedRoutedEventHandler(this.OnBack), (CanExecuteRoutedEventHandler)((s, e) => e.CanExecute = !this.SelectLayout)));
      //this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandExitApp, (ExecutedRoutedEventHandler)((s, e) => this.Close())));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandMinimizeApp, new ExecutedRoutedEventHandler(this.OnMinimize), new CanExecuteRoutedEventHandler(this.OnCanMinimize)));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandSendToAutoStart, new ExecutedRoutedEventHandler(this.OnSendToAutoStart), new CanExecuteRoutedEventHandler(this.OnCanLoadLayout)));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandFullScreen, new ExecutedRoutedEventHandler(this.OnFullScreen)));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandOpenIPConnConf, new ExecutedRoutedEventHandler(this.OnOpenIPConnConf)));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandEditFullScreen, new ExecutedRoutedEventHandler(this.OnEditFullScreen)));
      //this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandQuit, (ExecutedRoutedEventHandler)((s, e) => this.Close())));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandAbout, new ExecutedRoutedEventHandler(this.OnAbout)));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandHelp, new ExecutedRoutedEventHandler(this.OnHelp)));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandExtDocu, new ExecutedRoutedEventHandler(this.OnExtDocu)));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandOptions, new ExecutedRoutedEventHandler(this.OnOptions)));
      this.AddHandler(GaugeEventsManager.RegisterGaugeEvent, (Delegate)new RoutedEventHandler(this.ZusiMeterControl_RegisterGauge));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandPause, new ExecutedRoutedEventHandler(this.OnPause)));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandTimejump, new ExecutedRoutedEventHandler(this.OnTimejump)));
      this.CommandBindings.Add(new CommandBinding((ICommand)ZusiMeterControl.CommandTimelapse, new ExecutedRoutedEventHandler(this.OnTimelapse)));


      //this.Closing += new CancelEventHandler(this.ZusiMeterControl_Closing);
      this.Loaded += new RoutedEventHandler(this.ZusiMeterControl_Loaded);
      this._beaconReceiver.BeaconSignalReceived += new BeaconSignalReceivedEventHandler(this.BeaconReceiver_BeaconSignalReceived);
      this.IsIPBoardSwitchVisible = this.ZusiConfiguration == ZusiConfigurationMode.Manual;
      this._willIPBoardSwitchVisible = this.IsIPBoardSwitchVisible;
    }

    private void ZusiMeterControl_Closing(object sender, CancelEventArgs e)
    {
      e.Cancel = !this.SavePropmt((string)null);
      Disposable.Dispose<BeaconReceiver>(ref this._beaconReceiver);
      //this.DisconnectFahrpult();
      Zusiaccess.CreateZUSIMenuEntry(Bezeichnertext: "ZusiMeter (Layoutauswahl)", Vatermenu: "SpTBXSubmenuItemKonfiguration", MenuIndex: 17);
      Zusiaccess.CreateZUSIMenuEntry(Bezeichnertext: "ZusiMeter (Letztes Layout)", Vatermenu: "SpTBXSubmenuItemKonfiguration", MenuIndex: 18, Params: _curentlayoutfile);
    }

    private void ZusiMeterControl_Loaded(object sender, RoutedEventArgs e)
    {
      if (ZusiStart.Data.DataManager.Instance.options.Show_ZusiMeter_Data)
      {
        this.DataProcessor();
        ZusiStart.Data.DataManager.Instance.zusiMeterControl = this;
        string[] commandLineArgs = Environment.GetCommandLineArgs();
        BackgroundSettings.ApplyUserSettings(Settings.Default.DefaultDialBackground, Settings.Default.DefaultTextBackground);
        this.ZusiConfiguration = ZusiConfigurationMode.LocalHost;
        this.CheckConfiguration();
        string Standard_Layoutfilename = "";
        string? executableFilePath = Process.GetCurrentProcess().MainModule?.FileName;
        string? executablepath = null;
        if (executableFilePath != null)
          executablepath = System.IO.Path.GetDirectoryName(executableFilePath);
        if (string.IsNullOrEmpty(ZusiStart.Data.DataManager.Instance.options.ZusiMeter_Standard_Layoutfile))
        {
          Standard_Layoutfilename = executablepath+ "\\ZusiMeter\\ZusiMeterLayouts\\Zusimeter_Standard_Layout.zmlf";
          //Standard_Layoutfilename = "D:\\Development\\ZUSI-Tools\\_updated_sources\\_net8\\ZusiStart\\ZusiStart\\bin\\Debug\\net8.0-windows\\win-x64\\ZusiMeterLayouts\\Zusimeter_Standard_Layout.zmlf";
        }
        else
        {
          Standard_Layoutfilename = ZusiStart.Data.DataManager.Instance.options.ZusiMeter_Standard_Layoutfile;
          if (!System.IO.File.Exists(Standard_Layoutfilename))
          {
            Standard_Layoutfilename = executablepath + "\\ZusiMeter\\ZusiMeterLayouts\\Zusimeter_Standard_Layout.zmlf";
          }
        }
        //string filename = "D:\\Development\\ZUSI-Tools\\_updated_sources\\_net8\\ZusiStart\\ZusiStart\\bin\\Debug\\net8.0-windows\\win-x64\\ZusiMeterLayouts\\Example01.zmlf";
        this.Dispatcher.BeginInvoke(new Action<string>(s => this.LoadLayout(s)), DispatcherPriority.Loaded, (object)Standard_Layoutfilename);
        //LoadLayout2(Standard_Layoutfilename);
      }
    }

    private void ZusiMeterControl_RegisterGauge(object sender, RoutedEventArgs e)
    {
      e.Handled = true;
      if (this.SelectLayout || !(e.OriginalSource is IGauge originalSource))
        return;
      this._gauges.Add(originalSource);
      switch (originalSource)
      {
        case RailRunner railRunner:
          railRunner.VolumeChanged += (EventHandler)((s, a) =>
          {
            Settings.Default.RailRunnerVolume = ((RailRunner)s).Volume;
            Settings.Default.Save();
          });
          if (railRunner.Volume != 0.5)
            break;
          railRunner.Volume = Settings.Default.RailRunnerVolume;
          break;
        case DigitalNextStopGauge digitalNextStopGauge:
          digitalNextStopGauge.VolumeChanged += (EventHandler)((s, a) =>
          {
            Settings.Default.RailRunnerVolume = ((DigitalNextStopGauge)s).Volume;
            Settings.Default.Save();
          });
          break;
        case ComponentTueren componentTueren:
          componentTueren.DoorButton += new DoorButtonEventHandler(this.ComponentTueren_DoorButton);
          componentTueren.DoorSideSwitchPositionChanged += new DoorSideSwitchPositionChangedEventHandler(this.ComponentTueren_DoorSideSwitchPositionChanged);
          break;
      }
    }

    private void ComponentTueren_DoorButton(object sender, DoorButtonEventArgs e)
    {
      if (e.ButtonType == DoorButtonType.DoorRelease)
      {
        if (e.IsPressed)
          this._fahrpult?.SendKeyboardCommand(new KeyboardCommand(KbdAssignment.Türen, KbdAction.Down, KbdCommand.TuerenTasterDown));
        else
          this._fahrpult?.SendKeyboardCommand(new KeyboardCommand(KbdAssignment.Türen, KbdAction.Up, KbdCommand.TuerenTasterUp));
      }
      else
      {
        if (e.ButtonType != DoorButtonType.ForceClosing)
          return;
        if (e.IsPressed)
          this._fahrpult?.SendKeyboardCommand(new KeyboardCommand(KbdAssignment.Türen, KbdAction.Down, KbdCommand.TuerenZuDown));
        else
          this._fahrpult?.SendKeyboardCommand(new KeyboardCommand(KbdAssignment.Türen, KbdAction.Up, KbdCommand.TuerenZuUp));
      }
    }

    private void ComponentTueren_DoorSideSwitchPositionChanged(
      object sender,
      DoorSideSwitchPositionEventArg e)
    {
      if (this._fahrpult == null)
        return;
      int num = e.DesiredPosition - e.CurrentPosition;
      switch (num)
      {
        case -3:
          num = 1;
          break;
        case 0:
          return;
        case 3:
          num = -1;
          break;
      }
      for (int index = 0; index < Math.Abs(num); ++index)
      {
        this._fahrpult.SendKeyboardCommand(new KeyboardCommand(KbdAssignment.Türen, KbdAction.Down, num > 0 ? KbdCommand.TuerenReDown : KbdCommand.TuerenLiDown));
        this._fahrpult.SendKeyboardCommand(new KeyboardCommand(KbdAssignment.Türen, KbdAction.Up, num > 0 ? KbdCommand.TuerenReUp : KbdCommand.TuerenLiUp));
      }
    }

    private void Fahrpult_ClientConnected(object sender, ClientConnectedEventArgs e)
    {
      //this.Dispatcher.BeginInvoke((Delegate) ((v, i) =>  **HLI
      //{ **HLI
      //  this.ZusiConnectionState = e.ClientAccepted ? (e.NeededDataAccepted ? 2 : 1) : 0; **HLI
      //  this.ZusiConnectionString = "Zusi-Version " + v + "/[" + i + "]"; **HLI
      //}), (object) e.ZusiVersion, (object) e.ZusiConnectionInfo); **HLI
      this.Dispatcher.BeginInvoke(new Action<object, object>((v, i) => //**HLI
      { //**HLI
        this.ZusiConnectionState = e.ClientAccepted ? (e.NeededDataAccepted ? 2 : 1) : 0; //**HLI
        this.ZusiConnectionString = "Zusi-Version " + v + "/[" + i + "]"; //**HLI
      }), DispatcherPriority.Loaded, (object)e.ZusiVersion, (object)e.ZusiConnectionInfo); // **HLI
    }

    private void Fahrpult_FtdDataReceived(object sender, FtdDataReceivedEventArgs e)
    {
      this._dataQueue.Enqueue((EventArgs)e);
      this._dataWakeUp.Set();
    }

    private void Fahrpult_ProgDataReceived(object sender, ProgDataReceivedEventArgs e)
    {
      this._dataQueue.Enqueue((EventArgs)e);
      this._dataWakeUp.Set();
    }

    private void Fahrpult_Dump(object sender, DumpEventArgs e)
    {

      using (StreamWriter writer = new StreamWriter("dumpoutput.txt", true))
      {

        writer.AutoFlush = true; // Ensure AutoFlush is true so that output is written to the console immediately
        ZusiFahrpultLib.Miscellaneous.Utils.Dump(e.Data, writer, 5);
        //using (MemoryStream stream = new MemoryStream(e.Data))
        //{
        //  // Create a BinaryReader from the MemoryStream
        //  using (BinaryReader binaryReader = new BinaryReader(stream))
        //  {
        //    ZusiFahrpultLib.Miscellaneous.BinaryReaderEx.Dump(binaryReader, writer, 5, 5);

        //  }
        //}
      }
    }

    private void Fahrpult_Disconnected(object sender, EventArgs e)
    {
      ZusiMeterControl._log.Debug((object)"Fahrpult getrennt");
      this.Dispatcher.BeginInvoke((() =>
      {
        this.ZusiConnectionState = 0;
        this.ZusiConnectionString = (string)null;
        do
          ;
        while (this._dataQueue.TryDequeue(out EventArgs _));
        this._gauges.ForEach((Action<IGauge>)(g => g.ResetValue()));
      }));
    }

    private void CbxIPBoard_Checked(object sender, RoutedEventArgs e)
    {
      bool? isChecked = ((ToggleButton)sender).IsChecked;
      bool flag = false;
      if (!(isChecked.GetValueOrDefault() == flag & isChecked.HasValue) || !this._mustReconnect)
        return;
      this.ReconnectFahrpult();
    }

    private void StatusBar_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (!this.SelectLayout || (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
        return;
      this._timerGracePeriod.Stop();
      //this.cbxIPBoard.IsChecked = new bool?(false);
      this.IsZusiMelderConfVisible = true;
      this._timerZusiMelderConf.Start();
    }

    private void TimerGracePeriod_Elapsed(object sender, ElapsedEventArgs e)
    {
      this.Dispatcher.BeginInvoke((() => this.CheckConfiguration()));
    }

    private void TimerZusiMelderConf_Elapsed(object sender, ElapsedEventArgs e)
    {
      this.Dispatcher.BeginInvoke((() =>
      {
        this.IsZusiMelderConfVisible = false;
        this.IsIPBoardSwitchVisible = this._willIPBoardSwitchVisible;
      }));
    }

    private void ZusiMelderConf_MouseEnter(object sender, MouseEventArgs e)
    {
      this._timerZusiMelderConf.Stop();
    }

    private void ZusiMelderConf_MouseLeave(object sender, MouseEventArgs e)
    {
      this._timerZusiMelderConf.Start();
      //if (this.ZusiConfiguration == ZusiConfigurationMode.Manual)
      //    this.cbxIPBoard.IsChecked = true;
      //else
      //    this.cbxIPBoard.IsChecked = false;
    }

    private void OnAbout(object sender, ExecutedRoutedEventArgs e)
    {
      //AboutDlg aboutDlg = new AboutDlg();
      //aboutDlg.Owner = (Window)this;
      //aboutDlg.ShowDialog();
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
        string relativePath = "help/ZusiMeter_Docu_Deutsch.pdf";
        string absolutePath = Path.GetFullPath(relativePath);
        Process.Start("cmd.exe", $"/c start \"\" \"{absolutePath}\"");
        //Process.Start(absolutePath);
      }
      catch (Exception ex)
      {
        int num = (int)System.Windows.MessageBox.Show(ex.ToString(), "Fehler beim Öffnen der Dokumentation", MessageBoxButton.OK);

      }
    }

    public static bool IsOptionSet(string option)
    {
      if (string.IsNullOrEmpty(option)) return false;
      if (_selectedOptions.Contains(option)) return true;
      return false;
    }

    private void ReadOptions()
    {
      this.Host = Settings.Default.HostOrIP;
      this.Port = Settings.Default.Port;
      this.PrivateLayoutFolder = Settings.Default.PrivateLayoutFolder;

      if (string.IsNullOrEmpty(this.PrivateLayoutFolder))
      {
        this.PrivateLayoutFolder = GetZusiMeterLayoutFileDir();
      }
      _currentlayoutfolder = this.PrivateLayoutFolder;


      this.ExampleLayoutFolder = Settings.Default.ExampleLayoutFolder;
      if (string.IsNullOrEmpty(this.ExampleLayoutFolder))
      {
        this.ExampleLayoutFolder = GetZusiMeterExampleFileDir();
      }
      _currentexamplelayoutfolder = this.ExampleLayoutFolder;

      _selectedOptions = Properties.Settings.Default.OptionListSelectedItems;

      if (IsOptionSet("NoMoveCloseIcons"))
      {

      }
    }

    private void OnOptions(object sender, ExecutedRoutedEventArgs e)
    {
      OptionsDlg optionsDlg = new OptionsDlg(this);
      optionsDlg.Owner = Application.Current.MainWindow;


      optionsDlg.OptionsListBox.SelectedValue = _selectedOptions;
      bool? result = optionsDlg.ShowDialog();
      if (result == true)
      {
        _selectedOptions = optionsDlg.OptionsListBox.SelectedValue;
        Properties.Settings.Default.OptionListSelectedItems = _selectedOptions;
        Properties.Settings.Default.Save();
        ReadOptions();
        DataManager.Instance.RefreshLayouts();
      }
    }

    private static void OnHostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as ZusiMeterControl).OnHostChanged((string)e.NewValue);
    }

    private void OnHostChanged(string host)
    {
      if (!this._initialized)
        return;
      this.CheckConfiguration();
      Settings.Default.HostOrIP = host;
      Settings.Default.Save();
      this.DisconnectFahrpult();
      this._mustReconnect = true;
    }

    private static void OnLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(d is ZusiMeterControl ZusiMeterControl))
        return;
      ZusiMeterControl.OnLeftChanged((double)e.NewValue);
    }

    private void OnLeftChanged(double value)
    {
      if (!this._trackWindow)
        return;
      WindowPosition.Left = value;
    }

    private static void OnPortChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as ZusiMeterControl).OnPortChanged((int)e.NewValue);
    }

    private void OnPortChanged(int port)
    {
      if (!this._initialized)
        return;
      Settings.Default.Port = port;
      Settings.Default.Save();
      this.DisconnectFahrpult();
      this._mustReconnect = true;
    }

    private static void OnPrivateLayoutFolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as ZusiMeterControl).OnPrivateLayoutFolderChanged((string)e.NewValue);
    }

    private void OnPrivateLayoutFolderChanged(string PrivateLayoutFolder)
    {
      if (!this._initialized)
        return;
      Settings.Default.PrivateLayoutFolder = PrivateLayoutFolder;
      Settings.Default.Save();
      _currentlayoutfolder = PrivateLayoutFolder;
    }

    private static void OnExampleLayoutFolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as ZusiMeterControl).OnExampleLayoutFolderChanged((string)e.NewValue);
    }

    private void OnExampleLayoutFolderChanged(string ExampleLayoutFolder)
    {
      if (!this._initialized)
        return;
      Settings.Default.ExampleLayoutFolder = ExampleLayoutFolder;
      Settings.Default.Save();
      _currentexamplelayoutfolder = ExampleLayoutFolder;
    }


    private static void OnTopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(d is ZusiMeterControl ZusiMeterControl))
        return;
      ZusiMeterControl.OnTopChanged((double)e.NewValue);
    }

    private void OnTopChanged(double value)
    {
      if (!this._trackWindow)
        return;
      WindowPosition.Top = value;
    }

    private static void OnTopMostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as ZusiMeterControl).OnTopMostChanged((bool)e.NewValue);
    }

    private void OnTopMostChanged(bool value)
    {
      if (!this._trackWindow)
        return;
      WindowPosition.Topmost = value;
    }

    public static void OnZusiConfigurationChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(d is ZusiMeterControl ZusiMeterControl))
        return;
      ZusiMeterControl.OnZusiConfigurationChanged((ZusiConfigurationMode)e.NewValue);
    }

    public void OnZusiConfigurationChanged(ZusiConfigurationMode value)
    {
      Settings.Default.ZusiConfiguration = (int)value;
      Settings.Default.Save();
      this._willIPBoardSwitchVisible = this.SelectLayout && value == ZusiConfigurationMode.Manual;
      switch (value)
      {
        case ZusiConfigurationMode.LocalHost:
          this.MissingConfiguration = false;
          this._beaconReceiver.ShutDown();
          break;
        case ZusiConfigurationMode.AutoDetect:
          this.MissingConfiguration = false;
          if (!this._beaconReceiver.IsListening)
            this._beaconReceiver.ReceiveAsync();
          this._timerGracePeriod.Start();
          break;
        case ZusiConfigurationMode.Manual:
          this._beaconReceiver.ShutDown();
          this.CheckConfiguration();
          break;
      }
    }

    private void LvLayoutFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (e.AddedItems.Count <= 0)
        return;
      //this.startPage.preview?.ShowPreview((string) e.AddedItems[0]); **HLI
    }

    private void OnCanLoadLayout(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = this.startPage.lvLayoutFiles.SelectedItem != null;
    }

    private void OnLoadLayout(object sender, ExecutedRoutedEventArgs e)
    {
      //this.HideIPBoard();
      //this.Dispatcher.BeginInvoke((Delegate) (s => this.LoadLayout(s)), DispatcherPriority.Loaded, (object) (string) this.lvLayoutFiles.SelectedItem); **HLI
      this.Dispatcher.BeginInvoke(new Action<string>(s => this.LoadLayout(s)), DispatcherPriority.Loaded, (string)this.startPage.lvLayoutFiles.SelectedItem);
    }

    private void OnLoadOtherLayout(object sender, ExecutedRoutedEventArgs e)
    {
      //this.HideIPBoard();
      //string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "ZusiMeterLayouts");
      string path = Path.Combine(GetZusiMeterLayoutFileDir());
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      openFileDialog1.DefaultExt = "zmlf";
      openFileDialog1.Filter = "ZusiMeter-Layoutdateien|*.zmlf|Alle Dateien|*.*";
      openFileDialog1.InitialDirectory = path;
      openFileDialog1.Multiselect = false;
      openFileDialog1.Title = "Layout laden";
      OpenFileDialog openFileDialog2 = openFileDialog1;
      bool? nullable = openFileDialog2.ShowDialog();
      bool flag = true;
      if (!(nullable.GetValueOrDefault() == flag & nullable.HasValue))
        return;
      this.LoadLayout(openFileDialog2.FileName);
    }

    private void OnAppHelper(object sender, ExecutedRoutedEventArgs e)
    {
      AppHelper.CanBringZusiToFront = !AppHelper.CanBringZusiToFront;
      //this.Title = AppHelper.CanBringZusiToFront ? ZusiMeterControl._defaultTitle : ZusiMeterControl._defaultTitle + " (*)";
    }

    private void OnBack(object sender, ExecutedRoutedEventArgs e)
    {
      //if (WindowStyle == WindowStyle.None)
      //{
      //  //Fullscreenmode was on
      //  LoadLayoutFrameless(false);
      //}
      //else
      //{
      //  if (!this.SavePropmt("Soll das aktuelle Layout schnell noch gespeichert werden?"))
      //    return;
      //  this.SelectLayout = true;
      //  this.ShowMainMenu = true;
      //  this.ShowStatusBar = true;
      //  this.ShowNoMoveCloseIcons = IsOptionSet("ShowNoMoveCloseIcons");
      //   this.startPage.Visibility = Visibility.Visible;
      //  this.editorPage.Visibility = Visibility.Collapsed;
      //  Task.Run((Action)(() =>
      //  {
      //    //this.DisconnectFahrpult();
      //    this.Dispatcher.Invoke((Action)(() => this.ClearLayout()));
      //  }));
      //  //this._willIPBoardSwitchVisible = this.IsIPBoardSwitchVisible = Settings.Default.ZusiConfiguration == 2;
      //  this._trackWindow = false;
      //  this.Topmost = false;
      //}
    }

    private void BeaconReceiver_BeaconSignalReceived(object sender, BeaconSignalReceivedEventArgs e)
    {
      string[] strArray = e.SignalData.Split('/');
      int result;
      if (strArray.Length != 2 || !(strArray[0] == "ZusiSim") || !int.TryParse(strArray[1], out result))
        return;
      e.RemoteEndPoint = new IPEndPoint(e.Source, result);
      e.IsValid = true;
      this.Dispatcher.Invoke((Action)(() =>
      {
        this._beaconDetected = true;
        if (this.ZusiConfiguration != ZusiConfigurationMode.AutoDetect)
          return;
        this.MissingConfiguration = false;
      }));
    }

    private void OnPause(object sender, ExecutedRoutedEventArgs e)
    {
      this._fahrpult.SendControlCommand(ControlCommand.Pause, (object)ControlCommandValue.Toggle);
      AppHelper.BringZusiToFront();
    }

    private void OnTimejump(object sender, ExecutedRoutedEventArgs e)
    {
      this._fahrpult.SendControlCommand(ControlCommand.TimeJump, (object)ControlCommandValue.Toggle);
      AppHelper.BringZusiToFront();
    }

    private void OnTimelapse(object sender, ExecutedRoutedEventArgs e)
    {
      this._fahrpult.SendControlCommand(ControlCommand.TimeLapse, (object)ControlCommandValue.Toggle);
      AppHelper.BringZusiToFront();
    }

    private void OnSendToAutoStart(object sender, ExecutedRoutedEventArgs e)
    {
      string msg = "Wollen Sie dieses Layout in ZUSI AutoStart eintragen?";

      switch (System.Windows.MessageBox.Show(msg, "Nachfrage", MessageBoxButton.YesNo))
      {
        case MessageBoxResult.Cancel:
          return;
        case MessageBoxResult.Yes:
          string? executablePath = Process.GetCurrentProcess().MainModule?.FileName;
          Zusiaccess.CreateZUSIAutoStartEntry(executablePath, (string)this.startPage.lvLayoutFiles.SelectedItem);

          return;

        case MessageBoxResult.No:
          return;
      }
    }

    private void LoadLayoutFrameless(bool fullscreen, bool editlayout = false, string layoutfilename = null)
    {
      if (fullscreen == false)
      {
        // Create a new instance of the original window
        var originalWindow = new ZusiMeterControl
        {
          //AllowsTransparency = false,
          //WindowStyle = WindowStyle.ToolWindow,
          //Background = Brushes.White, // or whatever the original background was
          //WindowState = WindowState.Normal,
          //ResizeMode = ResizeMode.NoResize,
          //SizeToContent = SizeToContent.WidthAndHeight
        };
        this.DisconnectFahrpult();

        // Show the new window
        //originalWindow.Show();
        if (_Windowbackground != null)
          originalWindow.Background = _Windowbackground;
        originalWindow.SelectLayout = true;
        originalWindow.ShowMainMenu = true;
        originalWindow.ShowStatusBar = true;
        originalWindow.ShowNoMoveCloseIcons = IsOptionSet("ShowNoMoveCloseIcons");
        originalWindow.startPage.Visibility = Visibility.Visible;
        originalWindow.editorPage.Visibility = Visibility.Collapsed;

        // Close the current full-screen window
        //this.Close();
      }
      else
      {
        _Windowbackground = Background;

        // Create a new window instance
        var fullScreenWindow = new ZusiMeterControl
        {
          // Enter fullscreen
          //ResizeMode = ResizeMode.NoResize,
          //WindowStyle = WindowStyle.None,
          //AllowsTransparency = true,
          //Background = Brushes.Transparent, // Set the background to transparent
          //SizeToContent = SizeToContent.WidthAndHeight
        };
        // Show the new window
        this.DisconnectFahrpult();
        if (layoutfilename != null)
        {
          _curentlayoutfile = layoutfilename;
        }

        if (editlayout == true)
        {
          fullScreenWindow.editorPage.Dispatcher.BeginInvoke(new Action(() =>
          {
            fullScreenWindow.editorPage.LoadLayout((string)_curentlayoutfile);
          }), DispatcherPriority.Loaded);
        }
        else
        {

          fullScreenWindow.LoadLayout2(_curentlayoutfile);
          fullScreenWindow.MouseDown += new MouseButtonEventHandler(Window_MouseDown);
        }

        // Maximize after setting position and size
        //fullScreenWindow.WindowState = WindowState.Maximized;

        //fullScreenWindow.Show();
        //fullScreenWindow.SelectLayout = false;
        //fullScreenWindow.ShowMainMenu = false;
        //fullScreenWindow.ShowStatusBar = false;
        //fullScreenWindow.ShowNoMoveCloseIcons = IsOptionSet("ShowNoMoveCloseIcons");
        //fullScreenWindow.startPage.Visibility = Visibility.Collapsed;
        //fullScreenWindow.editorPage.Visibility = Visibility.Collapsed;


        //SizeToContent = SizeToContent.WidthAndHeight;

        // Close the current window
        //this.Close();

      }
    }

    private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
    {
      //if (e.ChangedButton == MouseButton.Left)
      //{
      //  this.DragMove();
      //}
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
      //if (e.ChangedButton == MouseButton.Left)
      //{
      //  this.DragMove();
      //}
    }

    private void OnFullScreen(object sender, ExecutedRoutedEventArgs e)
    {
      //if (this.WindowStyle == WindowStyle.None)
      //{
      //  this.LoadLayoutFrameless(false);
      //}
      //else
      //{
      //  this.LoadLayoutFrameless(true);

      //}
    }

    private void OnEditFullScreen(object sender, ExecutedRoutedEventArgs e)
    {
      //if (this.WindowStyle == WindowStyle.None)
      //{
      //  this.LoadLayoutFrameless(false);
      //  this.SelectLayout = false;
      //  this.ShowMainMenu = true;
      //  this.ShowStatusBar = true;
      //  this.startPage.Visibility = Visibility.Collapsed;
      //  this.editorPage.Visibility = Visibility.Visible;
      //}
      //else
      //{
      //  this.LoadLayoutFrameless(true,true);
      //  this.SelectLayout = false;
      //  this.ShowMainMenu = true;
      //  this.ShowStatusBar = true;
      //  this.startPage.Visibility = Visibility.Collapsed;
      //  this.editorPage.Visibility = Visibility.Visible;
      //}
    }


    private void ZusiMeterControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      this.Width = SystemParameters.PrimaryScreenWidth;
      this.Height = SystemParameters.PrimaryScreenHeight;
    }


    private void OnOpenIPConnConf(object sender, ExecutedRoutedEventArgs e)
    {
      this._timerGracePeriod.Stop();
      //this.cbxIPBoard.IsChecked = new bool?(false);
      this.IsZusiMelderConfVisible = true;
      this._timerZusiMelderConf.Start();
      return;
    }

    public void OnCheckConnection(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {

        this.DisconnectFahrpult();
        this.CreateFahrpult();

        if (Settings.Default.ZusiConfiguration == 2)
        {
          IPHostEntry entry = Dns.GetHostEntry(string.IsNullOrEmpty(this.Host) ? "localhost" : this.Host);
          //IEnumerable<IPAddress> ipv4s = entry.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork);
          //if (ipv4s.Any())
          //{

          //}
          //else
          //{

          //}
        }
        this.ConnectFahrpult();
        this._mustReconnect = false;
      }

      catch
      { }
    }

    private void OnCanMinimize(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = true; // this.WindowState != WindowState.Minimized;
    }

    public void OnDefinePrivateLayoutFolder(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {
        Debug.Print("OnDefinePrivateLayoutFolder");
        System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
        //fbd.RootFolder = Environment.SpecialFolder.MyDocuments;
        fbd.ShowNewFolderButton = true;
        fbd.Description = "Privates Layoutverzeichnis auswählen";

        if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
          PrivateLayoutFolder = fbd.SelectedPath;
      }

      catch
      { }
    }

    public void OnDefineExampleLayoutFolder(object sender, ExecutedRoutedEventArgs e)
    {
      try
      {
        Debug.Print("OnDefineExampleLayoutFolder");
        System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
        //fbd.RootFolder = Environment.SpecialFolder.MyDocuments;
        fbd.ShowNewFolderButton = true;
        fbd.Description = "Beispiellayoutverzeichnis auswählen";

        if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
          ExampleLayoutFolder = fbd.SelectedPath;

      }

      catch
      { }
    }

    private void OnMinimize(object sender, ExecutedRoutedEventArgs e)
    {
      //this.WindowState = WindowState.Minimized;
    }

    private void LayoutItem_DoubleClick(object sender, MouseButtonEventArgs e)
    {
      ZusiMeterControl.CommandLoadLayout.Execute((object)null, (IInputElement)null);
    }

    private async void DataProcessor()
    {
      await Task.Run((Action)(() =>
      {
        while (!this._dataCancellation)
        {
          this._dataWakeUp.WaitOne();
          if (this._dataCancellation)
            break;
          this.Dispatcher.Invoke((Action)(() =>
          {
            EventArgs result;
            while (this._dataQueue.TryDequeue(out result))
            {
              FtdDataReceivedEventArgs f = result as FtdDataReceivedEventArgs;
              if (f != null)
              {
                this._gauges.ForEach((Action<IGauge>)(g => g.SetFtdData(f)));
              }
              else
              {
                ProgDataReceivedEventArgs p = result as ProgDataReceivedEventArgs;
                if (p != null)
                  this._gauges.ForEach((Action<IGauge>)(g => g.SetProgData(p)));
              }
            }
          }));
        }
      }));
    }

    public void ZusiMeter_DataProcessor(EventArgs result)
    {
      FtdDataReceivedEventArgs f = result as FtdDataReceivedEventArgs;
      if (f != null)
      {
        this._gauges.ForEach((Action<IGauge>)(g => g.SetFtdData(f)));
      }
      else
      {
        ProgDataReceivedEventArgs p = result as ProgDataReceivedEventArgs;
        if (p != null)
          this._gauges.ForEach((Action<IGauge>)(g => g.SetProgData(p)));
      }
    }

    //private void HideIPBoard() => this.cbxIPBoard.IsChecked = new bool?(false);

    private void LoadLayout(string layoutFileName)
    {
      if (IsOptionSet("Modeframeless")) // load layout in frameless mode
      {
        if (!DataManager.Instance.framelesswindow_already_loaded)
        {
          DataManager.Instance.framelesswindow_already_loaded = true;
          LoadLayoutFrameless(true, false, layoutFileName);
        }
      }
      else
      {
        this.LoadLayout2(layoutFileName);
      }
    }

    private void LoadLayout2(string layoutFileName)
    {
      this._willIPBoardSwitchVisible = this.IsIPBoardSwitchVisible = false;
      this.DisconnectFahrpult();
      this.SelectLayout = false;
      this.ShowMainMenu = false;
      this.ShowStatusBar = true;
      this.startPage.Visibility = Visibility.Collapsed;
      this.editorPage.Visibility = Visibility.Collapsed;


      Visibility = Visibility.Visible;

      this._curentlayoutfile = layoutFileName;
      //if (!this.MissingConfiguration)
      //  this._beaconReceiver.ShutDown();
      this.ClearLayout();
      ZMLFile zmlFile = new ZMLFile(layoutFileName);
      zmlFile.ParseCompleted += (EventHandler)((s, e) =>
      {
        this.DisconnectFahrpult();
        this.CreateFahrpult();
        this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
        {
          this._gaugeIds.Clear();
          this._gaugeProgIds.Clear();
          foreach (IGauge gauge in this._gauges)
          {
            if (gauge.FtdID != ZFtdID.None)
              this._gaugeIds.Add(gauge.FtdID);
            if (gauge.GetAdditionalIDs().Count<ZFtdID>() > 0)
            {
              foreach (ZFtdID additionalId in gauge.GetAdditionalIDs())
                this._gaugeIds.Add(additionalId);
            }
            if (gauge.GetProgIDs().Count<ZProgID>() > 0)
            {
              foreach (ZProgID progId in gauge.GetProgIDs())
                this._gaugeProgIds.Add(progId);
            }
          }
          this.ConnectFahrpult();
          //if (WindowPosition.IsValid)
          //{
          //  this.Left = WindowPosition.Left;
          //  this.Top = WindowPosition.Top;
          //  this.Topmost = WindowPosition.Topmost;
          //  ZusiMeterControl._log.Debug((object)string.Format("window pos applied: {0} --> {1} | {2} --> {3} | {4} --> {5}", (object)WindowPosition.Left, (object)this.Left, (object)WindowPosition.Top, (object)this.Top, (object)WindowPosition.Topmost, (object)this.Topmost));
          //}
          //else
          //  ZusiMeterControl._log.Debug((object)"no window pos applied");
          this._trackWindow = true;
        }));
      });
      zmlFile.ParseDocument += (ZMLReadDocumentEventHandler)((s, e) =>
      {
        float version = XElementEx.GetAttrValue(e.Layout, (XName)"version", 0.0f);
        XElement x = e.Layout.Element((XName)"Background");
        if (x != null)
          this._background.Initialize(x);
        else
          this._background.BackgroundMode = XElementEx.GetAttrValue(e.Layout, (XName)"displayMode", "").ToLower() == "text" ? BackgroundMode.Text : BackgroundMode.Burlwood;
        if (this._background.BackgroundMode == BackgroundMode.Text)
        {
          this._colWidth = 75.0;
          this._rowHeight = 20.0;
          version = 1.1f;
        }
        else
        {
          this._colWidth = 50.0;
          this._rowHeight = 50.0;
        }
        this.Zoom = (double)XElementEx.GetAttrValue(e.Layout, (XName)"zoom", 1f);
        foreach (XElement element in e.Layout.Elements())
        {
          if (!(element.Name.LocalName == "Background"))
            this.InsertGauge(GaugeTemplateFactory.CreateTemplate(e.Namespace, element, version));
        }
      });
      zmlFile.Parse();
    }

    private void ClearLayout()
    {
      this._gauges.Clear();
      this.layoutGrid.Children.Clear();
      this.layoutGrid.RowDefinitions.Clear();
      this.layoutGrid.ColumnDefinitions.Clear();
    }

    private void InsertGauge(IGaugeTemplate gt)
    {
      if (!(gt is DependencyObject))
        return;
      int column = gt.Column;
      int row = gt.Row;
      int sizeX = gt.SizeX;
      int sizeY = gt.SizeY;
      this.PrepareGrid(column, row, sizeX, sizeY);
      if (!(gt.GetGauge() is Control gauge))
        return;
      Grid.SetColumn((UIElement)gauge, column);
      Grid.SetColumnSpan((UIElement)gauge, sizeX);
      Grid.SetRow((UIElement)gauge, row);
      Grid.SetRowSpan((UIElement)gauge, sizeY);
      this.layoutGrid.Children.Add((UIElement)gauge);
    }

    private void PrepareGrid(int col, int row, int sizeX, int sizeY)
    {
      int num1 = col + sizeX;
      GridLength gridLength1 = new GridLength(this._colWidth);
      while (this.layoutGrid.ColumnDefinitions.Count < num1)
        this.layoutGrid.ColumnDefinitions.Add(new ColumnDefinition()
        {
          Width = gridLength1
        });
      int num2 = row + sizeY;
      GridLength gridLength2 = new GridLength(this._rowHeight);
      while (this.layoutGrid.RowDefinitions.Count < num2)
        this.layoutGrid.RowDefinitions.Add(new RowDefinition()
        {
          Height = gridLength2
        });
    }

    private void ObtainLayoutFiles()
    {
      //string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "ZusiMeterLayouts");
      string path = _currentexamplelayoutfolder;

      if (!Directory.Exists(path))
        return;
      foreach (string enumerateFile in Directory.EnumerateFiles(path, "*.zmlf", SearchOption.TopDirectoryOnly))
      {
        if (!enumerateFile.Contains<char>('~'))
          this._layoutFiles.Add(enumerateFile);
      }

      path = _currentlayoutfolder; //Path.Combine(GetZusiMeterLayoutFileDir());

      if (!Directory.Exists(path))
        return;
      foreach (string enumerateFile in Directory.EnumerateFiles(path, "*.zmlf", SearchOption.TopDirectoryOnly))
      {
        if (!enumerateFile.Contains<char>('~'))
          this._layoutFiles.Add(enumerateFile);
      }
    }

    public void assign_fahrpult(ZusiStart.Connection.Fahrpult main_fahrpult)
    {
      //this._fahrpult = main_fahrpult;
    }

    private void ConnectFahrpult()
    {
      if (ZusiConnectionState == 2)
      {
        return;
      }

      switch (this.ZusiConfiguration)
      {
        case ZusiConfigurationMode.LocalHost:
          //this._fahrpult.Autodetect = false;
          break;
        case ZusiConfigurationMode.AutoDetect:
          //this._fahrpult.Autodetect = true;
          break;
        case ZusiConfigurationMode.Manual:
          //this._fahrpult.Autodetect = false;
          break;
      }

      this._fahrpult.SetNeededData(this._gaugeIds.Distinct<ZFtdID>());
      this._fahrpult.SetNeededData(this._gaugeProgIds.Distinct<ZProgID>());
      if (Settings.Default.ZusiConfiguration == 0)
        this._fahrpult.OpenAsync();
      else if (this.Port <= 0)
        this._fahrpult.OpenAsync(this.Host);
      else
        this._fahrpult.OpenAsync(this.Host, this.Port);
    }

    private void CreateFahrpult()
    {
      if (this._fahrpult == null)
      {
        this._fahrpult = new FahrpultClient(ParsingMode.Internal, AsmInfo.Product, VersionEx.ToString(AsmInfo.Version, "%M.%m.%b"))
        {
          LogSocketExceptions = true,
          WaitLoopTime = 4000
        };
        this._fahrpult.ClientConnected += new ClientConnectedEventHandler(this.Fahrpult_ClientConnected);
        this._fahrpult.Disconnected += new EventHandler(this.Fahrpult_Disconnected);
        this._fahrpult.FtdDataReceived += new FtdDataReceivedEventHandler(this.Fahrpult_FtdDataReceived);
        this._fahrpult.ProgDataReceived += new ProgDataReceivedEventHandler(this.Fahrpult_ProgDataReceived);
        if (_generateFahrpultDump == true)
        {
          this._fahrpult.Dump += new DumpEventHandler(this.Fahrpult_Dump);
        }

      }
    }

    private void DisconnectFahrpult()
    {
      if (this._fahrpult == null)
        return;
      this._fahrpult.ClientConnected -= new ClientConnectedEventHandler(this.Fahrpult_ClientConnected);
      this._fahrpult.Disconnected -= new EventHandler(this.Fahrpult_Disconnected);
      this._fahrpult.FtdDataReceived -= new FtdDataReceivedEventHandler(this.Fahrpult_FtdDataReceived);
      this._fahrpult.ProgDataReceived -= new ProgDataReceivedEventHandler(this.Fahrpult_ProgDataReceived);
      if (_generateFahrpultDump == true)
      {
        this._fahrpult.Dump -= new DumpEventHandler(this.Fahrpult_Dump);
      }
      try
      {
        this._fahrpult.Dispose();
      }
      catch
      {
      }
      finally
      {
        this._fahrpult = (FahrpultClient)null;
        this.Fahrpult_Disconnected((object)null, EventArgs.Empty);
      }
    }

    private void ReconnectFahrpult()
    {
      try
      {
        this.DisconnectFahrpult();
        this.CreateFahrpult();
        this.ConnectFahrpult();
        this._mustReconnect = false;
      }
      catch (Exception ex)
      {
      }
    }

    private void CheckConfiguration()
    {
      switch (this.ZusiConfiguration)
      {
        case ZusiConfigurationMode.LocalHost:
          this.MissingConfiguration = false;
          break;
        case ZusiConfigurationMode.AutoDetect:
          this.MissingConfiguration = this.SelectLayout && !this._beaconDetected;
          break;
        case ZusiConfigurationMode.Manual:
          this.MissingConfiguration = this.SelectLayout && string.IsNullOrEmpty(this.Host);
          break;
      }
    }
    // Konfigurator functions
    private void ZusiMeterControl_DefaultDialBackgroundSettingsChanged(object sender, RoutedEventArgs e)
    {
      Settings.Default["DefaultDialBackground"] = (object)BackgroundSettings.DefaultDialBackground;
      Settings.Default.Save();
    }

    private void ZusiMeterControl_DefaultTextBackgroundSettingsChanged(object sender, RoutedEventArgs e)
    {
      Settings.Default["DefaultTextBackground"] = (object)BackgroundSettings.DefaultTextBackground;
      Settings.Default.Save();
    }

    private void OnWillNewLayout(object sender, ExecutedRoutedEventArgs e)
    {
      if (!this.SavePropmt((string)null))
        return;
      this.SelectLayout = true;
      this.ShowMainMenu = true;
      this.ShowStatusBar = true;
      this.editorPage.Visibility = Visibility.Collapsed;
      this.startPage.Visibility = Visibility.Visible;
    }

    private void OnNewGraphicLayout(object sender, ExecutedRoutedEventArgs e)
    {
      if (!this.SavePropmt("Soll das aktuelle Layout schnell noch gespeichert werden?"))
        return;
      this.SelectLayout = false;
      this.ShowMainMenu = true;
      this.ShowStatusBar = true;
      this.startPage.Visibility = Visibility.Collapsed;
      this.editorPage.Visibility = Visibility.Visible;
      this.editorPage.Dispatcher.BeginInvoke(new Action(() => this.editorPage.NewLayout(false)), DispatcherPriority.Loaded);
    }

    private void OnNewTextLayout(object sender, ExecutedRoutedEventArgs e)
    {
      if (!this.SavePropmt("Soll das aktuelle Layout schnell noch gespeichert werden?"))
        return;
      this.SelectLayout = false;
      this.ShowMainMenu = true;
      this.ShowStatusBar = true;
      this.startPage.Visibility = Visibility.Collapsed;
      this.editorPage.Visibility = Visibility.Visible;
      this.editorPage.Dispatcher.BeginInvoke(new Action(() => this.editorPage.NewLayout(true)), DispatcherPriority.Loaded);
    }

    private void OnLoadLayoutEdit(object sender, ExecutedRoutedEventArgs e)
    {
      if (!this.SavePropmt("Soll das aktuelle Layout schnell noch gespeichert werden?"))
        return;
      this.SelectLayout = false;
      this.ShowMainMenu = true;
      this.ShowStatusBar = true;
      this.startPage.Visibility = Visibility.Collapsed;
      this.editorPage.Visibility = Visibility.Visible;
      this._curentlayoutfile = (string)this.startPage.lvLayoutFiles.SelectedItem;
      //this.editorPage.Dispatcher.BeginInvoke((Delegate) (s => this.editorPage.LoadLayout(s)), DispatcherPriority.Loaded, (object) (string) this.startPage.lvLayoutFiles.SelectedItem);
      this.editorPage.Dispatcher.BeginInvoke(new Action(() =>
      {
        this.editorPage.LoadLayout((string)this.startPage.lvLayoutFiles.SelectedItem);
      }), DispatcherPriority.Loaded);
    }

    private void OnOpenLayout(object sender, ExecutedRoutedEventArgs e)
    {
      if (!this.SavePropmt((string)null))
        return;
      //string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "ZusiMeterLayouts");
      string path = GetZusiMeterLayoutFileDir();
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
      OpenFileDialog openFileDialog1 = new OpenFileDialog();
      openFileDialog1.DefaultExt = "zmlf";
      openFileDialog1.Filter = "ZusiMeter-Layoutdateien|*.zmlf|Alle Dateien|*.*";
      openFileDialog1.InitialDirectory = path;
      openFileDialog1.Multiselect = false;
      openFileDialog1.Title = "Layout öffnen";
      OpenFileDialog openFileDialog2 = openFileDialog1;
      bool? nullable = openFileDialog2.ShowDialog();
      bool flag = true;
      if (!(nullable.GetValueOrDefault() == flag & nullable.HasValue))
        return;
      this.SelectLayout = false;
      this.ShowMainMenu = true;
      this.startPage.Visibility = Visibility.Collapsed;
      this.editorPage.Visibility = Visibility.Visible;
      //this.editorPage.Dispatcher.BeginInvoke((Delegate) (s => this.editorPage.LoadLayout(s)), DispatcherPriority.Loaded, (object) openFileDialog2.FileName);
      this.editorPage.Dispatcher.BeginInvoke(new Action<string>(s =>
      {
        this.editorPage.LoadLayout(s);
      }), DispatcherPriority.Loaded, openFileDialog2.FileName);
    }

    private void OnCanBackToLayout(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = DataManager.Instance.HasLayout;
    }

    private void OnBackToLayout(object sender, ExecutedRoutedEventArgs e)
    {
      //if (WindowStyle == WindowStyle.None)
      //{
      //  //Fullscreenmode was on
      //  LoadLayoutFrameless(false);
      //}
      //else
      //{
      //  this.SelectLayout = true;
      //  this.ShowMainMenu = true;
      //  DataManager.Instance.SelectLayoutFile(DataManager.Instance.LayoutFileName);
      //  this.startPage.Visibility = Visibility.Collapsed;
      //  this.editorPage.Visibility = Visibility.Visible;
      //}
    }

    private bool SavePropmt(string msg)
    {
      if (DataManager.Instance.IsLayoutDirty)
      {
        if (this.editorPage.placeholder.HasGauges)
        {
          if (string.IsNullOrEmpty(msg))
            msg = ZusiMeterControl._defaultSavePrompt;
          switch (System.Windows.MessageBox.Show(msg, "Nachfrage", MessageBoxButton.YesNoCancel))
          {
            case MessageBoxResult.Cancel:
              return false;
            case MessageBoxResult.Yes:
              if (!this.editorPage.SaveLayout())
                return false;
              break;
            case MessageBoxResult.No:
              DataManager.Instance.IsLayoutDirty = false;
              break;
          }
        }
        else
        {
          string layoutFileName = DataManager.Instance.LayoutFileName;
          if (!string.IsNullOrEmpty(layoutFileName) && File.Exists(layoutFileName))
          {
            msg = ZusiMeterControl._defaultSavePrompt2;
            switch (System.Windows.MessageBox.Show(msg, "Nachfrage", MessageBoxButton.YesNoCancel))
            {
              case MessageBoxResult.Cancel:
                return false;
              case MessageBoxResult.Yes:
                try
                {
                  File.Delete(layoutFileName);
                  break;
                }
                catch
                {
                  break;
                }
              case MessageBoxResult.No:
                DataManager.Instance.IsLayoutDirty = false;
                break;
            }
          }
        }
      }
      return true;
    }
  }
}
