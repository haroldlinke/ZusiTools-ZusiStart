using log4net;
using Microsoft.Win32;
using System;
using System.IO;

namespace ZusiKlassenLib2
{
  public enum FrictionPreset
  {
    Dry,
    Wet,
    Misty,
    Icy
  }

  public static class ZusiSettings
  {
    private const string _zusiMaximized = "Maximiert";
    private const string _zusiFriction = "mueHaftreibungSchiene";

    private static readonly ILog Log = LogManager.GetLogger(typeof(ZusiSettings));

    private static readonly RegistryView _view = RegistryView.Registry64;
    private static readonly bool _failed;
    private static double _friction;
    private static readonly double _initialFriction;
    private static bool _maximized;

    //---------------------------------------------------------------------
    public static double Friction
    {
      get => _friction;
      set
      {
        if (_friction != value)
        {
          _friction = value;
          Save();
        }
      }
    }

    //---------------------------------------------------------------------
    public static bool IsMaximized => _maximized;

    //---------------------------------------------------------------------
    static ZusiSettings()
    {
      if (!ReadRegistry())
      {
        _view = RegistryView.Registry64;
        if (!ReadRegistry())
        {
          _failed = true;
          _friction = 0.4f;
        }
      }
      _initialFriction = _friction;
    }

    //---------------------------------------------------------------------
    public static void Reset()
    {
      Friction = _initialFriction;
    }

    //---------------------------------------------------------------------
    public static void SetFrictionPreset(FrictionPreset preset)
    {
      switch (preset)
      {
        case FrictionPreset.Dry:
          Friction = 0.4f;
          break;
        case FrictionPreset.Wet:
          Friction = 0.2f;
          break;
        case FrictionPreset.Misty:
          Friction = 0.1f;
          break;
        case FrictionPreset.Icy:
          Friction = 0.05f;
          break;
      }
    }

    //---------------------------------------------------------------------
    private static bool ReadRegistry()
    {
      bool res = false;

      try
      {
        using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, _view);
        using (RegistryKey k = baseKey.OpenSubKey(@"Software\Zusi3\Fahrsim", false))
        {
          if (k != null)
          {
            _maximized = Convert.ToBoolean(k.GetValue(_zusiMaximized));
          }
        }

        using (RegistryKey k = baseKey.OpenSubKey(@"Software\Zusi3\Fahrsim\Einstellungen", false))
        {
          if (k != null)
          {
            byte[] value = (byte[])k.GetValue(_zusiFriction);
            _friction = BitConverter.ToDouble(value, 0);
            res = true;
          }
        }
      }
      catch (Exception ex)
      {
        res = false;
        //Log.Fatal(ex.ToString());
      }
      if (!res)
      {
        try
        {
          using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, _view);
          using (RegistryKey k = baseKey.OpenSubKey(@"Software\Zusi3\Fahrsimsteam", false))
          {
            if (k != null)
            {
              _maximized = Convert.ToBoolean(k.GetValue(_zusiMaximized));
            }
          }

          using (RegistryKey k = baseKey.OpenSubKey(@"Software\Zusi3\Fahrsimsteam\Einstellungen", false))
          {
            if (k != null)
            {
              byte[] value = (byte[])k.GetValue(_zusiFriction);
              _friction = BitConverter.ToDouble(value, 0);
              res = true;
            }
          }
        }
        catch (Exception ex)
        {
          res = false;
          //Log.Fatal(ex.ToString());
        }
      }

      return res;
    }

    //---------------------------------------------------------------------
    private static void Save()
    {
      if (!_failed)
      {
        try
        {
          using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, _view);
          using RegistryKey k = baseKey.OpenSubKey(@"Software\Zusi3\Fahrsim\Einstellungen", true);
          if (k != null)
          {
            byte[] buffer = new byte[8];
            using (MemoryStream ms = new(buffer))
            {
              using BinaryWriter bw = new(ms);
              bw.Write(_friction);
            }
            k.SetValue(_zusiFriction, buffer);
          }
        }
        catch (Exception ex)
        {
          Log.Fatal(ex.ToString());
        }
      }
    }
  }
}
