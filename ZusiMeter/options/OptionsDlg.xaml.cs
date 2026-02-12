using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using ZusiMeter.About;
using ZusiMeter.Miscellaneous;
using ZusiMeter.Properties;

#nullable disable
namespace ZusiMeter.Options
{
    
  public partial class OptionsDlg : Window, IComponentConnector
  {
    public static readonly RoutedUICommand CommandClose = new RoutedUICommand("Schließen", nameof (CommandClose), typeof (OptionsDlg));

    public static readonly RoutedUICommand CommandSave = new RoutedUICommand("Speichern", nameof(CommandSave), typeof(OptionsDlg));
    public static readonly RoutedUICommand CommandCheckConnection = new RoutedUICommand("Verbindung Testen", nameof(CommandCheckConnection), typeof(OptionsDlg));

    public static readonly RoutedUICommand CommandDefinePrivateLayoutFolder = new RoutedUICommand("Privaten Layoutfolder bestimmen", nameof(CommandDefinePrivateLayoutFolder), typeof(OptionsDlg));
    public static readonly RoutedUICommand CommandDefineExampleLayoutFolder = new RoutedUICommand("Beispiel Layoutfolder bestimmen", nameof(CommandDefineExampleLayoutFolder), typeof(OptionsDlg));


    public OptionsDlg(ZusiMeterControl ZusiMeterControl)
    {
      this.InitializeComponent();
      this.DataContext = ZusiMeterControl;
      this.CommandBindings.Add(new CommandBinding((ICommand) OptionsDlg.CommandClose, (ExecutedRoutedEventHandler) ((s, e) => this.DialogResult = new bool?(false)), (CanExecuteRoutedEventHandler) ((s, e) => e.CanExecute = true)));
      this.CommandBindings.Add(new CommandBinding((ICommand)OptionsDlg.CommandSave, (ExecutedRoutedEventHandler)((s, e) => this.DialogResult = new bool?(true)), (CanExecuteRoutedEventHandler)((s, e) => e.CanExecute = true)));
      this.CommandBindings.Add(new CommandBinding((ICommand)OptionsDlg.CommandCheckConnection, new ExecutedRoutedEventHandler(ZusiMeterControl.OnCheckConnection)));

      this.CommandBindings.Add(new CommandBinding((ICommand)OptionsDlg.CommandDefinePrivateLayoutFolder, new ExecutedRoutedEventHandler(ZusiMeterControl.OnDefinePrivateLayoutFolder)));
      this.CommandBindings.Add(new CommandBinding((ICommand)OptionsDlg.CommandDefineExampleLayoutFolder, new ExecutedRoutedEventHandler(ZusiMeterControl.OnDefineExampleLayoutFolder)));

    }
  }
}
