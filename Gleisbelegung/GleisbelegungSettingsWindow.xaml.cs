using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ZusiStart.Controls;
using ZusiStart.Data;
using static ZusiStart.Controls.GleisbelegungControl;

namespace ZusiStart.Gleisbelegung
{
  /// <summary>
  /// Interaktionslogik für GleisbelegungSettingsWindow.xaml
  /// </summary>
  /// 
  public class TrainTypeColor
  {
    public string TrainType { get; set; }
    public Color Color { get; set; }
  }

  public class TrainTypeColorConfig
  {
    public List<TrainTypeColor> Mappings { get; set; } = new();
  }

  public static class TrainTypeColorManager
  {

    private static string FilePath => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DataManager.localfoldername, "TrainTypeColors.json");
    //private static string FilePath => "TrainTypeColors.json";

    public static TrainTypeColorConfig Load()
    {
      if (!File.Exists(FilePath))
        return new TrainTypeColorConfig();

      try
      {
        string json = File.ReadAllText(FilePath);
        var loaded = JsonSerializer.Deserialize<TrainTypeColorConfig>(json);
        return loaded ?? new TrainTypeColorConfig();
      }
      catch
      {
        return new TrainTypeColorConfig();
      }
    }
    public static void Save(TrainTypeColorConfig config)
    {
      string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
      File.WriteAllText(FilePath, json);
    }
  }

  public static class GleisbelegungColorManager
  {
    //private static string FilePath => "GleisbelegungColors.json";
    private static string FilePath => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DataManager.localfoldername, "GleisbelegungColors.json");

    public static GleisbelegungColors Load()
    {
      if (!File.Exists(FilePath))
        return new GleisbelegungColors();

      try
      {
        var json = File.ReadAllText(FilePath);
        var loaded = JsonSerializer.Deserialize<GleisbelegungColors>(json);

        // Fallback für fehlende Werte
        return loaded ?? new GleisbelegungColors();
      }
      catch
      {
        return new GleisbelegungColors();
      }
    }


    public static void Save(GleisbelegungColors colors)
    {
      string json = JsonSerializer.Serialize(colors, new JsonSerializerOptions { WriteIndented = true });
      File.WriteAllText(FilePath, json);
    }
  }



  public partial class GleisbelegungSettingsWindow : Window
  {
    private GleisbelegungColors _colors;
    private TrainTypeColorConfig _trainTypes;

    public GleisbelegungSettingsWindow()
    {
      InitializeComponent();

      _colors = GleisbelegungColorManager.Load();
      _trainTypes = TrainTypeColorManager.Load();

      GridTrainTypes.ItemsSource = _trainTypes.Mappings;
    }



    private Color PickColor(Color current)
    {
      var dlg = new System.Windows.Forms.ColorDialog
      {
        Color = System.Drawing.Color.FromArgb(current.A, current.R, current.G, current.B)
      };

      if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        return Color.FromArgb(dlg.Color.A, dlg.Color.R, dlg.Color.G, dlg.Color.B);
      }

      return current;
    }
    private void PickBackground(object sender, RoutedEventArgs e)
    {
      _colors.Background = PickColor(_colors.Background);
    }

    private void PickTimeLine(object sender, RoutedEventArgs e)
    {
      _colors.TimeLine = PickColor(_colors.TimeLine);
    }

    private void PickTrackLine(object sender, RoutedEventArgs e)
    {
      _colors.TrackLine = PickColor(_colors.TrackLine);
    }

    private void PickConflictBorder(object sender, RoutedEventArgs e)
    {
      _colors.ConflictBorder = PickColor(_colors.ConflictBorder);
    }

    private void PickConflictOverlay(object sender, RoutedEventArgs e)
    {
      _colors.ConflictOverlay = PickColor(_colors.ConflictOverlay);
    }

    private void PickTrainTypeColor(object sender, MouseButtonEventArgs e)
    {
      if (sender is Border b && b.DataContext is TrainTypeColor entry)
      {
        entry.Color = PickColor(entry.Color);
        GridTrainTypes.Items.Refresh();
      }
    }

    private void AddTrainType(object sender, RoutedEventArgs e)
    {
      _trainTypes.Mappings.Add(new TrainTypeColor
      {
        TrainType = "",
        Color = Colors.SteelBlue
      });

      GridTrainTypes.Items.Refresh();
    }

    private void DeleteTrainType(object sender, RoutedEventArgs e)
    {
      if (sender is Button btn && btn.DataContext is TrainTypeColor entry)
      {
        _trainTypes.Mappings.Remove(entry);
        GridTrainTypes.Items.Refresh();
      }
    }

    private void SaveAll(object sender, RoutedEventArgs e)
    {
      GleisbelegungColorManager.Save(_colors);
      TrainTypeColorManager.Save(_trainTypes);

      MessageBox.Show("Einstellungen gespeichert.");
      Close();
    }

    private void Cancel(object sender, RoutedEventArgs e)
    {
      Close();
    }

  }

}
