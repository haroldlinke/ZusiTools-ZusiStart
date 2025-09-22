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
using ZusiKlassenLib.Cab;
using ZusiKlassenLib.Fahrplan;
using ZusiKlassenLib.Vehicle;
using ZusiMeterGaugesLib.Gauges;
using ZusiStart.Classes;
using ZusiStart.Data;

namespace ZusiStart.Dialogs
{
    public enum LocoPositionType
    {
        Leading,
        Pushing
    }

    public enum TractionType
    {
        SingleHeading,
        DoubleHeading
    }

    /// <summary>
    /// Interaktionslogik für LocoReplacementDialog.xaml
    /// </summary>
    public partial class LocoReplacementDialog : Window
    {
        private readonly string[] _zugdatenAnalog = new string[] { "automatisch", "U", "M", "O" };
        private readonly string[] _zugdatenRechner = new string[] { "automatisch", "1", "8" };
        private readonly string[] _zugdatenLZB80 = new string[] { "automatisch", "2", "3", "4", "5", "6", "7", "9" };

        private readonly ObservableCollection<Notch> _zugarten = new() 
        { 
            new Notch() { Label = "automatisch" }
        };
        //private TrainItem _train;
        string? _textFirstLoco;
        string? _textSecondLoco;
        private ZugdatenType _zugdatenType;

        public static readonly DependencyProperty ReleaseDoubleHeadingProperty = DependencyProperty.Register(
            "ReleaseDoubleHeading",
            typeof(bool),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(false, OnReleaseDoubleHeadingChanged));
        public bool ReleaseDoubleHeading
        {
            get => (bool)GetValue(ReleaseDoubleHeadingProperty);
            set => SetValue(ReleaseDoubleHeadingProperty, value);
        }

        public static readonly DependencyProperty SelectedReplLoco1Property = DependencyProperty.Register(
            "SelectedReplLoco1",
            typeof(int),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(-1, OnSelectedReplLoco1Changed));
        public int SelectedReplLoco1
        {
            get => (int)GetValue(SelectedReplLoco1Property);
            set => SetValue(SelectedReplLoco1Property, value);
        }

        public static readonly DependencyProperty SelectedReplLoco2Property = DependencyProperty.Register(
            "SelectedReplLoco2",
            typeof(int),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(-1, OnSelectedReplLoco2Changed));
        public int SelectedReplLoco2
        {
            get => (int)GetValue(SelectedReplLoco2Property);
            set => SetValue(SelectedReplLoco2Property, value);
        }

        public static readonly DependencyProperty LocoPosition1Property = DependencyProperty.Register(
            "LocoPosition1",
            typeof(LocoPositionType),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(LocoPositionType.Leading, OnLocoPosition1Changed));
        public LocoPositionType LocoPosition1
        {
            get => (LocoPositionType)GetValue(LocoPosition1Property);
            set => SetValue(LocoPosition1Property, value);
        }

        public static readonly DependencyProperty LocoPosition2Property = DependencyProperty.Register(
            "LocoPosition2",
            typeof(LocoPositionType),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(LocoPositionType.Leading));
        public LocoPositionType LocoPosition2
        {
            get => (LocoPositionType)GetValue(LocoPosition2Property);
            set => SetValue(LocoPosition2Property, value);
        }

        public static readonly DependencyProperty Traction1Property = DependencyProperty.Register(
            "Traction1",
            typeof(TractionType),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(TractionType.SingleHeading, OnTraction1Changed));
        public TractionType Traction1
        {
            get => (TractionType)GetValue(Traction1Property);
            set => SetValue(Traction1Property, value);
        }

        public static readonly DependencyProperty Traction2Property = DependencyProperty.Register(
            "Traction2",
            typeof(TractionType),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(TractionType.SingleHeading, OnTraction2Changed));
        public TractionType Traction2
        {
            get => (TractionType)GetValue(Traction2Property);
            set => SetValue(Traction2Property, value);
        }

        public static readonly DependencyProperty ZugartProperty = DependencyProperty.Register(
            "Zugart",
            typeof(Zugart),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(Zugart.None, OnZugartChanged));
        public Zugart Zugart
        {
            get => (Zugart)GetValue(ZugartProperty);
            set => SetValue(ZugartProperty, value);
        }

        public static readonly DependencyProperty BremsstellungProperty = DependencyProperty.Register(
            "Bremsstellung",
            typeof(Bremsstellung),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(Bremsstellung.Unknown, OnBremsstellungChanged));
        public Bremsstellung Bremsstellung
        {
            get => (Bremsstellung)GetValue(BremsstellungProperty);
            set => SetValue(BremsstellungProperty, value);
        }

        public static readonly DependencyProperty BrHProperty = DependencyProperty.Register(
            "BrH",
            typeof(int),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(0));
        public int BrH
        {
            get => (int)GetValue(BrHProperty);
            set => SetValue(BrHProperty, value);
        }

        public static readonly DependencyProperty BrHSelectorProperty = DependencyProperty.Register(
            "BrHSelector",
            typeof(uint),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(0u));
        public uint BrHSelector
        {
            get => (uint)GetValue(BrHSelectorProperty);
            set => SetValue(BrHSelectorProperty, value);
        }

        public static readonly DependencyProperty Pantograph1_0Property = DependencyProperty.Register(
            "Pantograph1_0",
            typeof(uint),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(0u));
        public uint Pantograph1_0
        {
            get => (uint)GetValue(Pantograph1_0Property);
            set => SetValue(Pantograph1_0Property, value);
        }

        public static readonly DependencyProperty Pantograph1_1Property = DependencyProperty.Register(
            "Pantograph1_1",
            typeof(uint),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(1u));
        public uint Pantograph1_1
        {
            get => (uint)GetValue(Pantograph1_1Property);
            set => SetValue(Pantograph1_1Property, value);
        }

        public static readonly DependencyProperty Pantograph1_2Property = DependencyProperty.Register(
            "Pantograph1_2",
            typeof(uint),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(0u));
        public uint Pantograph1_2
        {
            get => (uint)GetValue(Pantograph1_2Property);
            set => SetValue(Pantograph1_2Property, value);
        }

        public static readonly DependencyProperty Pantograph1_3Property = DependencyProperty.Register(
            "Pantograph1_3",
            typeof(uint),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(0u));
        public uint Pantograph1_3
        {
            get => (uint)GetValue(Pantograph1_3Property);
            set => SetValue(Pantograph1_3Property, value);
        }

        public static readonly DependencyProperty Pantograph2_0Property = DependencyProperty.Register(
            "Pantograph2_0",
            typeof(uint),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(0u));
        public uint Pantograph2_0
        {
            get => (uint)GetValue(Pantograph2_0Property);
            set => SetValue(Pantograph2_0Property, value);
        }

        public static readonly DependencyProperty Pantograph2_1Property = DependencyProperty.Register(
            "Pantograph2_1",
            typeof(uint),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(1u));
        public uint Pantograph2_1
        {
            get => (uint)GetValue(Pantograph2_1Property);
            set => SetValue(Pantograph2_1Property, value);
        }

        public static readonly DependencyProperty Pantograph2_2Property = DependencyProperty.Register(
            "Pantograph2_2",
            typeof(uint),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(0u));
        public uint Pantograph2_2
        {
            get => (uint)GetValue(Pantograph2_2Property);
            set => SetValue(Pantograph2_2Property, value);
        }

        public static readonly DependencyProperty Pantograph2_3Property = DependencyProperty.Register(
            "Pantograph2_3",
            typeof(uint),
            typeof(LocoReplacementDialog),
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
            typeof(LocoReplacementDialog),
            new PropertyMetadata(false));
        public static readonly DependencyProperty HasDoubleHeadingProperty = _hasDoubleHeadingKey.DependencyProperty;
        public bool HasDoubleHeading
        {
            get => (bool)GetValue(HasDoubleHeadingProperty);
            private set => SetValue(_hasDoubleHeadingKey, value);
        }

        private static readonly DependencyPropertyKey _textFirstLocoKey = DependencyProperty.RegisterReadOnly(
            "TextFirstLoco",
            typeof(string),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(null));
        public static readonly DependencyProperty TextFirstLocoProperty = _textFirstLocoKey.DependencyProperty;
        public string TextFirstLoco
        {
            get => (string)GetValue(TextFirstLocoProperty);
            private set => SetValue(_textFirstLocoKey, value);
        }

        private static readonly DependencyPropertyKey _brFirstLocoKey = DependencyProperty.RegisterReadOnly(
            "BRFirstLoco",
            typeof(string),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(null));
        public static readonly DependencyProperty BRFirstLocoProperty = _brFirstLocoKey.DependencyProperty;
        public string BRFirstLoco
        {
            get => (string)GetValue(BRFirstLocoProperty);
            private set => SetValue(_brFirstLocoKey, value);
        }

        private static readonly DependencyPropertyKey _descrFirstLocoKey = DependencyProperty.RegisterReadOnly(
            "DescrFirstLoco",
            typeof(string),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(null));
        public static readonly DependencyProperty DescrFirstLocoProperty = _descrFirstLocoKey.DependencyProperty;
        public string DescrFirstLoco
        {
            get => (string)GetValue(DescrFirstLocoProperty);
            private set => SetValue(_descrFirstLocoKey, value);
        }

        private static readonly DependencyPropertyKey _textSecondLocoKey = DependencyProperty.RegisterReadOnly(
            "TextSecondLoco",
            typeof(string),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(null));
        public static readonly DependencyProperty TextSecondLocoProperty = _textSecondLocoKey.DependencyProperty;
        public string TextSecondLoco
        {
            get => (string)GetValue(TextSecondLocoProperty);
            private set => SetValue(_textSecondLocoKey, value);
        }

        private static readonly DependencyPropertyKey _brSecondLocoKey = DependencyProperty.RegisterReadOnly(
            "BRSecondLoco",
            typeof(string),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(null));
        public static readonly DependencyProperty BRSecondLocoProperty = _brSecondLocoKey.DependencyProperty;
        public string? BRSecondLoco
        {
            get => (string)GetValue(BRSecondLocoProperty);
            private set => SetValue(_brSecondLocoKey, value);
        }

        private static readonly DependencyPropertyKey _descrSecondLocoKey = DependencyProperty.RegisterReadOnly(
            "DescrSecondLoco",
            typeof(string),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(null));
        public static readonly DependencyProperty DescrSecondLocoProperty = _descrSecondLocoKey.DependencyProperty;
        public string? DescrSecondLoco
        {
            get => (string)GetValue(DescrSecondLocoProperty);
            private set => SetValue(_descrSecondLocoKey, value);
        }

        private static readonly DependencyPropertyKey _buttonTextKey = DependencyProperty.RegisterReadOnly(
            "ButtonText",
            typeof(string),
            typeof(LocoReplacementDialog),
            new PropertyMetadata("_Lok tauschen"));
        public static readonly DependencyProperty ButtonTextProperty = _buttonTextKey.DependencyProperty;
        public string ButtonText
        {
            get => (string)GetValue(ButtonTextProperty);
            private set => SetValue(_buttonTextKey, value);
        }

        private static readonly DependencyPropertyKey _locoPosition2aEnabledKey = DependencyProperty.RegisterReadOnly(
            "LocoPosition2aEnabled",
            typeof(bool),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(false));
        public static readonly DependencyProperty LocoPosition2aEnabledProperty = _locoPosition2aEnabledKey.DependencyProperty;
        public bool LocoPosition2aEnabled
        {
            get => (bool)GetValue(LocoPosition2aEnabledProperty);
            private set => SetValue(_locoPosition2aEnabledKey, value);
        }

        private static readonly DependencyPropertyKey _locoPosition2aTextKey = DependencyProperty.RegisterReadOnly(
            "LocoPosition2aText",
            typeof(string),
            typeof(LocoReplacementDialog),
            new PropertyMetadata("2. führende Lok"));
        public static readonly DependencyProperty LocoPosition2aTextProperty = _locoPosition2aTextKey.DependencyProperty;
        public string LocoPosition2aText
        {
            get => (string)GetValue(LocoPosition2aTextProperty);
            private set => SetValue(_locoPosition2aTextKey, value);
        }

        private static readonly DependencyPropertyKey _locoPosition2bTextKey = DependencyProperty.RegisterReadOnly(
            "LocoPosition2bText",
            typeof(string),
            typeof(LocoReplacementDialog),
            new PropertyMetadata("schiebende Lok"));
        public static readonly DependencyProperty LocoPosition2bTextProperty = _locoPosition2bTextKey.DependencyProperty;
        public string LocoPosition2bText
        {
            get => (string)GetValue(LocoPosition2bTextProperty);
            private set => SetValue(_locoPosition2bTextKey, value);
        }

        private static readonly DependencyPropertyKey _dialogTitleKey = DependencyProperty.RegisterReadOnly(
            "DialogTitle",
            typeof(string),
            typeof(LocoReplacementDialog),
            new PropertyMetadata("Loktausch"));
        public static readonly DependencyProperty DialogTitleProperty = _dialogTitleKey.DependencyProperty;
        public string DialogTitle
        {
            get => (string)GetValue(DialogTitleProperty);
            private set => SetValue(_dialogTitleKey, value);
        }

        private static readonly DependencyPropertyKey _showPantographOptionsKey = DependencyProperty.RegisterReadOnly(
            "ShowPantographOptions",
            typeof(bool),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(false));
        public static readonly DependencyProperty ShowPantographOptionsProperty = _showPantographOptionsKey.DependencyProperty;
        public bool ShowPantographOptions
        {
            get => (bool)GetValue(ShowPantographOptionsProperty);
            private set => SetValue(_showPantographOptionsKey, value);
        }

        private static readonly DependencyPropertyKey _isFirstLocoElectricalKey = DependencyProperty.RegisterReadOnly(
            "IsFirstLocoElectrical",
            typeof(bool),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(false));
        public static readonly DependencyProperty IsFirstLocoElectricalProperty = _isFirstLocoElectricalKey.DependencyProperty;
        public bool IsFirstLocoElectrical
        {
            get => (bool)GetValue(IsFirstLocoElectricalProperty);
            private set => SetValue(_isFirstLocoElectricalKey, value);
        }

        private static readonly DependencyPropertyKey _isSecondLocoElectricalKey = DependencyProperty.RegisterReadOnly(
            "IsSecondLocoElectrical",
            typeof(bool),
            typeof(LocoReplacementDialog),
            new PropertyMetadata(false));
        public static readonly DependencyProperty IsSecondLocoElectricalProperty = _isSecondLocoElectricalKey.DependencyProperty;
        public bool IsSecondLocoElectrical
        {
            get => (bool)GetValue(IsSecondLocoElectricalProperty);
            private set => SetValue(_isSecondLocoElectricalKey, value);
        }

        public ObservableCollection<Notch> Zugarten => _zugarten;

        public static readonly RoutedUICommand ReplLocoCommand = new("_Lok tauschen", "ReplLocoCommand", typeof(LocoReplacementDialog));

        public LocoReplacementDialog()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(ReplLocoCommand, OnReplLoco, OnCanReplLoco));
        }

        public int GetPantograph(int loco)
        {
            int res = 0;

            if (loco == 1 && IsFirstLocoElectrical)
            {
                if (Pantograph1_0 != 0) res |= 1;
                if (Pantograph1_1 != 0) res |= 2;
                if (Pantograph1_2 != 0) res |= 4;
                if (Pantograph1_3 != 0) res |= 8;
            }
            else if (loco == 2 && IsSecondLocoElectrical)
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

            _textFirstLoco = _textSecondLoco = null;
            BRFirstLoco = BRSecondLoco = null;
            DescrFirstLoco = DescrSecondLoco = null;

            //_train = train;
            DialogTitle = string.Format("{0} • Loktausch", train.Nummer);

            if (train.FirstIsLeadingLoco)
            {
                _textFirstLoco = "führende Lok";
            }
            else
            {
                _textFirstLoco = "schiebende Lok";
            }
            if (train.SecondLoco != null)
            {
                _textSecondLoco = "zweite Lok";
                fv = train.SecondLoco.Value.GetVariante();
                BRSecondLoco = fv.BR;
                DescrSecondLoco = fv.Beschreibung; 
                ButtonText = "_Loks tauschen";
            }
            else
            {
                ButtonText = "Lok tauschen";
            }

            FahrzeugInfo fi = train.FirstLoco.Value;
            fv = fi.GetVariante();
            BRFirstLoco = fv.BR;
            DescrFirstLoco = fv.Beschreibung;

            TextFirstLoco = _textFirstLoco;
            TextSecondLoco = _textSecondLoco;

            HasDoubleHeading = train.HasDoubleHeading;
        }

        private static void OnReleaseDoubleHeadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as LocoReplacementDialog)?.OnReleaseDoubleHeadingChanged((bool)e.NewValue);
        }

        private void OnReleaseDoubleHeadingChanged(bool value)
        {
            if (value && HasDoubleHeading)
            {
                TextFirstLoco = "Doppeltraktion";
                Traction1 = TractionType.SingleHeading;
                Traction2 = TractionType.SingleHeading;
            }
            else
            {
                TextFirstLoco = _textFirstLoco;
                TextSecondLoco = _textSecondLoco;
            }
            CalculateBrakeHundredth();
        }

        private static void OnLocoPosition1Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as LocoReplacementDialog)?.OnLocoPosition1Changed((LocoPositionType)e.NewValue);
        }

        private void OnLocoPosition1Changed(LocoPositionType value)
        {
            if (value == LocoPositionType.Leading)
            {
                LocoPosition2aText = "2. führende Lok";
                LocoPosition2bText = "schiebende Lok";
                LocoPosition2aEnabled = SelectedReplLoco2 >= 0;
            }
            else
            {
                LocoPosition2aText = "führende Lok";
                LocoPosition2bText = "2. schiebende Lok";
                LocoPosition2 = LocoPositionType.Pushing;
                LocoPosition2aEnabled = false;
            }
        }

        private static void OnBremsstellungChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as LocoReplacementDialog)?.OnBremsstellungChanged((Bremsstellung)e.NewValue);
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
            CalculateBrakeHundredth();
        }

        private static void OnSelectedReplLoco1Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as LocoReplacementDialog)?.OnSelectedReplLoco1Changed((int)e.NewValue);
        }

        private void OnSelectedReplLoco1Changed(int value)
        {
            DataManager dm = DataManager.Instance;
            if (value >= 0 && value < dm.ReplacementLocos.Count)
            {
                _zugarten.Clear();
                _zugdatenType = ZugdatenType.None;

                ReplacementLoco rl = dm.ReplacementLocos[value];
                VehicleVariant vv = rl.SelectedVariant;
                foreach (Indusi i in vv.Indusis)
                {
                    switch (i.ZugdatenType)
                    {
                        case ZugdatenType.IndusiAnalog:
                            foreach (string s in _zugdatenAnalog)
                            {
                                _zugarten.Add(new Notch() { Label = s });
                            }
                            _zugdatenType = ZugdatenType.IndusiAnalog;
                            break;
                        case ZugdatenType.IndusiRechner:
                            foreach (string s in _zugdatenRechner)
                            {
                                _zugarten.Add(new Notch() { Label = s });
                            }
                            _zugdatenType = ZugdatenType.IndusiRechner;
                            break;
                        case ZugdatenType.LZB80:
                            foreach (string s in _zugdatenLZB80)
                            {
                                _zugarten.Add(new Notch() { Label = s });
                            }
                            _zugdatenType = ZugdatenType.LZB80;
                            break;
                    }
                    if (_zugdatenType != ZugdatenType.None)
                        break;
                }
                Zugart = Zugart.None;
                Bremsstellung = Bremsstellung.Unknown;
                BrHSelector = 0;
                IsFirstLocoElectrical = vv.IsLocoElectrical;
            }
            else
            {
                IsFirstLocoElectrical = false;
            }
            CalculateBrakeHundredth();
            UpdateShowPantographOptions();
        }

        private static void OnSelectedReplLoco2Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as LocoReplacementDialog)?.OnSelectedReplLoco2Changed((int)e.NewValue);
        }

        private void OnSelectedReplLoco2Changed(int value)
        {
            DataManager dm = DataManager.Instance;
            if (value >= 0 && value < dm.ReplacementLocos.Count)
            {
                ReplacementLoco rl = dm.ReplacementLocos[value];
                VehicleVariant vv = rl.SelectedVariant;
                IsSecondLocoElectrical = vv.IsLocoElectrical;
                LocoPosition2aEnabled = LocoPosition1 == LocoPositionType.Leading;
            }
            else
            {
                IsSecondLocoElectrical = false;
                LocoPosition2aEnabled = false;
            }
            UpdateShowPantographOptions();
        }

        private static void OnTraction1Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as LocoReplacementDialog)?.OnTraction1Changed((TractionType)e.NewValue);
        }

        private void OnTraction1Changed(TractionType value)
        {
            CalculateBrakeHundredth();
            if (value == TractionType.DoubleHeading)
            {
                IsSecondLocoElectrical = IsFirstLocoElectrical;
            }
            else
            {
                OnSelectedReplLoco2Changed(SelectedReplLoco2);
            }
        }

        private static void OnTraction2Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as LocoReplacementDialog)?.CalculateBrakeHundredth();
        }

        private static void OnZugartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as LocoReplacementDialog)?.OnZugartChanged((Zugart)e.NewValue);
        }

        private void OnZugartChanged(Zugart value)
        {
            if (value == Zugart.None)
            {
                BrHSelector = 0;
            }
        }

        private void OnCanReplLoco(object sender, CanExecuteRoutedEventArgs e)
        {
            if (HasDoubleHeading)
            {
                if (ReleaseDoubleHeading)
                {
                    e.CanExecute = SelectedReplLoco1 >= 0 && Traction1 == TractionType.SingleHeading;
                }
                else
                {
                    e.CanExecute = SelectedReplLoco1 >= 0 && SelectedReplLoco2 >= 0;
                }
            }
            else
            {
                e.CanExecute = SelectedReplLoco1 >= 0;
            }
        }

        private void OnReplLoco(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CalculateBrakeHundredth()
        {
            if (Bremsstellung > Bremsstellung.Unknown && SelectedReplLoco1 >= 0)
            {
                DataManager dm = DataManager.Instance;
                ReplacementLoco rl = dm.ReplacementLocos[SelectedReplLoco1];
                double m = rl.SelectedVariant.Mass;
                double bw = rl.SelectedVariant.GetBrakeWeight(Bremsstellung);
                BrH = m > 0 ? (int)(bw * 100 / m) : 0;
            }
            else
            {
                BrH = 0;
            }
        }

        private void UpdateShowPantographOptions()
        {
            ShowPantographOptions = IsFirstLocoElectrical || IsSecondLocoElectrical;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            
        }
    }

    [ValueConversion(typeof(Bremsstellung), typeof(uint))]
    public class BremsstellungConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Bremsstellung b)
            {
                switch (b)
                {
                    case Bremsstellung.G:
                        return 1;
                    case Bremsstellung.P:
                    case Bremsstellung.P_Mg:
                        return 2;
                    case Bremsstellung.R:
                    case Bremsstellung.R_Mg:
                        return 3;
                }
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is uint u)
            {
                switch (u)
                {
                    case 1:
                        return Bremsstellung.G;
                    case 2:
                        return Bremsstellung.P;
                    case 3:
                        return Bremsstellung.R;
                }
            }

            return Bremsstellung.Unknown;
        }
    }

  [ValueConversion(typeof(Zugart), typeof(uint))]
  public class ZugartConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value is Zugart z ? (uint)z : 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value is uint u && u <= 7 ? (Zugart)u : Zugart.None;
    }
  }

  public class BrakeHundredthEnabledConverter : IMultiValueConverter
  {
    // 0: Anzahl Labels
    // 1: Zugart
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      return values.Length == 2 && values[0] is int c && c > 1 && c != 4 && values[1] is Zugart z && z != Zugart.None;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class DoubleHeadingBorderConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 2 || values[0] is not bool b0 || values[1] is not bool b1)
        return DependencyProperty.UnsetValue;

      return b0 && !b1 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  public class OrientationConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value is bool b && b ? Orientation.Vertical : Orientation.Horizontal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
