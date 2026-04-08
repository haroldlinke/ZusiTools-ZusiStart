using log4net;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Fahrplan;
using ZusiKlassenLib2.Vehicle;
using ZusiStart.Dialogs;
using ZusiStart.Data;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace ZusiStart.Data
{
  public class TrainItem : DependencyObject
  {
    private readonly string? _abfahrtsZeit;
    private readonly string? _nummer;
    private readonly ZugDatei? _zugDatei;
    private static readonly ILog _log = LogManager.GetLogger(typeof(DataManager));
    private readonly Zug? _zug;
    private ZugReihung? _zugReihung;
    private ZugReihung? _replaceZugReihung;
    private LinkedListNode<FahrzeugInfo>? _firstLoco;
    private LinkedListNode<FahrzeugInfo>? _secondLoco;
    private bool _firstIsLeadingLoco;

    private static readonly DependencyPropertyKey _hasDoubleHeadingKey = DependencyProperty.RegisterReadOnly(
        "HasDoubleHeading",
        typeof(bool),
        typeof(TrainItem),
        new PropertyMetadata(false));
    public static readonly DependencyProperty HasDoubleHeadingProperty = _hasDoubleHeadingKey.DependencyProperty;
    public bool HasDoubleHeading
    {
      get => (bool)GetValue(HasDoubleHeadingProperty);
      private set => SetValue(_hasDoubleHeadingKey, value);
    }

    public static readonly DependencyProperty IsImportantProperty = DependencyProperty.Register(
        "IsImportant",
        typeof(bool),
        typeof(TrainItem),
        new PropertyMetadata(true));
    public bool IsImportant
    {
      get => (bool)GetValue(IsImportantProperty);
      set => SetValue(IsImportantProperty, value);
    }

    private static readonly DependencyPropertyKey _isLocoReplacedKey = DependencyProperty.RegisterReadOnly(
        "IsLocoReplaced",
        typeof(bool),
        typeof(TrainItem),
        new PropertyMetadata(false));
    public static readonly DependencyProperty IsLocoReplacedProperty = _isLocoReplacedKey.DependencyProperty;
    public bool IsLocoReplaced
    {
      get => (bool)GetValue(IsLocoReplacedProperty);
      private set => SetValue(_isLocoReplacedKey, value);
    }

    private static readonly DependencyPropertyKey _isTrainReplacedKey = DependencyProperty.RegisterReadOnly(
        "IsTrainReplaced",
        typeof(bool),
        typeof(TrainItem),
        new PropertyMetadata(false));
    public static readonly DependencyProperty IsTrainReplacedProperty = _isTrainReplacedKey.DependencyProperty;
    public bool IsTrainReplaced
    {
      get => (bool)GetValue(IsTrainReplacedProperty);
      private set => SetValue(_isTrainReplacedKey, value);
    }

    private static readonly DependencyPropertyKey _isLocoInFrontKey = DependencyProperty.RegisterReadOnly(
        "IsLocoInFront",
        typeof(bool),
        typeof(TrainItem),
        new PropertyMetadata(false));
    public static readonly DependencyProperty IsLocoInFrontProperty = _isLocoInFrontKey.DependencyProperty;
    public bool IsLocoInFront
    {
      get => (bool)GetValue(IsLocoInFrontProperty);
      private set => SetValue(_isLocoInFrontKey, value);
    }

    private static readonly DependencyPropertyKey _isTrainTurnedKey = DependencyProperty.RegisterReadOnly(
       "IsTrainTurned",
       typeof(bool),
       typeof(TrainItem),
       new PropertyMetadata(false));
    public static readonly DependencyProperty IsTrainTurnedProperty = _isTrainTurnedKey.DependencyProperty;
    public bool IsTrainTurned
    {
      get => (bool)GetValue(IsLocoInFrontProperty);
      private set => SetValue(_isLocoInFrontKey, value);
    }

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
        "IsSelected",
        typeof(bool),
        typeof(TrainItem),
        new PropertyMetadata(false, OnIsSelectedChanged));
    public bool IsSelected
    {
      get => (bool)GetValue(IsSelectedProperty);
      set => SetValue(IsSelectedProperty, value);
    }

    private static readonly DependencyPropertyKey _locoKey = DependencyProperty.RegisterReadOnly(
        "Loco",
        typeof(string),
        typeof(TrainItem),
        new PropertyMetadata(null));
    public static readonly DependencyProperty LocoProperty = _locoKey.DependencyProperty;
    public string Loco
    {
      get => (string)GetValue(LocoProperty);
      private set => SetValue(_locoKey, value);
    }

    public string AbfahrtsZeit => _abfahrtsZeit;
    public string Nummer => _nummer;
    public ZugReihung Reihung => _zugReihung;
    public ZugReihung ReplaceReihung => _replaceZugReihung;
    public Zug Zug => _zug;
    public Zug OrigZug;
    public ZugDatei ZugDatei => _zugDatei;
    public string Zuglauf => Zug.Zuglauf;

    public LinkedListNode<FahrzeugInfo> FirstLoco => _firstLoco;
    public LinkedListNode<FahrzeugInfo> SecondLoco => _secondLoco;
    public bool FirstIsLeadingLoco => _firstIsLeadingLoco;

    public TrainItem(ZugDatei zugDatei)
        : this(zugDatei.Root)
    {
      _zugDatei = zugDatei;
    }

    public TrainItem(Zug zug)
    {
      _zug = zug;
      _abfahrtsZeit = zug.StartTime.Value.ToString("HH:mm");
      _nummer = string.IsNullOrEmpty(zug.Gattung) ? zug.Nummer : string.Format("{0} {1}", zug.Gattung, zug.Nummer);
      OrigZug = null;

      //SetLocos();
      SetTrains();
    }

    public bool CheckImportance(Zug zug)
    {
      if (Zug.Aufgleiszeit == null || Zug.Abgleiszeit == null || zug.Aufgleiszeit == null || zug.Abgleiszeit == null)
      {
        IsImportant = true;
        //_log.Info("CheckImportance: One Time is NULL");
      }
      else
      {
        IsImportant = !(Zug.Aufgleiszeit.Value > zug.Abgleiszeit.Value || Zug.Abgleiszeit.Value < zug.Aufgleiszeit.Value);
        //if (IsImportant )
        //    _log.Info("CheckImportance TRUE: "+ Zug.Nummer.ToString() + "-" +Zug.Aufgleiszeit.ToString()+ "-" + zug.Abgleiszeit.ToString() + " -- " + Zug.Abgleiszeit.ToString() + "-" + zug.Aufgleiszeit.ToString());
        //else
        //    _log.Info("CheckImportance FALSE: " + Zug.Nummer.ToString() + "-" + Zug.Aufgleiszeit.ToString() + "-" + zug.Abgleiszeit.ToString() + " -- " + Zug.Abgleiszeit.ToString() + "-" + zug.Aufgleiszeit.ToString());

      }
      return IsImportant;
    }

    public void ReplaceLoco(Window window)
    {
      LocoReplacementDialog dlg = new()
      {
        Owner = window
      };
      dlg.SetTrain(this);
      if (dlg.ShowDialog() == true)
      {
        if ((bool)dlg.IgnoreDoors.IsChecked)
        {
          foreach (FahrzeugInfo fi in _zugReihung)
          {
            // ignore all door systems - except for the replacement loco
            fi.Tuerignorieren = true;
          }
        }


        FahrzeugInfoParameter fip = new()
        {
          Parent = _firstLoco.Value.Parent,
          Zugart = dlg.Zugart,
          Bremsstellung = dlg.Bremsstellung,
          Bremshundertstel = dlg.BrHSelector == 0 ? 0 : dlg.BrH,
          DotraMode = DotraMode.Default,
          //IgnoreDoors = _firstLoco.Value.Tuerignorieren,
          IgnoreDoors = (bool)dlg.IgnoreDoors.IsChecked,
          Gedreht = (bool)dlg.Gedreht1.IsChecked,
          SASchaltung = dlg.GetPantograph(1),
          Zugrichtung = (bool)dlg.Lokrichtung.IsChecked,
          //Zuggedreht = (bool)dlg.Zuggedreht.IsChecked
        };

        if (fip.Zuggedreht)
        {
          //LinkedListNode<FahrzeugInfo> prev = null;
          //LinkedListNode<FahrzeugInfo> current = _zugReihung.First; // Head;
          //LinkedListNode<FahrzeugInfo> next = null;

          //while (current != null)
          //{
          //  next = current.Next;
          //  current.Next = prev;
          //  prev = current;
          //  current = next;
          //}

          //Head = prev;
        }
        ReplacementLoco rloco = DataManager.Instance.ReplacementLocos[dlg.SelectedReplLoco1];
        _firstLoco.Value = rloco.GetFahrzeugInfo(fip);

        if (_firstIsLeadingLoco)
        {
          if (dlg.LocoPosition1 == LocoPositionType.Pushing)
          {
            _zugReihung.Remove(_firstLoco);
            _zugReihung.AddLast(_firstLoco);
          }
        }
        else
        {
          if (dlg.LocoPosition1 == LocoPositionType.Leading)
          {
            _zugReihung.Remove(_firstLoco);
            _zugReihung.AddFirst(_firstLoco);
          }
        }

        if (dlg.Traction1 == TractionType.DoubleHeading)
        {
          fip = new FahrzeugInfoParameter
          {
            Parent = _firstLoco.Value.Parent,
            Zugart = dlg.Zugart,
            Bremsstellung = dlg.Bremsstellung,
            Bremshundertstel = dlg.BrHSelector == 0 ? 0 : dlg.BrH,
            DotraMode = DotraMode.PartOfMultipleHeading,
            IgnoreDoors = (bool)dlg.IgnoreDoors.IsChecked,
            //IgnoreDoors = _firstLoco.Value.Tuerignorieren,
            Gedreht = (bool)dlg.Gedreht1.IsChecked,
            SASchaltung = dlg.GetPantograph(1),
            Zugrichtung = false,
            Zuggedreht = false
          };
          LinkedListNode<FahrzeugInfo> n = new(rloco.GetFahrzeugInfo(fip));
          _zugReihung.AddAfter(_firstLoco, n);
        }

        if (dlg.ReleaseDoubleHeading)
        {
          _zugReihung.Remove(_secondLoco);
        }
        else if (_secondLoco != null)
        {
          fip = new FahrzeugInfoParameter
          {
            Parent = _secondLoco.Value.Parent,
            Zugart = dlg.Zugart,
            Bremsstellung = dlg.Bremsstellung,
            Bremshundertstel = dlg.BrHSelector == 0 ? 0 : dlg.BrH,
            DotraMode = DotraMode.PartOfMultipleHeading,
            IgnoreDoors = (bool)dlg.IgnoreDoors.IsChecked,
            //*IgnoreDoors = _secondLoco.Value.Tuerignorieren,
            Gedreht = (bool)dlg.Gedreht2.IsChecked,
            SASchaltung = dlg.GetPantograph(2),
            Zugrichtung = false,
            Zuggedreht = false
          };
          rloco = DataManager.Instance.ReplacementLocos[dlg.SelectedReplLoco2];
          _secondLoco.Value = rloco.GetFahrzeugInfo(fip);

          if (dlg.LocoPosition2 == LocoPositionType.Pushing)
          {
            _zugReihung.Remove(_secondLoco);
            _zugReihung.AddLast(_secondLoco);
          }
          else
          {
            _zugReihung.Remove(_secondLoco);
            _zugReihung.AddAfter(_firstLoco, _secondLoco);
          }

          if (dlg.Traction2 == TractionType.DoubleHeading)
          {
            LinkedListNode<FahrzeugInfo> n = new(_secondLoco.Value);
            _zugReihung.AddAfter(_secondLoco, n);

          }
        }
        IsLocoInFront = (bool)dlg.Lokrichtung.IsChecked;
        //*IsTrainTurned = (bool)dlg.Zuggedreht.IsChecked;
        IsLocoReplaced = true;
      }
    }

    public void UndoReplaceLoco()
    {
      SetLocos();
      IsTrainReplaced = false;
      DataManager.Instance.CurrentTrain = DataManager.Instance.CurrentTrainItem.OrigZug;
      IsLocoReplaced = false;
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as TrainItem)?.OnIsSelectedChanged((bool)e.NewValue);
    }

    private void OnIsSelectedChanged(bool value)
    {
      if (!value)
      {
        UndoReplaceTrain();
      }
    }

    private void SetLocos()
    {
      try
      {
        _zugReihung = new ZugReihung(Zug);
        _zugReihung.BuildTrain();

        if (_zugReihung.Count > 0)
        {

          LinkedListNode<FahrzeugInfo> n = _zugReihung.First;
          FahrzeugVariante fv = n.Value.GetVariante();
          if (fv != null)
          {
            if (fv.Drivetrain != null)
            {
              _firstLoco = n;
              _firstIsLeadingLoco = true;
            }
          }
          n = n.Next;
          while (n != null)
          {
            fv = n.Value.GetVariante();
            if (fv.Drivetrain != null)
            {
              if (_firstLoco == null)
              {
                _firstLoco = n;
              }
              else
              {
                _secondLoco = n;
                break;
              }
            }
            n = n.Next;
          }

          HasDoubleHeading = _secondLoco != null && _secondLoco.Value.DotraModus == DotraMode.PartOfMultipleHeading;

          if (_firstLoco != null)
          {
            fv = _firstLoco.Value.GetVariante();
            StringBuilder sb = new();
            sb.Append('(');
            sb.Append(fv.BR);
            if (HasDoubleHeading)
            {
              sb.Append('+');
            }
            sb.Append(')');
            Loco = sb.ToString();
          }
          else
          {
            Loco = "(?)";
          }
        }
      }
      catch (Exception ex)
      {
        _log.Error("SetLocos:" + ex.ToString() + " Zug:" + Zug.Gattung + " " + Zug.Nummer);
        MessageBox.Show(ex.Message, "Fehler in Zugdefinition - Zugnummer:" + Zug.Gattung + " " + Zug.Nummer, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }


    public void ReplaceTrain(Window window)
    {
      TrainReplacementDialog dlg = new()
      {
        Owner = window
      };
      dlg.SetTrain(this);
      if (dlg.ShowDialog() == true)
      {
        FahrzeugInfoParameter fip = new()
        {
          Parent = _firstLoco.Value.Parent,
          Zugart = dlg.Zugart,
          Bremsstellung = dlg.Bremsstellung,
          Bremshundertstel = dlg.BrHSelector == 0 ? 0 : dlg.BrH,
          DotraMode = DotraMode.Default,
          //IgnoreDoors = _firstLoco.Value.Tuerignorieren,
          IgnoreDoors = false,//(bool)dlg.IgnoreDoors.IsChecked,
          Gedreht = false, //(bool)dlg.Gedreht1.IsChecked,
          SASchaltung = dlg.GetPantograph(1),
          Zugrichtung = false, //(bool)dlg.Lokrichtung.IsChecked,
          Zuggedreht = (bool)dlg.Zuggedreht.IsChecked
        };  //


        ReplacementTrain rloco = DataManager.Instance.ReplacementTrains[dlg.SelectedReplTrain1];
        _replaceZugReihung = rloco.GetZugReihung();

        IsTrainTurned = (bool)dlg.Zuggedreht.IsChecked;
        IsTrainReplaced = true;
        if (rloco != null)
        {

        }
      }
    }

    public void ReplaceTrain2(bool zuggedreht)
    {

      ReplacementTrain rloco = DataManager.Instance.SelectedReplacementTrain;
      if (rloco != null)
      {
        _replaceZugReihung = rloco.GetZugReihung();

        IsTrainTurned = zuggedreht; // (bool)dlg.Zuggedreht.IsChecked;
        IsTrainReplaced = true;
      }

    }

    public void UndoReplaceTrain()
    {
      SetTrains();
      IsTrainReplaced = false;
      IsLocoReplaced = false;
      DataManager.Instance.CurrentTrain = DataManager.Instance.CurrentTrainItem.OrigZug;
    }

    private void SetTrains()
    {
      try
      {
        _zugReihung = new ZugReihung(Zug);
        _zugReihung.BuildTrain();

        if (_zugReihung.Count > 0)
        {

          LinkedListNode<FahrzeugInfo> n = _zugReihung.First;
          FahrzeugVariante fv = n.Value.GetVariante();
          if (fv != null)
          {
            if (fv.Drivetrain != null)
            {
              _firstLoco = n;
              _firstIsLeadingLoco = true;
            }
          }
          n = n.Next;
          while (n != null)
          {
            fv = n.Value.GetVariante();
            if (fv.Drivetrain != null)
            {
              if (_firstLoco == null)
              {
                _firstLoco = n;
              }
              else
              {
                _secondLoco = n;
                break;
              }
            }
            n = n.Next;
          }

          HasDoubleHeading = _secondLoco != null && _secondLoco.Value.DotraModus == DotraMode.PartOfMultipleHeading;

          if (_firstLoco != null)
          {
            fv = _firstLoco.Value.GetVariante();
            StringBuilder sb = new();
            sb.Append('(');
            sb.Append(fv.BR);
            if (HasDoubleHeading)
            {
              sb.Append('+');
            }
            sb.Append(')');
            Loco = sb.ToString();
          }
          else
          {
            Loco = "(?)";
          }
        }
      }
      catch (Exception ex)
      {
        _log.Error("SetLocos:" + ex.ToString() + " Zug:" + Zug.Gattung + " " + Zug.Nummer);
        MessageBox.Show(ex.Message, LocalizationManager.Translate("Fehler in Zugdefinition - Zugnummer:") + Zug.Gattung + " " + Zug.Nummer, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }
  }

#if false
    public enum DuplicatedTrain
    {
        SameFile,
        SameNumber,
        SameBuchfahrplan
    }

    public class DuplicatedTrainItem : TrainItem
    {
        private readonly DuplicatedTrain _reason;
        private readonly TrainItem _duplicate;

        public DuplicatedTrain Reason { get => _reason; }

        public DuplicatedTrainItem(DuplicatedTrain reason, ZugDatei zugDatei, TrainItem duplicate)
            : base(zugDatei)
        {
            _reason = reason;
            _duplicate = duplicate;
        }
    }
#endif
}
