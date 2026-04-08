using Sovoma;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ZusiCLIProject.FileLibrary.Zusi3;
using ZusiCLIProject.Routegraph2;
using ZusiKlassenLib2.Buchfahrplan;
using ZusiKlassenLib2.Common;
using ZusiStart.Data;
using ZusiStart.ViewModels;

namespace ZusiCLIProject.Routegraph2
{
  public class TrackingItem : Canvas, IIgnoreTransformation
  {

    public TrackingItem(string text, int radius, Strecke.UTM umtRefPunkt, DpiScale predetectDpi)
    {
      m_utmRefPunkt = umtRefPunkt;

      // Erzeuge das Path-Element für Fadenkreuz + Kreis
      Path crosshair = new Path
      {
        Stroke = Brushes.Red,
        StrokeThickness = 3,
        Opacity = 0.75,
        Data = new GeometryGroup
        {
          Children = new GeometryCollection
        {
            // Horizontale Linie
            new LineGeometry(new Point(-20, 0), new Point(20, 0)),
            // Vertikale Linie
            new LineGeometry(new Point(0, -20), new Point(0, 20)),
            // Kreis um das Zentrum (Radius = 12)
            new EllipseGeometry(new Point(0, 0), 12, 12)
        }
        }
      };

      this.Children.Add(crosshair);

      var tgr = new TransformGroup();
      tgr.Children.Add(new ScaleTransform(1, -1));
      tgr.Children.Add(new MatrixTransform(Matrix.Identity));
      tgr.Children.Add(new MatrixTransform(Matrix.Identity));
      tgr.Children.Add(new MatrixTransform(Matrix.Identity));
      this.RenderTransform = tgr;

    }


    private Strecke.UTM m_utmRefPunkt = new Strecke.UTM();
    public string Text { get; private set; }
    public double WidthCalculated { get; private set; }
    private TextBlock m_textBlock = new();

    //private DreieckItem m_dreieck = new(0,"Zug", Colors.Chocolate,true);

    public System.Windows.Media.Color Farbe
    {
      get { return m_farbe; }
      set
      {
        m_farbe = value;
        m_textBlock.Foreground = new System.Windows.Media.SolidColorBrush(value);
      }
    }
    private System.Windows.Media.Color m_farbe;

    private System.Windows.Point m_pos;
    public System.Windows.Point Pos
    {
      get { return m_pos; }
      set
      {
        m_pos = value;
        ((TransformGroup)this.RenderTransform).Children[2] = new TranslateTransform(value.X, value.Y);
        this.RenderTransform = this.RenderTransform;
      }
    }

    public void MoveBy(double x, double y)
    {
      Pos = new Point(Pos.X + x, Pos.Y + y);
    }

    public void UpdateMarkerPosition(double utmX, double utmY, double phi)
    {
      double d_utmX = utmX - m_utmRefPunkt.WE;
      double d_utmY = utmY - m_utmRefPunkt.NS;
      Pos = new Point(d_utmX * 1000, d_utmY * 1000);

      try
      {
        TabViewModel? zdbTab = DataManager.Instance.Tabs.FirstOrDefault(t => t.Title == DataManager.Instance.tab_title_routegraph);
        if (zdbTab.RouteGraphContent != null) // **TEST * *
        {
          RouteGraph2Control routeGraph2Control = zdbTab.RouteGraphContent as RouteGraph2Control;
          routeGraph2Control.ZentriereView(Pos.X, Pos.Y);
        }
      }
      catch
      {
      }
    }

    public void AssignInverseTransform(System.Windows.Media.Transform transform)
    {
      //((TransformGroup)this.RenderTransform).Children[1] = transform;
      this.LayoutTransform = transform;
    }
    public void AssignLocalTransform(System.Windows.Media.Transform transform)
    {
      ((TransformGroup)this.RenderTransform).Children[3] = transform;
    }
    public float MinLod { get; set; } = 0;
    bool m_opaque = true;
    public void SetLod(double value)
    {
      bool opaque = (value > MinLod);
      if (opaque == m_opaque)
        return;
      m_opaque = opaque;
      this.Visibility = m_opaque ? Visibility.Visible : Visibility.Collapsed;
    }
  }
}
