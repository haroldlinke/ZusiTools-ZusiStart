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
  public class ReplacementTrainException : Exception
  {
    public ReplacementTrainException(string name, string filename)
        : base($"Für die Ersatzlok '{name} konnte die Fahrzeugdatei '{filename}' nicht gefunden werden.")
    { }
  }

  public class FahrzeugInfoParameter
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
    public bool Zuggedreht { get; set; }
  }

  public class ReplacementTrain : DependencyObject
  {
   
    private readonly bool _assignVariant;
    private bool _dirty;
    private readonly bool _ignoreChanges;

    public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
        "Name",
        typeof(string),
        typeof(ReplacementTrain),
        new PropertyMetadata("<unbenannt>", MakeDirty));
    public string Name
    {
      get => (string)GetValue(NameProperty);
      set => SetValue(NameProperty, value);
    }

    public static readonly DependencyProperty SelectedZDProperty = DependencyProperty.Register(
        "SelectedZD",
        typeof(ZugDatei),
        typeof(ReplacementTrain),
        new PropertyMetadata(null, MakeDirty));
    public ZugDatei? SelectedZD
    {
      get => (ZugDatei)GetValue(SelectedZDProperty);
      set => SetValue(SelectedZDProperty, value);
    }

    public static readonly DependencyProperty SelectedZugProperty = DependencyProperty.Register(
        "SelectedZug",
        typeof(Zug),
        typeof(ReplacementTrain),
        new PropertyMetadata(null, MakeDirty));
    public Zug? SelectedZug
    {
      get => (Zug)GetValue(SelectedZugProperty);
      set => SetValue(SelectedZugProperty, value);
    }

    public static readonly DependencyProperty VehicleTFileProperty = DependencyProperty.Register(
        "VehicleTFile",
        typeof(string),
        typeof(ReplacementTrain),
        new PropertyMetadata(null, OnVehicleTFileChanged));
    public string VehicleTFile
    {
      get => (string)GetValue(VehicleTFileProperty);
      set => SetValue(VehicleTFileProperty, value);
    }

    public static readonly DependencyProperty VehicleListProperty = DependencyProperty.Register(
        "VehicleList",
        typeof(ZugReihung),
        typeof(ReplacementTrain),
        new PropertyMetadata(null));
    public ZugReihung VehicleList
    {
      get => (ZugReihung)GetValue(VehicleListProperty);
      set => SetValue(VehicleListProperty, value);
    }

    public bool IsDirty => _dirty;

    public bool IsValid => GetIsValid();

    public ReplacementTrain()
    {
      Name = "<unbenannt>";
      _assignVariant = true;
      _dirty = true;
    }

    public ReplacementTrain(XElement x)
    {
      Name = x.GetAttrValue("name", "<unbenannt>");

      string vehicleFile = Zusi.DataPath[0] + x.GetAttrValue("file", "");
      if (!File.Exists(vehicleFile))
      {
        vehicleFile = Zusi.DataPath[2] + x.GetAttrValue("file", "");
        if (!File.Exists(vehicleFile))
        {
          throw new ReplacementTrainException(Name, vehicleFile);
        }
      }
      VehicleTFile = vehicleFile;

      _assignVariant = true;
      _dirty = false;
    }

    public ReplacementTrain(ReplacementTrain source)
    {
      _ignoreChanges = false; // true;

      try
      {

        Name = source.Name;
        //SelectedVariant = source.SelectedVariant;
        VehicleTFile = source.VehicleTFile;

        _assignVariant = true;
        _dirty = false;
      }
      finally
      {
        _ignoreChanges = false;
      }
    }

    //public FahrzeugInfo? GetFahrzeugInfo(FahrzeugInfoParameter parameter)
    //{
    //  if (true) //(SelectedVariant != null)
    //  {
    //    DataPathType dtp = DataPathType.Unknown;
    //    XElement x = new("FahrzeugInfo",
    //        new XElement("Datei", new XAttribute("Dateiname", Zusi.GetRelativePathOf(VehicleTFile, ref dtp))));
    //    //if (SelectedVariant.IDHaupt > 0 && SelectedVariant.IDNeben > 0)
    //    //{
    //    //  x.Add(new XAttribute("IDHaupt", SelectedVariant.IDHaupt));
    //    //  x.Add(new XAttribute("IDNeben", SelectedVariant.IDNeben));
    //    //}
    //    //else
    //    //{
    //    //  x.Add(new XAttribute("VariantenIndex", _variants.IndexOf(SelectedVariant)));
    //    //}

    //    if (parameter.Zugart > Zugart.None)
    //    {
    //      string nodeName = null;
    //      for (int j = 0; j < SelectedVariant.Indusis.Count; j++)
    //      {
    //        Indusi i = SelectedVariant.Indusis[j];
    //        switch (i.ZugdatenType)
    //        {
    //          case ZugdatenType.IndusiAnalog:
    //            nodeName = "ZugdatenIndusiAnalog";
    //            break;
    //          case ZugdatenType.IndusiRechner:
    //            nodeName = "ZugdatenIndusiRechner";
    //            break;
    //          case ZugdatenType.PZ80:
    //            nodeName = "ZugdatenPZ80";
    //            break;
    //          case ZugdatenType.LZB80:
    //            nodeName = "ZugdatenLZB80";
    //            break;
    //        }
    //        if (nodeName != null)
    //          break;
    //      }
    //      if (nodeName != null)
    //      {
    //        XElement xZugdaten = new(nodeName);
    //        if (nodeName != "ZugdatenPZ80")
    //        {
    //          xZugdaten.Add(new XAttribute("ZugsicherungHS", 2),
    //              new XAttribute("Lufthahn", 2),
    //              new XAttribute("PZBStoerschalter", 2));
    //        }
    //        if (nodeName != "ZugdatenIndusiRechner")
    //        {
    //          xZugdaten.Add(new XAttribute("IndusiZugart", (int)parameter.Zugart));
    //        }
    //        if (nodeName == "ZugdatenIndusiRechner" || nodeName == "ZugdatenLZB80")
    //        {
    //          xZugdaten.Add(new XAttribute("BRA", (int)parameter.Zugart));
    //          if (parameter.Bremshundertstel > 0)
    //          {
    //            xZugdaten.Add(new XAttribute("BRH", parameter.Bremshundertstel));
    //          }
    //        }
    //        x.Add(xZugdaten);
    //        x.Add(new XAttribute("EigeneZugart", 1));
    //      }
    //    }

    //    if (parameter.DotraMode != DotraMode.Default)
    //    {
    //      x.Add(new XAttribute("DotraModus", (int)parameter.DotraMode));
    //    }

    //    if (parameter.IgnoreDoors)
    //    {
    //      x.Add(new XAttribute("Tuerignorieren", 1));
    //    }

    //    if (parameter.Gedreht)
    //    {
    //      x.Add(new XAttribute("Gedreht", 1));
    //    }

    //    if (parameter.Bremsstellung > Bremsstellung.Unknown)
    //    {
    //      x.Add(new XAttribute("EigeneBremsstellung", 1));
    //      x.Add(new XAttribute("BremsstellungFahrzeug", (int)parameter.Bremsstellung));
    //    }

    //    if (parameter.SASchaltung > 0)
    //    {
    //      x.Add(new XAttribute("SASchaltung", (int)parameter.SASchaltung));
    //    }

    //    return new FahrzeugInfo(parameter.Parent, x);
    //  }

    //  return null;
    //}

    public ZugReihung? GetZugReihung()
    {
      ZugReihung? zr = new ZugReihung(SelectedZD.Root);
      zr.BuildTrain();
      return zr;
     
    }

    public void Save(XmlWriter writer)
    {
      writer.WriteStartElement("train");
      writer.WriteAttribute("name", Name);
      DataPathType dtp = DataPathType.Unknown;
      writer.WriteAttribute("file", Zusi.GetRelativePathOf(VehicleTFile, ref dtp));
      writer.WriteEndElement();
    }

    private static void OnVehicleTFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as ReplacementTrain)?.OnVehicleTFileChanged((string)e.NewValue);
    }

    private void OnVehicleTFileChanged(string value)
    {
      if (_ignoreChanges)
        return;

      SelectedZD = null;

      if (!string.IsNullOrEmpty(value) && File.Exists(value))
      {
        try
        {
          IZusiObjectParent parent = null;
         
          ZugDatei zd = new(parent,value);
          zd.Parse();
          SelectedZD = zd;
          VehicleList = GetZugReihung();
          SelectedZug = zd.Root;
        }
        catch
        {

        }
      }

      SetDirty(true);
    }

    private static void MakeDirty(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as ReplacementTrain)?.SetDirty(true);
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
      return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(VehicleTFile) && File.Exists(VehicleTFile);
    }
  }
}
