using log4net;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
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
using Z8Routegraph2n;
using ZusiCLIProject.FileLibrary.Zusi3;
using ZusiCLIProject.Routegraph2;
using ZusiStart.Data;
using ZusiStart.Miscellaneous;
//using ZusiMeter.Data;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using static ZusiCLIProject.FileLibrary.Zusi3.Strecke;

namespace ZusiCLIProject.Routegraph2
{
  /// <summary>
  /// Interaktionslogik für MainWindow.xaml
  /// </summary>
  public partial class RouteGraph2Control : System.Windows.Controls.UserControl
  {
    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public RouteGraph2Control()
    {
      isInCtor = true;
      InitializeComponent();
      //DefaultTitel = this.Title;
      //DataManager.Instance.routeGraph2Control = this;

      ActionModulAnfuegen.IsEnabled = !m_streckennetz.IsEmpty;
      ActionOrdnerAnfuegen.IsEnabled = !m_streckennetz.IsEmpty;

      //GleisfunktionMenuItem.IsChecked = true; **Test**
      KeineMenuItem.IsChecked = true;
      isInCtor = false;

      var grpTransf = new TransformGroup();
      grpTransf.Children.Add(new ScaleTransform(1, -1));
      grpTransf.Children.Add(new TranslateTransform(0, m_legendeView.Height));
      m_legendeView.RenderTransform = grpTransf;

      for (int i = 0; true; ++i)
      {
        object? val = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Zusi3\\Fahrsim\\Fahrplan", i.ToString(), null);
        if (val == null)
          break;
        string pfad = (string)val;
        if (string.IsNullOrEmpty(pfad))
          break;
        if (i == 0)
        {
          var sep = new Separator();
          MenuDatei.Items.Insert(MenuDatei.Items.IndexOf(ActionOrdnerAnfuegen) + 1, sep);
        }
        var men1 = new MenuItem();
        men1.Header = (i + 1).ToString() + " " + System.IO.Path.GetFileName(pfad);
        if (System.IO.File.Exists(Datei.TryFindFirstExistingFile(Datei.GetZusiDataDirs(), pfad)))
        {
          men1.Click += delegate (object sender, RoutedEventArgs e)
          {
            IEnumerable<string> dateinamen = new[] { Datei.TryFindFirstExistingFile(Datei.GetZusiDataDirs(), pfad) };
            LoadingWindow.OeffneDateien(Datei.GetZusiDataDirs(), dateinamen, m_streckennetz, false, false, delegate (LoadingWindow? loadingWindow)
            {
              SetzeTitel(dateinamen);
              AktualisiereDarstellung();
              SetzeAnsichtZurueck();
            });
          };
        }
        else
          men1.IsEnabled = false;
        MenuDatei.Items.Insert(MenuDatei.Items.IndexOf(ActionOrdnerAnfuegen) + 2 + i, men1);
      }

      Loaded += RouteGraph2Control_Loaded;

      //if (DataManager.Instance.routeGraphOpenFile != "")
      //{
      //  ModulOeffnen([DataManager.Instance.routeGraphOpenFile]);
      //}
    }

    private void RouteGraph2Control_Loaded(object sender, RoutedEventArgs e)
    {
      try
      {
        if (DataManager.Instance.routeGraphOpenFile != "")
        {
          ModulOeffnen([DataManager.Instance.routeGraphOpenFile]);
          DataManager.Instance.routeGraphOpenFile = "";
        }

      }
      catch (Exception ex)
      {
        LogHelper.LogException(ex, "Something went wrong loading routegraph");
      }
    }

    private bool isInCtor = false;

    private Streckennetz m_streckennetz = new();
    private StreckeScene? m_streckeScene = null;
    //private QGraphicsScene m_legendeScene;
    private bool m_zeigeBetriebsstellen = false;
    private bool m_zeigeBetriebsstelleninRot = false;

    ////---------------------------------------------------------------------
    //private static readonly DependencyPropertyKey _routeGraphVisibilityKey = DependencyProperty.RegisterReadOnly(
    //    "RouteGraphVisibility",
    //    typeof(Visibility),
    //    typeof(DataManager),
    //    new PropertyMetadata(Visibility.Collapsed));
    //public static readonly DependencyProperty RouteGraphVisibilityProperty = _routeGraphVisibilityKey.DependencyProperty;
    //public Visibility RouteGraphVisibility
    //{
    //  get { return (Visibility)GetValue(RouteGraphVisibilityProperty); }
    //  private set { SetValue(_routeGraphVisibilityKey, value); }
    //}

    //public Visibility m_routeGraphVisibility = Visibility.Collapsed;

    private string DefaultTitel;
    private void SetzeTitel(IEnumerable<string> dateinamen)
    {
      if (dateinamen.Count() != 1)
      {
        //this.Title = DefaultTitel;
        return;
      }
      string single = dateinamen.Single();
      string dateiname = System.IO.Path.GetFileName(single);
      //if (string.IsNullOrEmpty(dateiname))
      //	this.Title = DefaultTitel;
      //else
      //	this.Title = DefaultTitel + " [" + dateiname + "]";
    }

    private void SetzeAnsichtZurueck()
    {
      m_streckeView.SkaliereAufAnsicht(true);
    }

    public void ZentriereView(double x,double y)
    {
      m_streckeView.Zentrieren(x,y);
    }

    public void SetzeTransform(System.Windows.Media.Matrix matrix)
    {
      m_streckeView.RenderTransform = new MatrixTransform(matrix);
    }

    /** Oeffnet Streckendateien und fuegt sie zur Liste der offenen Strecken hinzu. */
    private void OeffneStrecken(IEnumerable<string> dateinamen)
    {
      var timer = DateTime.Now;

      var messages = new List<string>();
      bool hasErrors = false;

      var zusiDataDirs = Datei.GetZusiDataDirs();
      var datList = new List<Datei>();
      var buf = new Dictionary<string, Zusi>(System.StringComparer.InvariantCultureIgnoreCase);
      foreach (var kv in m_streckennetz)
        buf.Add(kv.Key, kv.Value.ParentBuffer);
      foreach (var dateiname in dateinamen)
      {
        var dat = new Datei();
        dat.Dateiname = dateiname;
        foreach (var pfad1 in zusiDataDirs)
        {
          if (!dat.Dateiname.ToLower().StartsWith(pfad1.ToLower()))
            continue;
          dat.Dateiname = dateiname.Substring(pfad1.Length);
          if (dat.Dateiname.StartsWith("\\"))
            dat.Dateiname = dat.Dateiname.Substring(1);
          if (dateiname.ToLower() != Datei.TryFindFirstExistingFile(zusiDataDirs, dat.Dateiname).ToLower())
            messages.Add("Datei " + dat.Dateiname + " wird aus den Eigenen Daten geladen.");
          if (buf.ContainsKey(dat.Dateiname))
            messages.Add("Datei " + dat.Dateiname + " bereits geladen.");
          break;
        }
        datList.Add(dat);
      }
      while (datList.Count > 0)
      {
        Datei dat = datList[0];
        datList.RemoveAt(0);

        try
        {
          dat.Load(zusiDataDirs, buf);

          foreach (var f1 in dat.Content.Fahrplaene)
          {
            foreach (var fs1 in f1.Streckendateien)
            {
              datList.Add(fs1.Datei);
            }
          }
        }
        catch (Exception ex)
        {
          string message = ex.Message;
          string? executableFilenamePath = Process.GetCurrentProcess().MainModule?.FileName;
          string executablePath = System.IO.Path.GetDirectoryName(executableFilenamePath);
          if (executablePath != null)
            message = message.Replace(executablePath, "");
          
          messages.Add("Fehler beim Laden von " + dat.Dateiname + " (" + message + ")");
          hasErrors = true;
        }
      }
      var clearBuf = new List<string>();
      foreach (var b in buf)
      {
        try
        {
          var sub = b.Value.CollectDateiSubmemberExceptHauptlandschaft();
          foreach (var dat1 in sub)
          {
            dat1.Peek(buf);
          }
          b.Value.FillParentBuffer();
        }
        catch (Exception ex)
        {
          messages.Add("Fehler beim Vorbereiten von " + b.Key + " (" + ex.Message + ")");
          hasErrors = true;
          clearBuf.Add(b.Key);
        }
      }
      foreach (string b in clearBuf)
        buf.Remove(b);
      clearBuf.Clear();
      foreach (var b in buf)
      {
        try
        {
          foreach (Strecke s in b.Value.Strecken)
          {
            s.UpdateBufferIfInvalidated();
            s.LoadReferenzenDestinationBuffer();
            foreach (Strecke.Referenz.Eintrag e in b.Value.CollectReferenzSubmember())
            {
              e.TryLoad(s);
            }
          }
        }
        catch (Exception ex)
        {
          messages.Add("Fehler beim Linken von " + b.Key + " (" + ex.Message + ")");
          hasErrors = true;
          clearBuf.Add(b.Key);
        }
      }
      foreach (var b in buf)
      {
        try
        {
          foreach (Strecke s in b.Value.Strecken)
          {
            s.RefreshStreckenelementNachfolgerBuffer(delegate (int src, Datei d1, int target) { });
          }
        }
        catch (Exception ex)
        {
          messages.Add("Fehler beim Linken von " + b.Key + " (" + ex.Message + ")");
          hasErrors = true;
          clearBuf.Add(b.Key);
        }
      }
      foreach (string b in clearBuf)
        buf.Remove(b);
      clearBuf.Clear();
      var streckenMitUtmPunkt = new Dictionary<string, Zusi>(System.StringComparer.InvariantCultureIgnoreCase);
      foreach (var b in buf)
      {
        foreach (Strecke s in b.Value.Strecken)
        {
          if (s.UTMPoint.Zone != 0 && !streckenMitUtmPunkt.ContainsKey(b.Key))
            streckenMitUtmPunkt.Add(b.Key, b.Value);
        }
      }

      var timeDiff = DateTime.Now.Subtract(timer);
      System.Diagnostics.Debug.WriteLine("Lesen der Strecke in {0}", timeDiff);

      if (messages.Count > 0)
      {
        //WPF bekommt es offenbar nicht hin, die MessageBox mit Visuellen Stilen zu zeichnen...
        System.Windows.Forms.MessageBox.Show((hasErrors ? "Fehler beim Laden:" : "Hinweis:") + "\r\n" + string.Join("\r\n", messages.ToArray()), hasErrors ? "Fehler beim Laden:" : "Hinweis:");
        Log.Debug((hasErrors ? "Fehler beim Laden:" : "Hinweis:") + "\r\n" + string.Join("\r\n", messages.ToArray()));
      }

      m_streckennetz.AddByBuffer((streckenMitUtmPunkt.Count > 0) ? streckenMitUtmPunkt : buf);

      ActionModulAnfuegen.IsEnabled = !m_streckennetz.IsEmpty;
      ActionOrdnerAnfuegen.IsEnabled = !m_streckennetz.IsEmpty;
    }

    /** Zeigt die aktuell geladenen Strecken im Strecken-Widget an. */
    private void AktualisiereDarstellung()
    {
      Visualisierung? visualisierung;
      if (KeineMenuItem.IsChecked)
        visualisierung = null;
      else if (KruemmungMenuItem.IsChecked)
        visualisierung = new KruemmungVisualisierung();
      else if (UeberhoehungMenuItem.IsChecked)
        visualisierung = new UeberhoehungVisualisierung();
      else if (NeigungMenuItem.IsChecked)
        visualisierung = new NeigungVisualisierung();
      else if (GeschwindigkeitMenuItem.IsChecked)
        visualisierung = new GeschwindigkeitVisualisierung();
      else if (OberbauMenuItem.IsChecked)
        visualisierung = new OberbauVisualisierung();
      else if (FahrleitungMenuItem.IsChecked)
        visualisierung = new FahrleitungVisualisierung();
      else if (ETCSTrustedAreasMenuItem.IsChecked)
        visualisierung = new EtcsTrustedAreaVisualisierung();
      else
        visualisierung = null;

      if (GleisfunktionMenuItem.IsChecked)
        visualisierung = new GleisfunktionVisualisierung(visualisierung);

      var timer = DateTime.Now;

      m_streckeScene = new StreckeScene(m_streckennetz, visualisierung, m_zeigeBetriebsstellen, ETCSTrustedAreasMenuItem.IsChecked);

      var timeDiff = DateTime.Now.Subtract(timer);
      System.Diagnostics.Debug.WriteLine("Erstellen der Segmente in {0}", timeDiff);
      this.m_streckeView.ResetScene(this.m_streckeScene);

      //DataManager.Instance.m_streckeScene = new StreckeScene(m_streckennetz, visualisierung, m_zeigeBetriebsstellen, ETCSTrustedAreasMenuItem.IsChecked);

      //DataManager.Instance.utmBounds = new UtmBounds(m_streckeScene.minUtmX, m_streckeScene.minUtmY, m_streckeScene.maxUtmX, m_streckeScene.maxUtmY);
      //DataManager.Instance.canvasBounds = new CanvasBounds(m_streckeScene.minCanvasX, m_streckeScene.minCanvasY, m_streckeScene.maxCanvasX, m_streckeScene.maxCanvasY);

      DataManager.Instance.utmBounds = new UtmBounds(m_streckeScene.m_utmRefPunkt.WE*1000, m_streckeScene.m_utmRefPunkt.NS*1000, m_streckeScene.m_utmRefPunkt.WE*1000+m_streckeScene.maxCanvasX, m_streckeScene.m_utmRefPunkt.NS * 1000 +m_streckeScene.maxCanvasY);
      DataManager.Instance.canvasBounds = new CanvasBounds(0, 0, m_streckeScene.maxCanvasX, m_streckeScene.maxCanvasY);

      m_legendeView.Children.Clear();

      var legende = (visualisierung == null) ? null : visualisierung.Legende;
      if (legende != null)
      {
        m_legendeView.Children.Add(legende);
        if (visualisierung?.LegendeWidth != null)
          legende.RenderTransform = new TranslateTransform(-visualisierung.LegendeWidth.Value / 2.0f, 0);
      }
      
    }

    public void set_canvas_min_max()
    {
      DataManager.Instance.canvasBounds = new CanvasBounds(0, 0, m_streckeScene.ActualWidth, m_streckeScene.ActualHeight);
    }

    /** Zeigt einen Dialog zum Oeffnen einer Strecke mit passendem Startverzeichnis und liefert den Dateinamen zurueck. */
    private IEnumerable<string>? ZeigeStreckeOeffnenDialog()
    {
      var dialog = new ZusiCLIProject.FileLibrary.CommonGUI.OpenFileDialogDualFolder();
      dialog.Filter = "Strecken oder Fahrpläne|*.fpn;*.st3|Strecken|*.st3|Fahrpläne|*.fpn";
      dialog.Multiselect = true;
      dialog.Title = "Strecke oder Fahrplan öffnen...";
      dialog.BasepathsForAutomaticLinkCreation = Datei.GetZusiDataDirs();
      if (Datei.GetZusiDataDirs().Length > 0)
        dialog.InitialFolder = Datei.GetZusiDataDirs().First();
      dialog.ShowDialog();
      return dialog.FileNames;
    }

    /** Zeigt einen Dialog zum Oeffnen eines Streckenordners und liefert eine Liste von ST3-Dateien in diesem Verzeichnis zurueck. */
    private IEnumerable<string>? ZeigeOrdnerOeffnenDialog()
    {
      var dialog = new ZusiCLIProject.FileLibrary.CommonGUI.OpenDirectoryDialogDualFolder();
      dialog.Multiselect = true;
      dialog.Title = "Streckenordner öffnen...";
      dialog.BasepathsForAutomaticLinkCreation = Datei.GetZusiDataDirs();
      if (Datei.GetZusiDataDirs().Length > 0)
        dialog.InitialFolder = Datei.GetZusiDataDirs().First();
      dialog.ShowDialog();
      return dialog.FileNames;
    }

    public async void ModulOeffnen(IEnumerable<string> dateinamen)
    {
      try
      {
        var overlay = new BusyOverlayWindow { Owner = DataManager.Instance.main_window };
        string single = dateinamen.Single();
        string dateiname = System.IO.Path.GetFileName(single);

        overlay.Message = LocalizationManager.Translate("Erstelle Streckenplan für Fahrplan ") + dateiname;
        overlay.Show();
        // Update message dynamically

        SetzeTitel(dateinamen);
        m_streckennetz.Clear();
        OeffneStrecken(dateinamen);
        AktualisiereDarstellung();
        SetzeAnsichtZurueck();
        overlay.Close();
      }
      catch (Exception ex)
      {
        System.Windows.Forms.MessageBox.Show("Fehler beim Laden der Strecke: " + ex.Message, "Fehler");
      }
    }
    private void ModulOeffnen_Click(object sender, RoutedEventArgs e)
    {
      IEnumerable<string>? dateinamen = ZeigeStreckeOeffnenDialog();
      if (dateinamen == null)
        return;
      LoadingWindow.OeffneDateien(Datei.GetZusiDataDirs(), dateinamen, m_streckennetz, false, false, delegate (LoadingWindow? loadingWindow)
      {
        SetzeTitel(dateinamen);
        AktualisiereDarstellung();
        SetzeAnsichtZurueck();
      });
    }
    private void ModulAnfuegen_Click(object sender, RoutedEventArgs e)
    {
      IEnumerable<string>? dateinamen = ZeigeStreckeOeffnenDialog();
      if (dateinamen == null)
        return;
      LoadingWindow.OeffneDateien(Datei.GetZusiDataDirs(), dateinamen, m_streckennetz, true, false, delegate (LoadingWindow? loadingWindow)
      {
        //this.Title = DefaultTitel;
        AktualisiereDarstellung();
      });
    }
    private void OrdnerOeffnen_Click(object sender, RoutedEventArgs e)
    {
      IEnumerable<string>? ordnernamen = ZeigeOrdnerOeffnenDialog();
      if (ordnernamen == null)
        return;
      LoadingWindow.OeffneDateien(Datei.GetZusiDataDirs(), ordnernamen, m_streckennetz, false, true, delegate (LoadingWindow? loadingWindow)
      {
        SetzeTitel(ordnernamen);
        AktualisiereDarstellung();
        SetzeAnsichtZurueck();
      });
    }
    private void OrdnerAnfuegen_Click(object sender, RoutedEventArgs e)
    {
      IEnumerable<string>? ordnernamen = ZeigeOrdnerOeffnenDialog();
      if (ordnernamen == null)
        return;
      LoadingWindow.OeffneDateien(Datei.GetZusiDataDirs(), ordnernamen, m_streckennetz, true, true, delegate (LoadingWindow? loadingWindow)
      {
        //this.Title = DefaultTitel;
        AktualisiereDarstellung();
      });
    }


    private void GleisfunktionTriggered(object sender, RoutedEventArgs e)
    {
      if (m_streckeView != null)
      {
        // Transformation und Scroll-Position speichern und wiederherstellen
        var tranfsorm = m_streckeView.RenderTransform;
        var centerPointH = m_streckeView.HorizontalOffset;
        var centerPointV = m_streckeView.VerticalOffset;
        AktualisiereDarstellung();
        m_streckeView.RenderTransform = tranfsorm;
        m_streckeView.ScrollToHorizontalOffset(centerPointH);
        m_streckeView.ScrollToVerticalOffset(centerPointV);
      }
    }
    private bool isInVisualisierung = false;
    private void VisualisierungTriggered(object sender, RoutedEventArgs e)
    {
      if (isInCtor)
        return;
      isInVisualisierung = true;
      KeineMenuItem.IsChecked = KeineMenuItem == sender;
      KruemmungMenuItem.IsChecked = KruemmungMenuItem == sender;
      UeberhoehungMenuItem.IsChecked = UeberhoehungMenuItem == sender;
      NeigungMenuItem.IsChecked = NeigungMenuItem == sender;
      GeschwindigkeitMenuItem.IsChecked = GeschwindigkeitMenuItem == sender;
      OberbauMenuItem.IsChecked = OberbauMenuItem == sender;
      FahrleitungMenuItem.IsChecked = FahrleitungMenuItem == sender;
      ETCSTrustedAreasMenuItem.IsChecked = ETCSTrustedAreasMenuItem == sender;

      // Transformation und Scroll-Position speichern und wiederherstellen
      var tranfsorm = m_streckeView.RenderTransform;
      var centerPointH = m_streckeView.HorizontalOffset;
      var centerPointV = m_streckeView.VerticalOffset;
      AktualisiereDarstellung();
      m_streckeView.RenderTransform = tranfsorm;
      m_streckeView.ScrollToHorizontalOffset(centerPointH);
      m_streckeView.ScrollToVerticalOffset(centerPointV);
      isInVisualisierung = false;
    }
    private void VisualisierungUnchecked(object sender, RoutedEventArgs e)
    {
      if (isInVisualisierung)
        return;
      ((MenuItem)sender).IsChecked = true; //Unchecking nicht erlauben
    }

    private void VergroessernMenuItem_Click(object sender, RoutedEventArgs e)
    {
      m_streckeView.Vergroessern();
    }

    private void VerkleinernMenuItem_Click(object sender, RoutedEventArgs e)
    {
      m_streckeView.Verkleinern();
    }
    private void SetzeAnsichtZurueckMenuItem_Click(object sender, RoutedEventArgs e)
    {
      SetzeAnsichtZurueck();
    }

    private void BetriebsstellennamenMenuItem_Checked(object sender, RoutedEventArgs e)
    {
      m_zeigeBetriebsstellen = BetriebsstellennamenMenuItem.IsChecked;

      if (isInCtor)
        return;

      // Transformation und Scroll-Position speichern und wiederherstellen
      // Transformation und Scroll-Position speichern und wiederherstellen
      var tranfsorm = m_streckeView.RenderTransform;
      var centerPointH = m_streckeView.HorizontalOffset;
      var centerPointV = m_streckeView.VerticalOffset;
      AktualisiereDarstellung();
      m_streckeView.RenderTransform = tranfsorm;
      m_streckeView.ScrollToHorizontalOffset(centerPointH);
      m_streckeView.ScrollToVerticalOffset(centerPointV);
    }

    private void BetriebsstellennamenRotMenuItem_Checked(object sender, RoutedEventArgs e)
    {
      DataManager.BetriebsstellenManager.betriebstelle_in_rot = BetriebsstellennamenRotMenuItem.IsChecked;

      if (isInCtor)
        return;

      // Transformation und Scroll-Position speichern und wiederherstellen
      var tranfsorm = m_streckeView.RenderTransform;
      var centerPointH = m_streckeView.HorizontalOffset;
      var centerPointV = m_streckeView.VerticalOffset;
      AktualisiereDarstellung();
      m_streckeView.RenderTransform = tranfsorm;
      m_streckeView.ScrollToHorizontalOffset(centerPointH);
      m_streckeView.ScrollToVerticalOffset(centerPointV);
    }
    private void Beenden_Click(object sender, RoutedEventArgs e)
    {
      //this.Close();
    }
    private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
      //WPF bekommt es offenbar nicht hin, die MessageBox mit Visuellen Stilen zu zeichnen...
      System.Windows.Forms.MessageBox.Show("Routegraph2n " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() + "\r\n\r\n" +
        "Routegraph2n kann unter der GNU GENERAL PUBLIC LICENSE Version 3 verwendet werden." + "\r\n\r\n" +
        "Folgende Zusi-Datenverzeichnisse wurden erkannt:\r\n" + string.Join("\r\n", Datei.GetZusiDataDirs()) + "\r\n\r\n" +
        "Routegrahp2n verwendet die Bibliothek UTM, die unter der GNU LESSER GENERAL PUBLIC LICENSE Version 3 verwendet werden kann.", "Routegraph2n");
    }

  }
    
}
