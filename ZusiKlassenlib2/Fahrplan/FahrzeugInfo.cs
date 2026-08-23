/*
 * Copyright 2017 Holger Maaß
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using log4net;
using Sovoma;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Vehicle;

namespace ZusiKlassenLib2.Fahrplan
{
  public enum DotraMode
  {
    Default,
    PartOfMultipleHeading,
    NoDrivetrain
  }

  //---------------------------------------------------------------------
  [Serializable]
  public class FahrzeugInfo : ZusiObject, ITrainAssembly
  {
    #region attributes & elements
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "IDHaupt",
            "IDNeben",
            "DotraModus",
            "SASchaltung",
            "Gedreht",
            "EigeneBremsstellung",
            "Bremsstellung",
            "Zugart",
            "Tuerignorieren",
            "EigeneZugart",
            "FahrzeugZusatzinfo",
            "BremsstellungFahrzeug",
            "GrunddatenIndex",
            "VariantenIndex",
            "NVRNummer",
            "StartAntriebIndex"
        };

    private static readonly string[] _knownElems =
    {
            "Datei",
            "ZugdatenIndusiAnalog",
            "ZugdatenIndusiRechner",
            "ZugdatenLZB80",
            "ZugdatenETCS"
        };
#pragma warning restore IDE0052
    #endregion

    private static readonly ILog Log = LogManager.GetLogger(typeof(FahrzeugInfo));

    #region xml attributes

    private readonly IntAttribute _idHaupt;
    private readonly IntAttribute _idNeben;
    private readonly IntAttribute _grunddatenIndex;
    private readonly IntAttribute _variantenIndex;
    private DotraMode _dotraModus;
    private readonly IntAttribute _saSchaltung;
    private readonly BoolAttribute _gedreht;
    private readonly BoolAttribute _eigeneBremsstellung;
    private readonly StringAttribute _bremsstellung;
    private readonly EnumAttribute<Bremsstellung> _bremsstellungFahrzeug;
    private BoolAttribute _tuerignorieren;
    private readonly BoolAttribute _eigeneZugart;
    private readonly StringAttribute _fahrzeugZusatzinfo;
    private readonly IntAttribute _startantriebIndex;
    private readonly StringAttribute _nvrNummer;

    #endregion

    #region xml elements

    private readonly Datei _datei;
    private readonly Zugdaten _zugdatenPZB;
    private readonly ZugdatenLZB80 _zugdatenLZB;
    private readonly ZugdatenETCS _zugdatenETCS;

    #endregion

    #region private fields

    private Fahrzeug _fahrzeug;
    //private readonly Zugart _zugart;

    #endregion

    #region public properties

    public Datei Datei { get { return _datei; } }

    public DotraMode DotraModus
    {
      get => _dotraModus;
      set => _dotraModus = value;
    }
    public bool EigeneBremsstellung => _eigeneBremsstellung;
    public string Bremsstellung => _bremsstellung;
    public string NVRNummer => _nvrNummer;
    //public Zugart Zugart => _zugart;
    //public bool Tuerignorieren => _tuerignorieren;
    public bool Tuerignorieren
    {
      get => _tuerignorieren;
      set => _tuerignorieren = new BoolAttribute("Tuerignorieren",value);
    }
    public bool EigeneZugart => _eigeneZugart;
    public Bremsstellung BremsstellungFahrzeug
    {
      get => _bremsstellungFahrzeug.Value;
      set
      {
        if (_bremsstellungFahrzeug.Value != value)
        {
          _bremsstellungFahrzeug.Value = value;
        }
      }
    }

    public Fahrzeug Fahrzeug
    {
      get
      {
        EnsureFahrzeug();
        return _fahrzeug;
      }
    }

    public bool Gedreht { get => _gedreht; }

    public int IDHaupt { get => _idHaupt; }

    public int IDNeben { get => _idNeben; }

    public int SASchaltung
    {
      get => _saSchaltung;
      set => _saSchaltung.Value = value;
    }


    public int VariantenIndex { get => _variantenIndex; }

    public Zugdaten ZugdatenPZB => _zugdatenPZB;

    public Zugdaten ZugdatenLZB => _zugdatenLZB;

    public Zugdaten ZugdatenETCS => _zugdatenETCS;

    #endregion

    #region ctor/dtor

    public FahrzeugInfo(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    {
      _idHaupt = new IntAttribute(x, "IDHaupt");
      _idNeben = new IntAttribute(x, "IDNeben");
      _grunddatenIndex = new IntAttribute(x, "GrunddatenIndex");
      _variantenIndex = new IntAttribute(x, "VariantenIndex", -1);
      _dotraModus = (DotraMode)new IntAttribute(x, "DotraModus").Value;
      _saSchaltung = new IntAttribute(x, "SASchaltung");
      _gedreht = new BoolAttribute(x, "Gedreht");
      _eigeneBremsstellung = new BoolAttribute(x, "EigeneBremsstellung");
      _bremsstellung = new StringAttribute(x, "Bremsstellung");
      _nvrNummer = new StringAttribute(x, "NVRNummer");
      _startantriebIndex = new IntAttribute(x, "StartAntriebIndex");

      _tuerignorieren = new BoolAttribute(x, "Tuerignorieren");
      _fahrzeugZusatzinfo = new StringAttribute(x, "FahrzeugZusatzinfo");

      _bremsstellungFahrzeug = new EnumAttribute<Bremsstellung>(x, "BremsstellungFahrzeug");
      

      _datei = new Datei(this, x.Element("Datei"));

      foreach (string elem in new string[] { "ZugdatenIndusiAnalog", "ZugdatenIndusiRechner", "ZugdatenPZ80" })
      {
        _zugdatenPZB = x.GetOptionalElement(this, elem, (p, e) => CreateZugdaten(elem, p, e), null);
        if (_zugdatenPZB != null)
          break;
      }

      _zugdatenLZB = x.GetOptionalElement(this, "ZugdatenLZB80", (p, e) => new ZugdatenLZB80(p, e), null);

      _zugdatenETCS = x.GetOptionalElement(this, "ZugdatenETCS", (p, e) => new ZugdatenETCS(p, e), null);

      _eigeneZugart = new BoolAttribute("EigeneZugart", _zugdatenPZB != null || _zugdatenLZB != null || _zugdatenETCS != null);
    }

    public FahrzeugInfo(IZusiObjectParent parent, FahrzeugInfo source)
        : base(parent, source)
    {
      _idHaupt = new IntAttribute(source._idHaupt);
      _idNeben = new IntAttribute(source._idNeben);
      _grunddatenIndex = new IntAttribute(source._grunddatenIndex);
      _variantenIndex = new IntAttribute(source._variantenIndex);
      _dotraModus = source._dotraModus;
      _saSchaltung = new IntAttribute(source._saSchaltung);
      _gedreht = new BoolAttribute(source._gedreht);
      _eigeneBremsstellung = new BoolAttribute(source._eigeneBremsstellung);
      _bremsstellung = new StringAttribute(source._bremsstellung);
      _nvrNummer = new StringAttribute(source._nvrNummer);
      _tuerignorieren = new BoolAttribute(source._tuerignorieren);
      _fahrzeugZusatzinfo = new StringAttribute(source._fahrzeugZusatzinfo);
      _bremsstellungFahrzeug = source._bremsstellungFahrzeug;
      
      _startantriebIndex = new IntAttribute(source._startantriebIndex);

      _datei = new Datei(this, source._datei);

      if (source._zugdatenPZB != null)
      {
        _zugdatenPZB = ZusiObject.Clone(this, source._zugdatenPZB);
      }

      if (source._zugdatenLZB != null)
      {
        _zugdatenLZB = Clone(this, source._zugdatenLZB);
      }

      if (source._zugdatenETCS != null)
      {
        _zugdatenETCS = Clone(this, source._zugdatenETCS);
      }

      _eigeneZugart = new BoolAttribute("EigeneZugart", _zugdatenPZB != null || _zugdatenLZB != null || _zugdatenETCS != null);
    }

    #endregion

    #region public methods

    public void BuildTrain(LinkedList<FahrzeugInfo> zugReihung)
    {
      zugReihung.AddLast(this);
    }

    public FahrzeugVariante GetVariante()
    {
      return Fahrzeug is Fahrzeug fzg ? fzg.GetVariante(_idHaupt, _idNeben, _variantenIndex) : null;
    }

    #endregion

    #region overrides

    protected override void SaveAttributes(XmlWriter writer)
    {
      base.SaveAttributes(writer);

      _idHaupt.Write(writer);
      _idNeben.Write(writer);
      _variantenIndex.Write(writer, -1);
      _grunddatenIndex.Write(writer);
      writer.WriteAttributeIf(_dotraModus > DotraMode.Default, "DotraModus", (int)_dotraModus);
      _saSchaltung.Write(writer);
      _gedreht.Write(writer);
      _bremsstellung.Write(writer);
      _nvrNummer.Write(writer);
      _tuerignorieren.Write(writer);
      _fahrzeugZusatzinfo.Write(writer);
      _startantriebIndex.Write(writer);
      

      Bremsstellung bs = _bremsstellungFahrzeug.Value;
      if (bs == Vehicle.Bremsstellung.G || bs == Vehicle.Bremsstellung.P || bs == Vehicle.Bremsstellung.R)
      {
        _eigeneBremsstellung.Value = true;
        _eigeneBremsstellung.Write(writer);
        _bremsstellungFahrzeug.Write(writer);
      }

      _eigeneZugart.Write(writer);
    }

    protected override void SaveElements(XmlWriter writer)
    {
      base.SaveElements(writer);

      _datei.Save(writer);
      _zugdatenPZB?.Save(writer);
      _zugdatenLZB?.Save(writer);
      _zugdatenETCS?.Save(writer);
    }

    #endregion

    #region private method's

    private void EnsureFahrzeug()
    {
      if (_fahrzeug == null)
      {
        try
        {
          if (_datei.Exists)
          {
            _fahrzeug = VehicleCache.GetFahrzeug(_datei);
          }
          else
          {
            Log.FatalFormat("Vehicle '{0}' doesn't exists", _datei.FullPath);
          }
        }
        catch (Exception ex)
        {
          Log.Error(ex.ToString());
        }
      }
    }

    private static Zugdaten CreateZugdaten(string typeName, IZusiObjectParent parent, XElement x)
    {
      IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Name == typeName);
      if (types == null || !types.Any())
      {
        Log.FatalFormat("Type not found: {0}", typeName);
      }
      else if (types.Count() > 1)
      {
        Log.FatalFormat("Ambiguous type found: {0}", typeName);
      }
      else
      {
        return Activator.CreateInstance(types.ElementAt(0), parent, x) as Zugdaten;
      }

      return null;
    }

    #endregion
  }
}
