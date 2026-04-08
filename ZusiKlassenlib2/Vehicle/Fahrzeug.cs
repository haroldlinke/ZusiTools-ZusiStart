/*
 * Copyright 2017-2021 Holger Maaß
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

using Sovoma;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Vehicle
{
  [Flags]
  public enum VehicleKind
  {
    Unknown = 0,
    // traction part
    Steam = 1 << 0,
    Diesel = 1 << 1,
    Electric = 1 << 2,
    Battery = 1 << 3,
    // kind part
    Locomotive = 1 << 4,
    RailCar = 1 << 5,
    Coach = 1 << 6,
    FreightWagon = 1 << 7,
    ServiceWagon = 1 << 8,
    Special = 1 << 9,
    // masks
    IsPowered = Locomotive | RailCar,
    IsWaggon = Coach | FreightWagon | ServiceWagon,
    KindMask = Locomotive | RailCar | Coach | FreightWagon | ServiceWagon
  }

  [Serializable]
  public class Fahrzeug : ZusiGenericObject
  {
    #region attributes & elements
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "MaxIDHaupt"
        };

    private static readonly string[] _knownElems =
    {
            "FahrzeugVariante",
            "FahrzeugGrunddaten",
            "Bremscomputer",
            // Bremssystemnamen, 3 .. 10
            "Bremse_KE_GP",
            "Bremse_KE_GPR_Klotz",
            "Bremse_KE_GPR_Scheibe",
            "Bremse_KE_L2a",
            "Bremse_KE_L2d",
            "Bremse_KE_Tm",
            "BremseLuftDirekt",
            "BremseLuftEinlK1",
            "Handbremse",
            // dynamische Bremsen, 12 .. 14
            "DynbremseHydrodynamisch",
            "DynbremseElektrDrehstrom",
            "DynbremseElektrReihenschluss",
            // --
            "FederspeicherBremse",
            "MgBremse",
            "ExterneDatei",
            "Luftpresser",
            // Antriebe 19 .. 22
            "AntriebsmodellEinfach",
            "AntriebsmodellDieselHydraulisch",
            "AntriebsmodellElektrischDrehstrom",
            "AntriebsmodellElektrischReihenschluss",
            "AntriebsmodellDieselElektrischDrehstrom",
            
     
            // --
            "FahrzeugSound",
            "FzgTuersystemTB0",
            "FzgTuersystemTB5",
            "FzgTuersystemSAT",
            "FzgTuersystemSST",
            "FzgTuersystemTAV",
            "FzgTuersystemUICWTB",
            "FzgTuersystemSBahn",
            "FktIndividuell",
            "FahrzeugBeladung",
            // --
            "Antriebsumschaltung"  //HL 19.1.2025
        };
#pragma warning restore IDE0052
    #endregion

    [NonSerialized]
    private readonly List<Bremssystem> _bremssysteme = new();
    [NonSerialized]
    private List<FzgTuerSystemBasis> _doorSystems;
    private readonly List<Fahrzeug> _includes = new();
    private List<FahrzeugVariante> _varianten;
    private readonly List<FahrzeugBeladung> _beladungen;

    private readonly string _name;
    private VehicleKind _kind;
    private string _country;
    private int _epoche;
    private readonly string _wagen;
    private readonly Antriebsmodell _drivetrain;
    private readonly Dynbremse _dynamicBrake;

    public Bremscomputer Bremscomputer => Object<Bremscomputer>();
    public Bremssystem[] Bremssysteme => GetBremssystems();
    public List<FzgTuerSystemBasis> DoorSystems => GetDoorSystems();
    public Antriebsmodell Drivetrain => GetDrivetrain();
    public Dynbremse DynamicBrake => GetDynamicBrake();
    public DateiFuehrerstand Fuehrerstand => GetFuehrerstand();
    public FahrzeugGrunddaten Grunddaten => GetGrunddaten();
    public Handbremse Handbrake => GetHandbrake();
    public bool HasKlotzBremse => GetHasKlotzBremse();
    public bool HasScheibenBremse => GetHasScheibenBremse();
    public bool HasSingleReleaseBrake => GetHasSingleReleaseBrake();
    public bool IsMainVehicleFile => Varianten.Count > 0;
    public MgBremse RailBrake => GetRailBrake();
    public FahrzeugBeladung[] Beladungen => GetBeladungen();
    public List<FahrzeugVariante> Varianten
    {
      get
      {
        if (_varianten == null)
        {
          _varianten = Objects<FahrzeugVariante>();
        }
        return _varianten;
      }
    }

    public string Name => _name;
    public string Country { get => _country; set => _country = value; }
    public int Epoche { get => _epoche; set => _epoche = value; }
    public VehicleKind Kind { get => _kind; set => _kind = value; }
    public string Wagen => _wagen;

    //---------------------------------------------------------------------
    public Fahrzeug(ZusiDocumentBase parent, XElement x)
        : base(parent, x)
    {
      _name = Path.GetFileNameWithoutExtension(parent.Filename).Replace(".rv", "");//.ToProper();

      string name = _name.ToUpper();
      Regex rex = new(@"[A-Z]\-WAGEN");
      Match m = rex.Match(name);
      if (m.Success)
      {
        _wagen = m.Value.Substring(0, 1);
      }
      else
      {
        // BR 450
        rex = new Regex(@"GT8-100[CD]_2S(-M)??_(?<wg>[ABC]{1,2})");
        m = rex.Match(name);
        if (m.Success)
        {
          _wagen = m.Groups["wg"].Value;
        }
        else
        {
          // 612
          if (name.StartsWith("612_0"))
          {
            _wagen = "A";
          }
          else if (name.StartsWith("612_5"))
          {
            _wagen = "B";
          }
          // 628
          else if (name.StartsWith("628_2"))
          {
            _wagen = "A";
          }
          else if (name.StartsWith("628_4"))
          {
            _wagen = "B";
          }
          else if (name.StartsWith("928_2"))
          {
            _wagen = "C";
          }
          else if (name.StartsWith("928_4"))
          {
            _wagen = "D";
          }
        }
      }

      ParseExternalFiles();

      for (int i = 3; i <= 10; i++)
      {
        Bremssystem bb = Object<Bremssystem>(_knownElems[i]);
        if (bb != null)
        {
          _bremssysteme.Add(bb);
        }
      }

      for (int i = 12; i <= 14; i++)
      {
        Dynbremse db = Object<Dynbremse>(_knownElems[i]);
        if (db != null)
        {
          _dynamicBrake = db;
          break;
        }
      }

      for (int i = 19; i <= 23; i++)
      {
        Antriebsmodell dt = Object<Antriebsmodell>(_knownElems[i]);
        if (dt != null)
        {
          _drivetrain = dt;
          break;
        }
      }

      _beladungen = Objects<FahrzeugBeladung>();
    }

    //---------------------------------------------------------------------
    public string ComputeHash()
    {
      StringBuilder sb = new();
      if (!string.IsNullOrEmpty(_name))
      {
        sb.Append(_name.ToLower());
      }
      if (!string.IsNullOrEmpty(_country))
      {
        sb.Append(_country.ToLower());
      }
      sb.Append(_kind);
      sb.Append(_epoche);

      return sb.ToString().ComputeMD5Hash();
    }

    //---------------------------------------------------------------------
    public FahrzeugVariante GetVariante(int idHaupt, int idNeben, int index)
    {
      FahrzeugVariante fv = Varianten.Where(v => v.IDHaupt == idHaupt && v.IDNeben == idNeben).FirstOrDefault();
      if (fv == null && index >= 0 && index < Varianten.Count)
      {
        fv = Varianten[index];
      }
      return fv;
    }

    //---------------------------------------------------------------------
    public override string ToString()
    {
      return $"{_name}";
    }

    //---------------------------------------------------------------------
    private FahrzeugBeladung[] GetBeladungen()
    {
      List<FahrzeugBeladung> list = new();

      list.AddRange(_beladungen);

      foreach (Fahrzeug f in _includes)
      {
        list.AddRange(f.Beladungen);
      }

      return list.ToArray();
    }

    //---------------------------------------------------------------------
    private Bremssystem[] GetBremssystems()
    {
      List<Bremssystem> list = new();

      list.AddRange(_bremssysteme);

      foreach (Fahrzeug f in _includes)
      {
        list.AddRange(f.Bremssysteme);
      }

      return list.ToArray();
    }

    //---------------------------------------------------------------------
    private List<FzgTuerSystemBasis> GetDoorSystems()
    {
      if (_doorSystems == null)
      {
        _doorSystems = new List<FzgTuerSystemBasis>();

        _doorSystems.AddRange(Objects<FzgTuerSystemBasis>());

        foreach (Fahrzeug f in _includes)
        {
          _doorSystems.AddRange(f.GetDoorSystems());
        }
      }

      return _doorSystems;
    }

    //---------------------------------------------------------------------
    private Antriebsmodell GetDrivetrain()
    {
      if (_drivetrain != null)
      {
        return _drivetrain;
      }

      foreach (Fahrzeug f in _includes)
      {
        if (f.Drivetrain != null)
        {
          return f.Drivetrain;
        }
      }

      return null;
    }

    //---------------------------------------------------------------------
    private Dynbremse GetDynamicBrake()
    {
      if (_dynamicBrake != null)
      {
        return _dynamicBrake;
      }

      foreach (Fahrzeug f in _includes)
      {
        if (f.DynamicBrake != null)
        {
          return f.DynamicBrake;
        }
      }

      return null;
    }

    //---------------------------------------------------------------------
    private DateiFuehrerstand GetFuehrerstand()
    {
      foreach (Fahrzeug f in _includes)
      {
        if (f.Fuehrerstand != null)
        {
          return f.Fuehrerstand;
        }
      }

      return null;
    }

    //---------------------------------------------------------------------
    private FahrzeugGrunddaten GetGrunddaten()
    {
      FahrzeugGrunddaten data = Object<FahrzeugGrunddaten>();
      if (data != null)
      {
        return data;
      }

      foreach (Fahrzeug f in _includes)
      {
        if (f.Grunddaten != null)
          return f.Grunddaten;
      }

      return null;
    }

    //---------------------------------------------------------------------
    private Handbremse GetHandbrake()
    {
      Handbremse b = Object<Handbremse>();
      if (b != null)
      {
        return b;
      }

      foreach (Fahrzeug f in _includes)
      {
        b = f.Handbrake;
        if (b != null)
        {
          return b;
        }
      }

      return null;
    }

    //---------------------------------------------------------------------
    private bool GetHasKlotzBremse()
    {
      if (Bremssysteme is Bremssystem[] bb)
      {
        var q = from b in bb
                where b.IsKlotzBremse
                select b;
        return q.Any();
      }

      return false;
    }

    //---------------------------------------------------------------------
    private bool GetHasScheibenBremse()
    {
      if (Bremssysteme is Bremssystem[] bb)
      {
        var q = from b in bb
                where b.IsScheibenBremse
                select b;
        return q.Any();
      }

      return false;
    }

    //---------------------------------------------------------------------
    private bool GetHasSingleReleaseBrake()
    {
      if (Bremssysteme is Bremssystem[] bb)
      {
        var q = from b in bb
                where b.IsEinloesig
                select b;
        return q.Any();
      }

      return false;
    }

    //---------------------------------------------------------------------
    private MgBremse GetRailBrake()
    {
      MgBremse b = Object<MgBremse>();
      if (b != null)
      {
        return b;
      }

      foreach (Fahrzeug f in _includes)
      {
        b = f.RailBrake;
        if (b != null)
        {
          return b;
        }
      }

      return null;
    }

    //---------------------------------------------------------------------
    private void ParseExternalFiles()
    {
      List<ExterneDatei> files = Objects<ExterneDatei>();
      files.ForEach(f =>
      {
        string fn = f.Datei.FullPath;
        if (string.Compare(Path.GetExtension(fn), ".fzg", true) == 0)
        {
          Fahrzeug fzg = VehicleCache.GetFahrzeug(fn);
          if (fzg != null)
          {
            _includes.Add(fzg);
          }
        }
      });
    }
  }
}
