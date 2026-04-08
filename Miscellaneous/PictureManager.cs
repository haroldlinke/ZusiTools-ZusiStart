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
using ZusiKlassenLib2;
using ZusiKlassenLib2.Buchfahrplan;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.TimeTable;
using ZusiPicLib;
using ZusiKlassenLib2.Vehicle;
using ZusiKlassenLib2.Fahrplan;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Xml.Linq;
using System.Drawing;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.InteropServices;
using ZusiStart.Miscellaneous;
using System.Drawing.Imaging;
using ZusiStart.Data;

namespace ZusiStart.Miscellaneous
{
  class PictureManager
  {
    private static readonly ILog _log = LogManager.GetLogger(typeof(PictureManager));
    // calculate width
    int height = 100;
    double width_factor = 5.5; // emprisch ermittel

    int pixel_pro_meter = 20;   // Pixel pro Meter für Renderinggröße
    double fahrzeug_höhe = 5.5; // Höhe 5,5m

    public Thickness Calculate_Margin2(BitmapImage imagesource, double fahrzeuglänge, bool rotated)
    {
      if (imagesource != null)
      {
        double imagesize_factor = DataManager.Instance.ScreenScaleFactor;
        double front_margin = 0.0;
        double rear_margin = 0.0;
        //int image_height = (int)Math.Round(imagesource.Height * imagesize_factor);
        double width_d = (int)Math.Round(imagesource.Width * imagesize_factor);
        int image_width = (int)Math.Round(width_d);
        int vehiclewidth = (int)Math.Round(fahrzeuglänge * pixel_pro_meter);

        double front_gap = 50;
        double rear_gap = image_width - (front_gap + vehiclewidth);
        if (rear_gap < 0)
        {
          rear_gap = 0;
        }
        double margin_factor = -1.0 / DataManager.Instance.ScreenScaleFactor;
        front_margin = margin_factor * front_gap;
        rear_margin = margin_factor * rear_gap;
        return new Thickness(front_margin, 0, rear_margin, 12);
      }
      else
      {
        return new Thickness(0, 0, 0, 0);
      }
    }

    public Thickness Calculate_Margin(BitmapImage imagesource, double fahrzeuglänge, bool rotated)
    {
      if (Properties.Settings.Default.Use_LS3_Renderer_DLL == 1)
      {
        return Calculate_Margin2(imagesource, fahrzeuglänge, rotated);
      }
      double imagesize_factor = 3.0 / 2.0;
      double length_factor = (16.55 / 100); //empirisch ermittelt bei erzeugter Bildhöhe 100
      double front_margin = 0.0;
      double rear_margin = 0.0;
      int image_height = (int)Math.Round(imagesource.Height * imagesize_factor);
      double width_d = (int)Math.Round(imagesource.Width * imagesize_factor);
      int image_width = (int)Math.Round(width_d);
      int vehiclewidth = (int)Math.Round(fahrzeuglänge * length_factor * image_height);

      double front_gap = image_height / 2.0;
      double rear_gap = image_width - (front_gap + vehiclewidth);
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

    public void render_fahrzeug(string fahrzeug_dateiname, string output_dateiname, float Fahrzeuglaenge, int gedreht)
    {
      try
      {
        if (LS3RenderWrapper.ls3render_Init() == 1)
        {
          _log.Debug("Initialization successful!");

          // Set resolution
          LS3RenderWrapper.ls3render_SetPixelProMeter(pixel_pro_meter);

          //float Winkel = 90;
          //float Skalierung = (float) 0.5;

          //LS3RenderWrapper.ls3render_SetAxonometrieParameter(Winkel, Skalierung);


          // Enable multisampling
          LS3RenderWrapper.ls3render_SetMultisampling(4);

          LS3RenderWrapper.ls3render_Reset();

          // Add a vehicle

          int result = LS3RenderWrapper.ls3render_AddFahrzeug(fahrzeug_dateiname, 50.0f/pixel_pro_meter, Fahrzeuglaenge, gedreht, 4.5f, 1, 0, 0, 0,0,0,0,0);
          if (result == 1)
          {
            _log.Debug("Vehicle added successfully!");
          }

          // Render the scene
          int bufferSize = LS3RenderWrapper.ls3render_GetAusgabepufferGroesse();
          IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
          if (LS3RenderWrapper.ls3render_Render(buffer) == 1)
          {
            _log.Debug("Rendering successful!");

            // Get image dimensions
            int width = LS3RenderWrapper.ls3render_GetBildbreite();
            int height = LS3RenderWrapper.ls3render_GetBildhoehe();

            // Create a bitmap and copy the buffer data into it
            using (Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
              BitmapData bitmapData = bitmap.LockBits(
                  new Rectangle(0, 0, width, height),
                  ImageLockMode.WriteOnly,
                  System.Drawing.Imaging.PixelFormat.Format32bppArgb);

              // Copy data from buffer to bitmap
              byte[] data = new byte[bufferSize];
              Marshal.Copy(buffer, data, 0, bufferSize);
              Marshal.Copy(data, 0, bitmapData.Scan0, bufferSize);

              bitmap.UnlockBits(bitmapData);

              // Save the bitmap as a PNG file
              bitmap.Save(output_dateiname, ImageFormat.Png);
            }
          }
          Marshal.FreeHGlobal(buffer);

          // Cleanup
          LS3RenderWrapper.ls3render_Cleanup();
        }
        else
        {
          _log.Error("Initialization failed!");
        }
      }
      catch (Exception ex)
      {
        _log.Error("Error dll-access:" + ex.Message);
      }
    }

    public BitmapImage getPicture2(Fahrzeug fzg, FahrzeugVariante fv, bool gedreht, string cachepath, Window? popup_message = null, bool create_no_image = false)
    {
      try
      {
        FahrzeugGrunddaten fzggd = fv.Grunddaten;
        //double width_d = height * width_factor;
        //int width = (int)Math.Round(width_d);
        //int vehiclewidth = (int)Math.Round(fzggd.Laenge * 16.55);
        //System.Drawing.Size picsize = new(width, height);

        string cachefilename2 = string.Format("{0}-{1}-{2}", fzg.Name, fv.IDHaupt, fv.IDNeben + (!gedreht ? "-r" : "-n")).ToLower();
        string cachefilepathname2 = cachepath + "\\" + cachefilename2 + ".png";
        _log.Debug("LS3_Render.DLL - get Image for:" + cachefilename2);
        if (!System.IO.File.Exists(cachefilepathname2))
        {
          if (popup_message != null)
            popup_message.Show();
          string Arbeitsverzeichnis = ZusiKlassenLib2.Zusi.DataPath[0];
          string cachefilename = string.Format("{0}-{1}-{2}", fzg.Name, fv.IDHaupt, fv.IDNeben).ToLower();
          string ls3_filename = fv.DateiAussenansicht.Dateiname;
          string ls3_filenamepath = Arbeitsverzeichnis + "\\" + ls3_filename;
          int gedreht_int = 0;
          if (!gedreht)
            gedreht_int = 1;

          _log.Debug("Generate Image for:" + cachefilename);
          //string filename;
          try
          {
            render_fahrzeug(ls3_filenamepath, cachefilepathname2, (float)fzggd.Laenge, gedreht_int);
          }
          catch (Exception ex)
          {
            _log.Error("Error Generate Image for:" + cachefilename + " - " + ex.Message);
            return null;
          }

          if (System.IO.File.Exists(cachefilepathname2))
          {
            using (Bitmap bitmap = new Bitmap(cachefilepathname2))
            {
              // Define the color to make transparent (light grey)
              //System.Drawing.Color lightGrey = System.Drawing.Color.FromArgb(255, 166, 200, 255); // Adjust RGB values as needed
              // Make the specified color transparent
              //bitmap.MakeTransparent(lightGrey);

              //  Flip the bitmap vertically
              bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

              // Save the flipped bitmap to a new file
              bitmap.Save(cachefilepathname2, System.Drawing.Imaging.ImageFormat.Png);
            }
          }
          else
          {
            return null;
          }
        }

        if (create_no_image)
        {
          return null;
        }
        else
        {
          // Create a BitmapImage from a file
          BitmapImage bitmap2 = new BitmapImage();
          bitmap2.BeginInit();
          bitmap2.UriSource = new Uri(cachefilepathname2, UriKind.RelativeOrAbsolute);
          bitmap2.EndInit();
          bitmap2.Freeze(); // Freeze for performance benefits
          return bitmap2;
        }
      }
      catch (Exception ex)
      {
        _log.Error("Error Generate Image:" + ex.Message);
        return null;
      }
    }

    public BitmapImage getPicture(Fahrzeug fzg, FahrzeugVariante fv, bool gedreht, string cachepath, Window? popup_message = null, bool create_no_image = false)
    {
      
      if (Properties.Settings.Default.Use_LS3_Renderer_DLL == 1)
      {
        return getPicture2(fzg, fv, gedreht, cachepath, popup_message, create_no_image);
      }
      
      FahrzeugGrunddaten fzggd = fv.Grunddaten;
      double width_d = height * width_factor;
      int width = (int)Math.Round(width_d);
      int vehiclewidth = (int)Math.Round(fzggd.Laenge * 16.55);
      System.Drawing.Size picsize = new(width, height);

      string cachefilename2 = string.Format("{0}-{1}-{2}", fzg.Name, fv.IDHaupt, fv.IDNeben + (!gedreht ? "-r" : "-n")).ToLower();
      string cachefilepathname2 = cachepath + "\\" + cachefilename2 + ".png";
      _log.Info("Z3STRBIE.DLL - get Image for:" + cachefilename2);
      if (!System.IO.File.Exists(cachefilepathname2))
      {
        if (popup_message != null)
          popup_message.Show();
        string cachefilename = string.Format("{0}-{1}-{2}", fzg.Name, fv.IDHaupt, fv.IDNeben + (!gedreht ? "-s2" : "-s1")).ToLower();
        string ls3_filename = fv.DateiAussenansicht.Dateiname;

        string Arbeitsverzeichnis = ZusiKlassenLib2.Zusi.DataPath[0];
        string[] ZusiDataDirs = { Arbeitsverzeichnis };
        string DateiNameRelativ = ls3_filename;
        NativeMethods.ls3Ansicht mode = NativeMethods.ls3Ansicht.Seitenansicht;
        if (!gedreht)
        {
          mode = NativeMethods.ls3Ansicht.Seitenansicht2;
        }
        else
          _log.Debug("Generate Image for:" + cachefilename);
        string filename;
        try
        {
          filename = NativeMethods.ls3Vorschau(ZusiDataDirs, DateiNameRelativ, mode, picsize, cachefilename, cachepath);
        }
        catch (Exception ex)
        {
          _log.Error("Error Generate Image for:" + cachefilename + " - " + ex.Message);
          filename = "";
        }

        if (System.IO.File.Exists(filename))
        {
          using (Bitmap bitmap = new Bitmap(filename))
          {
            // Define the color to make transparent (light grey)
            System.Drawing.Color lightGrey = System.Drawing.Color.FromArgb(255, 166, 200, 255); // Adjust RGB values as needed
            // Make the specified color transparent
            bitmap.MakeTransparent(lightGrey);
            //if (!gedreht)
            //{
            //  // Flip the bitmap horizontally
            //  bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
            //}
            // Save the flipped bitmap to a new file
            bitmap.Save(cachefilepathname2, System.Drawing.Imaging.ImageFormat.Png);
          }
        }
        else
        {
          return null;
        }
      }

      if (create_no_image)
      {
        return null;
      }
      else
      {
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
}
