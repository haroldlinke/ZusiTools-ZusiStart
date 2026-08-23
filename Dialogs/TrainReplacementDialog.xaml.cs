using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using ZusiKlassenLib2.Cab;
using ZusiKlassenLib2.Fahrplan;
using ZusiKlassenLib2.Vehicle;
using ZusiMeterGaugesLib.Gauges;
//using ZusiStart.Classes;
using ZusiStart.Data;

namespace ZusiStart.Dialogs
{
  /// <summary>
  /// Interaktionslogik für TrainReplacementDialog.xaml
  /// </summary>
  public partial class TrainReplacementDialog : Window
  {
    private readonly string[] _zugdatenAnalog = new string[] { "automatisch", "U", "M", "O" };
    private readonly string[] _zugdatenRechner = new string[] { "automatisch", "1", "8" };
    private readonly string[] _zugdatenLZB80 = new string[] { "automatisch", "2", "3", "4", "5", "6", "7", "9" };

    private readonly ObservableCollection<Notch> _zugarten = new()
        {
            new Notch() { Label = "automatisch" }
        };
    //private TrainItem _train;
    string? _textFirstTrain;
    string? _textSecondTrain;
    private ZugdatenType _zugdatenType;

    public static readonly DependencyProperty ReleaseDoubleHeadingProperty = DependencyProperty.Register(
        "ReleaseDoubleHeading",
        typeof(bool),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(false, OnReleaseDoubleHeadingChanged));
    public bool ReleaseDoubleHeading
    {
      get => (bool)GetValue(ReleaseDoubleHeadingProperty);
      set => SetValue(ReleaseDoubleHeadingProperty, value);
    }

    public static readonly DependencyProperty SelectedReplTrain1Property = DependencyProperty.Register(
        "SelectedReplTrain1",
        typeof(int),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(-1, OnSelectedReplTrain1Changed));
    public int SelectedReplTrain1
    {
      get => (int)GetValue(SelectedReplTrain1Property);
      set => SetValue(SelectedReplTrain1Property, value);
    }

    public static readonly DependencyProperty SelectedReplTrain2Property = DependencyProperty.Register(
        "SelectedReplTrain2",
        typeof(int),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(-1, OnSelectedReplTrain2Changed));
    public int SelectedReplTrain2
    {
      get => (int)GetValue(SelectedReplTrain2Property);
      set => SetValue(SelectedReplTrain2Property, value);
    }

    //public static readonly DependencyProperty TrainPosition1Property = DependencyProperty.Register(
    //    "TrainPosition1",
    //    typeof(TrainPositionType),
    //    typeof(TrainReplacementDialog),
    //    new PropertyMetadata(TrainPositionType.Leading, OnTrainPosition1Changed));
    //public TrainPositionType TrainPosition1
    //{
    //  get => (TrainPositionType)GetValue(TrainPosition1Property);
    //  set => SetValue(TrainPosition1Property, value);
    //}

    //public static readonly DependencyProperty TrainPosition2Property = DependencyProperty.Register(
    //    "TrainPosition2",
    //    typeof(TrainPositionType),
    //    typeof(TrainReplacementDialog),
    //    new PropertyMetadata(TrainPositionType.Leading));
    //public TrainPositionType TrainPosition2
    //{
    //  get => (TrainPositionType)GetValue(TrainPosition2Property);
    //  set => SetValue(TrainPosition2Property, value);
    //}

    //public static readonly DependencyProperty Traction1Property = DependencyProperty.Register(
    //    "Traction1",
    //    typeof(TractionType),
    //    typeof(TrainReplacementDialog),
    //    new PropertyMetadata(TractionType.SingleHeading, OnTraction1Changed));
    //public TractionType Traction1
    //{
    //  get => (TractionType)GetValue(Traction1Property);
    //  set => SetValue(Traction1Property, value);
    //}

    //public static readonly DependencyProperty Traction2Property = DependencyProperty.Register(
    //    "Traction2",
    //    typeof(TractionType),
    //    typeof(TrainReplacementDialog),
    //    new PropertyMetadata(TractionType.SingleHeading, OnTraction2Changed));
    //public TractionType Traction2
    //{
    //  get => (TractionType)GetValue(Traction2Property);
    //  set => SetValue(Traction2Property, value);
    //}

    public static readonly DependencyProperty TRSelectedZugProperty = DependencyProperty.Register(
        "TRSelectedZug",
        typeof(Zug),
        typeof(ReplacementTrain),
        new PropertyMetadata(null));
    public Zug? TRSelectedZug
    {
      get => (Zug)GetValue(TRSelectedZugProperty);
      set => SetValue(TRSelectedZugProperty, value);
    }

    public static readonly DependencyProperty ZugartProperty = DependencyProperty.Register(
        "Zugart",
        typeof(Zugart),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(Zugart.None, OnZugartChanged));
    public Zugart Zugart
    {
      get => (Zugart)GetValue(ZugartProperty);
      set => SetValue(ZugartProperty, value);
    }

    public static readonly DependencyProperty BremsstellungProperty = DependencyProperty.Register(
        "Bremsstellung",
        typeof(Bremsstellung),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(Bremsstellung.Unknown, OnBremsstellungChanged));
    public Bremsstellung Bremsstellung
    {
      get => (Bremsstellung)GetValue(BremsstellungProperty);
      set => SetValue(BremsstellungProperty, value);
    }

    public static readonly DependencyProperty BrHProperty = DependencyProperty.Register(
        "BrH",
        typeof(int),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(0));
    public int BrH
    {
      get => (int)GetValue(BrHProperty);
      set => SetValue(BrHProperty, value);
    }

    public static readonly DependencyProperty BrHSelectorProperty = DependencyProperty.Register(
        "BrHSelector",
        typeof(uint),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(0u));
    public uint BrHSelector
    {
      get => (uint)GetValue(BrHSelectorProperty);
      set => SetValue(BrHSelectorProperty, value);
    }

    public static readonly DependencyProperty Pantograph1_0Property = DependencyProperty.Register(
        "Pantograph1_0",
        typeof(uint),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(0u));
    public uint Pantograph1_0
    {
      get => (uint)GetValue(Pantograph1_0Property);
      set => SetValue(Pantograph1_0Property, value);
    }

    public static readonly DependencyProperty Pantograph1_1Property = DependencyProperty.Register(
        "Pantograph1_1",
        typeof(uint),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(1u));
    public uint Pantograph1_1
    {
      get => (uint)GetValue(Pantograph1_1Property);
      set => SetValue(Pantograph1_1Property, value);
    }

    public static readonly DependencyProperty Pantograph1_2Property = DependencyProperty.Register(
        "Pantograph1_2",
        typeof(uint),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(0u));
    public uint Pantograph1_2
    {
      get => (uint)GetValue(Pantograph1_2Property);
      set => SetValue(Pantograph1_2Property, value);
    }

    public static readonly DependencyProperty Pantograph1_3Property = DependencyProperty.Register(
        "Pantograph1_3",
        typeof(uint),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(0u));
    public uint Pantograph1_3
    {
      get => (uint)GetValue(Pantograph1_3Property);
      set => SetValue(Pantograph1_3Property, value);
    }

    public static readonly DependencyProperty Pantograph2_0Property = DependencyProperty.Register(
        "Pantograph2_0",
        typeof(uint),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(0u));
    public uint Pantograph2_0
    {
      get => (uint)GetValue(Pantograph2_0Property);
      set => SetValue(Pantograph2_0Property, value);
    }

    public static readonly DependencyProperty Pantograph2_1Property = DependencyProperty.Register(
        "Pantograph2_1",
        typeof(uint),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(1u));
    public uint Pantograph2_1
    {
      get => (uint)GetValue(Pantograph2_1Property);
      set => SetValue(Pantograph2_1Property, value);
    }

    public static readonly DependencyProperty Pantograph2_2Property = DependencyProperty.Register(
        "Pantograph2_2",
        typeof(uint),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(0u));
    public uint Pantograph2_2
    {
      get => (uint)GetValue(Pantograph2_2Property);
      set => SetValue(Pantograph2_2Property, value);
    }

    public static readonly DependencyProperty Pantograph2_3Property = DependencyProperty.Register(
        "Pantograph2_3",
        typeof(uint),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(0u));
    public uint Pantograph2_3
    {
      get => (uint)GetValue(Pantograph2_3Property);
      set => SetValue(Pantograph2_3Property, value);
    }

    //--

    private static readonly DependencyPropertyKey _hasDoubleHeadingKey = DependencyProperty.RegisterReadOnly(
        "HasDoubleHeading",
        typeof(bool),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(false));
    public static readonly DependencyProperty HasDoubleHeadingProperty = _hasDoubleHeadingKey.DependencyProperty;
    public bool HasDoubleHeading
    {
      get => (bool)GetValue(HasDoubleHeadingProperty);
      private set => SetValue(_hasDoubleHeadingKey, value);
    }

    private static readonly DependencyPropertyKey _textFirstTrainKey = DependencyProperty.RegisterReadOnly(
        "TextFirstTrain",
        typeof(string),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(null));
    public static readonly DependencyProperty TextFirstTrainProperty = _textFirstTrainKey.DependencyProperty;
    public string TextFirstTrain
    {
      get => (string)GetValue(TextFirstTrainProperty);
      private set => SetValue(_textFirstTrainKey, value);
    }

    private static readonly DependencyPropertyKey _brFirstTrainKey = DependencyProperty.RegisterReadOnly(
        "BRFirstTrain",
        typeof(string),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(null));
    public static readonly DependencyProperty BRFirstTrainProperty = _brFirstTrainKey.DependencyProperty;
    public string BRFirstTrain
    {
      get => (string)GetValue(BRFirstTrainProperty);
      private set => SetValue(_brFirstTrainKey, value);
    }

    private static readonly DependencyPropertyKey _descrFirstTrainKey = DependencyProperty.RegisterReadOnly(
        "DescrFirstTrain",
        typeof(string),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(null));
    public static readonly DependencyProperty DescrFirstTrainProperty = _descrFirstTrainKey.DependencyProperty;
    public string DescrFirstTrain
    {
      get => (string)GetValue(DescrFirstTrainProperty);
      private set => SetValue(_descrFirstTrainKey, value);
    }

    private static readonly DependencyPropertyKey _textSecondTrainKey = DependencyProperty.RegisterReadOnly(
        "TextSecondTrain",
        typeof(string),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(null));
    public static readonly DependencyProperty TextSecondTrainProperty = _textSecondTrainKey.DependencyProperty;
    public string TextSecondTrain
    {
      get => (string)GetValue(TextSecondTrainProperty);
      private set => SetValue(_textSecondTrainKey, value);
    }

    private static readonly DependencyPropertyKey _brSecondTrainKey = DependencyProperty.RegisterReadOnly(
        "BRSecondTrain",
        typeof(string),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(null));
    public static readonly DependencyProperty BRSecondTrainProperty = _brSecondTrainKey.DependencyProperty;
    public string? BRSecondTrain
    {
      get => (string)GetValue(BRSecondTrainProperty);
      private set => SetValue(_brSecondTrainKey, value);
    }

    private static readonly DependencyPropertyKey _descrSecondTrainKey = DependencyProperty.RegisterReadOnly(
        "DescrSecondTrain",
        typeof(string),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(null));
    public static readonly DependencyProperty DescrSecondTrainProperty = _descrSecondTrainKey.DependencyProperty;
    public string? DescrSecondTrain
    {
      get => (string)GetValue(DescrSecondTrainProperty);
      private set => SetValue(_descrSecondTrainKey, value);
    }

    private static readonly DependencyPropertyKey _buttonTextKey = DependencyProperty.RegisterReadOnly(
        "ButtonText",
        typeof(string),
        typeof(TrainReplacementDialog),
        new PropertyMetadata("_Zug tauschen"));
    public static readonly DependencyProperty ButtonTextProperty = _buttonTextKey.DependencyProperty;
    public string ButtonText
    {
      get => (string)GetValue(ButtonTextProperty);
      private set => SetValue(_buttonTextKey, value);
    }

    private static readonly DependencyPropertyKey _TrainPosition2aEnabledKey = DependencyProperty.RegisterReadOnly(
        "TrainPosition2aEnabled",
        typeof(bool),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(false));
    public static readonly DependencyProperty TrainPosition2aEnabledProperty = _TrainPosition2aEnabledKey.DependencyProperty;
    public bool TrainPosition2aEnabled
    {
      get => (bool)GetValue(TrainPosition2aEnabledProperty);
      private set => SetValue(_TrainPosition2aEnabledKey, value);
    }

    private static readonly DependencyPropertyKey _TrainPosition2aTextKey = DependencyProperty.RegisterReadOnly(
        "TrainPosition2aText",
        typeof(string),
        typeof(TrainReplacementDialog),
        new PropertyMetadata("2. führende Lok"));
    public static readonly DependencyProperty TrainPosition2aTextProperty = _TrainPosition2aTextKey.DependencyProperty;
    public string TrainPosition2aText
    {
      get => (string)GetValue(TrainPosition2aTextProperty);
      private set => SetValue(_TrainPosition2aTextKey, value);
    }

    private static readonly DependencyPropertyKey _TrainPosition2bTextKey = DependencyProperty.RegisterReadOnly(
        "TrainPosition2bText",
        typeof(string),
        typeof(TrainReplacementDialog),
        new PropertyMetadata("schiebende Lok"));
    public static readonly DependencyProperty TrainPosition2bTextProperty = _TrainPosition2bTextKey.DependencyProperty;
    public string TrainPosition2bText
    {
      get => (string)GetValue(TrainPosition2bTextProperty);
      private set => SetValue(_TrainPosition2bTextKey, value);
    }

    private static readonly DependencyPropertyKey _dialogTitleKey = DependencyProperty.RegisterReadOnly(
        "DialogTitle",
        typeof(string),
        typeof(TrainReplacementDialog),
        new PropertyMetadata("Zugtausch"));
    public static readonly DependencyProperty DialogTitleProperty = _dialogTitleKey.DependencyProperty;
    public string DialogTitle
    {
      get => (string)GetValue(DialogTitleProperty);
      private set => SetValue(_dialogTitleKey, value);
    }

    private static readonly DependencyPropertyKey _showPantographOptionsKey = DependencyProperty.RegisterReadOnly(
        "ShowPantographOptions",
        typeof(bool),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(false));
    public static readonly DependencyProperty ShowPantographOptionsProperty = _showPantographOptionsKey.DependencyProperty;
    public bool ShowPantographOptions
    {
      get => (bool)GetValue(ShowPantographOptionsProperty);
      private set => SetValue(_showPantographOptionsKey, value);
    }

    private static readonly DependencyPropertyKey _isFirstTrainElectricalKey = DependencyProperty.RegisterReadOnly(
        "IsFirstTrainElectrical",
        typeof(bool),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(false));
    public static readonly DependencyProperty IsFirstTrainElectricalProperty = _isFirstTrainElectricalKey.DependencyProperty;
    public bool IsFirstTrainElectrical
    {
      get => (bool)GetValue(IsFirstTrainElectricalProperty);
      private set => SetValue(_isFirstTrainElectricalKey, value);
    }

    private static readonly DependencyPropertyKey _isSecondTrainElectricalKey = DependencyProperty.RegisterReadOnly(
        "IsSecondTrainElectrical",
        typeof(bool),
        typeof(TrainReplacementDialog),
        new PropertyMetadata(false));
    public static readonly DependencyProperty IsSecondTrainElectricalProperty = _isSecondTrainElectricalKey.DependencyProperty;
    public bool IsSecondTrainElectrical
    {
      get => (bool)GetValue(IsSecondTrainElectricalProperty);
      private set => SetValue(_isSecondTrainElectricalKey, value);
    }

    public ObservableCollection<Notch> Zugarten => _zugarten;

    public static readonly RoutedUICommand ReplTrainCommand = new("_Zug tauschen", "ReplTrainCommand", typeof(TrainReplacementDialog));

    public TrainReplacementDialog()
    {
      InitializeComponent();

      CommandBindings.Add(new CommandBinding(ReplTrainCommand, OnReplTrain, OnCanReplTrain));
    }

    public int GetPantograph(int Train)
    {
      int res = 0;

      if (Train == 1 && IsFirstTrainElectrical)
      {
        if (Pantograph1_0 != 0) res |= 1;
        if (Pantograph1_1 != 0) res |= 2;
        if (Pantograph1_2 != 0) res |= 4;
        if (Pantograph1_3 != 0) res |= 8;
      }
      else if (Train == 2 && IsSecondTrainElectrical)
      {
        if (Pantograph2_0 != 0) res |= 1;
        if (Pantograph2_1 != 0) res |= 2;
        if (Pantograph2_2 != 0) res |= 4;
        if (Pantograph2_3 != 0) res |= 8;
      }

      return res;
    }

    public void SetTrain(TrainItem train)
    {
      FahrzeugVariante fv;

      _textFirstTrain = _textSecondTrain = null;
      BRFirstTrain = BRSecondTrain = null;
      DescrFirstTrain = DescrSecondTrain = null;

      //_train = train;
      DialogTitle = string.Format("{0} • Loktausch", train.Nummer);

      //if (train.FirstIsLeadingTrain)
      //{
      //    _textFirstTrain = "führende Lok";
      //}
      //else
      //{
      //    _textFirstTrain = "schiebende Lok";
      //}
      //if (train.SecondTrain != null)
      //{
      //    _textSecondTrain = "zweite Lok";
      //    fv = train.SecondTrain.Value.GetVariante();
      //    BRSecondTrain = fv.BR;
      //    DescrSecondTrain = fv.Beschreibung; 
      //    ButtonText = "_Loks tauschen";
      //}
      //else
      //{
      //    ButtonText = "Lok tauschen";
      //}

      //FahrzeugInfo fi = train.FirstTrain.Value;
      //fv = fi.GetVariante();
      //BRFirstTrain = fv.BR;
      //DescrFirstTrain = fv.Beschreibung;

      //TextFirstTrain = _textFirstTrain;
      //TextSecondTrain = _textSecondTrain;

      //HasDoubleHeading = train.HasDoubleHeading;
    }

    private static void OnReleaseDoubleHeadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as TrainReplacementDialog)?.OnReleaseDoubleHeadingChanged((bool)e.NewValue);
    }

    private void OnReleaseDoubleHeadingChanged(bool value)
    {
      //if (value && HasDoubleHeading)
      //{
      //    TextFirstTrain = "Doppeltraktion";
      //    Traction1 = TractionType.SingleHeading;
      //    Traction2 = TractionType.SingleHeading;
      //}
      //else
      //{
      //    TextFirstTrain = _textFirstTrain;
      //    TextSecondTrain = _textSecondTrain;
      //}
      //CalculateBrakeHundredth();
    }

    //private static void OnTrainPosition1Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{
    //  (d as TrainReplacementDialog)?.OnTrainPosition1Changed((TrainPositionType)e.NewValue);
    //}

    //private void OnTrainPosition1Changed(TrainPositionType value)
    //{
    //  //if (value == TrainPositionType.Leading)
    //  //{
    //  //    TrainPosition2aText = "2. führende Lok";
    //  //    TrainPosition2bText = "schiebende Lok";
    //  //    TrainPosition2aEnabled = SelectedReplTrain2 >= 0;
    //  //}
    //  //else
    //  //{
    //  //    TrainPosition2aText = "führende Lok";
    //  //    TrainPosition2bText = "2. schiebende Lok";
    //  //    TrainPosition2 = TrainPositionType.Pushing;
    //  //    TrainPosition2aEnabled = false;
    //  //}
    //}

    private static void OnBremsstellungChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as TrainReplacementDialog)?.OnBremsstellungChanged((Bremsstellung)e.NewValue);
    }

    private void OnBremsstellungChanged(Bremsstellung value)
    {
      switch (_zugdatenType)
      {
        case ZugdatenType.LZB80:
          switch (value)
          {
            case Bremsstellung.Unknown:
              Zugart = Zugart.None;
              break;
#if false
                        case Bremsstellung.G:
                            Zugart = Zugart.BRA1;
                            break;
                        default:
                            Zugart = Zugart.BRA8;
                            break;
#endif
          }
          break;
        case ZugdatenType.IndusiRechner:
          Zugart = value switch
          {
            Bremsstellung.Unknown => Zugart.None,
            Bremsstellung.G => Zugart.BRA1,
            _ => Zugart.BRA8,
          };
          break;
      }
      //CalculateBrakeHundredth();
    }

    private static void OnSelectedReplTrain1Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as TrainReplacementDialog)?.OnSelectedReplTrain1Changed((int)e.NewValue);
    }

    private void OnSelectedReplTrain1Changed(int value)
    {
      DataManager dm = DataManager.Instance;
      if (value >= 0 && value < dm.ReplacementTrains.Count)
      {
        _zugarten.Clear();
        _zugdatenType = ZugdatenType.None;

        ReplacementTrain rl = dm.ReplacementTrains[value];
        
        Zugart = Zugart.None;
        Bremsstellung = Bremsstellung.Unknown;
        BrHSelector = 0;
        IsFirstTrainElectrical = false;
        TRSelectedZug = rl.SelectedZug;
      }
      else
      {
        IsFirstTrainElectrical = false;
      }
      //CalculateBrakeHundredth();
      //UpdateShowPantographOptions();
    }

    private static void OnSelectedReplTrain2Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as TrainReplacementDialog)?.OnSelectedReplTrain2Changed((int)e.NewValue);
    }

    private void OnSelectedReplTrain2Changed(int value)
    {
      DataManager dm = DataManager.Instance;
      if (value >= 0 && value < dm.ReplacementTrains.Count)
      {
        ReplacementTrain rl = dm.ReplacementTrains[value];
        //VehicleVariant vv = rl.SelectedVariant;
        //IsSecondTrainElectrical = false; //  vv.IsTrainElectrical;
        //TrainPosition2aEnabled = TrainPosition1 == TrainPositionType.Leading;
      }
      else
      {
        IsSecondTrainElectrical = false;
        TrainPosition2aEnabled = false;
      }
      //UpdateShowPantographOptions();
    }

    //private static void OnTraction1Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{
    //  (d as TrainReplacementDialog)?.OnTraction1Changed((TractionType)e.NewValue);
    //}

    //private void OnTraction1Changed(TractionType value)
    //{
    //  CalculateBrakeHundredth();
    //  if (value == TractionType.DoubleHeading)
    //  {
    //    IsSecondTrainElectrical = IsFirstTrainElectrical;
    //  }
    //  else
    //  {
    //    OnSelectedReplTrain2Changed(SelectedReplTrain2);
    //  }
    //}

    private static void OnTraction2Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      //(d as TrainReplacementDialog)?.CalculateBrakeHundredth();
    }

    private static void OnZugartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as TrainReplacementDialog)?.OnZugartChanged((Zugart)e.NewValue);
    }

    private void OnZugartChanged(Zugart value)
    {
      if (value == Zugart.None)
      {
        BrHSelector = 0;
      }
    }

    private void OnCanReplTrain(object sender, CanExecuteRoutedEventArgs e)
    {
      if (false) // (HasDoubleHeading)
      {
        //if (ReleaseDoubleHeading)
        //{
        //  e.CanExecute = SelectedReplTrain1 >= 0 && Traction1 == TractionType.SingleHeading;
        //}
        //else
        //{
        //  e.CanExecute = SelectedReplTrain1 >= 0 && SelectedReplTrain2 >= 0;
        //}
      }
      else
      {
        e.CanExecute = SelectedReplTrain1 >= 0;
      }
    }

    private void OnReplTrain(object sender, ExecutedRoutedEventArgs e)
    {
      DialogResult = true;
    }

    //private void CalculateBrakeHundredth()
    //{
    //  if (Bremsstellung > Bremsstellung.Unknown && SelectedReplTrain1 >= 0)
    //  {
    //    DataManager dm = DataManager.Instance;
    //    ReplacementTrain rl = dm.ReplacementTrains[SelectedReplTrain1];
    //    double m = rl.SelectedVariant.Mass;
    //    double bw = rl.SelectedVariant.GetBrakeWeight(Bremsstellung);
    //    BrH = m > 0 ? (int)(bw * 100 / m) : 0;
    //  }
    //  else
    //  {
    //    BrH = 0;
    //  }
    //}

    //private void UpdateShowPantographOptions()
    //{
    //  ShowPantographOptions = IsFirstTrainElectrical || IsSecondTrainElectrical;
    //}

    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {

    }
  }

}
