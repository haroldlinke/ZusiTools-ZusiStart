using Sovoma;
using Sovoma.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using ZusiKlassenLib.Fahrplan;
using ZusiKlassenLib.TimeTable;
using ZusiStart.Miscellaneous;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using System.Runtime.CompilerServices;
//using ZusiCLIProject.FileLibrary.Zusi3;
//using ZusiCLIProject.FileLibrary.Zusi3;

namespace ZusiStart.Data
{
  //=========================================================================
  public class TrainsViewModel : BaseTreeViewViewModel<TrainsViewModel, Zug>
  {
    private readonly bool _hasSortButton;
    private readonly bool _isDecoTrain;
    private readonly DateTime? _startTime;
    private readonly TimeSpan? _journeyTime;
    private bool _isFavorite;
    private Brush _favoriteFill;
    private string? _comment;

    public bool HasSortButton { get => _hasSortButton; }
    public bool IsDecoTrain { get => _isDecoTrain; }
    public TimeSpan? JourneyTime { get => _journeyTime; }
    public DateTime? StartTime { get => _startTime; }
    public string? Comment2
    {
      get => _comment;
      set
      {
        _comment = value;
      }
    }
    //public bool IsFavorite { get => _isfavorite; }
    //public Brush FavoriteFill { get => _favoriteFill; }
    public Brush FavoriteFill => IsFavorite ? Brushes.Gold : Brushes.Transparent;

    public bool IsFavorite
    {
      get => _isFavorite;
      set
      {
        if (_isFavorite != value)
        {
          _isFavorite = value;
          RaisePropertyChanged(nameof(IsFavorite));
          RaisePropertyChanged(nameof(FavoriteFill)); // Notify UI to update star color
        }
      }
    }

    public ICommand TestCommand { get; }
    public ICommand MarkAsFavoriteCommand { get; }
    public ICommand UnmarkFavoriteCommand { get; }


    //---------------------------------------------------------------------
    public static TrainsViewModel BuildViewModel(TimeTable timeTable)
    {
      TrainsViewModel root = new TrainsViewModel("root");

      DataManager dataManager = DataManager.Instance;

      foreach (var g in dataManager.AllTrains
          .Where(z => z.BelongsToTimeTable == timeTable.ID)
          .GroupBy(z => z.Type)
          .Select(grp => new { TypeID = grp.Key, Members = grp.ToList() }))
      {
        TrainsViewModel tvm = new TrainsViewModel(g.TypeID);

        foreach (var mg in g.Members.GroupBy(zz => zz.FahrplanGruppe)
            .Select(grp => new
            {
              GroupID = grp.Key,
              Members = grp.OrderBy(o =>
                      {
                  FahrplanEintrag? fpe = null;
                  try
                  {
                    fpe = o.FahrplanEintraege.First(fe => fe.Departure != null);
                  }
                  catch { }
                  return fpe != null ? fpe.Departure.Value : default(DateTime);
                }).ToList()
            })
            .OrderBy(o => o.GroupID))
        {
          TrainsViewModel gvm = new TrainsViewModel(mg.GroupID);

          mg.Members.ForEach(m =>
          {
            gvm.Children.Add(new TrainsViewModel(m));
          });

          if (gvm.Children.Count >= 0)
          {
            tvm.Children.Add(gvm);
          }
        }

        if (tvm.Children.Count >= 0)
        {
          root.Children.Add(tvm);
        }
      }

      root.Initialize();
      return root;
    }

    //---------------------------------------------------------------------
    public static TrainsViewModel BuildViewModel(List<Zug> trains)
    {
      TrainsViewModel root = new TrainsViewModel("root");

      DataManager dataManager = DataManager.Instance;

      foreach (var g in trains.GroupBy(z => z.Type)
          .Select(grp => new { TypeID = grp.Key, Members = grp.ToList() }))
      {
        TrainsViewModel tvm = new TrainsViewModel(g.TypeID);

        foreach (var mg in g.Members.GroupBy(zz => zz.FahrplanGruppe)
            .Select(grp => new
            {
              GroupID = grp.Key,
              Members = grp.OrderBy(o =>
                      {
                  FahrplanEintrag? fpe = null;
                  try
                  {
                    fpe = o.FahrplanEintraege.First(fe => fe.Departure != null);
                  }
                  catch { }
                  return fpe != null ? fpe.Departure.Value : default(DateTime);
                }).ToList()
            })
            .OrderBy(o => o.GroupID))
        {
          TrainsViewModel gvm = new TrainsViewModel(mg.GroupID);

          mg.Members.ForEach(m =>
          {
            gvm.Children.Add(new TrainsViewModel(m));
          });

          if (gvm.Children.Count > 0)
          {
            tvm.Children.Add(gvm);
          }
        }

        if (tvm.Children.Count > 0)
        {
          root.Children.Add(tvm);
        }
      }

      root.Initialize();
      return root;
    }

    //---------------------------------------------------------------------
    public TrainsViewModel(TrainsViewModel parent, bool expanded, bool selected)
        : base(parent, expanded, selected)
    {
      
    }

    //---------------------------------------------------------------------
    private TrainsViewModel(string caption)
        : this(null, true, false)
    {
      _displayName = caption?.Trim();
      _hasSortButton = true;
      IsBold = true;
     
    }

    //---------------------------------------------------------------------
    private TrainsViewModel(TrainType type)
        : base(null, true, false)
    {
      _displayName = type == TrainType.Freight ? "Güterzüge" : "Personenzüge";
      IsBold = true;
      
    }

    //---------------------------------------------------------------------
    private TrainsViewModel(Zug zug)
        : this(null, true, false)
    {
      
      MarkAsFavoriteCommand = new RelayCommand<TrainsViewModel>(tvm =>
      {
        Zug train = tvm.Object as Zug;
        string timeTablefilename = train.FahrplanDatei.FullPath;
        string timeTableName = System.IO.Path.GetFileNameWithoutExtension(timeTablefilename);
        RecentTrain rt = new(train, timeTableName);

        if (!DataManager.Instance.RecentTrains.Any(f => f.Train.BelongsToTimeTable == train.BelongsToTimeTable && f.Train.Nummer == train.Nummer))
        {
          DataManager.Instance.RecentTrains.Add(rt);
          IsFavorite = true;
        }

          
      });
      UnmarkFavoriteCommand = new RelayCommand<TrainsViewModel>(tvm =>
      {
        Zug train = tvm.Object as Zug;
        string timeTablefilename = train.FahrplanDatei.FullPath;
        string timeTableName = System.IO.Path.GetFileNameWithoutExtension(timeTablefilename);
        RecentTrain rt = new(train, timeTableName);

        var recentTrain = DataManager.Instance.RecentTrains
    .FirstOrDefault(f => f.Train.BelongsToTimeTable == train.BelongsToTimeTable &&
                         f.Train.Nummer == train.Nummer);

        if (recentTrain != null)
        {
          DataManager.Instance.RecentTrains.Remove(recentTrain);
          IsFavorite = false;
        }
      });

      // Abfahrtszeit
      _startTime = zug.StartTime;
      // Gattung/Nummer
      _displayName = string.IsNullOrEmpty(zug.Gattung) ? zug.Nummer : string.Format("{0} {1}", zug.Gattung, zug.Nummer);
      // Fahrzeit
      _journeyTime = zug.JourneyTime;
      // object
      _object = zug;
      _isDecoTrain = zug.IsDecoTrain;

      var recentTrain = DataManager.Instance.RecentTrains
    .FirstOrDefault(f => f.Train.BelongsToTimeTable == zug.BelongsToTimeTable &&
                         f.Train.Nummer == zug.Nummer);

      if (recentTrain != null)
      {
        //_isFavorite = DataManager.Instance.RecentTrains.Any(f => f.Train.BelongsToTimeTable == zug.BelongsToTimeTable && f.Train.Nummer == zug.Nummer);
        _isFavorite = true;
        _comment = "Comment"; // recentTrain.Comment;
        Comment2 = "Comment2";
      }
      else
      {
        _isFavorite = false;
        _comment = "No Comment"; // null;
        Comment2 = "No Comment2";
      }
      _favoriteFill = _isFavorite ? Brushes.Gold : Brushes.Transparent;
    }
  }

  [ValueConversion(typeof(TimeSpan?), typeof(string))]
  public class JourneyTimeConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is TimeSpan ts)
      {
        return string.Format("{0} Min.", Math.Round(ts.TotalMinutes));
      }
      return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  [ValueConversion(typeof(DateTime?), typeof(string))]
  public class StartTimeConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is DateTime dt)
      {
        return dt.ToString("HH:mm");
      }
      return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class ViewSourceConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      CollectionViewSource cvs = new CollectionViewSource() { Source = value };
      cvs.Filter += Cvs_Filter;
      return cvs.View;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    private void Cvs_Filter(object sender, FilterEventArgs e)
    {
      if (e.Item is TrainsViewModel tvm)
      {
        //e.Accepted = tvm.Object is Zug z ? !z.IsDecoTrain || DataManager.Instance.IsDecoTrainsAllowed : true;

        string n = DataManager.Instance.FilterZugNummer.Replace(" ", "").Trim().ToLower();
        //if (n != "")
        //{
        //  //bool train_found = tvm.Object is Zug z2 ? (string.Contains(n, (z2.Nummer).Replace(" ", "").Trim()) == 0 ||
        //  //          string.Contains(n, (z2.Gattung + z2.Nummer).Replace(" ", "").Trim()) == 0):true;

        //  bool train_found = tvm.Object is Zug z2 ? (string.IsNullOrEmpty(n) || z2.Nummer.Contains(n) == true) : true;
        //  e.Accepted = e.Accepted && train_found;
        //}

        Zug z1 = tvm.Object as Zug;
        if (z1 == null)
        {
          e.Accepted = true;
        }
        else
        {
          //e.Accepted = !z1.IsDecoTrain || DataManager.Instance.IsDecoTrainsAllowed;
          bool dekozügeallowed = (bool)DataManager.Instance.main_window.Checkbox_Dekozüge.IsChecked;
          e.Accepted = !z1.IsDecoTrain || dekozügeallowed;
          if (e.Accepted==true)
          {
            if (!string.IsNullOrEmpty(n))
            {
              string zugnr = (z1.Gattung + z1.Nummer).Replace(" ", "").Trim().ToLower();
              bool train_found2 = zugnr.Contains(n) == true;
              e.Accepted = train_found2;

              //return z.FahrplanEintraege.Any(f => f.Bestrst != null && string.Compare(n, f.Bestrst.Replace(" ", "").Trim(), true) == 0);

              //check for Betriebsstelle

              if (train_found2 == false)
              {
                bool train_found3 = z1.FahrplanEintraege.Any(f => f.Bestrst != null && f.Bestrst.Replace(" ", "").Trim().ToLower().Contains(n, StringComparison.OrdinalIgnoreCase));
                e.Accepted = train_found3;
              }
            }
          }
        }
      }
      
      else
      {
        e.Accepted = false;
      }
    }
   
  }
}
