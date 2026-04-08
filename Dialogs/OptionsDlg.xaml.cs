using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using System.Globalization;
using log4net;

namespace ZusiStart.Dialogs
{
  /// <summary>
  /// Interaktionslogik für OptionsDlg.xaml
  /// </summary>
  public partial class OptionsDlg : Window
  {

    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


    public OptionsDlg()
    {
      InitializeComponent();
      // Sprache beim Öffnen setzen
      string lang = Properties.Settings.Default.Language;
      if (!string.IsNullOrEmpty(lang) && !lang.Contains("auto"))
      {
        DataManager.CurrentLanguage = lang;
      }
      else
      {
        // Automatische Erkennung, z. B. anhand CultureInfo
        DataManager.CurrentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        DataManager.CurrentLanguage = DataManager.CurrentLanguage switch
        {
          "de" => "de",
          "en" => "en",
          "fr" => "fr",
          _ => "en",
        };
      }
      GetOptions();
    }

    private void SaveOptions_Click(object sender, RoutedEventArgs e)
    {
      SaveOptions();
    }

    public void SaveOptions()
    {
      // Sprache speichern
      if (ComboBox_Language.SelectedItem is ComboBoxItem item)
      {
        Properties.Settings.Default.Language = (string)item.Tag;
      }
      Properties.Settings.Default.Save();

      var local_options = new Options
      {
        Show_ZSK = true,// CheckBox_ZSK.IsChecked ?? false,
        Show_ZDB = true, //CheckBox_ZDB.IsChecked ?? false,
        //Show_Bfpl = CheckBox_Bfpl.IsChecked ?? false,
        ZSK_Url = TextBox_ZSK_URL.Text,
        ZDB_Url = TextBox_ZDB_URL.Text,
        Bfpl_Exe = TextBox_Bfpl_Exe.Text,
        Start_FIS = true,//CheckBox_Start_FIS.IsChecked ?? false,
        ZusiDisplay_Exe = TextBox_ZusiDisplay_Exe.Text,
        ZusiDisplay_Param = TextBox_ZusiDisplay_Param.Text,
        Start_ZusiMeter = true, //CheckBox_Start_ZusiMeter.IsChecked ?? false,
        ZusiMeter_Exe = TextBox_ZusiMeter_Exe.Text,
        ZusiMeter_Param = TextBox_ZusiMeter_Param.Text,
        New_RenderEngine = CheckBox_New_RenderEngine.IsChecked ?? false,
        Blickwinkel_value = TextBox_Blickwinkel.Text,
        RemoteZusi = CheckBox_RemoteZusi.IsChecked ?? false,
        RemoteZusiIP = TextBox_RemoteZusiIP.Text,
        RemoteTrackingSupport = CheckBox_RemoteTrackingSupport.IsChecked ?? false,
        DecoTrain_Separate = CheckBox_DecoTrain_Separate.IsChecked ?? false,
        DonotHideZusiStart = CheckBox_DonotHideZusiStart.IsChecked ?? false,
        //StartOnlySelectedTrain = CheckBox_StartOnlySelectedTrain.IsChecked ?? false,
        Show_ZusiMeter_Data = CheckBox_Show_ZusiMeter_Data.IsChecked ?? false,
        ZusiMeter_Standard_Layoutfile = TextBox_ZusiMeter_Standard_Layoutfile.Text,
        RO_starttime_no_decotrains = CheckBox_RO_starttime_no_decotrains.IsChecked ?? false,
        RO_trainselectioncriteria_Stations = CheckBox_RO_trainselectioncriteria_Stations.IsChecked ?? false,
        RO_trainselection_Streckenmodule = CheckBox_RO_trainselection_Streckenmodule.IsChecked ?? false,
        Buchfahrplanlayout = ComboBox_BuchfahrplanLayout.SelectedItem.ToString() ?? "Automatisch aus TRN-Datei",
      };
      DataManager.Instance.options = local_options;
      DataManager.Instance.main_window.ZSKButtonVisibility = local_options.Show_ZSK ? Visibility.Visible : Visibility.Collapsed;
      DataManager.Instance.main_window.ZDBButtonVisibility = local_options.Show_ZDB ? Visibility.Visible : Visibility.Collapsed;
      DataManager.Instance.main_window.BfpButtonVisibility = local_options.Show_Bfpl ? Visibility.Visible : Visibility.Collapsed;
      this.Close();
      if (local_options.RemoteZusi)
      {
        ((RoutedUICommand)MainWindow.StartTrainCommand).Text = LocalizationManager.Translate("Remote Zug monitoren");
        DataManager.Instance.main_window.BtnStartTrain.Content = ((RoutedUICommand)MainWindow.StartTrainCommand).Text;
      }
      else
      {
        ((RoutedUICommand)MainWindow.StartTrainCommand).Text = LocalizationManager.Translate("Ausgewählten Zug fahren");
        DataManager.Instance.main_window.BtnStartTrain.Content = ((RoutedUICommand)MainWindow.StartTrainCommand).Text;
      }
    }

    private void UpdateLanguageComboBoxTexts()
    {
      ComboBoxItem_Auto.Content = LocalizationManager.Translate("Automatisch");
      ComboBoxItem_De.Content = LocalizationManager.Translate("Deutsch");
      ComboBoxItem_En.Content = LocalizationManager.Translate("Englisch");
      ComboBoxItem_Fr.Content = LocalizationManager.Translate("Französisch");
    }

    //Native Directorys for BuchfahrplanDLL 
    private static string[] NativeDirectorys;

    public static void ManualInitNativeStandardsFileName(string nativeStandardsFilePath)
    {
      NativeDirectorys = [];
      NativeDirectorys.Append("Automatic");
      string[] array = File.ReadAllText(nativeStandardsFilePath, Encoding.UTF8).Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
      foreach (string text in array)
      {
        if (!text.StartsWith(";"))
        {
          string[] array2 = text.Split('\t');
          if (array2.Length >= 2)
          {
            NativeDirectorys.Append(array2[0]);
          }
        }
      }
    }

    private void GetOptions()
    {
      var local_options = DataManager.Instance.options;

      //CheckBox_ZSK.IsChecked = local_options.Show_ZSK;
      if (!string.IsNullOrEmpty(local_options.ZSK_Url))
        TextBox_ZSK_URL.Text = local_options.ZSK_Url;
      else
      {
        TextBox_ZSK_URL.Text = "https://www.zusi-sk.eu/";
      }

      //CheckBox_ZDB.IsChecked = local_options.Show_ZDB;
      if (!string.IsNullOrEmpty(local_options.ZDB_Url))
        TextBox_ZDB_URL.Text = local_options.ZDB_Url;
      else
      {
        TextBox_ZDB_URL.Text = "http://zusidatenbank.de/?zusistart";
      }

      //CheckBox_Bfpl.IsChecked = local_options.Show_Bfpl;
      if (!string.IsNullOrEmpty(local_options.Bfpl_Exe))
        TextBox_Bfpl_Exe.Text = local_options.Bfpl_Exe;
      else
      {
        TextBox_Bfpl_Exe.Text = DataManager.Instance.BildfahrplanExePath;
      }

      //CheckBox_Zusi_Exe.IsChecked = local_options.Zusi_Exe_indiv;
      //if (!string.IsNullOrEmpty(local_options.Zusi_Exe))
      //  TextBox_Zusi_Exe.Text = local_options.Zusi_Exe;
      //else
      //{
      //  TextBox_Zusi_Exe.Text = "";
      //}
      //CheckBox_Start_FIS.IsChecked = local_options.Start_FIS;
      if (!string.IsNullOrEmpty(local_options.ZusiDisplay_Exe))
        TextBox_ZusiDisplay_Exe.Text = local_options.ZusiDisplay_Exe;
      else
      {
        TextBox_ZusiDisplay_Exe.Text = DataManager.Instance.ZusiDisplayStartCmd;
      }
      if (!string.IsNullOrEmpty(local_options.ZusiDisplay_Param))
        TextBox_ZusiDisplay_Param.Text = local_options.ZusiDisplay_Param;
      else
      {
        TextBox_ZusiDisplay_Param.Text = DataManager.Instance.ZusiDisplayStartParam;
      }
      //CheckBox_Start_ZusiMeter.IsChecked = local_options.Start_ZusiMeter;
      if (!string.IsNullOrEmpty(local_options.ZusiMeter_Exe))
        TextBox_ZusiMeter_Exe.Text = local_options.ZusiMeter_Exe;
      else
      {
        TextBox_ZusiMeter_Exe.Text = DataManager.Instance.ZusiMeterStartCmd;
      }
      if (!string.IsNullOrEmpty(local_options.ZusiMeter_Param))
        TextBox_ZusiMeter_Param.Text = local_options.ZusiMeter_Param;
      else
      {
        TextBox_ZusiMeter_Param.Text = DataManager.Instance.ZusiMeterStartParam;
      }

      CheckBox_Show_ZusiMeter_Data.IsChecked = local_options.Show_ZusiMeter_Data;
      if (!string.IsNullOrEmpty(local_options.ZusiMeter_Standard_Layoutfile))
        TextBox_ZusiMeter_Standard_Layoutfile.Text = local_options.ZusiMeter_Standard_Layoutfile;
      else
      {
        TextBox_ZusiMeter_Standard_Layoutfile.Text = DataManager.Instance.options.ZusiMeter_Standard_Layoutfile;
      }
      if (!string.IsNullOrEmpty(local_options.ZusiMeter_Param))
        TextBox_ZusiMeter_Param.Text = local_options.ZusiMeter_Param;
      else
      {
        TextBox_ZusiMeter_Param.Text = DataManager.Instance.ZusiMeterStartParam;
      }

      CheckBox_New_RenderEngine.IsChecked = local_options.New_RenderEngine;
      TextBox_Blickwinkel.Text = local_options.Blickwinkel_value;

      CheckBox_RemoteZusi.IsChecked = local_options.RemoteZusi;
      TextBox_RemoteZusiIP.Text = local_options.RemoteZusiIP;
      CheckBox_RemoteTrackingSupport.IsChecked = local_options.RemoteTrackingSupport;

      if (DataManager.Instance.options.RemoteZusi)
      {
        ((RoutedUICommand)MainWindow.StartTrainCommand).Text = LocalizationManager.Translate("Remote Zug monitoren");
        DataManager.Instance.main_window.BtnStartTrain.Content = ((RoutedUICommand)MainWindow.StartTrainCommand).Text;
      }
      else
      {
        ((RoutedUICommand)MainWindow.StartTrainCommand).Text = LocalizationManager.Translate("Ausgewählten Zug fahren");
        DataManager.Instance.main_window.BtnStartTrain.Content = ((RoutedUICommand)MainWindow.StartTrainCommand).Text;
      }
      // Sprache im Dialog auf aktuelle Auswahl setzen

      CheckBox_DecoTrain_Separate.IsChecked = local_options.DecoTrain_Separate;
      CheckBox_DonotHideZusiStart.IsChecked = local_options.DonotHideZusiStart;
      //CheckBox_StartOnlySelectedTrain.IsChecked = local_options.StartOnlySelectedTrain;

      CheckBox_RO_starttime_no_decotrains.IsChecked= local_options.RO_starttime_no_decotrains;
      CheckBox_RO_trainselectioncriteria_Stations.IsChecked= local_options.RO_trainselectioncriteria_Stations;
      CheckBox_RO_trainselection_Streckenmodule.IsChecked = local_options.RO_trainselection_Streckenmodule;

      string currentLang = DataManager.CurrentLanguage ?? "auto";
      foreach (ComboBoxItem item in ComboBox_Language.Items)
      {
        if ((string)item.Tag == currentLang)
        {
          ComboBox_Language.SelectedItem = item;
          break;
        }
      }
      UpdateLanguageComboBoxTexts();

      try
      {
        _log.Debug("Intializiere Buchfahrplanlayouts...");
        ComboBox_BuchfahrplanLayout.Items.Clear();
        ComboBox_BuchfahrplanLayout.Items.Add("Automatisch aus TRN-Datei");

        ZusiKlassenLib2.DataPathType _dataPath = ZusiKlassenLib2.DataPathType.Unknown;
        _log.Debug("Lade Buchfahrplanlayouts aus NativeStandards.csv");
        string nativestandards_csv_file = ZusiKlassenLib2.Zusi.ZusiPath + @"_InstSetup\lib\timetable\lib\NativeStandards.csv";

        if (nativestandards_csv_file == null || !File.Exists(nativestandards_csv_file))
        {
          _log.Error("NativeStandards.csv nicht gefunden. Buchfahrplanlayouts können nicht geladen werden.");
          return;
        }
        string[] layout_array = File.ReadAllText(nativestandards_csv_file, Encoding.UTF8).Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        _log.Debug($"Gefundene Buchfahrplanlayouts: {layout_array.Length}");

        foreach (string data in layout_array)
        {
          if (!data.StartsWith(";"))
          {
            string[] array2 = data.Split('\t');

            if (array2.Length >= 2)
            {
              ComboBox_BuchfahrplanLayout.Items.Add(array2[0]);
            }
          }
        }
        if (ComboBox_BuchfahrplanLayout.Items.Contains(local_options.Buchfahrplanlayout))
        {
          ComboBox_BuchfahrplanLayout.SelectedItem = local_options.Buchfahrplanlayout;
        }
        _log.Debug($"Aktuelles Buchfahrplanlayout: {local_options.Buchfahrplanlayout}");
      }
      catch (Exception ex)
      {
        _log.Error("Fehler beim Laden der Buchfahrplanlayouts: " + ex.Message);
      }


        //string currenttraincat = local_options.CurrentTrainCat ?? "all";
        //foreach (ComboBoxItem item in ComboBox_TrainCategories.Items)
        //{
        //  if ((string)item.Tag == currenttraincat)
        //  {
        //    ComboBox_TrainCategories.SelectedItem = item;
        //    break;
        //  }
        //}
      }
  }
}
