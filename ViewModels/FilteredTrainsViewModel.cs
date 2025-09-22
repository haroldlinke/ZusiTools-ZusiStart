using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;

public class Train
{
  public string DisplayName { get; set; }
  public string Category { get; set; } // Optional für spätere Erweiterung
  public string Operator { get; set; } // Optional
}

public class FilteredTrainsViewModel : INotifyPropertyChanged
{
  public ObservableCollection<Train> AllTrains { get; } = new ObservableCollection<Train>();
  public ICollectionView FilteredTrains { get; }

  private bool _showPersonenzüge = true;
  public bool ShowPersonenzüge
  {
    get => _showPersonenzüge;
    set
    {
      _showPersonenzüge = value;
      OnPropertyChanged(nameof(ShowPersonenzüge));
      FilteredTrains.Refresh();
    }
  }

  private bool _showGüterzüge = true;
  public bool ShowGüterzüge
  {
    get => _showGüterzüge;
    set
    {
      _showGüterzüge = value;
      OnPropertyChanged(nameof(ShowGüterzüge));
      FilteredTrains.Refresh();
    }
  }

  private string _searchText;
  public string SearchText
  {
    get => _searchText;
    set
    {
      _searchText = value;
      OnPropertyChanged(nameof(SearchText));
      _searchDelayTimer.Stop();
      _searchDelayTimer.Start();
    }
  }

  private readonly DispatcherTimer _searchDelayTimer;

  public FilteredTrainsViewModel()
  {
    // Beispiel-Daten
    AllTrains.Add(new Train { DisplayName = "Personenzüge" });
    AllTrains.Add(new Train { DisplayName = "Güterzüge" });
    AllTrains.Add(new Train { DisplayName = "Schnellzug" });

    FilteredTrains = CollectionViewSource.GetDefaultView(AllTrains);
    FilteredTrains.Filter = FilterTrains;

    _searchDelayTimer = new DispatcherTimer
    {
      Interval = TimeSpan.FromMilliseconds(300)
    };
    _searchDelayTimer.Tick += (s, e) =>
    {
      _searchDelayTimer.Stop();
      FilteredTrains.Refresh();
    };
  }

  private bool FilterTrains(object obj)
  {
    if (obj is not Train train) return false;

    bool typeMatch = (!ShowPersonenzüge && !ShowGüterzüge) ||
                     (ShowPersonenzüge && train.DisplayName == "Personenzüge") ||
                     (ShowGüterzüge && train.DisplayName == "Güterzüge");

    bool textMatch = string.IsNullOrWhiteSpace(SearchText) ||
                     train.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase);

    return typeMatch && textMatch;
  }

  public event PropertyChangedEventHandler PropertyChanged;
  protected void OnPropertyChanged(string name) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

