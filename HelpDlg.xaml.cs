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
using System.IO;
using log4net;

namespace ZusiStart
{
  /// <summary>
  /// Interaktionslogik für HelpDlg.xaml
  /// </summary>
  public partial class HelpDlg : Window
  {
    private static readonly ILog _log = LogManager.GetLogger(typeof(HelpDlg));

    public HelpDlg()
    {
      try
      {
        InitializeComponent();
        InitializeWebView();
      }
      catch (Exception ex)
      {
        _log.Fatal("Start WebView" + ex.ToString());
        int num = (int)System.Windows.MessageBox.Show(ex.ToString(), "Fehler beim Öffnen der Dokumentation", MessageBoxButton.OK);
      }
    }

    private async void InitializeWebView()
    {
      try
      {
        await webView.EnsureCoreWebView2Async(null);
        string relativePath = "help/ZusiStart_Docu_Deutsch.pdf";
        string absolutePath = Path.GetFullPath(relativePath);
        webView.Source = new Uri($"file:///{absolutePath}");
      }
      catch (Exception ex)
      {
        _log.Fatal("Open Doku" + ex.ToString());
        int num = (int)System.Windows.MessageBox.Show(ex.ToString(), "Fehler beim Öffnen der Dokumentation", MessageBoxButton.OK);
      }
    }
  }
}
