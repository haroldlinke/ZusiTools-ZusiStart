using Sovoma;
using Sovoma.ControlsKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ZusiStart.Data;
using ZusiStart.Miscellaneous;

namespace ZusiStart.Dialogs
{
  /// <summary>
  /// Interaktionslogik für DataLoaderWindow.xaml
  /// </summary>
  public partial class DataLoaderWindow : Window
  {
    private bool _closingCompleted;

    private static readonly DependencyPropertyKey _copyrightKey = DependencyProperty.RegisterReadOnly(
        "Copyright",
        typeof(string),
        typeof(DataLoaderWindow),
        new PropertyMetadata(AsmInfo.Copyright));
    public static readonly DependencyProperty CopyrightProperty = _copyrightKey.DependencyProperty;
    public string Copyright
    {
      get { return (string)GetValue(CopyrightProperty); }
      private set { SetValue(_copyrightKey, value); }
    }

    public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
        "Progress",
        typeof(double),
        typeof(DataLoaderWindow),
        new PropertyMetadata(0.0));
    public double Progress
    {
      get { return (double)GetValue(ProgressProperty); }
      set { SetValue(ProgressProperty, value); }
    }

    private static readonly DependencyPropertyKey _txtLoadKey = DependencyProperty.RegisterReadOnly(
        "TextLoad",
        typeof(string),
        typeof(DataLoaderWindow),
        new PropertyMetadata("die Daten werden geladen ..."));
    public static readonly DependencyProperty TextLoadProperty = _txtLoadKey.DependencyProperty;
    public string TextLoad
    {
      get { return (string)GetValue(TextLoadProperty); }
      private set { SetValue(_txtLoadKey, value); }
    }

    private static readonly DependencyPropertyKey _txtPatientKey = DependencyProperty.RegisterReadOnly(
        "TextPatient",
        typeof(string),
        typeof(DataLoaderWindow),
        new PropertyMetadata("Bitte ein paar Sekunden Geduld,"));
    public static readonly DependencyProperty TextPatientProperty = _txtPatientKey.DependencyProperty;
    public string TextPatient
    {
      get { return (string)GetValue(TextPatientProperty); }
      private set { SetValue(_txtPatientKey, value); }
    }

    private static readonly DependencyPropertyKey _versionKey = DependencyProperty.RegisterReadOnly(
        "Version",
        typeof(Version),
        typeof(DataLoaderWindow),
        new PropertyMetadata(AsmInfo.Version));
    public static readonly DependencyProperty VersionProperty = _versionKey.DependencyProperty;
    public Version Version
    {
      get { return (Version)GetValue(VersionProperty); }
      private set { SetValue(_versionKey, value); }
    }

    public DataLoaderWindow()
    {
      InitializeComponent();

      Closing += DataLoaderWindow_Closing;
    }

    public void SetLoaderType(LoaderType loaderType,string add_info="")
    {
      switch (loaderType)
      {
        case LoaderType.LoadComplete:
          TextPatient = "Bitte etwas Geduld,";
          TextLoad = "die Fahrpläne und Züge werden gelesen ...";
          Waitspinner.Visibility = Visibility.Visible;
          //pbLoaded.Foreground = Brushes.Red;
          break;
        case LoaderType.LoadFromCache:
          TextPatient = "Bitte ein paar Sekunden Geduld,";
          TextLoad = "die Daten werden geladen ...";
          Waitspinner.Visibility = Visibility.Visible;
          //pbLoaded.Foreground = Brushes.Green;
          break;
        case LoaderType.LoadVehicleGroups:
          TextPatient = "Bitte etwas Geduld,";
          TextLoad = "die Fahrzeugdaten werden verarbeitet und, wenn notwendig,\n neue Fahrzeugbilder erstellt (dies kann etwas dauern) ...";
          if (add_info != "")
          {
            TextLoad = TextLoad + "\n" + add_info;
          }
          Waitspinner.Visibility = Visibility.Collapsed;
          //pbLoaded.Foreground = Brushes.Green;
          break;
      }

    }

    private void DataLoaderWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (!_closingCompleted)
      {
        Duration duration = new(TimeSpan.FromMilliseconds(600));
        DoubleAnimation da = new(0, duration);
        da.Completed += Closing_Completed;
        BeginAnimation(OpacityProperty, da);
        e.Cancel = true;
      }
    }

    private void Closing_Completed(object sender, EventArgs e)
    {
      _closingCompleted = true;
      Close();
    }
  }
}
