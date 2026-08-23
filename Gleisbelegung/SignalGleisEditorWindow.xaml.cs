using System;
using System.Collections.Generic;
using System.IO;
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
using ZusiStart.Controls;

namespace ZusiStart.Gleisbelegung
{
  /// <summary>
  /// Interaktionslogik für SignalGleisEditorWindow.xaml
  /// </summary>
  /// 
  public class SignalGleisRow
  {
    public string Signal { get; set; }
    public int Gleis { get; set; }
    public string Richtung { get; set; }
  }

  public partial class SignalGleisEditorWindow : Window
  {
    private string _file;
    private List<SignalGleisRow> _rows = new();

    public SignalGleisEditorWindow(string stationName)
    {
      InitializeComponent();

      _file = GleisbelegungControl.GetMappingFileForStation(stationName);

      LoadCsv();
      Grid.ItemsSource = _rows;
    }

    private void LoadCsv()
    {
      _rows.Clear();

      if (!File.Exists(_file))
        return;

      foreach (var line in File.ReadAllLines(_file).Skip(1))
      {
        var parts = line.Split(',');
        if (parts.Length >= 2 && int.TryParse(parts[1], out int gleis))
        {
          _rows.Add(new SignalGleisRow
          {
            Signal = parts[0],
            Gleis = gleis,
            Richtung = parts.Length >= 3 ? parts[2] : ""
          });
        }
      }
    }

    private void SaveCsv()
    {
      using (var sw = new StreamWriter(_file, false))
      {
        sw.WriteLine("Signal,Gleis,Richtung");

        foreach (var row in _rows)
        {
          sw.WriteLine($"{row.Signal},{row.Gleis},{row.Richtung}");
        }
      }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
      SaveCsv();
      MessageBox.Show("Zuordnungen gespeichert.", "OK", MessageBoxButton.OK);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
      SaveCsv();
      Close();
    }

    private void DeleteRow_Click(object sender, RoutedEventArgs e)
    {
      if (sender is Button btn && btn.DataContext is SignalGleisRow entry)
      {
        if (MessageBox.Show($"Signal '{entry.Signal}' wirklich löschen?",
                            "Löschen",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
          _rows.Remove(entry);
          Grid.Items.Refresh();
        }
      }
    }

    private void AddSignal_Click(object sender, RoutedEventArgs e)
    {
      var newEntry = new SignalGleisRow
      {
        Signal = "",
        Gleis = 0,
        Richtung = ""
      };

      _rows.Add(newEntry);
      Grid.Items.Refresh();
      // Optional: direkt in Edit‑Modus springen
      Grid.SelectedItem = newEntry;
      Grid.ScrollIntoView(newEntry);
    }

  }

}
