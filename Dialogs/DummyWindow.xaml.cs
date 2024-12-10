using log4net;
using Sovoma;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ZusiKlassenLib.Common;
using ZusiKlassenLib.Landscape;
using ZusiKlassenLib.Vehicle;
using ZusiPicLib;

namespace ZusiStart
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class DummyWindow : Window
    {
        [Flags]
        private enum Prerequisites
        {
            None = 0,
            HasDataFolder = (1 << 0),
            HasProperRights = (1 << 1),
            HasClassesFile = (1 << 2),
            HasPicLibFolder = (1 << 3),
            HasPictureLib = (1 << 4),
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Prerequisites _prerequisites = Prerequisites.None;

        private static readonly DependencyPropertyKey _currentCommandKey = DependencyProperty.RegisterReadOnly(
            "CurrentCommand",
            typeof(RoutedUICommand),
            typeof(MainWindow),
            new PropertyMetadata(CancelCommand));
        public static readonly DependencyProperty CurrentCommandProperty = _currentCommandKey.DependencyProperty;
        public RoutedUICommand CurrentCommand
        {
            get => (RoutedUICommand)GetValue(CurrentCommandProperty);
            private set => SetValue(_currentCommandKey, value);
        }

        private static readonly DependencyPropertyKey _foundVehiclesKey = DependencyProperty.RegisterReadOnly(
            "FoundVehicles",
            typeof(string),
            typeof(MainWindow),
            new PropertyMetadata(null));
        public static readonly DependencyProperty FoundVehiclesProperty = _foundVehiclesKey.DependencyProperty;
        public string FoundVehicles
        {
            get => (string)GetValue(FoundVehiclesProperty);
            private set => SetValue(_foundVehiclesKey, value);
        }

        private static readonly DependencyPropertyKey _progressPackingKey = DependencyProperty.RegisterReadOnly(
            "ProgressPacking",
            typeof(double),
            typeof(MainWindow),
            new PropertyMetadata(0.0));
        public static readonly DependencyProperty ProgressPackingProperty = _progressPackingKey.DependencyProperty;
        public double ProgressPacking
        {
            get => (double)GetValue(ProgressPackingProperty);
            set => SetValue(_progressPackingKey, value);
        }

        private static readonly DependencyPropertyKey _progressPhotographingKey = DependencyProperty.RegisterReadOnly(
            "ProgressPhotographing",
            typeof(double),
            typeof(MainWindow),
            new PropertyMetadata(0.0));
        public static readonly DependencyProperty ProgressPhotographingProperty = _progressPhotographingKey.DependencyProperty;
        public double ProgressPhotographing
        {
            get => (double)GetValue(ProgressPhotographingProperty);
            private set => SetValue(_progressPhotographingKey, value);
        }

        private static readonly DependencyPropertyKey _stepKey = DependencyProperty.RegisterReadOnly(
            "Step",
            typeof(int),
            typeof(MainWindow),
            new PropertyMetadata(-1));
        public static readonly DependencyProperty StepProperty = _stepKey.DependencyProperty;
        public int Step
        {
            get => (int)GetValue(StepProperty);
            private set => SetValue(_stepKey, value);
        }

        public static readonly RoutedUICommand CancelCommand = new RoutedUICommand("Abbrechen", "CancelCommand", typeof(MainWindow));
        public static readonly RoutedUICommand OkCommand = new RoutedUICommand("Schließen", "OkCommand", typeof(MainWindow));

        public DummyWindow()
        {
                InitializeComponent();
        }
    }

    [ValueConversion(typeof(int), typeof(Brush))]
    public class Step2BrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i && parameter is string sp)
            {
                if (int.TryParse(sp, out int p))
                {
                    return i == p ? Brushes.Goldenrod : Brushes.Transparent;
                }
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
