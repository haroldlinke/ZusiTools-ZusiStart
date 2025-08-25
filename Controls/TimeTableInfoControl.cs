using log4net;
using Sovoma.WPF;// Utilities;
using Sovoma;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ZusiKlassenLib.Buchfahrplan;
using ZusiKlassenLib.Common;
using ZusiKlassenLib.Fahrplan;
using ZusiKlassenLib.Vehicle;
using ZusiStart.Miscellaneous;
using System.IO;
//using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Microsoft.VisualBasic.ApplicationServices;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Media.Media3D;
using ZusiStart.Dialogs;
using Microsoft.VisualBasic.Logging;
using ZusiStart.Data;
using static System.Net.Mime.MediaTypeNames;
using ZusiKlassenLib;

namespace ZusiStart.Controls
{
  //=========================================================================
  /// <summary>
  /// </summary>
  [TemplatePart(Name = "PART_PicsScroller", Type = typeof(ScrollViewer))]
  [TemplatePart(Name = "PART_PicsPanel", Type = typeof(StackPanel))]
  [TemplatePart(Name = "SMALL_PicsPanel", Type = typeof(StackPanel))]

  public class TimeTableInfoControl : Control
  {
    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private ScrollViewer _picsScroller;
    private StackPanel _picsPanel;
    private StackPanel _smallpicsPanel;


    //private static readonly OverhangData _defaultOverhangData = new(0, 0);
    private BitmapImage _nopic;
    //private Dictionary<string, OverhangData> _overhangData = new Dictionary<string, OverhangData>();

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _bremsstellungKey = DependencyProperty.RegisterReadOnly(
        "Bremsstellung",
        typeof(Bremsstellung),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(Bremsstellung.Unknown));
    public static readonly DependencyProperty BremsstellungProperty = _bremsstellungKey.DependencyProperty;
    public Bremsstellung Bremsstellung
    {
      get { return (Bremsstellung)GetValue(BremsstellungProperty); }
      private set { SetValue(_bremsstellungKey, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        "CornerRadius",
        typeof(CornerRadius),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(new CornerRadius(0)));
    [Category("Darstellung")]
    public CornerRadius CornerRadius
    {
      get { return (CornerRadius)GetValue(CornerRadiusProperty); }
      set { SetValue(CornerRadiusProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty DepartureProperty = DependencyProperty.Register(
        "Departure",
        typeof(DateTime?),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(null));
    public DateTime? Departure
    {
      get { return (DateTime?)GetValue(DepartureProperty); }
      set { SetValue(DepartureProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(
        "Duration",
        typeof(TimeSpan?),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(null));
    public TimeSpan? Duration
    {
      get { return (TimeSpan?)GetValue(DurationProperty); }
      set { SetValue(DurationProperty, value); }
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _isDecoTrainKey = DependencyProperty.RegisterReadOnly(
        "IsDecoTrain",
        typeof(bool),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(false));
    public static readonly DependencyProperty IsDecoTrainProperty = _isDecoTrainKey.DependencyProperty;
    public bool IsDecoTrain
    {
      get { return (bool)GetValue(IsDecoTrainProperty); }
      private set { SetValue(_isDecoTrainKey, value); }
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _isRunningTrainKey = DependencyProperty.RegisterReadOnly(
        "IsRunningTrain",
        typeof(bool),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(false));
    public static readonly DependencyProperty IsRunningTrainProperty = _isRunningTrainKey.DependencyProperty;
    public bool IsRunningTrain
    {
      get { return (bool)GetValue(IsRunningTrainProperty); }
      private set { SetValue(_isRunningTrainKey, value); }
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _isFISavailableKey = DependencyProperty.RegisterReadOnly(
        "IsFISavailable",
        typeof(bool),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(false));
    public static readonly DependencyProperty IsFISavailableProperty = _isFISavailableKey.DependencyProperty;
    public bool IsFISavailable
    {
      get { return (bool)GetValue(IsFISavailableProperty); }
      private set { SetValue(_isFISavailableKey, value); }
    }

    //---------------------------------------------------------------------
    private static readonly DependencyPropertyKey _isTrainReplacedKey = DependencyProperty.RegisterReadOnly(
        "IsTrainReplaced",
        typeof(bool),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(false));
    public static readonly DependencyProperty IsTrainReplacedProperty = _isTrainReplacedKey.DependencyProperty;
    public bool IsTrainReplaced
    {
      get { return (bool)GetValue(IsTrainReplacedProperty); }
      private set { SetValue(_isTrainReplacedKey, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        "Kind",
        typeof(string),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(null));
    public string Kind
    {
      get { return (string)GetValue(KindProperty); }
      set { SetValue(KindProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty LengthProperty = DependencyProperty.Register(
        "Length",
        typeof(double),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(0.0));
    public double Length
    {
      get { return (double)GetValue(LengthProperty); }
      set { SetValue(LengthProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
        "Number",
        typeof(string),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(null));
    public string Number
    {
      get { return (string)GetValue(NumberProperty); }
      set { SetValue(NumberProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        "Source",
        typeof(Zug),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(null, OnSourceChanged));
    [Category("Allgemein")]
    public Zug Source
    {
      get { return (Zug)GetValue(SourceProperty); }
      set { SetValue(SourceProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty TimeTableNameProperty = DependencyProperty.Register(
        "TimeTableName",
        typeof(string),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(null));
    public string TimeTableName
    {
      get { return (string)GetValue(TimeTableNameProperty); }
      set { SetValue(TimeTableNameProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        "Title",
        typeof(string),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(null));
    public string Title
    {
      get { return (string)GetValue(TitleProperty); }
      set { SetValue(TitleProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty TrainLengthProperty = DependencyProperty.Register(
        "TrainLength",
        typeof(double),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(0.0));
    public double TrainLength
    {
      get { return (double)GetValue(TrainLengthProperty); }
      set { SetValue(TrainLengthProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty TrainMassProperty = DependencyProperty.Register(
        "TrainMass",
        typeof(double),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(0.0));
    public double TrainMass
    {
      get { return (double)GetValue(TrainMassProperty); }
      set { SetValue(TrainMassProperty, value); }
    }

    //---------------------------------------------------------------------
    public static readonly DependencyProperty UseCounterProperty = DependencyProperty.Register(
        "UseCounter",
        typeof(int),
        typeof(TimeTableInfoControl),
        new PropertyMetadata(0));
    public int UseCounter
    {
      get { return (int)GetValue(UseCounterProperty); }
      set { SetValue(UseCounterProperty, value); }
    }

    //---------------------------------------------------------------------
    static TimeTableInfoControl()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeTableInfoControl), new FrameworkPropertyMetadata(typeof(TimeTableInfoControl)));
    }

    //---------------------------------------------------------------------
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      _picsScroller = Template?.FindMandatoryTemplatePart<ScrollViewer>("PART_PicsScroller", this);
      _picsPanel = Template?.FindMandatoryTemplatePart<StackPanel>("PART_PicsPanel", this);
      _smallpicsPanel = Template?.FindMandatoryTemplatePart<StackPanel>("SMALL_PicsPanel", this);


      if (Source != null)
      {
        AssembleTrain(Source);
      }
    }

    //private static Buchfahrplan.LaTable[] LoadLaTable()
    //{
    //  string[] zusiDirs = Datei.GetZusiDataDirs();
    //  string localPfad = @"_Setup\lib\timetable\buchfahrplan2\VzG-La-Streckennummern.csv";
    //  gaConfigPfad2 = Datei.TryFindFirstExistingFile(zusiDirs, localPfad);
    //  return Buchfahrplan.LaTable.GetLaFromCsvFile(gaConfigPfad2); //Soll ruhig eine Ausnahme schmeißen, wenn die VsGs dort nicht da liegen.
    //}

    //---------------------------------------------------------------------
    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      TimeTableInfoControl t = d as TimeTableInfoControl;
      t?.OnSourceChanged(e.NewValue as Zug);
    }

    //---------------------------------------------------------------------
    private void OnSourceChanged(Zug zug)
    {
      Length = 0;
      Departure = null;
      Duration = null;

      if (zug == null)
      {
        Title = null;
        Kind = null;
        Number = null;
        IsRunningTrain = false;
        IsFISavailable = false;
        if (Properties.Settings.Default.BremsstellungAnzeigen)
        {
          Bremsstellung = Bremsstellung.G;
        }
      }
      else
      {
        Title = zug.Zuglauf;
        Kind = zug.Gattung;
        Number = zug.Nummer;
        IsDecoTrain = zug.IsDecoTrain;
        IsRunningTrain = zug.StartSpeed != 0;
        IsFISavailable = DataManager.Instance.check_for_FIS(zug);
        if (DataManager.Instance.CurrentTrainItem != null)
        {
          IsTrainReplaced = DataManager.Instance.CurrentTrainItem.IsTrainReplaced;
        }
        else
        {
          IsTrainReplaced = false;
        }
        
        if (Properties.Settings.Default.BremsstellungAnzeigen)
        {
          Bremsstellung = zug.Bremsstellung;
        }

        Buchfahrplan bf = zug.Buchfahrplan;
        if (bf != null)
        {
          FplZeile zz = bf.FplZeilen.FirstOrDefault();
          double startLaufweg = zz != null ? zz.Laufweg : 0;
          zz = bf.FplZeilen.LastOrDefault();
          Length = zz != null ? (zz.Laufweg - startLaufweg) * 0.001 : 0;

          Departure = bf.GetStartTime();
          if (Departure != null)
          {
            DateTime? end = bf.GetEndTime();
            if (end != null)
            {
              TimeSpan ts = end.Value - Departure.Value;
              Duration = ts.Ticks < 0 ? ts.Negate() : ts;
            }
          }
        }
        else
        {
          Departure = zug.StartTime;
          Duration = zug.JourneyTime;
          Length = double.NaN;
        }

        try
        {
          AssembleTrain(zug);
          DataManager.Instance.main_window.SearchBuchfahrplan();

          DataPathType dtp = DataPathType.Unknown;
          string orgRelativeTimetableName = Zusi.GetRelativePathOf(zug.GetDocument().Filename, ref dtp);
          orgRelativeTimetableName = orgRelativeTimetableName.Replace("\\", "%5C");
          string url = "https://www.zusidatenbank.de/fahrplanzug/" + orgRelativeTimetableName;


          //DataManager.Instance.webview_ZDB.Source = new Uri(url);
          DataManager.Instance.set_websource("Zusi-DB",url);
        }
        catch (Exception ex)
        {
          Log.Error(ex.ToString());
        }
      }
    }

    //---------------------------------------------------------------------
    private void AssembleTrain(Zug zug)
    {
      PictureManager pictureManager = new PictureManager();

      if (_picsPanel == null || _smallpicsPanel == null)
      {
        return;
      }

      DummyWindow dummywindow = new DummyWindow();

      //dummywindow.Title = "Test";
      dummywindow.Show();

      _picsPanel.Children.Clear();
      _smallpicsPanel.Children.Clear();

      ZusiDocumentBase doc = zug.GetDocument();
      Log.DebugFormat("assemble train: {0}", doc.Filename);

      ZugReihung zr = new(zug);
      zr.BuildTrain();

      TrainLength = 0; // Math.Round(zr.Length, 0);
      TrainMass = Math.Round(zr.Mass * 0.001);
      //string cachepath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ZusiStart\\cache";
      string cachepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DataManager.localfoldername, "cache");
      //if (Properties.Settings.Default.Use_LS3_Renderer_DLL == 1)
      //{
      //  cachepath = cachepath + "1";
      //}
      if (zug.Buchfahrplan != null)
      {
        DataManager.Instance.spMax = zug.Buchfahrplan.MaxSpeed * 3.6;
      }
      else
      {
        DataManager.Instance.spMax = 0;
      }
      DataManager.Instance.cachepath = cachepath;

      if (!System.IO.Directory.Exists(cachepath))
      {
        try
        {
          System.IO.Directory.CreateDirectory(cachepath);
          Log.Debug("Cache Directory created:" + cachepath);
        }
        catch
        {
          Log.Debug("ERROR: Cache Directory cannot be created:" + cachepath);
        }
      }

      LinkedListNode<FahrzeugInfo> p = zr.First;
      int Fahrzeug_num = 0;

      bool background1 = false;

      SolidColorBrush myBrush1 = new SolidColorBrush(Colors.Blue);
      myBrush1.Opacity = 0.5; // Set to 50% opacity
      SolidColorBrush myBrush2 = new SolidColorBrush(Colors.Red);
      myBrush2.Opacity = 0.5; // Set to 50% opacity
      //dummywindow.VehicleProgressBar.Value = 0;

      while (p != null)
      {
        Fahrzeug fzg = p.Value.Fahrzeug;
        FahrzeugVariante fv = fzg?.GetVariante(p.Value.IDHaupt, p.Value.IDNeben, p.Value.VariantenIndex) ?? null;
        if (fv != null)
        {
          Grid grd = new();
          Grid smallgrd = new();
          FahrzeugGrunddaten fzggd = fv.Grunddaten;

          bool gedreht = p.Value.Gedreht;
          //dummywindow.VehicleProgressBar.Value = Fahrzeug_num*100/zr.Count;
          //dummywindow.percentageText.Text = (Fahrzeug_num * 100 / zr.Count).ToString();
          //dummywindow.UpdateLayout();
          //DataManager.Instance.dataLoaderWindow.pbLoaded.Value = Fahrzeug_num/zr.Count * 100;
          //DataManager.Instance.dataLoaderWindow.UpdateLayout();
          Fahrzeug_num++;
          BitmapImage imagesource = pictureManager.getPicture(fzg, fv, gedreht, cachepath, dummywindow);

          if (imagesource != null)
          {
            System.Windows.Controls.Image image = new() { Source = imagesource };
            grd.Children.Add(image);
            System.Windows.Controls.Image smallimage = new() { Source = imagesource, Height=20, Stretch= System.Windows.Media.Stretch.Uniform };
            smallgrd.Children.Add(smallimage);

            //double length_factor = 16.55 / 100; //empirisch ermittelt bei erzeugetr Bildhöhe 100
            //double front_margin = 0.0;
            //double rear_margin = 0.0;
            //int height = (int)Math.Round(imagesource.Height)*3/2; //imagesource.PixelHeight;
            //double width_d = (int)Math.Round(imagesource.Width) * 3/2; //imagesource.PixelWidth;
            //int width = (int)Math.Round(width_d);
            //int vehiclewidth = (int)Math.Round(fzggd.Laenge * length_factor * height);

            //double front_gap = height / 2;
            //double rear_gap = width - (front_gap + vehiclewidth);
            //if (rear_gap < 0)
            //{
            //  rear_gap = 0;
            //}

            //double margin_factor = -0.65;

            //if (p.Value.Gedreht)
            //{
            //  front_margin = margin_factor * front_gap;
            //  rear_margin = margin_factor * rear_gap;
            //}
            //else // fahrzeug ist gedreht, vertausche front und rear margin
            //{
            //  rear_margin = margin_factor * front_gap;
            //  front_margin = margin_factor * rear_gap;
            //}

            //grd.Margin = new Thickness(front_margin, 0, rear_margin, 12);

            Thickness orig_thickness = pictureManager.Calculate_Margin(imagesource, fzggd.Laenge, p.Value.Gedreht);
            grd.Margin = pictureManager.Calculate_Margin(imagesource, fzggd.Laenge, p.Value.Gedreht);

           
            
            double factor = 20.0 / imagesource.Height;
            smallgrd.Margin = new Thickness(grd.Margin.Left * factor, grd.Margin.Top * factor, grd.Margin.Right * factor, grd.Margin.Bottom * factor);

            //factor = 60.0 / imagesource.Height;
            //grd.Margin = new Thickness(grd.Margin.Left * factor, grd.Margin.Top * factor, grd.Margin.Right * factor, grd.Margin.Bottom * factor);
            //grd.ShowGridLines = true;
            //if (background1)
            //  grd.Background = myBrush1;
            //else
            //  grd.Background = myBrush2;
            background1 = !background1;
            string br = fv.BR.StartsWithNumber() ? $"BR {fv.BR}" : fv.BR;
            if (p.Value.Gedreht)
              br = br + "-r";
            double front_margin = grd.Margin.Left;
            double rear_margin = grd.Margin.Right;
            // fzg.Name
            // *test* br = string.Format("{0}-{1}-{2} ({3}-{4})", fzg.Name, fv.IDHaupt, fv.IDNeben, (int)Math.Round(front_margin,0), (int)Math.Round(rear_margin,0)).ToLower();
            TextBlock txt = new()
            {
              Text = br,
              Foreground = Brushes.Black,
              //Background = Brushes.Gray,
              FontStyle = FontStyles.Italic,
              // *test* FontSize = 8,
              HorizontalAlignment = HorizontalAlignment.Center,
              VerticalAlignment = VerticalAlignment.Bottom,
              Margin = new Thickness(-front_margin, 0, -rear_margin, -12)
            };

            Panel.SetZIndex(txt, 7);
            grd.Children.Add(txt);

            _picsPanel.Children.Insert(0, grd);
            _smallpicsPanel.Children.Insert(0, smallgrd);
          }
        }
        p = p.Next;
      }
      TrainLength = Math.Round(zr.Length, 0);
      dummywindow.Close();

      _picsScroller.ScrollToRightEnd();
    }
  }

  //=========================================================================
  public class AverageSpeedConverter : IMultiValueConverter
  {
    //---------------------------------------------------------------------
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      TimeSpan duration;

      if (values.Length == 2 && values[0] is double length && (values[1] as TimeSpan?) != null)
      {
        duration = ((TimeSpan?)values[1]).Value;
        if (duration.Ticks > 0 && length > 0)
        {
          string format = string.IsNullOrEmpty(parameter as string) ? "ø {0:N0} km/h" : (string)parameter;
          return string.Format(format, length / duration.TotalHours);
        }
      }

      return null;
    }

    //---------------------------------------------------------------------
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  //=========================================================================
  [ValueConversion(typeof(DateTime?), typeof(string))]
  public class DepartureConverter : IValueConverter
  {
    //---------------------------------------------------------------------
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      DateTime? dt = value as DateTime?;
      return dt != null ? string.Format("Abfahrt: {0} Uhr", dt.Value.ToString("HH:mm")) : null;
    }

    //---------------------------------------------------------------------
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  //=========================================================================
  [ValueConversion(typeof(TimeSpan?), typeof(string))]
  public class DurationConverter : IValueConverter
  {
    //---------------------------------------------------------------------
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string format = string.IsNullOrEmpty(parameter as string) ? "Fahrzeit: {0}:{1:D2}|Fahrzeit: {0} Minuten" : (string)parameter;
      string[] formats = format.Split('|');

      TimeSpan? t = value as TimeSpan?;
      if (t != null)
      {
        TimeSpan ts = t.Value;
        if (ts.Ticks > 0)
        {
          if (ts.Hours > 0)
          {
            return string.Format(formats[0], ts.Hours, ts.Minutes);
          }
          else
          {
            return string.Format(formats[1], ts.Minutes);
          }
        }
      }

      return null;
    }

    //---------------------------------------------------------------------
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  //=========================================================================
  [ValueConversion(typeof(double), typeof(string))]
  public class LengthConverter : IValueConverter
  {
    //---------------------------------------------------------------------
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string format = string.IsNullOrEmpty(parameter as string) ? "{0:N1} km" : (string)parameter;

      return value is double d && d > 0 ? string.Format(format, d) : null;
    }

    //---------------------------------------------------------------------
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  //=========================================================================
  [ValueConversion(typeof(double), typeof(string))]
  public class TrainLengthConverter : IValueConverter
  {
    //---------------------------------------------------------------------
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string format = string.IsNullOrEmpty(parameter as string) ? "{0:N0} m" : (string)parameter;
      return value is double v && v > 0 ? string.Format(CultureInfo.CurrentCulture, format, value) : null;
    }

    //---------------------------------------------------------------------
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  //=========================================================================
  [ValueConversion(typeof(double), typeof(string))]
  public class TrainMassConverter : IValueConverter
  {
    //---------------------------------------------------------------------
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string format = string.IsNullOrEmpty(parameter as string) ? "{0:N0} t" : (string)parameter;
      return value is double v && v > 0 ? string.Format(CultureInfo.CurrentCulture, format, value) : null;
    }

    //---------------------------------------------------------------------
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

  //=========================================================================
  [ValueConversion(typeof(int), typeof(string))]
  public class UseCounterConverter : IValueConverter
  {
    //---------------------------------------------------------------------
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value is int i ? string.Format("{0} mal gefahren", i) : string.Empty;
    }

    //---------------------------------------------------------------------
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
