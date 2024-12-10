using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ZusiStart.About
{
    /// <summary>
    /// Interaktionslogik für AboutDlg.xaml
    /// </summary>
    public partial class AboutDlg : Window
    {
        public static readonly RoutedUICommand CommandClose = new RoutedUICommand("Schließen", "CommandClose", typeof(AboutDlg));

        public AboutDlg()
        {
            InitializeComponent();
            Closing += AboutDlg_Closing;
            CommandBindings.Add(new CommandBinding(CommandClose, (s, e) => DialogResult = true, (s, e) => e.CanExecute = true));
        }

        private void AboutDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Closing -= AboutDlg_Closing;
            e.Cancel = true;
            var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(600));
            anim.Completed += (s, a) => Close();
            BeginAnimation(OpacityProperty, anim);
        }
    }
}
