using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
namespace ZusiStart.Miscellaneous
{
//  public class RailwayPolylineOverlayWpf
//  {
//    private readonly GMapControl _map;
//    private readonly List<(GMapMarker marker, List<PointLatLng> geoPoints)> _polylines = new();

//    public RailwayPolylineOverlayWpf(GMapControl map)
//    {
//      _map = map;
//      _map.OnMapZoomChanged += RedrawPolylines;
//      _map.OnMapDrag += RedrawPolylines;

//    }

//    public async Task LoadRailwayOverlayAsync(double minLat, double minLon, double maxLat, double maxLon)
//    {
//      string query = $@"
//[out:json][timeout:250];
//way[""railway""=""rail""]({minLat.ToString("F6", CultureInfo.InvariantCulture)},{minLon.ToString("F6", CultureInfo.InvariantCulture)},{maxLat.ToString("F6", CultureInfo.InvariantCulture)},{maxLon.ToString("F6", CultureInfo.InvariantCulture)});
//out body;
//>;
//out skel qt;";


//      string url = "https://overpass-api.de/api/interpreter?data=" + Uri.EscapeDataString(query);
//      try
//      {
//        using var client = new HttpClient();
//        string response = await client.GetStringAsync(url);
//        JObject json = JObject.Parse(response);


//        var nodes = json["elements"]
//            .Where(e => e["type"]?.ToString() == "node")
//            .ToDictionary(e => (long)e["id"], e => new PointLatLng((double)e["lat"], (double)e["lon"]));

//        var ways = json["elements"]
//            .Where(e => e["type"]?.ToString() == "way");

//        foreach (var way in ways)
//        {
//          var geoPoints = new List<PointLatLng>();
//          foreach (var nodeId in way["nodes"] ?? Enumerable.Empty<JToken>())
//          {
//            if (nodes.TryGetValue((long)nodeId, out var point))
//              geoPoints.Add(point);
//          }

//          if (geoPoints.Count > 1)
//          {
//            var marker = new GMapMarker(geoPoints[0])
//            {
//              Shape = new Canvas()
//            };
//            _map.Markers.Add(marker);
//            _polylines.Add((marker, geoPoints));
//          }
//        }

//        RedrawPolylines();
//      }
//      catch (Exception ex)
//      {
//        string message = "Error fetching railway data: " + ex.Message;
//        return;
//      }
//    }

//    private void RedrawPolylines()
//    {
//      foreach (var (marker, geoPoints) in _polylines)
//      {
//        var canvas = marker.Shape as Canvas;
//        canvas.Children.Clear();

//        var polyline = new System.Windows.Shapes.Polyline
//        {
//          Stroke = Brushes.Red,
//          StrokeThickness = 2,
//          Points = new PointCollection()
//        };

//        foreach (var geo in geoPoints)
//        {
//          var local = _map.FromLatLngToLocal(geo);
//          polyline.Points.Add(new System.Windows.Point(local.X, local.Y));
//        }

//        canvas.Children.Add(polyline);
//      }
//    }
//  }
}
