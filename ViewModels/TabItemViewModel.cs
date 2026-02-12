using GMap.NET;
using GMap.NET.WindowsPresentation;
using Microsoft.VisualBasic.Logging;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZusiCLIProject.Routegraph2;
using ZusiKlassenLib;
using ZusiKlassenLib.Buchfahrplan;
using ZusiKlassenLib.Cab;
using ZusiStart.Data;
using ZusiStart.Miscellaneous;
using ZusiStart.ViewModels;


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
    public GMapControl gmap { get; set; }

    // Cache the control instance here
    //public object RouteGraphContent { get; }
    public RouteGraph2Control RouteGraphContent { get; }

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

    public TabViewModel(string title, string url = null, bool isPdf = false, bool isIntro = false, bool isImageTab = false, bool isroutegraph = false, bool isgmap = false, string? tooltip = null)
    {
      Title = title;
      Tooltip = tooltip;
      Url = url;
      IsPdf = isPdf;
      IsIntro = isIntro;
      IsInitialised = false;
      IsImageTab = isImageTab;
      WebViewInstance = new WebView2();
      PDFViewerUrl = null;
      if (isgmap == true)
      {
        gmap = new GMapControl();
      }
      if (isroutegraph == true)
      {
        RouteGraphContent = new RouteGraph2Control();
      }
      
      // Assuming vm is your TrackingViewModel and RouteVisual already set
      //gmap.OnMapZoomChanged += () => UpdateRouteTransform(gmap, utmBounds, canvasBounds, zone: 32, northHemisphere: true);
      //gmap.OnMapDrag += () => UpdateRouteTransform(gmap, utmBounds, canvasBounds, zone: 32, northHemisphere: true);
    }

    private Matrix _routeTransformMatrix = Matrix.Identity;
    public Matrix RouteTransformMatrix
    {
      get => _routeTransformMatrix;
      set { _routeTransformMatrix = value; OnPropertyChanged(nameof(RouteTransformMatrix)); }
    }

    public void UpdateRouteTransform(GMap.NET.WindowsPresentation.GMapControl map,
                                     UtmBounds utm, int zone, bool northHemisphere = true)
    {
      // Compute matrix from UTM bbox and canvas bbox
      RouteGraphContent.set_canvas_min_max();
      var (matrix, _) = TransformHelper.ComputeBoundingBoxMatrix(map, utm, DataManager.Instance.canvasBounds, zone, northHemisphere);
      
      RouteTransformMatrix = matrix;
      RouteGraphContent.SetzeTransform(RouteTransformMatrix);
    }

    // INotifyPropertyChanged implementation...

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
          WebViewInstance.CoreWebView2.NavigationStarting += (s, e) =>
          {
            if (Title == DataManager.Instance.tab_title_Zusi_DB)
            {
              DataManager.Instance.main_window.CoreWebView2_NavigationStarting(s, e);
            }
          };
        }
        IsInitialised = true;
      }
    }

    public void SetUrl(string Url, string Urlparameter = null, bool copyfile = false)
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
          viewerUrl = $"https://pdfjs/viewer.html?file=https://zusi-pdfs/{Path.GetFileName(Url)}" + "?" + Urlparameter;
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

  public static class TransformHelper
  {
    public static (Matrix matrix, (Point SW, Point SE, Point NW, Point NE) mapCorners)
        ComputeBoundingBoxMatrix(GMapControl map, UtmBounds utm, CanvasBounds canvas, int zone, bool northHemisphere)
    {
      // UTM -> WGS84
      //var (latSW, lonSW) = UtmConverter.ToLatLon(utm.MinE, utm.MinN, zone, northHemisphere);
      //var (latSE, lonSE) = UtmConverter.ToLatLon(utm.MaxE, utm.MinN, zone, northHemisphere);
      //var (latNW, lonNW) = UtmConverter.ToLatLon(utm.MinE, utm.MaxN, zone, northHemisphere);
      //var (latNE, lonNE) = UtmConverter.ToLatLon(utm.MaxE, utm.MaxN, zone, northHemisphere);

      double latSW;
      double lonSW;
      double latSE;
      double lonSE;
      double latNW;
      double lonNW;
      double latNE;
      double lonNE;
      int southhemi = 0;

      ZusiStart.Miscellaneous.UTM.UtmToLatLon(utm.MinE, utm.MinN, zone, southhemi, out latSW, out lonSW);
      ZusiStart.Miscellaneous.UTM.UtmToLatLon(utm.MaxE, utm.MinN, zone, southhemi, out latSE, out lonSE);
      ZusiStart.Miscellaneous.UTM.UtmToLatLon(utm.MinE, utm.MaxN, zone, southhemi, out latNW, out lonNW);
      ZusiStart.Miscellaneous.UTM.UtmToLatLon(utm.MaxE, utm.MaxN, zone, southhemi, out latNE, out lonNE);

      // WGS84 -> Bildschirm (GPoint -> Point)
      var gpSW = map.FromLatLngToLocal(new PointLatLng(latSW, lonSW));
      var gpSE = map.FromLatLngToLocal(new PointLatLng(latSE, lonSE));
      var gpNW = map.FromLatLngToLocal(new PointLatLng(latNW, lonNW));
      var gpNE = map.FromLatLngToLocal(new PointLatLng(latNE, lonNE));

      var mapSW = new Point((double)gpSW.X, (double)gpSW.Y);
      var mapSE = new Point((double)gpSE.X, (double)gpSE.Y);
      var mapNW = new Point((double)gpNW.X, (double)gpNW.Y);
      var mapNE = new Point((double)gpNE.X, (double)gpNE.Y);

      // Canvas-Ecken (abhängig von deiner Zeichenlogik)
      var canvasSW = new Point(canvas.MinX, canvas.MaxY);
      var canvasSE = new Point(canvas.MaxX, canvas.MaxY);
      var canvasNW = new Point(canvas.MinX, canvas.MinY);
      var canvasNE = new Point(canvas.MaxX, canvas.MinY);

      // Nicht-uniforme Skalierung (affin)
      double scaleX = (mapSE.X - mapSW.X) / (canvasSE.X - canvasSW.X);
      double scaleY = (mapNW.Y - mapSW.Y) / (canvasNW.Y - canvasSW.Y);

      var m = Matrix.Identity;
      m.Scale(scaleX, scaleY);

      // Translation: SW-Ecke ausrichten
      m.Translate(
          mapSW.X - canvasSW.X * scaleX,
          mapSW.Y - canvasSW.Y * scaleY
      );

      return (m, (mapSW, mapSE, mapNW, mapNE));
    }
  }

}
