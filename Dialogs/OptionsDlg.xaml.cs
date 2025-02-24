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

namespace ZusiStart.Dialogs
{
  /// <summary>
  /// Interaktionslogik für OptionsDlg.xaml
  /// </summary>
  public partial class OptionsDlg : Window
  {

    public OptionsDlg()
    {
      InitializeComponent();
      GetOptions();

    }

    private void SaveOptions_Click(object sender, RoutedEventArgs e)
    {
      var local_options = new Options
      {
        Show_ZSK = CheckBox_ZSK.IsChecked ?? false,
        Show_ZDB = CheckBox_ZDB.IsChecked ?? false,
        Show_Bfpl = CheckBox_Bfpl.IsChecked ?? false,
        ZSK_Url = TextBox_ZSK_URL.Text,
        ZDB_Url = TextBox_ZDB_URL.Text,
        Bfpl_Exe = TextBox_Bfpl_Exe.Text,
        Start_FIS = CheckBox_Start_FIS.IsChecked ?? false,
        ZusiDisplay_Exe = TextBox_ZusiDisplay_Exe.Text,
        ZusiDisplay_Param = TextBox_ZusiDisplay_Param.Text,
        Start_ZusiMeter = CheckBox_Start_ZusiMeter.IsChecked ?? false,
        ZusiMeter_Exe = TextBox_ZusiMeter_Exe.Text,
        ZusiMeter_Param = TextBox_ZusiMeter_Param.Text,
      };
      DataManager.Instance.options = local_options;
      DataManager.Instance.main_window.ZSKButtonVisibility = local_options.Show_ZSK? Visibility.Visible: Visibility.Collapsed;
      DataManager.Instance.main_window.ZDBButtonVisibility = local_options.Show_ZDB ? Visibility.Visible : Visibility.Collapsed;
      DataManager.Instance.main_window.BfpButtonVisibility = local_options.Show_Bfpl ? Visibility.Visible : Visibility.Collapsed;
      this.Close();

    }

    private void GetOptions()
    {
      var local_options = DataManager.Instance.options;

      CheckBox_ZSK.IsChecked = local_options.Show_ZSK;
      if (!string.IsNullOrEmpty(local_options.ZSK_Url))
        TextBox_ZSK_URL.Text = local_options.ZSK_Url;
      else
      {
        TextBox_ZSK_URL.Text = "https://zusi-sk.eu/";
      }

      CheckBox_ZDB.IsChecked = local_options.Show_ZDB;
      if (!string.IsNullOrEmpty(local_options.ZDB_Url))
        TextBox_ZDB_URL.Text = local_options.ZDB_Url;
      else
      {
        TextBox_ZDB_URL.Text = "https://www.zusidatenbank.de/?zusistart";
      }

      CheckBox_Bfpl.IsChecked = local_options.Show_Bfpl;
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
      CheckBox_Start_FIS.IsChecked = local_options.Start_FIS;
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
      CheckBox_Start_ZusiMeter.IsChecked = local_options.Start_ZusiMeter;
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
    }
  }
}
