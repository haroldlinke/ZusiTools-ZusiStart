using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using ZusiKlassenLib.Buchfahrplan;
using ZusiKlassenLib.Common;
using ZusiKlassenLib.Vehicle;
//using ZusiPicLib;
using ZusiStart.Miscellaneous;
using ZusiPicLib;
using System.Windows.Media.Imaging;

namespace ZusiStart.Data
{
  public class VehicleContainer
  {
    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private readonly VehicleGroup _group;
    private string _displayName;
    private readonly Thickness[] _margins = new Thickness[3];
    private readonly ImageSource[] _sources = new ImageSource[3];
    private readonly Fahrzeug _vehicle;

    public string DisplayName { get => _displayName; }
    public VehicleGroup Group { get => _group; }
    public Thickness[] Margins { get => _margins; }
    public ImageSource[] Sources { get => _sources; }
    public Fahrzeug Vehicle { get => _vehicle; }

    //---------------------------------------------------------------------
    public VehicleContainer(DataManager data, VehicleGroup vgroup)
    {
      _group = vgroup;
      _vehicle = vgroup.Vehicle;

      AssembleDisplayName(vgroup.VClass);
      AssemblePictures(data, vgroup);
    }

    //---------------------------------------------------------------------
    private void AssembleDisplayName(string baseString)
    {
      try
      {
        if (!string.IsNullOrEmpty(baseString))
        {
          if (char.IsDigit(baseString[0]))
          {
            _displayName = "BR " + baseString;
          }
          else
          {
            _displayName = baseString;
          }
        }
        else
        {
          _displayName = "[" + _vehicle.Name + "]";
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
      }
    }

    //private Thickness Calculate_Margin(BitmapImage imagesource, double fahrzeuglänge, bool rotated)
    //{
    //  double length_factor = 16.55 / 100; //empirisch ermittelt bei erzeugetr Bildhöhe 100
    //  double front_margin = 0.0;
    //  double rear_margin = 0.0;
    //  int height = (int)Math.Round(imagesource.Height) * 3 / 2; //imagesource.PixelHeight;
    //  double width_d = (int)Math.Round(imagesource.Width) * 3 / 2; //imagesource.PixelWidth;
    //  int width = (int)Math.Round(width_d);
    //  int vehiclewidth = (int)Math.Round(fahrzeuglänge * length_factor * height);

    //  double front_gap = height / 2;
    //  double rear_gap = width - (front_gap + vehiclewidth);
    //  if (rear_gap < 0)
    //  {
    //    rear_gap = 0;
    //  }

    //  double margin_factor = -0.65;

    //  if (rotated)
    //  {
    //    front_margin = margin_factor * front_gap;
    //    rear_margin = margin_factor * rear_gap;
    //  }
    //  else // fahrzeug ist gedreht, vertausche front und rear margin
    //  {
    //    rear_margin = margin_factor * front_gap;
    //    front_margin = margin_factor * rear_gap;
    //  }

    //  return new Thickness(front_margin, 0, rear_margin, 12);
    //}


    //---------------------------------------------------------------------
    private void AssemblePictures(DataManager data, VehicleGroup vgroup)
    {
      try
      {
        PictureManager pictureManager = new PictureManager();
        //OverhangData overhangData;
        bool rotateFirst = false;

        for (int i = 1; i < 3; i++)
        {
          _sources[i] = null;
          _margins[i] = new Thickness();
        }
        string cachepath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ZusiStart\\cache";
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
        ClassFamily cf = ClassFamilies.Family(vgroup.VClass);
        BitmapImage image;
        if (cf != null)
        {
          Class vc = cf[vgroup.Variant.BR];
          if (vc != null)
          {
            int i = 1;
            foreach (Car c in vc.Cars)
            {
              var filtered = data.AllVariants.Where(v => v.BR == c.Name && v.IDHaupt == c.IDMajor && v.IDNeben == c.IDMinor)
                  .Where(v =>
                  {
                    Fahrzeug f = v.FindParent<Fahrzeug>();
                    return string.IsNullOrEmpty(c.Wagen) || c.Wagen == f.Wagen;
                  });
              if (filtered.Count() > 0)
              {
                FahrzeugVariante fv = filtered.ElementAt(0);
                Fahrzeug f = fv.FindParent<Fahrzeug>();
                bool rotated = c.Rotation;
                image = pictureManager.getPicture(f, fv, !rotated, cachepath);
                //if (overhangData != null)
                //{
                //  _margins[i] = overhangData.GetThickness(rotated);
                //}
                if (fv.Grunddaten != null)
                {
                  _margins[i] = pictureManager.Calculate_Margin(image, fv.Grunddaten.Laenge, !rotated);
                }
                _sources[i] = image;
              }
              i++;
            }

            rotateFirst = vc.Rotation;
          }
        }
        image = pictureManager.getPicture(_vehicle, vgroup.Variant, !rotateFirst, cachepath);
        _sources[0] = image;
        //if (overhangData != null)
        //{
        //  _margins[0] = overhangData.GetThickness(rotateFirst);
        //}
        if (_vehicle.Grunddaten != null)
        {
          _margins[0] = pictureManager.Calculate_Margin(image, _vehicle.Grunddaten.Laenge, !rotateFirst);
        }
        else
        {
          _margins[0] = pictureManager.Calculate_Margin(image, vgroup.Variant.Grunddaten.Laenge, !rotateFirst);
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex.ToString());
      }
    }
  }
}
