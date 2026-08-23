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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ZusiCLIProject.FileLibrary.Zusi3;
using ZusiKlassenLib2;
using ZusiStart.Data;

namespace ZusiStart.Gleisbelegung
{
  /// <summary>
  /// Interaktionslogik für GleisbelegungWindow.xaml
  /// </summary>
  public partial class GleisbelegungWindow : Window
  {
    public GleisbelegungWindow(Zug[] zugliste, string station)
    {
      InitializeComponent();

      GleisControl.Zugliste = zugliste;
      GleisControl.StationName = station;
      TimeTableRelation timetablerelation = DataManager.Instance.SelectedTimeTableRelation;
      
      GleisControl.FahrplanName = timetablerelation != null ? timetablerelation.TimeTableName : "Unbekannt";
    }

    public GleisbelegungWindow(ZusiKlassenLib2.Fahrplan.Zug[] zugliste, string station)
    {
      InitializeComponent();

      GleisControl.Zugliste2 = zugliste;
      GleisControl.StationName = station;
      TimeTableRelation timetablerelation = DataManager.Instance.SelectedTimeTableRelation;
      GleisControl.FahrplanName = timetablerelation != null ? timetablerelation.TimeTableName : "Unbekannt";
    }

    private void EditMapping_Click(object sender, RoutedEventArgs e)
    {
      var win = new SignalGleisEditorWindow(GleisControl.StationName);

      // Das übergeordnete Window ermitteln
      Window parent = Window.GetWindow(this);
      if (parent != null)
        win.Owner = parent;

      win.ShowDialog();

      GleisControl.LoadSignalMapping(GleisControl.StationName);
      GleisControl.Update();
    }

    private void ColorMapping_Click(object sender, RoutedEventArgs e)
    {
      var win = new GleisbelegungSettingsWindow();

      try
      {
        // Das übergeordnete Window ermitteln
        Window parent = Window.GetWindow(this);
        if (parent != null)
          win.Owner = parent;

        win.ShowDialog();

        //GleisControl.LoadSignalMapping(GleisControl.StationName);
        GleisControl.Update();
      }
      catch
      {

      }
    }

  }
}
