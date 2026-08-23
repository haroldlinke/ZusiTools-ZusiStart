using log4net;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Vehicle;

namespace ZusiKlassenLib2.Fahrplan
{
  //=========================================================================
  [Serializable]
  public class ZugReihung : LinkedList<FahrzeugInfo>, IZusiObject//, INotifyPropertyChanged
  {
    private static readonly ILog Log = LogManager.GetLogger(typeof(ZugReihung));

    private readonly List<ITrainAssembly> _vehicles = new();
    private readonly IZusiObject _source;
    private string _nodeName;
    private bool _dirty;
    private double _length;
    private double _mass;

    //---------------------------------------------------------------------
    public bool IsDirty
    {
      get => _dirty;
      set => _dirty = value;
    }

    public string NodeName
    {
      get => _nodeName;
      set => _nodeName = value;
    }

    public double Length => _length;
    public double Mass => _mass;

    //public event PropertyChangedEventHandler PropertyChanged;

    //---------------------------------------------------------------------
    public ZugReihung(IZusiObjectParent _/*parent*/, XElement x)
    {
      FahrzeugVarianten fvs = new(null, x);
      fvs.CollectVehicles(_vehicles);
      _source = fvs;
    }

    //---------------------------------------------------------------------
    public ZugReihung(Zug zug)
    {
      if (zug != null)
      {
        zug.Fahrzeuge.CollectVehicles(_vehicles);
        _source = zug;
      }
    }

    //---------------------------------------------------------------------
    public void BuildTrain()
    {
      _vehicles.ForEach(v => v.BuildTrain(this));

      var p = First;
      while (p != null)
      {
        Fahrzeug fzg = p.Value.Fahrzeug;
        if (fzg != null)
        {
          FahrzeugVariante fv = fzg.GetVariante(p.Value.IDHaupt, p.Value.IDNeben, p.Value.VariantenIndex);
          if (fv == null)
          {
            Log.WarnFormat("Vehicle {0} doesn't have neither variant {1}.{2} nor index {3}", fzg.Name, p.Value.IDHaupt, p.Value.IDNeben, p.Value.VariantenIndex);
            fv = fzg.GetVariante(1, 1, 1);
          }
          if (fv == null)
          {
            Log.WarnFormat("Vehicle {0} doesn't have default variant 1.1 index 1", fzg.Name);
          }
          else
          {
            FahrzeugGrunddaten fdata = fv.Grunddaten;
            if (fdata != null)
            {
              _length += fdata.Laenge;
              _mass += fdata.Masse;
            }
          }
        }
        else
        {
          Log.WarnFormat("Referenced vehicle doesn't exists ({0})", p.Value.Datei.FullPath);
        }

        p = p.Next;
      }
    }

    //---------------------------------------------------------------------
    public void ChangeParent(IZusiObjectParent parent)
    {
      // nothing to do, here
    }

    //---------------------------------------------------------------------
    public void Save(XmlWriter writer)
    {
      if (_source is Zug z)
      {
        z.Fahrzeuge.Save(writer);
      }
      else if (_source is FahrzeugVarianten fvs)
      {
        fvs.Save(writer);
      }
    }
  }
}
