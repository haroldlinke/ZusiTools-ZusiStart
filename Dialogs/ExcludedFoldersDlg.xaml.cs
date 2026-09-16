using System;
using System.Windows;
using System.Windows.Controls;
using log4net;
using ZusiStart.Data;
using ZusiKlassenLib2;

namespace ZusiStart.Dialogs
{
  /// <summary>
  /// Interaktionslogik für ExcludedFoldersDlg.xaml.
  /// Ermöglicht dem Anwender, beliebige Ordner zu verwalten, die von der Suche nach
  /// Zusi-Daten ausgeschlossen werden sollen. Die Liste wird dauerhaft als JSON-Datei
  /// gespeichert (siehe DataManager.ExcludedFoldersFilePath).
  /// </summary>
  public partial class ExcludedFoldersDlg : Window
  {
    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public ExcludedFoldersDlg()
    {
      InitializeComponent();
    }

    private void ExcludedFoldersDialog_Loaded(object sender, RoutedEventArgs e)
    {
      // ObservableCollection direkt binden, damit Änderungen sofort in der ListBox sichtbar sind
      ListBox_ExcludedFolders.ItemsSource = DataManager.Instance.ExcludedFolders;
    }

    private void ListBox_ExcludedFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      Button_Remove.IsEnabled = ListBox_ExcludedFolders.SelectedItem != null;
    }

    private void AddFolder_Click(object sender, RoutedEventArgs e)
    {
      using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
      {
        dialog.Description = LocalizationManager.Translate("Ordner auswählen, der von der Suche ausgeschlossen werden soll");
        dialog.ShowNewFolderButton = false;

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
          DataPathType dtp = DataPathType.Unknown;
          string folder = Zusi.GetRelativePathOf(dialog.SelectedPath, ref dtp);
          if (folder.StartsWith("Timetables\\"))
            folder = folder.Substring(11);
          
          if (!string.IsNullOrWhiteSpace(folder))
          {
            _log.Debug("Füge ausgeschlossenen Ordner hinzu: " + folder);
            DataManager.Instance.AddExcludedFolder(folder);
          }
        }
      }
    }

    private void RemoveFolder_Click(object sender, RoutedEventArgs e)
    {
      if (ListBox_ExcludedFolders.SelectedItem is string folder)
      {
        _log.Debug("Entferne ausgeschlossenen Ordner: " + folder);
        DataManager.Instance.RemoveExcludedFolder(folder);
      }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
