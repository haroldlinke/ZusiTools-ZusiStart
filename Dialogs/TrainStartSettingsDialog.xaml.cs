using System.Windows;
using System.Windows.Input;

namespace ZusiStart.Dialogs
{
  /// <summary>
  /// Interaktionslogik für TrainStartSettingsDialog.xaml
  /// </summary>
  public partial class TrainStartSettingsDialog : Window
  {
    //---------------------------------------------------------------------
    public static readonly DependencyProperty ForbidAlternativePicLibSourcesProperty = DependencyProperty.Register(
        "ForbidAlternativePicLibSources",
        typeof(bool),
        typeof(TrainStartSettingsDialog),
        new PropertyMetadata(false));
    public bool ForbidAlternativePicLibSources
    {
      get => (bool)GetValue(ForbidAlternativePicLibSourcesProperty);
      set => SetValue(ForbidAlternativePicLibSourcesProperty, value);
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty StartViaThrottleProperty = DependencyProperty.Register(
        "StartViaThrottle",
        typeof(bool),
        typeof(TrainStartSettingsDialog),
        new PropertyMetadata(false));
    public bool StartViaThrottle
    {
      get => (bool)GetValue(StartViaThrottleProperty);
      set => SetValue(StartViaThrottleProperty, value);
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty OptimiseScheduleProperty = DependencyProperty.Register(
        "OptimiseSchedule",
        typeof(bool),
        typeof(TrainStartSettingsDialog),
        new PropertyMetadata(false));
    public bool OptimiseSchedule
    {
      get => (bool)GetValue(OptimiseScheduleProperty);
      set => SetValue(OptimiseScheduleProperty, value);
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty Use_LS3_Renderer_DLLProperty = DependencyProperty.Register(
        "Use_LS3_Renderer_DLL",
        typeof(bool),
        typeof(TrainStartSettingsDialog),
        new PropertyMetadata(false));
    public bool Use_LS3_Renderer_DLL
    {
      get => (bool)GetValue(Use_LS3_Renderer_DLLProperty);
      set => SetValue(Use_LS3_Renderer_DLLProperty, value);
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty StartBildfahrplanProperty = DependencyProperty.Register(
        "StartBildfahrplan",
        typeof(bool),
        typeof(TrainStartSettingsDialog),
        new PropertyMetadata(false));
    public bool StartBildfahrplan
    {
      get => (bool)GetValue(StartBildfahrplanProperty);
      set => SetValue(StartBildfahrplanProperty, value);
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty StartFISProperty = DependencyProperty.Register(
        "StartFIS",
        typeof(bool),
        typeof(TrainStartSettingsDialog),
        new PropertyMetadata(false));
    public bool StartFIS
    {
      get => (bool)GetValue(StartFISProperty);
      set => SetValue(StartFISProperty, value);
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty StartZusiMeterProperty = DependencyProperty.Register(
        "StartZusiMeter",
        typeof(bool),
        typeof(TrainStartSettingsDialog),
        new PropertyMetadata(false));
    public bool StartZusiMeter
    {
      get => (bool)GetValue(StartZusiMeterProperty);
      set => SetValue(StartZusiMeterProperty, value);
    }
    
    //---------------------------------------------------------------------
    public static readonly RoutedUICommand OkCommand = new("Ok", "OkCommand", typeof(TrainStartSettingsDialog));

    //---------------------------------------------------------------------
    public TrainStartSettingsDialog()
    {
      InitializeComponent();

      CommandBindings.Add(new CommandBinding(OkCommand, (s, e) => DialogResult = true));

      Loaded += TrainStartSettingsDialog_Loaded;
    }

    //---------------------------------------------------------------------
    private void TrainStartSettingsDialog_Loaded(object sender, RoutedEventArgs e)
    {
      ForbidAlternativePicLibSources = Properties.Settings.Default.ForbidAlternativePicLibSources;
      StartViaThrottle = Properties.Settings.Default.TrainStartMode == 0;
      OptimiseSchedule = Properties.Settings.Default.OptimiseSchedule == 0;
      Use_LS3_Renderer_DLL = Properties.Settings.Default.Use_LS3_Renderer_DLL == 0;
      StartBildfahrplan = Properties.Settings.Default.StartBildfahrplan == 0;
      StartFIS = Properties.Settings.Default.StartFIS == 0;
      StartZusiMeter = Properties.Settings.Default.StartZusiMeter == 0;
    }
  }
}
