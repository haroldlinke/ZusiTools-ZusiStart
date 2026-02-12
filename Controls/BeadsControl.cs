using log4net;
using Sovoma;
using Sovoma.WPF;
using Sovoma.WPF.MathEx;
//using SovomaLib.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZusiKlassenLib.Buchfahrplan;
using ZusiKlassenLib.Fahrplan;
using ZusiStart.Data;

namespace ZusiStart.Controls
{
  //=========================================================================
  static class AnExt
  {
    //---------------------------------------------------------------------
    public static IEnumerable<T> RemoveDuplicates<T>(this IEnumerable<T> self) where T : BeadsLabel
    {
      List<T> result = new();

      if (self.Count() > 0)
      {
        result.Add(self.ElementAt(0));
      }

      T r = result.Last();
      for (int i = 1; i < self.Count(); i++)
      {
        T x = self.ElementAt(i);
        if (r.Equals(x))
        {
          r.AdjustStop(x);
        }
        else
        {
          result.Add(x);
          r = x;
        }
      }

      return result;
    }
  }

  //=========================================================================
  public class BeadsLabel
  {
    private int _hash = -1;

    public bool IsStop { get; private set; }
    public string Label { get; private set; }

    //---------------------------------------------------------------------
    public BeadsLabel(string label)
    {
      Label = label;
    }

    //---------------------------------------------------------------------
    public BeadsLabel(string label, bool isStop)
    {
      IsStop = isStop;
      Label = label;
    }

    //---------------------------------------------------------------------
    internal void AdjustStop(BeadsLabel src)
    {
      IsStop |= src.IsStop;
    }

    //---------------------------------------------------------------------
    public override bool Equals(object obj)
    {
      return obj is BeadsLabel label ? GetHashCode() == label.GetHashCode() : base.Equals(obj);
    }

    //---------------------------------------------------------------------
    public override int GetHashCode()
    {
      if (_hash == -1)
      {
        unchecked // Overflow is fine, just wrap
        {
          int hash = (int)2166136261;
          if (Label != null)
          {
            hash = (hash * 16777619) ^ Label.GetHashCode();
          }
          _hash = hash;
        }
      }

      return _hash;
    }
  }

  //=========================================================================
  /// <summary>
  /// </summary>
  public class BeadsControl : Control
  {
    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly ObservableCollection<BeadsLabel> _labels = new()
    {
      //new BeadsLabel ("Alexanderplatz", true),
      //new BeadsLabel ("Jannowitzbrücke"),
      //new BeadsLabel ("Ostbahnhof"),
      //new BeadsLabel ("Warschauer Straße"),
      //new BeadsLabel ("Ostkreuz", true),
      //new BeadsLabel ("Nöldnerplatz"),
      //new BeadsLabel ("Lichtenberg", true)
    };

    private bool _alternateView;
    private Point _startPoint;
    private readonly double _distance = 30;

    //---------------------------------------------------------------------
    public static readonly DependencyProperty DotBrushProperty = DependencyProperty.Register(
        "DotBrush",
        typeof(Brush),
        typeof(BeadsControl),
        new PropertyMetadata(null));
    public Brush DotBrush
    {
      get { return (Brush)GetValue(DotBrushProperty); }
      set { SetValue(DotBrushProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty DotBrushStopProperty = DependencyProperty.Register(
        "DotBrushStop",
        typeof(Brush),
        typeof(BeadsControl),
        new PropertyMetadata(null));
    public Brush DotBrushStop
    {
      get { return (Brush)GetValue(DotBrushStopProperty); }
      set { SetValue(DotBrushStopProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty DotSizeProperty = DependencyProperty.Register(
        "DotSize",
        typeof(double),
        typeof(BeadsControl),
        new PropertyMetadata(10.0));
    public double DotSize
    {
      get { return (double)GetValue(DotSizeProperty); }
      set { SetValue(DotSizeProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty DotSizeStopProperty = DependencyProperty.Register(
        "DotSizeStop",
        typeof(double),
        typeof(BeadsControl),
        new PropertyMetadata(12.0));
    public double DotSizeStop
    {
      get { return (double)GetValue(DotSizeStopProperty); }
      set { SetValue(DotSizeStopProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty LabelsProperty = DependencyProperty.Register(
        "Labels",
        typeof(ObservableCollection<BeadsLabel>),
        typeof(BeadsControl),
        new FrameworkPropertyMetadata(_labels, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
    [Category("Allgemein")]
    public ObservableCollection<BeadsLabel> Labels
    {
      get { return (ObservableCollection<BeadsLabel>)GetValue(LabelsProperty); }
      set { SetValue(LabelsProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty StrokeWidthProperty = DependencyProperty.Register(
        "StrokeWidth",
        typeof(double),
        typeof(BeadsControl),
        new PropertyMetadata(3.0));
    public double StrokeWidth
    {
      get { return (double)GetValue(StrokeWidthProperty); }
      set { SetValue(StrokeWidthProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        "Source",
        typeof(Zug),
        typeof(BeadsControl),
        new PropertyMetadata(null, OnSourceChanged));
    [Category("Allgemein")]
    public Zug Source
    {
      get { return (Zug)GetValue(SourceProperty); }
      set { SetValue(SourceProperty, value); }
    }

    //---------------------------------------------------------------------
    static BeadsControl()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(BeadsControl), new FrameworkPropertyMetadata(typeof(BeadsControl)));
    }

    //---------------------------------------------------------------------
    protected override Size MeasureOverride(Size constraint)
    {
      Size z = CalcLayout(/*constraint.Width - Padding.Left - Padding.Right*/);

      double cx = Math.Min(constraint.Width, z.Width + Padding.Left + Padding.Right + 4);
      double cy = Math.Min(constraint.Height, z.Height + Padding.Top + Padding.Bottom);

      return new Size(cx, cy);
    }

    //---------------------------------------------------------------------
    protected override void OnRender(DrawingContext dc)
    {
      if (_alternateView)
      {
        RenderAlternateView(dc);
      }
      else
      {
        RenderDefaultView(dc);
      }
    }

    //---------------------------------------------------------------------
    private Size CalcLayout(/*double maxWidth*/)
    {
      Typeface tf = new(FontFamily, FontStyle, FontWeights.Bold, FontStretch);

      if (_alternateView)
      {
        double h = 0;
        double w = 0;
        foreach (BeadsLabel blbl in Labels)
        {
          FormattedText ft = new(blbl.Label, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, tf, FontSize, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
          h += Math.Ceiling(ft.Height);
          double tw = Math.Ceiling(ft.Width);
          if (tw > w)
          {
            w = tw;
          }
        }

        _startPoint = new Point(0, 0);

        return new Size(w + Padding.Left + Padding.Right, h + Padding.Top + Padding.Bottom);
      }
      else
      {
        //double highX;// = 0;
        double overhang = 0;
        double rad = Math2D.Radians(-45);
        double right = 0;
        Point ps = new(5, 5);
        for (int i = 0; i < Labels.Count; i++)
        {
          FormattedText ft = new(Labels[i].Label, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, tf, FontSize, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
          double th = Math.Ceiling(ft.Height);
          double tw = Math.Ceiling(ft.Width);

          Point p = new(tw, th * -0.5);
          Point pr = p.Rotate(rad);
          pr.Offset(ps.X, ps.Y);
          if (pr.Y < overhang)
          {
            overhang = pr.Y;
          }

          p.Y = th * 0.5;
          pr = p.Rotate(rad);
          pr.Offset(ps.X, ps.Y);
          if (pr.X > right)
          {
            right = pr.X;
          }

          //highX = ps.X;
          ps.X += 30;
        }

        overhang = Math.Abs(overhang);

        _startPoint = new Point(5, overhang + 5);

        return new Size(Math.Ceiling(right), Math.Ceiling(overhang + DotSize * 0.5 + 5));
      }
    }

    //---------------------------------------------------------------------
    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      BeadsControl b = d as BeadsControl;
      b?.OnSourceChanged(e.NewValue as Zug);
    }

    //---------------------------------------------------------------------
    private void OnSourceChanged(Zug zug)
    {
      try
      {
        ObservableCollection<BeadsLabel> labels = new();

        if (zug != null)
        {
          if (zug.Buchfahrplan != null)
          {
            labels = zug.Buchfahrplan.FplZeilen.Where((z, b) =>
            {
              string s = z.Name?.Text;
              if (!string.IsNullOrEmpty(s))
              {
                s = s.Trim();
              }
              return IsValidName(s);
            })
            .Select((z, r) => new BeadsLabel(z.Name.Text, IsStop(z)))
            .RemoveDuplicates()
            .ToObservableCollection();
          }
          else
          {
            labels = zug.FahrplanEintraege.Where((fe, b) =>
            {
              string s = fe.Bestrst;
              if (!string.IsNullOrEmpty(s))
              {
                s = s.Trim();
              }
              return IsValidName(s);
            })
            .Select((fe, r) => new BeadsLabel(fe.Bestrst, IsStop(fe)))
            .RemoveDuplicates()
            .ToObservableCollection();
          }
        }

        _alternateView = labels.Count > 0 && labels[0].Label.ToLower().StartsWith("keine fahrplandar");

        // bestimme Betriebsstellen:
        var ziel = DataManager.BetriebsstellenManager.Instance.Betriebsstellen;
        ziel.Clear();
        string s;
        if (zug.Buchfahrplan != null)
        {
          bool first_entry = true;
          s = "dummy";
          foreach (var fe in zug.Buchfahrplan.FplZeilen)
          {
            s = fe.Name?.Text;
            if (!string.IsNullOrEmpty(s))
            {
              s = s.Trim();
            }
            else
            {
              continue;
            }

            if (!IsValidName(s) && !first_entry)
              continue;
            first_entry = false;
            ziel.Add(s); // oder s.ToUpper(), je nach Vergleichslogik
          }
          ziel.Add(s); // the last entry
        }
        else
        {
          bool first_entry = true;
          s="dummy";
          foreach (var fe in zug.FahrplanEintraege)
          {
            s = fe.Bestrst;
            if (!string.IsNullOrEmpty(s))
            {
              s = s.Trim();
            }

            if (!IsValidName(s) && !first_entry)
              continue;
            first_entry = false;
            ziel.Add(s); // oder s.ToUpper(), je nach Vergleichslogik
          }
          ziel.Add(s); // the last entry
        }


        Labels = labels;
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
        Labels = _labels;
      }
    }

    //---------------------------------------------------------------------
    private static bool IsStop(FahrplanEintrag fe)
    {
      return fe != null && fe.Arrival != null && fe.Departure != null;
    }

    //---------------------------------------------------------------------
    private static bool IsStop(FplZeile z)
    {
      return z != null && z.Abfahrt != null && z.Abfahrt.Time != null && z.Ankunft != null && z.Ankunft.Time != null;
    }

    //#pragma warning disable CA1307
    //---------------------------------------------------------------------
    private static bool IsValidName(string name)
    {
      if (!string.IsNullOrEmpty(name))
      {
        string s = name.ToLower();
        if (s.StartsWith("- zbf") ||
            s.StartsWith("- zf") ||
            s.StartsWith("- kein ") ||
            s.StartsWith("sbk") ||
            s.StartsWith("va") ||
            s.StartsWith("ve") ||
            s.StartsWith("abzw") ||
            s.StartsWith("esig") ||
            s.StartsWith("asig") ||
            s.StartsWith("avsig") ||
            s.StartsWith("bksig") ||
            s.StartsWith("zsig") ||
            s.StartsWith("zvsig") ||
            s.StartsWith("bü") ||
            s.StartsWith("üs") ||
            s.StartsWith("lzb") ||
            s.StartsWith("- eingl") ||
            s.StartsWith("betriebs") ||
            s.StartsWith("strende") ||
            s.StartsWith("streckenende") ||
            s.StartsWith("ende") ||
            s.StartsWith("ri.") ||
            s.StartsWith("von ") ||
            s.StartsWith("nach ") ||
            s.StartsWith("aufgl"))
        {
          return false;
        }

        return true;
      }

      return false;
    }
    //#pragma warning restore CA1307

    //---------------------------------------------------------------------
    private void RenderAlternateView(DrawingContext dc)
    {
      Typeface tfBold = new(FontFamily, FontStyle, FontWeights.Bold, FontStretch);
      Typeface tfNormal = new(FontFamily, FontStyle, FontWeight, FontStretch);
      Brush brush = DotBrush ?? Foreground;
      double midX = ActualWidth * 0.5;

      Point pt = new(0, _startPoint.Y + Padding.Top);
      bool first = true;
      foreach (BeadsLabel blbl in Labels)
      {
        FormattedText ft = new(blbl.Label, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, first ? tfBold : tfNormal, FontSize, brush, VisualTreeHelper.GetDpi(this).PixelsPerDip);
        pt.X = Math.Floor(midX - ft.Width * 0.5);
        dc.DrawText(ft, pt);
        pt.Y += Math.Ceiling(ft.Height);
        first = false;
      }
    }

    //---------------------------------------------------------------------
    private void RenderDefaultView(DrawingContext dc)
    {
      Brush dotBrush = DotBrush ?? Foreground;
      Brush dotBrushStop = DotBrushStop ?? Foreground;

      Typeface tfBold = new(FontFamily, FontStyle, FontWeights.Bold, FontStretch);
      Typeface tfNormal = new(FontFamily, FontStyle, FontWeight, FontStretch);
      Pen pen = new(dotBrush, StrokeWidth);
      Point pt = new(_startPoint.X + Padding.Left, _startPoint.Y + Padding.Top);

      if (Labels.Count > 1)
      {
        Point pt2 = new(pt.X + (Labels.Count - 1) * _distance, pt.Y);
        dc.DrawLine(pen, pt, pt2);
      }

      foreach (BeadsLabel blbl in Labels)
      {
        Typeface tf;
        double r;
        Brush br;
        Brush fbr;
        if (blbl.IsStop)
        {
          tf = tfBold;
          r = DotSizeStop * 0.5;
          br = dotBrushStop;
          fbr = dotBrushStop;
        }
        else
        {
          tf = tfNormal;
          r = DotSize * 0.5;
          br = dotBrush;
          fbr = dotBrush;
        }

        dc.DrawEllipse(br, null, pt, r, r);
        dc.DrawRotatedString(blbl.Label, -45, tf, FontSize, fbr, pt, TextAlignment.Left, VertAlignment.Bottom, TextAlignment.Left, this);

        pt.X += _distance;

      }
    }
  }
}
