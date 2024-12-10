using log4net;
using Microsoft.VisualBasic.Logging;
using Sovoma;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using ZusiKlassenLib;
using ZusiKlassenLib.Buchfahrplan;
using ZusiKlassenLib.Common;
using ZusiKlassenLib.TimeTable;
using ZusiPicLib;
using ZusiKlassenLib.Vehicle;
using ZusiKlassenLib.Fahrplan;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Xml.Linq;
using System.Drawing;
using System.Diagnostics.Eventing.Reader;

namespace ZusiStart.Miscellaneous
{
  class PictureManager
  {
    private static readonly ILog _log = LogManager.GetLogger(typeof(PictureManager));
    // calculate width
    int height = 100;
    double factor = 5.5; // emprisch ermittel

    public Thickness Calculate_Margin(BitmapImage imagesource, double fahrzeuglänge, bool rotated)
    {
      double imagesize_factor = 3.0 / 2.0;
      double length_factor = (16.55 / 100); //empirisch ermittelt bei erzeugter Bildhöhe 100
      double front_margin = 0.0;
      double rear_margin = 0.0;
      int height = (int)Math.Round(imagesource.Height * imagesize_factor);
      double width_d = (int)Math.Round(imagesource.Width * imagesize_factor); 
      int width = (int)Math.Round(width_d);
      int vehiclewidth = (int)Math.Round(fahrzeuglänge * length_factor * height);

      double front_gap = height/ 2.0;
      double rear_gap = width - (front_gap + vehiclewidth);
      if (rear_gap < 0)
      {
        rear_gap = 0;
      }

      double margin_factor = -0.66;

      if (rotated)
      {
        front_margin = margin_factor * front_gap;
        rear_margin = margin_factor * rear_gap;
      }
      else // fahrzeug ist gedreht, vertausche front und rear margin
      {
        rear_margin = margin_factor * front_gap;
        front_margin = margin_factor * rear_gap;
      }

      return new Thickness(front_margin, 0, rear_margin, 12);
    }



    public BitmapImage getPicture(Fahrzeug fzg, FahrzeugVariante fv, bool gedreht, string cachepath,Window? popup_message=null)
    {
      FahrzeugGrunddaten fzggd = fv.Grunddaten;
      double width_d = height * factor;
      int width = (int)Math.Round(width_d);
      int vehiclewidth = (int)Math.Round(fzggd.Laenge * 16.55);
      System.Drawing.Size picsize = new(width, height);
      
      string cachefilename2 = string.Format("{0}-{1}-{2}", fzg.Name, fv.IDHaupt, fv.IDNeben + (!gedreht ? "-r" : "-n")).ToLower();
      string cachefilepathname2 = cachepath + "\\"+ cachefilename2+".png";
      if (!System.IO.File.Exists(cachefilepathname2))
      {
        if (popup_message != null)
          popup_message.Show();
        string cachefilename = string.Format("{0}-{1}-{2}", fzg.Name, fv.IDHaupt, fv.IDNeben).ToLower();
        string ls3_filename = fv.DateiAussenansicht.Dateiname;
        
        string Arbeitsverzeichnis = ZusiKlassenLib.Zusi.DataPath[0];
        string[] ZusiDataDirs = { Arbeitsverzeichnis};
        string DateiNameRelativ = ls3_filename;
        NativeMethods.ls3Ansicht mode = NativeMethods.ls3Ansicht.Seitenansicht;
        _log.Debug("Generate Image for:"+cachefilename);
        string filename = NativeMethods.ls3Vorschau(ZusiDataDirs, DateiNameRelativ, mode, picsize, cachefilename, cachepath);
        if (System.IO.File.Exists(filename))
        {
          using (Bitmap bitmap = new Bitmap(filename))
          {
            // Define the color to make transparent (light grey)
            System.Drawing.Color lightGrey = System.Drawing.Color.FromArgb(255, 166, 200, 255); // Adjust RGB values as needed
            // Make the specified color transparent
            bitmap.MakeTransparent(lightGrey);
            if (!gedreht)
            {
              // Flip the bitmap horizontally
              bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            // Save the flipped bitmap to a new file
            bitmap.Save(cachefilepathname2, System.Drawing.Imaging.ImageFormat.Png);
          }
        }
        else
        {
          return null;
        }
      }
      // Create a BitmapImage from a file
      BitmapImage bitmap2 = new BitmapImage();
      bitmap2.BeginInit();
      bitmap2.UriSource = new Uri(cachefilepathname2, UriKind.RelativeOrAbsolute);
      bitmap2.EndInit();
      bitmap2.Freeze(); // Freeze for performance benefits
      return bitmap2;
    }
  }
}
