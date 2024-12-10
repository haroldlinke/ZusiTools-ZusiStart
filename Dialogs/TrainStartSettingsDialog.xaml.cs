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
        public static readonly DependencyProperty OptimiseScheduleCriteriaProperty = DependencyProperty.Register(
            "OptimiseScheduleCriteria",
            typeof(bool),
            typeof(TrainStartSettingsDialog),
            new PropertyMetadata(false));
        public bool OptimiseScheduleCriteria
        {
            get => (bool)GetValue(OptimiseScheduleCriteriaProperty);
            set => SetValue(OptimiseScheduleCriteriaProperty, value);
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
            OptimiseScheduleCriteria = Properties.Settings.Default.OptimiseScheduleCriteria == 0;
        }
    }
}
