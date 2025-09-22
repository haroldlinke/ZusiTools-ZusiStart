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
//using ZusiCLIProject.FileLibrary.Zusi3;
using ZusiKlassenLib.Fahrplan;
using ZusiStart.Data;

namespace ZusiStart.Dialogs
{
  /// <summary>
  /// Interaktionslogik für ReplacementTrainDialog.xaml
  /// </summary>
  public partial class ManageReplTrainsDialog : Window
  {
    private bool _newTrain;

    public static readonly RoutedUICommand AcceptCommand = new RoutedUICommand("Ü_bernehmen", "AcceptCommand", typeof(ManageReplTrainsDialog));
    public static readonly RoutedUICommand DiscardCommand = new RoutedUICommand("Änderungen _verwerfen", "DiscardCommand", typeof(ManageReplTrainsDialog));
    public static readonly RoutedUICommand DelCommand = new RoutedUICommand("_Löschen", "DelCommand", typeof(ManageReplTrainsDialog));
    public static readonly RoutedUICommand NewCommand = new RoutedUICommand("_Neuer Austauschzug", "NewCommand", typeof(ManageReplTrainsDialog));
    public static readonly RoutedUICommand OkCommand = new RoutedUICommand("_Fertig", "OkCommand", typeof(ManageReplTrainsDialog));
    public static readonly RoutedUICommand ReplaceTrainCommand = new RoutedUICommand("_Zug tauschen", "ReplaceTrainCommand", typeof(ManageReplTrainsDialog));

    public ManageReplTrainsDialog()
    {
      InitializeComponent();

      CommandBindings.Add(new CommandBinding(AcceptCommand, OnAccept, OnCanAccept));
      CommandBindings.Add(new CommandBinding(DiscardCommand, OnDiscard, OnCanDiscard));
      CommandBindings.Add(new CommandBinding(DelCommand, OnDel, OnCanDel));
      CommandBindings.Add(new CommandBinding(NewCommand, OnNew, OnCanNew));
      CommandBindings.Add(new CommandBinding(OkCommand, OnOk, OnCanOk));
      CommandBindings.Add(new CommandBinding(ReplaceTrainCommand, OnReplaceTrain, OnCanReplaceTrain));
    }

    private void OnCanAccept(object sender, CanExecuteRoutedEventArgs e)
    {
      ReplacementTrain Train = DataManager.Instance.SelectedTrainR;
      e.CanExecute = Train != null && Train.IsDirty && Train.IsValid;
    }

    private void OnAccept(object sender, ExecutedRoutedEventArgs e)
    {
      if (_newTrain)
      {
        DataManager.Instance.AcceptNewTrain();
        _newTrain = false;
      }
      else
      {
        DataManager.Instance.ApplyChangesTrain();
      }
    }

    private void OnCanDiscard(object sender, CanExecuteRoutedEventArgs e)
    {
      ReplacementTrain Train = DataManager.Instance.SelectedTrainR;
      e.CanExecute = Train != null && Train.IsDirty;
    }

    private void OnDiscard(object sender, ExecutedRoutedEventArgs e)
    {
      DataManager dm = DataManager.Instance;
      dm.SelectedTrainR = dm.SelectedReplacementTrain != null ? new ReplacementTrain(dm.SelectedReplacementTrain) : null;
      _newTrain = false;
    }

    private void OnCanDel(object sender, CanExecuteRoutedEventArgs e)
    {
      DataManager dm = DataManager.Instance;
      e.CanExecute = dm.SelectedReplacementTrain != null && (dm.SelectedTrainR == null || !dm.SelectedTrainR.IsDirty);
    }

    private void OnDel(object sender, ExecutedRoutedEventArgs e)
    {
      DataManager.Instance.RemoveSelectedTrain();
    }

    private void OnCanNew(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = DataManager.Instance.SelectedTrainR == null || !DataManager.Instance.SelectedTrainR.IsDirty;
    }

    private void OnNew(object sender, ExecutedRoutedEventArgs e)
    {
      DataManager.Instance.SelectedTrainR = new ReplacementTrain();
      _newTrain = true;
    }

    private void OnCanOk(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = DataManager.Instance.SelectedTrainR == null || !DataManager.Instance.SelectedTrainR.IsDirty;
    }

    private void OnOk(object sender, ExecutedRoutedEventArgs e)
    {
      DialogResult = true;
    }

    private void OnCanReplaceTrain(object sender, CanExecuteRoutedEventArgs e)
    {
      DataManager dm = DataManager.Instance;
      e.CanExecute = dm.SelectedReplacementTrain != null && DataManager.Instance.CurrentTrain != null && (dm.SelectedTrainR == null || !dm.SelectedTrainR.IsDirty);
    }

    private void OnReplaceTrain(object sender, ExecutedRoutedEventArgs e)
    {
      DialogResult = true;
      DataManager dm = DataManager.Instance;
      if (DataManager.Instance.CurrentTrainItem != null)
      {
        DataManager.Instance.CurrentTrainItem.ReplaceTrain2(true);
      }
      else
      {
        DataManager.Instance.CurrentTrainItem = new TrainItem(DataManager.Instance.CurrentTrain);
        DataManager.Instance.CurrentTrainItem.ReplaceTrain2(true);
      }
      DataManager.Instance.CurrentTrainItem.OrigZug = DataManager.Instance.CurrentTrain;
      Zug replacetrain = new(DataManager.Instance.CurrentTrain.Parent, DataManager.Instance.CurrentTrain);
      replacetrain.ReplaceTrain(DataManager.Instance.CurrentTrainItem.ReplaceReihung);
      DataManager.Instance.CurrentTrain = replacetrain;

    }
  }
}
