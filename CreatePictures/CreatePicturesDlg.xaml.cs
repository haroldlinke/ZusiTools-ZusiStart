using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Landscape;
using ZusiKlassenLib2.Vehicle;
using log4net;
using ZusiStart.Miscellaneous;
using ZusiStart.Data;

namespace ZusiStart.CreatePictures
{
  /// <summary>
  /// Interaktionslogik für CreatePicturesDlg.xaml
  /// </summary>
  public partial class CreatePicturesDlg : Window
  {
    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
    public static readonly RoutedUICommand CommandClose = new RoutedUICommand("Abbrechen", "CommandClose", typeof(CreatePicturesDlg));

    private bool _cancelled;

    public CreatePicturesDlg()
    {
      InitializeComponent();
      Closing += CreatePicturesDlg_Closing;
      _cancelled = false;
      CommandBindings.Add(new CommandBinding(CommandClose, (s, e) => DialogResult = true, (s, e) => e.CanExecute = true));
      //TakePhotographs();
    }

    private void CreatePicturesDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      Closing -= CreatePicturesDlg_Closing;
      e.Cancel = true;
      var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(600));
      anim.Completed += (s, a) => Close();
      BeginAnimation(OpacityProperty, anim);
      _cancelled = true;
    }

    private async void StartProgress_Click(object sender, RoutedEventArgs e)
    {
      progressBar.Value = 0;
      percentageText.Text = "0%";

      TakePhotographs();
    }

    private async void TakePhotographs()
    {
      bool success = false;
      string reason = null;

      PictureManager pictureManager = new PictureManager();

      string cachepath = DataManager.Instance.cachepath;       // Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ZusiStart\\cache";

      List<Fahrzeug> fahrzeuge = Fahrzeuge.EnumerateVehicles(null, new CancellationTokenSource().Token);
      int nn = 0;
      foreach (Fahrzeug f in fahrzeuge)
      {
        nn += f.Varianten.Count;
      }
      _log.DebugFormat(" {0} Fahrzeuge mit {1} Varianten gefunden", fahrzeuge.Count, nn);

      int Fahrzeug_num = 0;

      BitmapImage? imagesource = null;

      foreach (Fahrzeug fzg in fahrzeuge)
      {
        ZusiDocumentBase doc = fzg.GetDocument();

        foreach (FahrzeugVariante fv in fzg.Varianten)
        {
          if (_cancelled)
            break;

          int i = (Fahrzeug_num * 100) / nn;
          progressBar.Value = i;
          percentageText.Text = $"Fahrzeug {Fahrzeug_num} von {nn} - {i}%";
          await Task.Delay(1); // update progress bar and allow UI to react to mouse clicks

          FahrzeugGrunddaten fgd = fv.Grunddaten;
          if (fgd == null)
          {
            _log.Debug("...............................................................");
            _log.Debug(doc.Filename);
            _log.WarnFormat("Variante {0}.{1}: vehicle doesn't have base data, skipped", fv.IDHaupt, fv.IDNeben);
            continue;
          }
          Fahrzeug_num++;

          imagesource = pictureManager.getPicture(fzg, fv, false, cachepath, create_no_image:true);
          imagesource = pictureManager.getPicture(fzg, fv, true, cachepath,create_no_image:true);

        }
      }

      if (!_cancelled)
      {
        success = true;
        Close();
      }
      else
      {
        reason = "Das Fotografieren wurde abgebrochen.";
      }

      //OnSessionFinished(success, reason);
    }
  }
}
