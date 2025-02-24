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
using Microsoft.Web.WebView2.Core;
using ZusiKlassenLib.Fahrplan;
using ZusiKlassenLib;
using ZusiStart.Data;
using System.Security.Policy;
using Sovoma;
using System.Windows.Forms;
using Microsoft.Extensions.Hosting;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using ZusiKlassenLib.TimeTable;

namespace ZusiStart.Dialogs
{
  /// <summary>
  /// Interaktionslogik für WebView_Window.xaml
  /// </summary>
  public partial class WebView_Window : Window
  {

    public string _websource = "https://www.zusidatenbank.de/?zusistart";
    private bool _navigation_completed;

    public WebView_Window()
    {
      InitializeComponent();
      InitializeWebView();
      RestoreWindowState();
      //InitializeWebView();
    }
    private async void InitializeWebView()
    {
      //try
      //{
      //  await webViewWin.EnsureCoreWebView2Async(DataManager.Instance.webview_environment);
      //}
      //catch (Exception ex)
      //{
      // string errormessage = ex.ToString();
      //}

      await webViewWin.EnsureCoreWebView2Async(DataManager.Instance.webview_environment);
      await webViewWin.EnsureCoreWebView2Async(null);
      webViewWin.NavigationCompleted += WebView2_NavigationCompleted;
      webViewWin.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
      webViewWin.CoreWebView2.SourceChanged += CoreWebView2SourceChanged;
      webViewWin.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;

      webViewWin.CoreWebView2.Navigate(_websource);
      //webViewWin.CoreWebView2.SourceChanged += CoreWebView2SourceChanged;
      //webViewWin.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
    }

    public void SetClipboardText(string text)
    {
      System.Windows.Clipboard.SetText(text);
    }

    public string GetClipboardText()
    {
      if (System.Windows.Clipboard.ContainsText())
      {
        return System.Windows.Clipboard.GetText();
      }
      return string.Empty;
    }

    public void NavigateToUrl(string url)
    {
      if (webViewWin.CoreWebView2 != null)
      {
        webViewWin.CoreWebView2.Navigate(url);
      }
    }

    private void WebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
      Console.WriteLine("Navigation completed: " + e.IsSuccess);
      _navigation_completed = true;
    }

    public async System.Threading.Tasks.Task NavigateToUrlAsync(string url)
    {
      var tcs = new TaskCompletionSource<bool>();

      EventHandler<CoreWebView2NavigationCompletedEventArgs> handler = null;
      handler = (sender, e) =>
      {
        webViewWin.NavigationCompleted -= handler;
        tcs.SetResult(true);
      };

      webViewWin.NavigationCompleted += handler;
      try
      {

        webViewWin.CoreWebView2.Navigate(url);
      }
      catch (Exception ex)
      {
        Xceed.Wpf.Toolkit.MessageBox.Show("URL: < " + url+" > \n"+ex.Message+ "\nBitte in den Optionen korrigieren", "Fehler beim Öffnen der URL", MessageBoxButton.OK, MessageBoxImage.Error);


      }
      // webViewWin.CoreWebView2.Reload();

      await tcs.Task;
      //webViewWin.CoreWebView2.Reload();
    }

    public async void set_websource(string value)
    {
      _websource = value;
      await webViewWin.EnsureCoreWebView2Async(null);
      if (webViewWin.CoreWebView2 != null)
      {
        _navigation_completed = false;
        NavigateToUrlAsync(value);
        //webViewWin.CoreWebView2.Navigate(value);
      }
      //webViewWin.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
      if (webViewWin.CanGoBack)
      {
        webViewWin.GoBack();
      }
    }

    private void ForwardButton_Click(object sender, RoutedEventArgs e)
    {
      if (webViewWin.CanGoForward)
      {
        webViewWin.GoForward();
      }
      //string clipboard_str = GetClipboardText();

      //if (clipboard_str.Contains("zusi-sk.eu/#"))
      //{
      //  string timetablename = DataManager.Instance.SelectedTimeTableRelation.TimeTableName;
      //  if (DataManager.Instance.Fpn2zsklinkDictionary.ContainsKey(timetablename))
      //  {
      //    DataManager.Instance.Fpn2zsklinkDictionary[timetablename] = clipboard_str;
      //  }
      //  else
      //  {
      //    DataManager.Instance.Fpn2zsklinkDictionary.Add(timetablename, clipboard_str);
      //  }

      //  DataManager.Instance.Savefpn2zsk_link_json();
      //}
    }

    private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
      urlTextBox.Text = webViewWin.Source.ToString();
    }

    private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
    {
      // Check if the URL is the one you want to intercept
      if (e.Uri.Contains("?zugstart="))
      {
        //starte aktuellen Zug:
        object _sender = null;
        ExecutedRoutedEventArgs _e = null;
        e.Cancel = true;
        DataManager.Instance.main_window.OnStartTrain(_sender, _e);
        // Navigate to the new URL
        //webView2.CoreWebView2.Navigate("https://newurl.com");
      }
      if (e.Uri.Contains("fpndatei="))
      {
        string uri = e.Uri;
        
        string[] pathitems = uri.Split("=");
        if (pathitems != null && pathitems.Count() == 2)
        {
          e.Cancel = true;
          string fpnname = pathitems[1];
          fpnname = fpnname.Replace("?", "\\");
          TimeTable timeTable = DataManager.Instance.GetTimeTableOfFpnName(fpnname);

          if (timeTable != null)
          {
            TimeTableRelation value = new(0, timeTable);

            DataManager.Instance.OnSelectedTimeTableChanged(value);
          }
        }
      }
    }

    private void CoreWebView2SourceChanged(object? sender, CoreWebView2SourceChangedEventArgs e)
    {

      string newSource = webViewWin.Source.ToString();
      DataManager.Instance.webview_ZDB_source = newSource;
      urlTextBox.Text = newSource;
      //string[] source_parts = newSource.Split('?'); // ZUSIDatabase Start-Button
      //newSource = source_parts[0];

      if (newSource.EndsWith(".trn/"))
      {
        string train_number = "";
        string[] pathitems = newSource.Split("%5C");
        if (pathitems != null && pathitems.Count() == 5)
        {
          train_number = Zusi.DataPath[0] + "Timetables\\" + pathitems[1] + "\\" + pathitems[2] + "\\" + pathitems[3] + "\\" + pathitems[4];
          train_number = train_number.Replace(".trn/", "");
          train_number = train_number + ".trn";
          //bool train_found = ((DataManager)DataContext).SearchTrain(train_number);
          ZugDatei zd = new ZugDatei(null, train_number);
          if (zd.Root == null)
          {
            zd.Parse();

          }
          DataManager.Instance.CurrentTrain = zd.Root;

          //if (source_parts.Count() > 1)
          //{
          //  object dummy_sender = null;
          //  ExecutedRoutedEventArgs dummy_e = null;
          //  OnStartTrain(dummy_sender, dummy_e);
          //}
        }
      }
      else if (newSource.EndsWith(".st3"))
      {
        string train_number = "";
        string[] pathitems = newSource.Split("%5C");
        if (pathitems != null && pathitems.Count() == 5)
        {
          train_number = pathitems[4];

          bool train_found = DataManager.Instance.SearchTrain(train_number);

          if (train_found)
          {
            DataManager.Instance.CurrentTrain = null;
            DataManager.Instance.SelectedRecentTrain = null;
          }

          //if (source_parts.Count() > 1)
          //{
          //  object dummy_sender = null;
          //  ExecutedRoutedEventArgs dummy_e = null;
          //  OnStartTrain(dummy_sender, dummy_e);
          //}
        }
      }
      else if (newSource.Contains("?zugstart="))
      {
        //starte aktuellen Zug:
        object _sender = null;
        ExecutedRoutedEventArgs _e = null;
        DataManager.Instance.main_window.OnStartTrain(_sender, _e);
      }
    }

    private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
      string url_requested = e.Uri;
      string[] url_requested_parts;
      string betriebsstelle = "";
      string strecke = "";
      //if (url_requested == "https://www.zusidatenbank.de/streckenmodule/Routes%5CDeutschland%5C32U_0006_0054%5C000640_005369_Gablingen%5CGablingen_1990.st3")
      //{
      //  e.Handled = true; // Verhindert das Öffnen eines neuen Fensters
      //                    // handle link to "Strecke"
      //  url_requested = "https://www.zusidatenbank.de/streckenmodule/Routes%5CDeutschland%5C32U_0006_0054%5C000640_005369_Gablingen%5CGablingen_1990.st3?Betriebsstelle=Gablingen";
      //}
      if (url_requested.Contains("?Betriebsstelle="))
      {
        url_requested_parts = url_requested.Split("?");
        if (url_requested_parts.Count() == 2)
        {
          if (url_requested_parts[0].EndsWith(".st3"))
          {
            string searchString = "Betriebsstelle=";
            string url = url_requested_parts[1];

            int startIndex = url.IndexOf(searchString) + searchString.Length;
            betriebsstelle = url.Substring(startIndex);
            betriebsstelle = betriebsstelle.Replace("+", " ");
            //DataManager.Instance.main_window.tbxTrainNumber.Text = betriebsstelle;
            bool betriebsstelle_found = DataManager.Instance.SearchTrain(betriebsstelle);

            if (betriebsstelle_found)
            {
              e.Handled = true; // Verhindert das Öffnen eines neuen Fensters
              DataManager.Instance.CurrentTrain = null;
              DataManager.Instance.SelectedRecentTrain = null;
              //DataManager.Instance.main_window.tbxTrainNumber.Text = betriebsstelle;
              return;
            }
          }
        }
      }

      if (url_requested.Contains("?Strecke="))
      {
        url_requested_parts = url_requested.Split("?");
        if (url_requested_parts.Count() == 2)
        {
          if (url_requested_parts[0].EndsWith(".st3"))
          {
            string searchString = "Strecke=";
            string url = url_requested_parts[1];

            int startIndex = url.IndexOf(searchString) + searchString.Length;
            strecke = url.Substring(startIndex);
            strecke = strecke.Replace("+", " ");
            url_requested = url_requested_parts[0];
          }
        }
      }

      if (url_requested.EndsWith(".st3"))
      {
        e.Handled = true; // Verhindert das Öffnen eines neuen Fensters
        string train_number = "";
        string[] pathitems = url_requested.Split("%5C");
        if (pathitems != null && pathitems.Count() == 5)
        {
          train_number = pathitems[4];

          bool train_found = DataManager.Instance.SearchTrain(train_number);

          if (train_found)
          {
            e.Handled = true; // Verhindert das Öffnen eines neuen Fensters
            DataManager.Instance.CurrentTrain = null;
            DataManager.Instance.SelectedRecentTrain = null;
            //DataManager.Instance.main_window.tbxTrainNumber.Text = train_number;

            if (strecke != "")
            {
              DataManager.Instance.SearchResultTitle = strecke;
            }
          }


          //if (source_parts.Count() > 1)
          //{
          //  object dummy_sender = null;
          //  ExecutedRoutedEventArgs dummy_e = null;
          //  OnStartTrain(dummy_sender, dummy_e);
          //}
        }
      }
    }

    private void RestoreWindowState()
    {
      //this.WindowState = Properties.Settings.Default.WindowZDBState;

      var screen = Screen.AllScreens.ElementAtOrDefault(Properties.Settings.Default.WindowZDBScreen) ?? Screen.PrimaryScreen;

      if (Properties.Settings.Default.WindowZDBState == WindowState.Normal)
      {
        this.WindowState = WindowState.Normal;
        RestoreWindowPosition();
      }
      else if (Properties.Settings.Default.WindowZDBState == WindowState.Maximized)
      {
        // Set the window to the correct screen before maximizing
        this.WindowStartupLocation = WindowStartupLocation.Manual;
        this.Top = screen.WorkingArea.Top;
        this.Left = screen.WorkingArea.Left;
        this.Width = screen.WorkingArea.Width;
        this.Height = screen.WorkingArea.Height;
        this.WindowState = WindowState.Maximized;
      }
      else
      {
        this.WindowState = Properties.Settings.Default.WindowZDBState;
      }
    }

    private void RestoreWindowPosition()
    {
      var screen = Screen.AllScreens.ElementAtOrDefault(Properties.Settings.Default.WindowZDBScreen) ?? Screen.PrimaryScreen;

      //if (this.WindowState == WindowState.Normal)
      //{

      //if (Properties.Settings.Default.WindowZDBTop != 0)
      //{
      //  this.Top = Properties.Settings.Default.WindowTop;
      //}
      //if (Properties.Settings.Default.WindowZDBLeft != 0)
      //{
      //  this.Left = Properties.Settings.Default.WindowLeft;
      //}
      //if (Properties.Settings.Default.WindowZDBWidth != 0)
      //{
      //  this.Width = Properties.Settings.Default.WindowWidth;
      //}
      //if (Properties.Settings.Default.WindowZDBHeight != 0)
      //{
      //  this.Height = Properties.Settings.Default.WindowHeight;
      //}

      this.Top = Properties.Settings.Default.WindowTop;

      this.Left = Properties.Settings.Default.WindowLeft;

      this.Width = Properties.Settings.Default.WindowWidth;

      this.Height = Properties.Settings.Default.WindowHeight;


      // Ensure the window is within the bounds of the selected screen
      if (this.Left < screen.WorkingArea.Left || this.Left > screen.WorkingArea.Right - this.Width)
      {
        this.Left = screen.WorkingArea.Left;
      }
      if (this.Top < screen.WorkingArea.Top || this.Top > screen.WorkingArea.Bottom - this.Height)
      {
        this.Top = screen.WorkingArea.Top;
      }
      //}
      //else
      //{
      //  this.Left = screen.WorkingArea.Left;
      //  this.Top = screen.WorkingArea.Top;
      //}

    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      base.OnClosing(e);
      SaveWindowState();
    }

    private void SaveWindowState()
    {
      Properties.Settings.Default.WindowZDBState = this.WindowState;

      // Save the screen index
      var screen = Screen.FromPoint(new System.Drawing.Point((int)this.Left, (int)this.Top));
      Properties.Settings.Default.WindowZDBScreen = Array.IndexOf(Screen.AllScreens, screen);

      if (this.WindowState == WindowState.Normal)
      {
        Properties.Settings.Default.WindowZDBTop = this.Top;
        Properties.Settings.Default.WindowZDBLeft = this.Left;
        Properties.Settings.Default.WindowZDBWidth = this.Width;
        Properties.Settings.Default.WindowZDBHeight = this.Height;
      }

      Properties.Settings.Default.Save();
    }
  }
}
