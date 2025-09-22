using Sovoma;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib;
using ZusiKlassenLib.Cab;
using ZusiKlassenLib.Common;
using ZusiKlassenLib.Fahrplan;
using ZusiKlassenLib.Vehicle;

namespace ZusiStart.Data
{
  public class ReplacementLocoException : Exception
  {
    public ReplacementLocoException(string name, string filename)
        : base($"Für die Ersatzlok '{name} konnte die Fahrzeugdatei '{filename}' nicht gefunden werden.")
    { }
  }

  public class FahrzeugInfoParameter2
  {
    public IZusiObjectParent? Parent { get; set; }
    public Zugart Zugart { get; set; }
    public Bremsstellung Bremsstellung { get; set; }
    public int Bremshundertstel { get; set; }
    public DotraMode DotraMode { get; set; }
    public bool IgnoreDoors { get; set; }
    public bool Gedreht { get; set; }
    public int SASchaltung { get; set; }
    public bool Zugrichtung { get; set; }
  }

  public class ReplacementLoco : DependencyObject
  {
    private readonly ObservableCollection<VehicleVariant> _variants = new();
    private readonly bool _assignVariant;
    private bool _dirty;
    private readonly bool _ignoreChanges;

    public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
        "Name",
        typeof(string),
        typeof(ReplacementLoco),
        new PropertyMetadata("<unbenannt>", MakeDirty));
    public string Name
    {
      get => (string)GetValue(NameProperty);
      set => SetValue(NameProperty, value);
    }

    public static readonly DependencyProperty SelectedVariantProperty = DependencyProperty.Register(
        "SelectedVariant",
        typeof(VehicleVariant),
        typeof(ReplacementLoco),
        new PropertyMetadata(null, MakeDirty));
    public VehicleVariant? SelectedVariant
    {
      get => (VehicleVariant)GetValue(SelectedVariantProperty);
      set => SetValue(SelectedVariantProperty, value);
    }

    public static readonly DependencyProperty VehicleFileProperty = DependencyProperty.Register(
        "VehicleFile",
        typeof(string),
        typeof(ReplacementLoco),
        new PropertyMetadata(null, OnVehicleFileChanged));
    public string VehicleFile
    {
      get => (string)GetValue(VehicleFileProperty);
      set => SetValue(VehicleFileProperty, value);
    }

    public bool IsDirty => _dirty;

    public bool IsValid => GetIsValid();

    public ObservableCollection<VehicleVariant> Variants => _variants;

    public ReplacementLoco()
    {
      Name = "<unbenannt>";
      _assignVariant = true;
      _dirty = true;
    }

    public ReplacementLoco(XElement x)
    {
      Name = x.GetAttrValue("name", "<unbenannt>");

      string vehicleFile = Zusi.DataPath[0] + x.GetAttrValue("file", "");
      if (!File.Exists(vehicleFile))
      {
        vehicleFile = Zusi.DataPath[2] + x.GetAttrValue("file", "");
        if (!File.Exists(vehicleFile))
        {
          throw new ReplacementLocoException(Name, vehicleFile);
        }
      }
      VehicleFile = vehicleFile;

      string variant = x.GetAttrValue("variant", "");
      foreach (VehicleVariant v in _variants)
      {
        string vName = v.Name;
        if (vName != null)
        {
          if (string.Compare(variant, vName) == 0)
          {
            SelectedVariant = v;
            break;
          }
        }
        else
        {
          if (string.Compare(variant, v.Description, true) == 0)
          {
            SelectedVariant = v;
            break;
          }
        }
      }

      _assignVariant = true;
      _dirty = false;
    }

    public ReplacementLoco(ReplacementLoco source)
    {
      _ignoreChanges = true;

      try
      {
        foreach (VehicleVariant v in source._variants)
        {
          _variants.Add(v);
        }

        Name = source.Name;
        SelectedVariant = source.SelectedVariant;
        VehicleFile = source.VehicleFile;

        _assignVariant = true;
        _dirty = false;
      }
      finally
      {
        _ignoreChanges = false;
      }
    }

    public FahrzeugInfo? GetFahrzeugInfo(FahrzeugInfoParameter parameter)
    {
      if (SelectedVariant != null)
      {
        DataPathType dtp = DataPathType.Unknown;
        XElement x = new("FahrzeugInfo",
            new XElement("Datei", new XAttribute("Dateiname", Zusi.GetRelativePathOf(VehicleFile, ref dtp))));
        if (SelectedVariant.IDHaupt > 0 && SelectedVariant.IDNeben > 0)
        {
          x.Add(new XAttribute("IDHaupt", SelectedVariant.IDHaupt));
          x.Add(new XAttribute("IDNeben", SelectedVariant.IDNeben));
        }
        else
        {
          x.Add(new XAttribute("VariantenIndex", _variants.IndexOf(SelectedVariant)));
        }

        if (parameter.Zugart > Zugart.None)
        {
          string nodeName = null;
          for (int j = 0; j < SelectedVariant.Indusis.Count; j++)
          {
            Indusi i = SelectedVariant.Indusis[j];
            switch (i.ZugdatenType)
            {
              case ZugdatenType.IndusiAnalog:
                nodeName = "ZugdatenIndusiAnalog";
                break;
              case ZugdatenType.IndusiRechner:
                nodeName = "ZugdatenIndusiRechner";
                break;
              case ZugdatenType.PZ80:
                nodeName = "ZugdatenPZ80";
                break;
              case ZugdatenType.LZB80:
                nodeName = "ZugdatenLZB80";
                break;
            }
            if (nodeName != null)
              break;
          }
          if (nodeName != null)
          {
            XElement xZugdaten = new(nodeName);
            if (nodeName != "ZugdatenPZ80")
            {
              xZugdaten.Add(new XAttribute("ZugsicherungHS", 2),
                  new XAttribute("Lufthahn", 2),
                  new XAttribute("PZBStoerschalter", 2));
            }
            if (nodeName != "ZugdatenIndusiRechner")
            {
              xZugdaten.Add(new XAttribute("IndusiZugart", (int)parameter.Zugart));
            }
            if (nodeName == "ZugdatenIndusiRechner" || nodeName == "ZugdatenLZB80")
            {
              xZugdaten.Add(new XAttribute("BRA", (int)parameter.Zugart));
              if (parameter.Bremshundertstel > 0)
              {
                xZugdaten.Add(new XAttribute("BRH", parameter.Bremshundertstel));
              }
            }
            x.Add(xZugdaten);
            x.Add(new XAttribute("EigeneZugart", 1));
          }
        }

        if (parameter.DotraMode != DotraMode.Default)
        {
          x.Add(new XAttribute("DotraModus", (int)parameter.DotraMode));
        }

        if (parameter.IgnoreDoors)
        {
          x.Add(new XAttribute("Tuerignorieren", 1));
        }

        if (parameter.Gedreht)
        {
          x.Add(new XAttribute("Gedreht", 1));
        }

        if (parameter.Bremsstellung > Bremsstellung.Unknown)
        {
          x.Add(new XAttribute("EigeneBremsstellung", 1));
          x.Add(new XAttribute("BremsstellungFahrzeug", (int)parameter.Bremsstellung));
        }

        if (parameter.SASchaltung > 0)
        {
          x.Add(new XAttribute("SASchaltung", (int)parameter.SASchaltung));
        }

        return new FahrzeugInfo(parameter.Parent, x);
      }

      return null;
    }

    public void Save(XmlWriter writer)
    {
      writer.WriteStartElement("loco");
      writer.WriteAttribute("name", Name);
      DataPathType dtp = DataPathType.Unknown;
      writer.WriteAttribute("file", Zusi.GetRelativePathOf(VehicleFile, ref dtp));
      string variant = SelectedVariant.Name;
      if (variant == null)
      {
        variant = SelectedVariant.Description;
      }
      writer.WriteAttribute("variant", variant);
      writer.WriteEndElement();
    }

    private static void OnVehicleFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as ReplacementLoco)?.OnVehicleFileChanged((string)e.NewValue);
    }

    private void OnVehicleFileChanged(string value)
    {
      if (_ignoreChanges)
        return;

      SelectedVariant = null;

      if (!string.IsNullOrEmpty(value) && File.Exists(value))
      {
        try
        {
          FahrzeugDatei fd = new(value);
          fd.Parse();
          Fahrzeug f = fd.Root;

          foreach (VehicleVariant vv in f.Varianten.Select(fv => new VehicleVariant(fv)))
          {
            _variants.Add(vv);
          }

          if (_assignVariant && _variants.Count > 0)
          {
            SelectedVariant = _variants[0];
          }
        }
        catch
        {

        }
      }

      SetDirty(true);
    }

    private static void MakeDirty(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as ReplacementLoco)?.SetDirty(true);
    }

    private void SetDirty(bool value)
    {
      if (!_ignoreChanges)
      {
        _dirty |= value;
      }
    }

    private bool GetIsValid()
    {
      return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(VehicleFile) && File.Exists(VehicleFile) && SelectedVariant != null;
    }
  }
}
