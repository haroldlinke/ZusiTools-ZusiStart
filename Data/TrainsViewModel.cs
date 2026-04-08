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
using ZusiKlassenLib2.Fahrplan;
using ZusiKlassenLib2.TimeTable;
using ZusiStart.Miscellaneous;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using System.Runtime.CompilerServices;
using ZusiKlassenLib2.Vehicle;
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

    //private string _searchText;
    //public string SearchText
    //{
    //  get => _searchText;
    //  set
    //  {
    //    _searchText = value;
    //    RaisePropertyChanged(nameof(SearchText));
    //    //_searchDelayTimer.Stop();
    //    //_searchDelayTimer.Start();
    //  }
    //}

    public ICommand TestCommand { get; }
    public ICommand MarkAsFavoriteCommand { get; }
    public ICommand UnmarkFavoriteCommand { get; }
    public ICommand MarkAsRepTrainCommand { get; }

    //---------------------------------------------------------------------
    public static bool TrainFiltered_ok(Zug m)
    {

      bool train_accepted = true;

      if (!string.IsNullOrEmpty(DataManager.Instance.FilterZugNummer))
      {
        string n = DataManager.Instance.FilterZugNummer.Replace(" ", "").Trim().ToLower();
        train_accepted = string.IsNullOrEmpty(n) || (m.Gattung + m.Nummer).Replace(" ", "").Trim().ToLower().Contains(n) == true;
        //check for Betriebsstelle
        if (train_accepted == false)
        {
          train_accepted = m.FahrplanEintraege.Any(f => f.Bestrst != null && DataManager.IsValidName(f.Bestrst) && f.Bestrst.Replace(" ", "").Trim().ToLower().Contains(n, StringComparison.OrdinalIgnoreCase));
        }
        DataManager.Instance.main_window.GrpTrains.BorderBrush = Brushes.Red;
      }

      return train_accepted;
    }

    //---------------------------------------------------------------------
    public static TrainsViewModel BuildViewModel(TimeTable timeTable)
    {
      TrainsViewModel root = new TrainsViewModel("root");

      DataManager dataManager = DataManager.Instance;

      bool separate_decotrains = DataManager.Instance.options.DecoTrain_Separate;

      if (separate_decotrains)
      {
        var sortOrder = new List<string> { "passenger", "freight", "decopassenger", "decofreight" };

        foreach (var g in dataManager.AllTrains
            .Where(z => z.BelongsToTimeTable == timeTable.ID)
            .GroupBy(z => new
            {
              Type = z.Type,
              IsDeco = z.IsDecoTrain
            })
            .Select(grp => new { 
              TypeID = grp.Key.IsDeco
              ? "deco" + grp.Key.Type.ToString()
              : grp.Key.Type.ToString(),
             Members = grp.ToList() })
            .OrderBy(grp => sortOrder.IndexOf(grp.TypeID.ToLower()))
            )
        {
          TrainsViewModel tvm = new TrainsViewModel(g.TypeID,true);

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
              if (TrainFiltered_ok(m))
              {
                gvm.Children.Add(new TrainsViewModel(m));
              }
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

      }
      else
      {
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
              if (TrainFiltered_ok(m))
              {
                gvm.Children.Add(new TrainsViewModel(m));
              }
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
      _displayName = type == TrainType.Freight ? LocalizationManager.Translate("Güterzüge") : LocalizationManager.Translate("Personenzüge");
      IsBold = true;

    }

    //---------------------------------------------------------------------
    private TrainsViewModel(string type_str,bool test)
        : base(null, true, false)
    {
      if (type_str == "decoFreight")
        _displayName = LocalizationManager.Translate("Deko-Güterzüge");
      else if (type_str == "decoPassenger")
        _displayName = LocalizationManager.Translate("Deko-Personenzüge");
      else if (type_str == "deco")
        _displayName = LocalizationManager.Translate("Deko-Züge");
      else if (type_str == "Freight")
        _displayName = LocalizationManager.Translate("Güterzüge");
      else if (type_str == "Passenger")
        _displayName = LocalizationManager.Translate("Personenzüge");
      else
        _displayName = type_str;
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
          DataManager.Instance.RecentTrains.Save();
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

      MarkAsRepTrainCommand = new RelayCommand<TrainsViewModel>(tvm =>
      {
        Zug train = tvm.Object as Zug;
        ZugDatei trainfile = train.Parent as ZugDatei;
        if (trainfile != null)
        {
          string trainfilename = trainfile.Filename;
          //string timeTableName = System.IO.Path.GetFileNameWithoutExtension(timeTablefilename);

          DataManager.Instance.ReplacementTrains.Add(new ReplacementTrain(trainfilename));
        }
        else
        {
          //Error
          TimeTableFile trainttfile = train.Parent as TimeTableFile;
          FahrzeugVarianten fzgvar = train.Fahrzeuge;
          DataManager.Instance.ReplacementTrains.Add(new ReplacementTrain(fzgvar));
        }
      });

      // Abfahrtszeit
      _startTime = zug.StartTime;
      // Gattung/Nummer
      _displayName = string.IsNullOrEmpty(zug.Gattung) ? zug.Nummer : string.Format("{0} {1}", zug.Gattung, zug.Nummer);
      string baureihe = "";

      List<FahrzeugVarianten> Fahrzeuggruppe = zug.Fahrzeuge.FahrzeugGruppen;

      if (zug.BRAngabe != null && zug.BRAngabe != "")
      {
        baureihe = zug.BRAngabe;
      }
      else
      {

        if (Fahrzeuggruppe.Count > 0)
        {
          foreach (FahrzeugVarianten fg in Fahrzeuggruppe)
          {
            string lokbr = DetLeadingLocoBR(fg);

            if (!string.IsNullOrEmpty(lokbr))
            {
              if (baureihe != "")
                baureihe += "+";
              baureihe += string.Format("{0}", lokbr);
            }
          }
        }
        if (string.IsNullOrEmpty(baureihe))
        {
          FahrzeugVarianten Fahrzeuge = zug.Fahrzeuge;
          if (Fahrzeuge != null)
          {
            string lokbr = DetLeadingLocoBR(Fahrzeuge);
            if (!string.IsNullOrEmpty(lokbr))
            {
              if (baureihe != "")
                baureihe += "+";
              baureihe += string.Format("{0}", lokbr);
            }
          }
          else
          {
          }
        }
      }

      if (!string.IsNullOrEmpty(baureihe))
      {
        _displayName += " (" + baureihe + ")";
      }
      else
      {

      }


      // Fahrzeit
      _journeyTime = zug.JourneyTime;
      // object
      _object = zug;
      _isDecoTrain = zug.IsDecoTrain;

      var recentTrain = DataManager.Instance.RecentTrains.FirstOrDefault(f => f.Train.BelongsToTimeTable == zug.BelongsToTimeTable && f.Train.Nummer == zug.Nummer);

      if (recentTrain != null)
      {
        //_isFavorite = DataManager.Instance.RecentTrains.Any(f => f.Train.BelongsToTimeTable == zug.BelongsToTimeTable && f.Train.Nummer == zug.Nummer);
        _isFavorite = true;
        _comment = "Comment"; // recentTrain.Comment;
        Comment2 = zug.Zuglauf + " - " + recentTrain.Comment;
      }
      else
      {
        _isFavorite = false;
        _comment = "No Comment"; // null;
        Comment2 = zug.Zuglauf;
      }
      _favoriteFill = _isFavorite ? Brushes.Gold : Brushes.Transparent;
    }


    public string DetLeadingLocoBR(FahrzeugVarianten fv)
    {
      string lokbr = "";

      if (fv != null)
      {
        if (fv.FahrzeugGruppen != null && fv.FahrzeugGruppen.Count > 0)
        {
          foreach (var fg in fv.FahrzeugGruppen)
          {
            lokbr = DetLeadingLocoBR(fg);
            if (!string.IsNullOrEmpty(lokbr))
              return lokbr;
          }
        }
        if (lokbr != "")
        {
          return lokbr;
        }
        if (fv.Fahrzeuge != null && fv.Fahrzeuge.Count > 0)
        {
          foreach (FahrzeugInfo fi in fv.Fahrzeuge)
          {
            if (fi.Fahrzeug != null)
            {
              if (fi.Fahrzeug.Varianten != null && fi.Fahrzeug.Varianten.Count > 0)
              {
                foreach (FahrzeugVariante v in fi.Fahrzeug.Varianten)
                {
                  if (v.Drivetrain != null && fi.IDHaupt == v.IDHaupt && fi.IDNeben == v.IDNeben)
                  {
                    return v.BR;
                  }
                }
              }
            }
          }
        }
        else
        {
          return "";
        }
      }
      return "";
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
        if (DataManager.Instance.FilterZugNummer != null)
        {
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
            bool dekozügeallowed = DataManager.Instance.IsDecoTrainsAllowed;
            bool personenzügeallowed = DataManager.Instance.IsPersonTrainsAllowed; ; // (bool)DataManager.Instance.main_window.Checkbox_Personenzüge.IsChecked;
            bool güterzügeallowed = DataManager.Instance.IsFreightTrainsAllowed; ;
            if (personenzügeallowed || güterzügeallowed)
            {
              bool train_ok = (z1.Type == TrainType.Freight && güterzügeallowed) || (z1.Type == TrainType.Passenger && personenzügeallowed);
              e.Accepted = train_ok && (!z1.IsDecoTrain || dekozügeallowed);
            }
            else
            {
              e.Accepted = z1.IsDecoTrain && dekozügeallowed;
            }
            if (e.Accepted == true)
            {
              if (!string.IsNullOrEmpty(n))
              {
                //string zugnr = (z1.Gattung + z1.Nummer).Replace(" ", "").Trim().ToLower();
                string zugnr = tvm.DisplayName.Replace(" ", "").Trim().ToLower();
                bool train_found2 = zugnr.Contains(n) == true;
                e.Accepted = train_found2;

                //return z.FahrplanEintraege.Any(f => f.Bestrst != null && string.Compare(n, f.Bestrst.Replace(" ", "").Trim(), true) == 0);

                //check for Betriebsstelle

                if (train_found2 == false)
                {
                  bool train_found3 = z1.FahrplanEintraege.Any(f => f.Bestrst != null && DataManager.IsValidName(f.Bestrst) && f.Bestrst.Replace(" ", "").Trim().ToLower().Contains(n, StringComparison.OrdinalIgnoreCase));
                  e.Accepted = train_found3;
                }
              }
              else
              {
                e.Accepted = true;
              }
            }

            if (DataManager.SearchTrainValue != null)
            {
              n = DataManager.SearchTrainValue.Replace(" ", "").Trim().ToLower();
              if (!string.IsNullOrEmpty(n))
              {
                string zugnr = (z1.Gattung + z1.Nummer).Replace(" ", "").Trim().ToLower();
                bool train_found2 = zugnr.Contains(n) == true;
                bool train_found3 = false;

                //return z.FahrplanEintraege.Any(f => f.Bestrst != null && string.Compare(n, f.Bestrst.Replace(" ", "").Trim(), true) == 0);

                //check for Betriebsstelle

                if (train_found2 == false)
                {
                  train_found3 = z1.FahrplanEintraege.Any(f => f.Bestrst != null && DataManager.IsValidName(f.Bestrst) && f.Bestrst.Replace(" ", "").Trim().ToLower().Contains(n, StringComparison.OrdinalIgnoreCase));
                }
                e.Accepted = e.Accepted && (train_found3 || train_found2);
              }
            }
          }
        }

        if (DataManager.SearchVehicleGroupValue != null)
        {
          if (tvm.Object is Zug z3)
          {
            bool vehiclegroup_found = z3.Fahrzeuge.ContainsVehicle(DataManager.SearchVehicleGroupValue.AllVariants) == true;
            e.Accepted = e.Accepted && vehiclegroup_found;
          }
        }
      }

      else
      {
        e.Accepted = false;
      }
    }
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

  }
}

