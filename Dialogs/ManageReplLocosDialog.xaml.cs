using System;
using System.Collections.Generic;
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
using ZusiStart.Data;

namespace ZusiStart.Dialogs
{
    /// <summary>
    /// Interaktionslogik für ReplacementLocoDialog.xaml
    /// </summary>
    public partial class ManageReplLocosDialog : Window
    {
        private bool _newLoco;

        public static readonly RoutedUICommand AcceptCommand = new RoutedUICommand("Ü_bernehmen", "AcceptCommand", typeof(ManageReplLocosDialog));
        public static readonly RoutedUICommand DiscardCommand = new RoutedUICommand("Änderungen _verwerfen", "DiscardCommand", typeof(ManageReplLocosDialog));
        public static readonly RoutedUICommand DelCommand = new RoutedUICommand("_Löschen", "DelCommand", typeof(ManageReplLocosDialog));
        public static readonly RoutedUICommand NewCommand = new RoutedUICommand("_Neue Austauschlok", "NewCommand", typeof(ManageReplLocosDialog));
        public static readonly RoutedUICommand OkCommand = new RoutedUICommand("_Fertig", "OkCommand", typeof(ManageReplLocosDialog));

        public ManageReplLocosDialog()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(AcceptCommand, OnAccept, OnCanAccept));
            CommandBindings.Add(new CommandBinding(DiscardCommand, OnDiscard, OnCanDiscard));
            CommandBindings.Add(new CommandBinding(DelCommand, OnDel, OnCanDel));
            CommandBindings.Add(new CommandBinding(NewCommand, OnNew, OnCanNew));
            CommandBindings.Add(new CommandBinding(OkCommand, OnOk, OnCanOk));
        }

        private void OnCanAccept(object sender, CanExecuteRoutedEventArgs e)
        {
            ReplacementLoco loco = DataManager.Instance.SelectedLoco;
            e.CanExecute = loco != null && loco.IsDirty && loco.IsValid;
        }

        private void OnAccept(object sender, ExecutedRoutedEventArgs e)
        {
            if (_newLoco)
            {
                DataManager.Instance.AcceptNewLoco();
                _newLoco = false;
            }
            else
            {
                DataManager.Instance.ApplyChangesLoco();
            }
        }

        private void OnCanDiscard(object sender, CanExecuteRoutedEventArgs e)
        {
            ReplacementLoco loco = DataManager.Instance.SelectedLoco;
            e.CanExecute = loco != null && loco.IsDirty;
        }

        private void OnDiscard(object sender, ExecutedRoutedEventArgs e)
        {
            DataManager dm = DataManager.Instance;
            dm.SelectedLoco = dm.SelectedReplacementLoco != null ? new ReplacementLoco(dm.SelectedReplacementLoco) : null;
            _newLoco = false;
        }

        private void OnCanDel(object sender, CanExecuteRoutedEventArgs e)
        {
            DataManager dm = DataManager.Instance;
            e.CanExecute = dm.SelectedReplacementLoco != null && (dm.SelectedLoco == null || !dm.SelectedLoco.IsDirty);
        }

        private void OnDel(object sender, ExecutedRoutedEventArgs e)
        {
            DataManager.Instance.RemoveSelectedLoco();
        }

        private void OnCanNew(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataManager.Instance.SelectedLoco == null || !DataManager.Instance.SelectedLoco.IsDirty;
        }

        private void OnNew(object sender, ExecutedRoutedEventArgs e)
        {
            DataManager.Instance.SelectedLoco = new ReplacementLoco();
            _newLoco = true;
        }

        private void OnCanOk(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataManager.Instance.SelectedLoco == null || !DataManager.Instance.SelectedLoco.IsDirty;
        }

        private void OnOk(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
