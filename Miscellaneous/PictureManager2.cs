using log4net;
using Microsoft.VisualBasic.Logging;
using Sovoma;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml.Linq;
using ZusiKlassenLib2;
using ZusiKlassenLib2.Buchfahrplan;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Fahrplan;
using ZusiKlassenLib2.TimeTable;
using ZusiKlassenLib2.Vehicle;
using ZusiPicLib;
using ZusiStart.Data;
using ZusiStart.Miscellaneous;

namespace ZusiStart.Miscellaneous
{
  class PictureManager2
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

          float Winkel = 90;
          float Skalierung = (float)0.5;

          LS3RenderWrapper.ls3render_SetAxonometrieParameter(Winkel, Skalierung);


          // Enable multisampling
          LS3RenderWrapper.ls3render_SetMultisampling(4);

          //LS3RenderWrapper.ls3render_Reset();

          // Add a vehicle

          int result = LS3RenderWrapper.ls3render_AddFahrzeug(fahrzeug_dateiname, 50.0f / pixel_pro_meter, Fahrzeuglaenge, gedreht, 4.5f, 1, 0, 0, 0,0,0,0,0);
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

    public void render_fahrzeug2(string output_dateiname, float? blickwinkel = 0)
    {
      try
      {
        // Set resolution
        LS3RenderWrapper.ls3render_SetPixelProMeter(pixel_pro_meter);

        float Skalierung = (float)0.5;

        if (blickwinkel != null)
        {
          float blickwinkel_f = (float)blickwinkel;
          LS3RenderWrapper.ls3render_SetAxonometrieParameter(blickwinkel_f, Skalierung);
        }


        // Enable multisampling
        LS3RenderWrapper.ls3render_SetMultisampling(4);

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
      catch (Exception ex)
      {
        _log.Error("Error dll-access:" + ex.Message);
      }
    }

    float offset_x = 0.0f;

    public void add_Vehicle(Fahrzeug fzg, FahrzeugVariante fv, bool gedreht, int saSchaltung, int spitzenlicht, int schlusslicht, string cachepath, Window? popup_message = null, bool create_no_image = false, bool von_rechts_nach_links = true)
    {
      try
      {
        FahrzeugGrunddaten fzggd = fv.Grunddaten;

        string cachefilename2 = string.Format("{0}-{1}-{2}", fzg.Name, fv.IDHaupt, fv.IDNeben + (!gedreht ? "-r" : "-n")).ToLower();
        string cachefilepathname2 = cachepath + "\\" + cachefilename2 + ".png";
        //_log.Info("LS3_Render.DLL - get Image for:" + cachefilename2);

        if (popup_message != null)
          popup_message.Show();
        string Arbeitsverzeichnis = ZusiKlassenLib2.Zusi.DataPath[0];
        string cachefilename = string.Format("{0}-{1}-{2}", fzg.Name, fv.IDHaupt, fv.IDNeben).ToLower();
        string ls3_filename = fv.DateiAussenansicht.Dateiname;
        string ls3_filenamepath = Arbeitsverzeichnis + "\\" + ls3_filename;
        int gedreht_int = 1;
        if (!gedreht)
          gedreht_int = 0;

        _log.Debug("Generate Image for:" + cachefilename);
        //string filename;
        try
        {
          add_fahrzeug(ls3_filenamepath, cachefilepathname2, offset_x, (float)fzggd.Laenge, gedreht_int, saSchaltung, spitzenlicht, schlusslicht, von_rechts_nach_links: von_rechts_nach_links);
        }
        catch (Exception ex)
        {
          _log.Error("Error Generate Image for:" + cachefilename + " - " + ex.Message);
          return;
        }
        offset_x += (float)fzggd.Laenge;
        if (create_no_image)
        {
          return;
        }
      }

      catch (Exception ex)
      {
        _log.Error("Error Generate Image:" + ex.Message);
        return;
      }
    }

    public void init_renderEngine()
    {
      try
      {
        if (LS3RenderWrapper.ls3render_Init() == 1)
        {
          _log.Debug("Initialization successful!");
          // Set resolution
          LS3RenderWrapper.ls3render_SetPixelProMeter(pixel_pro_meter);

          LS3RenderWrapper.ls3render_Reset();
          // Enable multisampling 
          LS3RenderWrapper.ls3render_SetMultisampling(4);
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


    public void add_fahrzeug(string fahrzeug_dateiname, string output_dateiname, float offset_x, float Fahrzeuglaenge, int gedreht, int saSchaltung, int spitzenlicht, int schlusslicht, bool von_rechts_nach_links = true)
    {
      try
      {
        // Add a vehicle

        //  int result = LS3RenderWrapper.ls3render_AddFahrzeug(fahrzeug_dateiname, 50.0f / pixel_pro_meter, Fahrzeuglaenge, gedreht, 4.5f, 1, 0, 0, 0);
        int Pantograph1_0 = 0;
        int Pantograph1_1 = 0;
        int Pantograph1_2 = 0;
        int Pantograph1_3 = 0;
        int SpitzenlichtVorneAn = 0;
        int SpitzenlichtHintenAn = 0;
        int SchlusslichtVorneAn = 0;
        int SchlusslichtHintenAn = 0;

        if (saSchaltung != 0)
        {
          if (gedreht == 1)
          {
            Pantograph1_0 = ((saSchaltung & (1 << 0)) != 0) ? 1 : 0;
            Pantograph1_1 = ((saSchaltung & (1 << 1)) != 0) ? 1 : 0;
            Pantograph1_2 = ((saSchaltung & (1 << 2)) != 0) ? 1 : 0;
            Pantograph1_3 = ((saSchaltung & (1 << 3)) != 0) ? 1 : 0;
          }
          else
          {
            Pantograph1_1 = ((saSchaltung & (1 << 0)) != 0) ? 1 : 0;
            Pantograph1_0 = ((saSchaltung & (1 << 1)) != 0) ? 1 : 0;
            Pantograph1_3 = ((saSchaltung & (1 << 2)) != 0) ? 1 : 0;
            Pantograph1_2 = ((saSchaltung & (1 << 3)) != 0) ? 1 : 0;
          }
        }



        //if (gedreht == 0)
        //{
        //  if (von_rechts_nach_links)
        //    stromabnehmer2 = 1;
        //  else
        //    stromabnehmer1 = 1;
        //}
        //else
        //{
        //  if (von_rechts_nach_links)
        //    stromabnehmer1 = 1;
        //  else
        //    stromabnehmer2 = 1;
        //}
        if (spitzenlicht == 1)
        {
          if (gedreht == 1)
          {
            SpitzenlichtVorneAn = 1;
            //SpitzenlichtHintenAn = 1;
          }
          else
          {
            //SpitzenlichtVorneAn = 1;
            SpitzenlichtHintenAn = 1;
          }
        }
        if (schlusslicht == 1)
        {
          if (gedreht == 1)
          {
            SchlusslichtHintenAn = 1;
          }
          else
          {
            SchlusslichtVorneAn = 1;
          }

          
        }
        int result = LS3RenderWrapper.ls3render_AddFahrzeug(fahrzeug_dateiname, offset_x, Fahrzeuglaenge, gedreht, 4.5f, Pantograph1_0, Pantograph1_1, Pantograph1_2, Pantograph1_3, SpitzenlichtVorneAn,SpitzenlichtHintenAn,SchlusslichtVorneAn,SchlusslichtHintenAn);

        if (result == 1)
        {
          _log.Debug("Vehicle added successfully!");
        }

      }
      catch (Exception ex)
      {
        _log.Error("Error dll-access:" + ex.Message);
      }
    }

    public BitmapImage getPicture3(string filename, string cachepath, Window? popup_message = null, bool create_no_image = false, float? blickwinkel = 0, bool ignorecache = false)
    {
      try
      {
        //FahrzeugGrunddaten fzggd = fv.Grunddaten;
        //double width_d = height * width_factor;
        //int width = (int)Math.Round(width_d);
        //int vehiclewidth = (int)Math.Round(fzggd.Laenge * 16.55);
        //System.Drawing.Size picsize = new(width, height);

        //string cachefilename2 = string.Format("{0}-{1}-{2}", fzg.Name, fv.IDHaupt, fv.IDNeben + (!gedreht ? "-r" : "-n")).ToLower();
        string cachefilename2 = filename;
        string cachefilepathname2 = cachepath + "\\" + cachefilename2 + ".png";
        _log.Info("LS3_Render.DLL - get Image for:" + cachefilename2);
        if (!System.IO.File.Exists(cachefilepathname2) || ignorecache)
        {
          if (popup_message != null)
            popup_message.Show();
          string Arbeitsverzeichnis = ZusiKlassenLib2.Zusi.DataPath[0];
          //string cachefilename = string.Format("{0}-{1}-{2}", fzg.Name, fv.IDHaupt, fv.IDNeben).ToLower();
          string cachefilename = filename;
          //string ls3_filename = fv.DateiAussenansicht.Dateiname;
          string ls3_filename = filename;
          string ls3_filenamepath = Arbeitsverzeichnis + "\\" + ls3_filename;

          _log.Debug("Generate Image for:" + cachefilename);
          //string filename;
          try
          {
            render_fahrzeug2(cachefilepathname2, blickwinkel: blickwinkel);
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
        //_log.Info("LS3_Render.DLL - get Image for:" + cachefilename2);
        if (!System.IO.File.Exists(cachefilepathname2))
        {
          if (popup_message != null)
            popup_message.Show();
          string Arbeitsverzeichnis = ZusiKlassenLib2.Zusi.DataPath[0];
          string cachefilename = string.Format("{0}-{1}-{2}", fzg.Name, fv.IDHaupt, fv.IDNeben).ToLower();
          string ls3_filename = fv.DateiAussenansicht.Dateiname;
          string ls3_filenamepath = Arbeitsverzeichnis + "\\" + ls3_filename;
          int gedreht_int = 1;
          if (!gedreht)
            gedreht_int = 0;

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

    //---------------------------------------------------------------------
    public BitmapImage AssembleTrain(Zug zug, Window? dummywindow, bool zugrichtung_von_links_nach_rechts)
    {
      PictureManager2 pictureManager = new PictureManager2();

      ZusiDocumentBase doc = zug.GetDocument();
      _log.DebugFormat("assemble train: {0}", doc.Filename);

      ZugReihung zr = new(zug);
      zr.BuildTrain();

      int TrainLength = 0; // Math.Round(zr.Length, 0);
      double TrainMass = Math.Round(zr.Mass * 0.001);
      //string cachepath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ZusiStart\\cache";
      string cachepath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DataManager.localfoldername, "cache2");
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
          _log.Debug("Cache Directory created:" + cachepath);
        }
        catch
        {
          _log.Error("ERROR: Cache Directory cannot be created:" + cachepath);
        }
      }


      int Fahrzeug_num = 0;

      bool background1 = false;

      SolidColorBrush myBrush1 = new SolidColorBrush(Colors.Blue);
      myBrush1.Opacity = 0.5; // Set to 50% opacity
      SolidColorBrush myBrush2 = new SolidColorBrush(Colors.Red);
      myBrush2.Opacity = 0.5; // Set to 50% opacity
      //dummywindow.VehicleProgressBar.Value = 0;

      pictureManager.init_renderEngine();
      string blickwinkel_str = "";
      string cachefilename = "S";

      if (DataManager.Instance.options.New_RenderEngine)
      {
        blickwinkel_str = DataManager.Instance.options.Blickwinkel_value;


        cachefilename = "W" + blickwinkel_str;
      }

      int spitzenlicht = 0;
      int schlusslicht = 0;

      LinkedListNode<FahrzeugInfo> p = zr.First;

      if (zugrichtung_von_links_nach_rechts)
      {
        p = zr.Last; // start with last vehicle
        schlusslicht = 1;
      }
      else
      {
        spitzenlicht = 1; // erstes Fahrzeug hat das Spitzenlicht angeschaltet
      }
        

      while (p != null)
      {
        Fahrzeug fzg = p.Value.Fahrzeug;
        FahrzeugVariante fv = fzg?.GetVariante(p.Value.IDHaupt, p.Value.IDNeben, p.Value.VariantenIndex) ?? null;

        if (zugrichtung_von_links_nach_rechts)
        {
          if (p.Previous == null)
          {
            spitzenlicht = 1; // erstes Fahrzeug hat das Spitzenlicht angeschaltet
          }
        }
        else
        {
          if (p.Next == null)
          {
            schlusslicht = 1; // letztes Fahrzeug hat das Schlusslicht angeschaltet
          }
        }
        if (fv != null)
        {
          Grid grd = new();
          Grid smallgrd = new();
          FahrzeugGrunddaten fzggd = fv.Grunddaten;

          bool gedreht = p.Value.Gedreht;
          int saSchaltung = p.Value.SASchaltung;

          if (zugrichtung_von_links_nach_rechts)
          {
            gedreht = !gedreht;
          }

          Fahrzeug_num++;
          pictureManager.add_Vehicle(fzg, fv, gedreht, saSchaltung, spitzenlicht, schlusslicht, cachepath, dummywindow, von_rechts_nach_links: !zugrichtung_von_links_nach_rechts);
          spitzenlicht = 0; // nur erstes Fahrzeug hat das Spitzenlicht angeschaltet
          schlusslicht = 0;
          if (Fahrzeug_num <= 10)
          {
            cachefilename += string.Format("{0}-{1}-{2}", fzg.Name, fv.IDHaupt, fv.IDNeben).ToLower();
          }

          //if (fzggd != null)
          //{
          //  //System.Windows.Controls.Image image = new() { Source = imagesource };
          //  //grd.Children.Add(image);
          //  //System.Windows.Controls.Image smallimage = new() { Source = imagesource, Height = 20, Stretch = System.Windows.Media.Stretch.Uniform };
          //  //smallgrd.Children.Add(smallimage);

          //  Thickness orig_thickness = pictureManager.Calculate_Margin(imagesource, fzggd.Laenge, p.Value.Gedreht);
          //grd.Margin = pictureManager.Calculate_Margin(imagesource, fzggd.Laenge, p.Value.Gedreht);

          //  double factor = 20.0 / imagesource.Height;
          //  smallgrd.Margin = new Thickness(grd.Margin.Left * factor, grd.Margin.Top * factor, grd.Margin.Right * factor, grd.Margin.Bottom * factor);

          //  factor = 110.0445 / imagesource.Height;
          //  grd.Margin = new Thickness(grd.Margin.Left * factor, grd.Margin.Top * factor, grd.Margin.Right * factor, grd.Margin.Bottom * factor);

          //  background1 = !background1;
          //  string br = fv.BR.StartsWithNumber() ? $"BR {fv.BR}" : fv.BR;
          //  if (p.Value.Gedreht)
          //    br = br + "-r";
          //  double front_margin = grd.Margin.Left;
          //  double rear_margin = grd.Margin.Right;

          //  TextBlock txt = new()
          //  {
          //    Text = br,
          //    Foreground = Brushes.Black,
          //    //Background = Brushes.Gray,
          //    FontStyle = FontStyles.Italic,
          //    // *test* FontSize = 8,
          //    HorizontalAlignment = HorizontalAlignment.Center,
          //    VerticalAlignment = VerticalAlignment.Bottom,
          //    Margin = new Thickness(-front_margin, 0, -rear_margin, -12)
          //  };

          //  Panel.SetZIndex(txt, 7);
          //  grd.Children.Add(txt);

          //  //_picsPanel.Children.Insert(0, grd);
          //  //_smallpicsPanel.Children.Insert(0, smallgrd);
          //}
        }
        if (zugrichtung_von_links_nach_rechts)
        {
          p = p.Previous; // weiter mit dem vorherigen Fahrzeug
        }
        else
        {
          p = p.Next;
        }
      }

      Grid grd1 = new();
      Grid smallgrd1 = new();
      string filename = "";

      if (cachefilename.Length < 190)
      {
        filename = cachefilename;
      }
      else
      {
        filename = cachefilename.Substring(0, 190) + zug.Gattung + zug.Nummer;
      }

      float? blickwinkel = null;

      if (DataManager.Instance.options.New_RenderEngine)
      {

        if (float.TryParse(blickwinkel_str, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
          //Console.WriteLine($"Erfolgreich: {result}");
          blickwinkel = result * 0.01745329252f;
        }
        else
        {
          blickwinkel = 0;
        }

      }
      bool ignorecache = false;
      if (DataManager.Instance.CurrentTrainItem != null)
      {
        ignorecache = DataManager.Instance.CurrentTrainItem.IsLocoReplaced || DataManager.Instance.CurrentTrainItem.IsLocoReplaced;
        ignorecache = true;
        if (ignorecache)
        {
          filename = "tmp_image";
        }
      }

      BitmapImage imagesource = pictureManager.getPicture3(filename, cachepath, dummywindow, blickwinkel: blickwinkel, ignorecache: ignorecache);

      return imagesource;
    }
  }
}
