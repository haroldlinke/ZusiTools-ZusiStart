using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ZusiStart.Controls
{
  public partial class NumericUpDown : UserControl
  {
    private static readonly Regex _allowedRegex = new(@"^-?\d+$");
    private bool _internalUpdate;

    public NumericUpDown()
    {
      InitializeComponent();
      PART_TextBox.Text = Value.ToString();
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
      nameof(Value), typeof(int), typeof(NumericUpDown),
      new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceValueCallback));

    public int Value
    {
      get => (int)GetValue(ValueProperty);
      set => SetValue(ValueProperty, value);
    }

    private static object CoerceValueCallback(DependencyObject d, object baseValue)
    {
      if (d is NumericUpDown nd && baseValue is int v)
      {
        if (v < nd.Min) return nd.Min;
        if (v > nd.Max) return nd.Max;
        return v;
      }
      return baseValue!;
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is not NumericUpDown nd) return;

      // Wenn intern gerade geschrieben wird, nichts tun
      if (nd._internalUpdate) return;

      nd._internalUpdate = true;
      void UpdateText()
      {
        if (nd.PART_TextBox != null)
          nd.PART_TextBox.Text = nd.Value.ToString();
      }

      if (nd.PART_TextBox != null)
      {
        UpdateText();
        nd._internalUpdate = false;
      }
      else
      {
        // sicherstellen, dass UI-Element vorhanden ist (z. B. wenn SetValue vor InitializeComponent / Loaded aufgerufen wurde)
        nd.Dispatcher.BeginInvoke((Action)(() =>
        {
          UpdateText();
          nd._internalUpdate = false;
        }), System.Windows.Threading.DispatcherPriority.Loaded);
      }
    }

    public static readonly DependencyProperty MinProperty = DependencyProperty.Register(
      nameof(Min), typeof(int), typeof(NumericUpDown), new PropertyMetadata(int.MinValue, OnMinMaxChanged));

    public int Min
    {
      get => (int)GetValue(MinProperty);
      set => SetValue(MinProperty, value);
    }

    public static readonly DependencyProperty MaxProperty = DependencyProperty.Register(
      nameof(Max), typeof(int), typeof(NumericUpDown), new PropertyMetadata(int.MaxValue, OnMinMaxChanged));

    public int Max
    {
      get => (int)GetValue(MaxProperty);
      set => SetValue(MaxProperty, value);
    }

    private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is NumericUpDown nd)
      {
        nd.CoerceValue(ValueProperty);
      }
    }

    public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
      nameof(Step), typeof(int), typeof(NumericUpDown), new PropertyMetadata(1));

    public int Step
    {
      get => (int)GetValue(StepProperty);
      set => SetValue(StepProperty, value);
    }

    public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register(
      nameof(MaxLength), typeof(int), typeof(NumericUpDown), new PropertyMetadata(5));

    public int MaxLength
    {
      get => (int)GetValue(MaxLengthProperty);
      set => SetValue(MaxLengthProperty, value);
    }

    private void PART_ButtonUp_Click(object sender, RoutedEventArgs e)
    {
      ChangeValueBy(Step);
    }

    private void PART_ButtonDown_Click(object sender, RoutedEventArgs e)
    {
      ChangeValueBy(-Step);
    }

    private void ChangeValueBy(int delta)
    {
      int newVal = Value + delta;
      if (newVal > Max) newVal = Max;
      if (newVal < Min) newVal = Min;
      Value = newVal;
    }

    private void PART_TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      // Allow digits and optional leading '-' if Min < 0
      string proposed = GetProposedText(e.Text);
      e.Handled = !IsTextValidNumber(proposed);
    }

    private void PART_TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
      if (e.DataObject.GetDataPresent(DataFormats.Text))
      {
        var text = e.DataObject.GetData(DataFormats.Text) as string ?? string.Empty;
        string proposed = GetProposedText(text);
        if (!IsTextValidNumber(proposed))
          e.CancelCommand();
      }
      else
      {
        e.CancelCommand();
      }
    }

    private string GetProposedText(string input)
    {
      var tb = PART_TextBox;
      string text = tb.Text ?? string.Empty;
      int selStart = tb.SelectionStart;
      int selLen = tb.SelectionLength;
      string before = text.Substring(0, selStart);
      string after = text.Substring(Math.Min(text.Length, selStart + selLen));
      return before + input + after;
    }

    private bool IsTextValidNumber(string text)
    {
      if (string.IsNullOrEmpty(text)) return true;
      if (text == "-" && Min < 0) return true;
      return _allowedRegex.IsMatch(text);
    }

    private void PART_TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
      ApplyTextToValue();
    }

    private void PART_TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        ApplyTextToValue();
        // move focus to commit
        MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        e.Handled = true;
      }
      else if (e.Key == Key.Up)
      {
        ChangeValueBy(Step);
        e.Handled = true;
      }
      else if (e.Key == Key.Down)
      {
        ChangeValueBy(-Step);
        e.Handled = true;
      }
    }

    private void ApplyTextToValue()
    {
      if (_internalUpdate) return;
      string txt = PART_TextBox.Text?.Trim() ?? string.Empty;
      if (string.IsNullOrEmpty(txt) || txt == "-")
      {
        // keep current value if empty
        PART_TextBox.Text = Value.ToString();
        return;
      }
      if (int.TryParse(txt, out int v))
      {
        if (v < Min) v = Min;
        if (v > Max) v = Max;
        _internalUpdate = true;
        Value = v;
        PART_TextBox.Text = Value.ToString();
        _internalUpdate = false;
      }
      else
      {
        PART_TextBox.Text = Value.ToString();
      }
    }

    private void PART_TextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (e.Delta > 0) ChangeValueBy(Step);
      else ChangeValueBy(-Step);
      e.Handled = true;
    }
  }
}