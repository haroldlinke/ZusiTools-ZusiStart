//using ZusiMeter.Data;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using log4net;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using ZusiStart.Data;
using ZusiStart.Miscellaneous;

namespace ZusiStart.Controls
{
  /// <summary>
  /// Interaktionslogik für MainWindow.xaml
  /// </summary>
  public partial class OSMGmapControl : System.Windows.Controls.UserControl
  {
    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public static readonly DependencyProperty gmapProperty =
      DependencyProperty.Register(nameof(gmap), typeof(GMapControl), typeof(OSMGmapControl), new PropertyMetadata(null));

    private bool mapinitialized = false;

    public GMapControl gmap
    {
      get => (GMapControl)GetValue(gmapProperty);
      set => SetValue(gmapProperty, value);
    }

    private GMapMarker marker;

    public OSMGmapControl()
    {
      InitializeComponent();
      mapinitialized = false;
      //DefaultTitel = this.Title;
      DataManager.Instance.osmGmapControl = this;
      gmap = new GMapControl();
      Loaded += OSMGmapControl_Loaded;
    }

    private void InitializeMap()
    {
      try
      {
        mapinitialized = true;
        GMapProvider.UserAgent = "ZusiStart (contact: support@zusi-tools.hlinke.de)";
        gmap.MapProvider = GMap.NET.MapProviders.GMapProviders.OpenStreetMap;
        gmap.Position = new PointLatLng(50.0, 8.0); // Startposition
        if (gmap.Zoom < 4)
        {
          gmap.Zoom = 12;
        }
        gmap.MinZoom = 5;
        gmap.MaxZoom = 18;
        gmap.ShowCenter = false;
        gmap.MouseWheelZoomEnabled = true;
        gmap.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
        gmap.IgnoreMarkerOnMouseWheel = true;

        // Marker direkt zur Markers-Sammlung hinzufügen
        //marker = new GMapMarker(new PointLatLng(50.0, 8.0))
        //{
        //  Shape = new Ellipse
        //  {
        //    Width = 24,
        //    Height = 24,
        //    Stroke = System.Windows.Media.Brushes.Red,
        //    StrokeThickness = 10
        //  }
        //};
        if (marker != null)
        {
          gmap.Markers.Remove(marker);
        }
        marker = new GMapMarker(new PointLatLng(50.0, 8.0))
        {
          Shape = new System.Windows.Shapes.Path
          {
            Stroke = System.Windows.Media.Brushes.Red,
            StrokeThickness = 3,
            Opacity = 0.75,
            Data = new GeometryGroup
            {
              Children = new GeometryCollection
        {
            // Horizontale Linie
            new LineGeometry(new System.Windows.Point(-20, 0), new System.Windows.Point(20, 0)),
            // Vertikale Linie
            new LineGeometry(new System.Windows.Point(0, -20), new System.Windows.Point(0, 20)),
            // Kreis um das Zentrum (Radius = 12)
            new EllipseGeometry(new System.Windows.Point(0, 0), 12, 12)
        }
            }
          }
        };

        gmap.Markers.Add(marker);

        // Assuming vm is your TrackingViewModel and RouteVisual already set **Test** 
        //zdbTab.gmap.OnMapZoomChanged += () => zdbTab.UpdateRouteTransform(zdbTab.gmap, DataManager.Instance.utmBounds, zone: 32, northHemisphere: true);
        //zdbTab.gmap.OnMapDrag += () => zdbTab.UpdateRouteTransform(zdbTab.gmap, DataManager.Instance.utmBounds, zone: 32, northHemisphere: true);
      }
      catch (Exception ex)
      {
        Log.Error("Fehler beim Initialisieren der Karte: " + ex.Message);
      }
    }

    private void OSMGmapControl_Loaded(object sender, RoutedEventArgs e)
    {

      InitializeMap();

    }

    public void UpdateMarkerPosition(double latitude, double longitude)
    {
      if (marker == null)
      {
        InitializeMap();
      }
      Log.Debug("UpdateMarkerPosition - Pos" + latitude.ToString() + "," + longitude.ToString());

      marker.Position = new PointLatLng(latitude, longitude);
      
      gmap.Position = marker.Position; // Karte auf neue Position zentrieren

    }
  }

}
