using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using ZusiKlassenLib;
using ZusiKlassenLib.Buchfahrplan;
using ZusiStart.Data;
using ZusiStart.Miscellaneous;

namespace ZusiStart.ViewModels
{

  public class TabViewModel : INotifyPropertyChanged
  {
    public string Title { get; set; }
    public string? Tooltip { get; set; }
    public string Url { get; set; } // Für Webseiten oder Pfad zur PDF
    public bool IsPdf { get; set; } // Kennzeichnet PDF-Tabs
    public bool IsInitialised { get; set; } //  initalisation status
    public bool IsIntro { get; set; }
    public bool IsImageTab { get; set; }
    public WebView2 WebViewInstance { get; set; }
    public string PDFViewerUrl { get; set; } // Für kompletten Pfad zur PDF
    private ImageSource _imageSource;
    public ImageSource ImageSource
    {
      get => _imageSource;
      set
      {
        _imageSource = value;
        OnPropertyChanged(nameof(ImageSource));
      }
    }
    public ObservableCollection<string> LaNumberList => DataManager.Instance.LaNumberList;
    public RecentTrainsCollection RecentTrains => DataManager.Instance.RecentTrains;
    public RecentTrain SelectedRecentTrain
    {
      get => DataManager.Instance.SelectedRecentTrain;
      set => DataManager.Instance.SelectedRecentTrain = value;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public TabViewModel(string title, string url = null, bool isPdf = false, bool isIntro = false, bool isImageTab = false, string? tooltip=null)
    {
      Title = title;
      Tooltip = tooltip;
      Url = url;
      IsPdf = isPdf;
      IsIntro = isIntro;
      IsInitialised = false;
      IsImageTab = isImageTab;
      WebViewInstance = new WebView2();
      IsIntro = isIntro;
      PDFViewerUrl = null;
    }

    public async Task InitializeWebViewAsync(CoreWebView2Environment environment)
    {
      if (!IsInitialised)
      {
        await WebViewInstance.EnsureCoreWebView2Async(environment);

        if (IsPdf)
        {
          string pdfjsPath = Path.Combine(AppContext.BaseDirectory, "Assets/pdfjs/web");
          string pdfsPath = Path.Combine(Path.GetTempPath(), "ZusiPDFs");
          Directory.CreateDirectory(pdfsPath);
          string[] urllist = Url.Split(",");
          foreach (string urlitem in urllist)
            {
            DataPathType dpt = DataPathType.Unknown;
            string copyfile = Zusi.GetAbsolutePathOf(urlitem, ref dpt);
            string targetFile = Path.Combine(pdfsPath, Path.GetFileName(urlitem));
            File.Copy(copyfile, targetFile, true);
          }
          string mainurl = urllist[0];

          WebViewInstance.CoreWebView2.SetVirtualHostNameToFolderMapping(
              "pdfjs", pdfjsPath, CoreWebView2HostResourceAccessKind.Allow);

          WebViewInstance.CoreWebView2.SetVirtualHostNameToFolderMapping(
              "zusi-pdfs", pdfsPath, CoreWebView2HostResourceAccessKind.Allow);

          PDFViewerUrl = $"https://pdfjs/viewer.html?file=https://zusi-pdfs/{Path.GetFileName(mainurl)}";
          WebViewInstance.Source = new Uri(PDFViewerUrl);
        }
        else
        {
          WebViewInstance.Source = new Uri(Url);

          WebViewInstance.CoreWebView2.SourceChanged += (s, e) =>
          {
            if (Title == DataManager.Instance.tab_title_Streckenkarte)
            {
              DataManager.Instance.main_window.CoreWebView2SourceChanged_ZSK(s, e);
            }
            else if (Title == DataManager.Instance.tab_title_Zusi_DB)
            {
              DataManager.Instance.main_window.CoreWebView2SourceChanged_ZDB(s, e);
            }
          };
        }
        IsInitialised = true;
      }
    }

    public void SetUrl(string Url, string Urlparameter=null, bool copyfile=false)
    {
      if (IsPdf)
      {
        string pdfsPath = Path.Combine(Path.GetTempPath(), "ZusiPDFs");
        if (copyfile)
        {
          Directory.CreateDirectory(pdfsPath);

          string targetFile = Path.Combine(pdfsPath, Path.GetFileName(Url));
          File.Copy(Url, targetFile, true);

          WebViewInstance.CoreWebView2.SetVirtualHostNameToFolderMapping(
              "zusi-pdfs", pdfsPath, CoreWebView2HostResourceAccessKind.Allow);
        }
        string viewerUrl = "";
        if (Urlparameter != null)
        {
           viewerUrl = $"https://pdfjs/viewer.html?file=https://zusi-pdfs/{Path.GetFileName(Url)}"+"?"+Urlparameter;
        }
        else
        {

           viewerUrl = $"https://pdfjs/viewer.html?file=https://zusi-pdfs/{Path.GetFileName(Url)}";
        }
        WebViewInstance.Source = new Uri(viewerUrl);
      }
      else
      {
        WebViewInstance.Source = new Uri(Url);
      }
    }
  }
  

}
