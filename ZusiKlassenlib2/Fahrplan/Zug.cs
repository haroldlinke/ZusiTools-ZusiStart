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
 *
 * 01.01.2021 integrierte Fahrplän hinzugefügt
 */


using Sovoma;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Buchfahrplan;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Vehicle;

namespace ZusiKlassenLib2.Fahrplan
{
  public enum TrainType
  {
    Freight,
    Passenger
  }

  //---------------------------------------------------------------------
  [Serializable]
  public class Zug : ZusiObject
  {
    #region attributes & elements
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "Zugtyp",                   // 0: Güterzug, 1: Reisezug
            "Buchfahrplandll",
            "Gattung",
            "Nummer",
            "Zuglauf",
            "BRAngabe",
            "Prio",
            "Bremsstellung",
            "MBrh",
            "ReisendenDichte",
            "FahrplanGruppe",
            "Rekursionstiefe",
            "FahrstrName",
            "KeineVorplanKorrektur",
            "LODzug",
            "spZugNiedriger",
            "Standortmodus",
            "Dekozug",
            "StartVorschubweg",
            "spAnfang",
            "BuchfahrplanEinfach",
            "BremsstellungZug",
            "Verkehrstage",
            "Grenzlast",
            "APBeschl",
            "FplMasse",
            "FplZuglaenge",
            "TuerSystemBezeichner",
            "AufgleisenRegisterpruefen",
            "ZugsicherungStartmodus",
            "FplBremsstellungTextvorgabe",
            "EnergieVorgabe",
            "ColdMovement"

        };

    private static readonly string[] _knownElems =
    {
            "BuchfahrplanRohDatei",
            "FahrplanEintrag",
            "Datei",
            "FahrzeugVarianten",
            "BuchfahrplanBMPDatei",
            "Anfangsbefehl",
            "Aufgleisreferenz"
        };
#pragma warning restore IDE0052
    #endregion

    [NonSerialized]
    private Buchfahrplan.Buchfahrplan _buchfahrplan;
    [NonSerialized]
    private FahrzeugVarianten _fahrzeugVariantenOrg;

    private readonly float _apBeschl;
    private bool _decoTrain;
    private double _startSpeed; // **HLI allow to write startspeed
    private readonly TrainType _trainType = TrainType.Freight;
    private readonly string _buchfahrplanDll;
    private readonly string _gattung;
    private readonly string _nummer;
    private readonly string _zuglauf;
    private readonly string _brAngabe;
    private readonly string _prio;
    private readonly Bremsstellung _bremsstellung = Bremsstellung.G;
    //private readonly string _mbrh;
    private readonly int _mbrh;
    private readonly int _fplmasse;
    private readonly int _fplzuglaenge;
    private readonly int _energieVorgabe;
    private readonly string _tuersystembezeichner;
    private readonly string _reisendenDichte;
    private readonly string _fahrplanGruppe;
    private readonly string _rekursionstiefe;
    private readonly string _zugsicherungStartmodus;
    private readonly string _fahrstrName;
    private readonly string _keineVorplanKorrektur;
    private readonly string _lodZug;
    private readonly string _spZugNiedriger;
    private readonly string _standortmodus;
    private readonly string _startVorschubweg;
    private readonly string _buchfahrplanEinfach;
    private readonly Bremsstellung _bremsstellungZug = Bremsstellung.G;
    private readonly string _verkehrstage;
    private readonly string _coldMovement;
    private readonly bool _grenzlast;
    private readonly bool _aufgleisenregisterpruefen;
    private readonly string _fplBremsstellungTextvorgabe;

    private readonly BuchfahrplanRohDatei _buchfahrplanRohDatei;
    private readonly Datei _timetableFile;
    private readonly BuchfahrplanBMPDatei _buchfahrplanBMPDatei;
    private readonly Anfangsbefehl _anfangsbefehl;
    private FahrzeugVarianten _fahrzeugVarianten;

    private readonly List<FahrplanEintrag> _fahrplanEintraege = new();

    public float APBeschl => _apBeschl;
    public DateTime? Abgleiszeit => GetAbgleiszeit();
    public DateTime? Aufgleiszeit => GetAufgleiszeit();
    public ulong BelongsToTimeTable { get; set; }
    public string BRAngabe => _brAngabe;
    public Bremsstellung Bremsstellung => _bremsstellung;
    public string BuchfahrplanDll => _buchfahrplanDll;
    public Buchfahrplan.Buchfahrplan Buchfahrplan => GetBuchfahrplan();
    public BuchfahrplanBMPDatei BuchfahrplanBMPDatei => _buchfahrplanBMPDatei;
    public BuchfahrplanRohDatei BuchfahrplanRohDatei => _buchfahrplanRohDatei;
    public DateTime? EndTime => GetEndTime();
    public Datei FahrplanDatei => _timetableFile;
    public List<FahrplanEintrag> FahrplanEintraege => _fahrplanEintraege;
    public string FahrplanGruppe => _fahrplanGruppe;
    public string FahrstrName => _fahrstrName;
    public FahrzeugVarianten Fahrzeuge => _fahrzeugVarianten;
    public string Gattung => _gattung;
    public bool IsDecoTrain => _decoTrain;
    public TimeSpan? JourneyTime => GetJourneyTime();
    public int MinBremshundertstel => _mbrh;
    public int FplMasse => _fplmasse;
    public int FplZugLaenge => _fplzuglaenge;
    public string LODZug => _lodZug;
    public int EnergieVorgabe => _energieVorgabe;
    public string TuerSystemBezeichner => _tuersystembezeichner;
    public string Nummer => _nummer;
    public DateTime? StartTime => GetStartTime();
    //public double StartSpeed => _startSpeed; **HLI added get and set methods
    
    public double StartSpeed
    {
      get => _startSpeed;
      set
      {
        _startSpeed = value;
        RaisePropertyChanged(nameof(StartSpeed));
      }
    }


    public TrainType Type => _trainType;
    public string Zuglauf => _zuglauf;
    public bool AufgleisenRegisterpruefen => _aufgleisenregisterpruefen;

    //---------------------------------------------------------------------
    public Zug(ZusiDocumentBase parent, XElement x)
        : base(parent, x)
    {
      _trainType = x.GetAttrEnum<TrainType>("Zugtyp", TrainType.Freight);
      _buchfahrplanDll = x.GetAttrValue("Buchfahrplandll", (string)null);
      _gattung = x.GetAttrValue("Gattung", (string)null);
      _nummer = x.GetAttrValue("Nummer", (string)null);
      _zuglauf = x.GetAttrValue("Zuglauf", (string)null);
      _brAngabe = x.GetAttrValue("BRAngabe", (string)null);
      _prio = x.GetAttrValue("Prio", (string)null);
      _bremsstellung = x.GetAttrEnum<Bremsstellung>("Bremsstellung", Bremsstellung.G);
      _mbrh = (int)Math.Round(GetAttrValueDouble(x, "MBrh", 0.0) * 100);
      _fplmasse = (int)Math.Round(GetAttrValueDouble(x, "FplMasse", 0.0));
      _fplzuglaenge = (int)Math.Round(GetAttrValueDouble(x, "FplZugLaenge", 0.0));
      _reisendenDichte = x.GetAttrValue("ReisendenDichte", (string)null);
      _fahrplanGruppe = x.GetAttrValue("FahrplanGruppe", (string)null);
      _rekursionstiefe = x.GetAttrValue("Rekursionstiefe", (string)null);
      _zugsicherungStartmodus = x.GetAttrValue("ZugsicherungStartmodus", (string)null);
      _fahrstrName = x.GetAttrValue("FahrstrName", (string)null);
      _keineVorplanKorrektur = x.GetAttrValue("KeineVorplanKorrektur", (string)null);
      _lodZug = x.GetAttrValue("LODzug", (string)null);
      _spZugNiedriger = x.GetAttrValue("spZugNiedriger", (string)null);
      _standortmodus = x.GetAttrValue("Standortmodus", (string)null);
      _decoTrain = x.GetAttrValue("Dekozug", false);
      _startVorschubweg = x.GetAttrValue("StartVorschubweg", (string)null);
      _startSpeed = GetAttrValueDouble(x, "spAnfang", 0.0);
      _buchfahrplanEinfach = x.GetAttrValue("BuchfahrplanEinfach", (string)null);
      _bremsstellungZug = x.GetAttrEnum<Bremsstellung>("BremsstellungZug", Bremsstellung.G);
      _verkehrstage = x.GetAttrValue("Verkehrstage", (string)null);
      _coldMovement = x.GetAttrValue("ColdMovement", (string)null);
      _grenzlast = x.GetAttrValue("Grenzlast", false);
      _apBeschl = x.GetAttrValue("APBeschl", 0.0f);
      _tuersystembezeichner = x.GetAttrValue("TuerSystemBezeichner", (string)null);
      _aufgleisenregisterpruefen = x.GetAttrValue("AufgleisenRegisterpruefen", false);

      _buchfahrplanRohDatei = x.GetOptionalElement(this, "BuchfahrplanRohDatei", (p, e) => new BuchfahrplanRohDatei(p, e));
      _buchfahrplanBMPDatei = x.GetOptionalElement(this, "BuchfahrplanBMPDatei", (p, e) => new BuchfahrplanBMPDatei(p, e));
      _anfangsbefehl = x.GetOptionalElement(this, "Anfangsbefehl", (p, e) => new Anfangsbefehl(p, e));
      _timetableFile = x.GetOptionalElement(this, "Datei", (pa, xx) => new Datei(pa, xx));

      _fahrzeugVarianten = new FahrzeugVarianten(this, x.Element("FahrzeugVarianten"));
      _fplBremsstellungTextvorgabe = x.GetAttrValue("FplBremsstellungTextvorgabe", (string)null);
      _energieVorgabe = (int)Math.Round(GetAttrValueDouble(x, "EnergieVorgabe", 0.0));

      foreach (XElement xfpe in x.Elements("FahrplanEintrag"))
      {
        _fahrplanEintraege.Add(new FahrplanEintrag(this, xfpe));
      }
    }

    //---------------------------------------------------------------------
    public Zug(IZusiObjectParent parent, Zug source)
    : base(parent, source)
    {
      _trainType = source._trainType;
      _buchfahrplanDll = source._buchfahrplanDll;
      _gattung = source._gattung;
      _nummer = source._nummer;
      _zuglauf = source._zuglauf;
      _brAngabe = source._brAngabe;
      _prio = source._prio;
      _bremsstellung = source._bremsstellung;
      _mbrh = source._mbrh;
      _fplmasse = source._fplmasse;
      _fplzuglaenge = source._fplzuglaenge;
      _reisendenDichte = source._reisendenDichte;
      _fahrplanGruppe = source._fahrplanGruppe;
      _rekursionstiefe = source._rekursionstiefe;
      _zugsicherungStartmodus = source._zugsicherungStartmodus;
      _fahrstrName = source._fahrstrName;
      _keineVorplanKorrektur = source._keineVorplanKorrektur;
      _lodZug = source._lodZug;
      _spZugNiedriger = source._spZugNiedriger;
      _standortmodus = source._standortmodus;
      _decoTrain = source._decoTrain;
      _startVorschubweg = source._startVorschubweg;
      _startSpeed = source._startSpeed;
      _buchfahrplanEinfach = source._buchfahrplanEinfach;
      _bremsstellungZug = source._bremsstellungZug;
      _verkehrstage = source._verkehrstage;
      _coldMovement = source._coldMovement;
      _grenzlast = source._grenzlast;
      _apBeschl = source._apBeschl;
      _tuersystembezeichner = source._tuersystembezeichner;
      _aufgleisenregisterpruefen = source._aufgleisenregisterpruefen;

      _buchfahrplanRohDatei = Clone(this, source._buchfahrplanRohDatei);
      _buchfahrplanBMPDatei = Clone(this, source._buchfahrplanBMPDatei);
      _anfangsbefehl = Clone(this, source._anfangsbefehl);
      _timetableFile = Clone(this, source._timetableFile);
      _fplBremsstellungTextvorgabe = source._fplBremsstellungTextvorgabe;
      _energieVorgabe = source._energieVorgabe;

      _fahrzeugVarianten = new FahrzeugVarianten(this, source._fahrzeugVarianten);

      foreach (FahrplanEintrag fpe in source._fahrplanEintraege)
      {
        _fahrplanEintraege.Add(new FahrplanEintrag(this, fpe));
      }
    }

    //---------------------------------------------------------------------
    public void CheckDecoTrain()
    {
      if (!_decoTrain)
      {
        _decoTrain = Fahrzeuge.HasDecoVehicle();
      }
    }

    //---------------------------------------------------------------------
    public void ReplaceTrain(ZugReihung reihung)
    {
      if (reihung != null)
      {
        if (_fahrzeugVariantenOrg == null)
        {
          _fahrzeugVariantenOrg = _fahrzeugVarianten;
        }
        _fahrzeugVarianten = new FahrzeugVarianten(this, _fahrzeugVariantenOrg, reihung);
      }
      else
      {
        _fahrzeugVarianten = _fahrzeugVariantenOrg;
        _fahrzeugVariantenOrg = null;
      }
    }

    //---------------------------------------------------------------------
    private DateTime? GetAbgleiszeit()
    {
      //FahrplanEintrag fpe = FahrplanEintraege.LastOrDefault(f => f.Departure != null);
      //return fpe?.Departure;

      List<FahrplanEintrag> all = FahrplanEintraege;
      if (all.Count > 0)
      {
        foreach (FahrplanEintrag fpe in all.ToArray().Reverse())
        {
          if (fpe.Arrival != null)
          {
            return fpe.Arrival;
          }
          if (fpe.Departure != null)
          {
            return fpe.Departure;
          }
        }
      }

      return null;

    }

    //---------------------------------------------------------------------
    private DateTime? GetAufgleiszeit()
    {
      //FahrplanEintrag fpe = FahrplanEintraege.FirstOrDefault(f => f.Arrival != null);
      //return fpe?.Arrival;


      FahrplanEintrag fpe = FahrplanEintraege.FirstOrDefault(f => f.Departure != null);
      if (fpe == null)
      {
        //Log.DebugFormat("Book-Timetable {0} has no departure time", FindParent<ZusiDocumentBase>().Filename);
        fpe = FahrplanEintraege.FirstOrDefault(f => f.Arrival != null);
        return fpe?.Arrival;
      }
      else
      {
        return fpe.Departure;
      }
    }

    //---------------------------------------------------------------------
    private Buchfahrplan.Buchfahrplan GetBuchfahrplan()
    {
      if (_buchfahrplan == null)
      {
        if (_buchfahrplanRohDatei is BuchfahrplanRohDatei bfrd && !string.IsNullOrEmpty(bfrd.FullPath))
        {
          try
          {
            BuchfahrplanDatei bfd = new(bfrd.FullPath);
            bfd.Parse();
            _buchfahrplan = bfd.Root;
          }
          catch { }
        }
      }
      return _buchfahrplan;
    }

    //---------------------------------------------------------------------
    private TimeSpan? GetJourneyTime()
    {
      DateTime? start = GetStartTime();
      DateTime? end = GetEndTime();
      TimeSpan? ts = start != null && end != null ? end.Value - start.Value : (TimeSpan?)null;
      if (ts != null && ts.Value.Ticks < 0)
      {
        ts = ts.Value.Negate();
      }
      return ts;
    }

    //---------------------------------------------------------------------
    private DateTime? GetStartTime()
    {
      DateTime? start = null;

      FahrplanEintrag fpe = FahrplanEintraege.FirstOrDefault(f => f.Departure != null);
      if (fpe == null)
      {
        //Log.DebugFormat("Book-Timetable {0} has no departure time", FindParent<ZusiDocumentBase>().Filename);
        fpe = FahrplanEintraege.FirstOrDefault(f => f.Arrival != null);
        start = fpe?.Arrival;
      }
      else
      {
        start = fpe.Departure;
      }

      return start;
    }

    //---------------------------------------------------------------------
    private DateTime? GetEndTime()
    {
      List<FahrplanEintrag> all = FahrplanEintraege;
      if (all.Count > 0)
      {
        foreach (FahrplanEintrag fpe in all.ToArray().Reverse())
        {
          if (fpe.Arrival != null)
          {
            return fpe.Arrival;
          }
          if (fpe.Departure != null)
          {
            return fpe.Departure;
          }
        }
      }

      return null;
    }

    //---------------------------------------------------------------------
    protected override void SaveAttributes(XmlWriter writer)
    {
      base.SaveAttributes(writer);
      writer.WriteAttributeIf(_trainType != TrainType.Freight, "Zugtyp", (int)_trainType);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_buchfahrplanDll), "Buchfahrplandll", _buchfahrplanDll);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_gattung), "Gattung", _gattung);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_nummer), "Nummer", _nummer);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_zuglauf), "Zuglauf", _zuglauf);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_brAngabe), "BRAngabe", _brAngabe);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_prio), "Prio", _prio);
      writer.WriteAttributeIf(_bremsstellung != Bremsstellung.G, "Bremsstellung", (int)_bremsstellung);
      //writer.WriteAttributeIf(!string.IsNullOrEmpty(_mbrh), "MBrh", _mbrh);
      writer.WriteAttributeIf(_mbrh > 0, "MBrh", _mbrh/100.0);
      writer.WriteAttributeIf(_fplmasse > 0, "FplMasse", _fplmasse);
      writer.WriteAttributeIf(_fplzuglaenge > 0, "FplZugLaenge", _fplzuglaenge);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_reisendenDichte), "ReisendenDichte", _reisendenDichte);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_fahrplanGruppe), "FahrplanGruppe", _fahrplanGruppe);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_rekursionstiefe), "Rekursionstiefe", _rekursionstiefe);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_zugsicherungStartmodus), "ZugsicherungStartmodus", _zugsicherungStartmodus);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_fahrstrName), "FahrstrName", _fahrstrName);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_keineVorplanKorrektur), "KeineVorplanKorrektur", _keineVorplanKorrektur);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_lodZug), "LODzug", _lodZug);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_spZugNiedriger), "spZugNiedriger", _spZugNiedriger);
      writer.WriteAttributeFloatIf(_apBeschl != 0, "APBeschl", _apBeschl, 4);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_standortmodus), "Standortmodus", _standortmodus);
      writer.WriteAttributeIf(_decoTrain, "Dekozug", 1);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_startVorschubweg), "StartVorschubweg", _startVorschubweg);
      writer.WriteAttributeDoubleIf(_startSpeed != 0.0, "spAnfang", _startSpeed, 4);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_buchfahrplanEinfach), "BuchfahrplanEinfach", _buchfahrplanEinfach);
      writer.WriteAttributeIf(_bremsstellungZug != Bremsstellung.G, "BremsstellungZug", (int)_bremsstellungZug);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_verkehrstage), "Verkehrstage", _verkehrstage);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_coldMovement), "ColdMovement", _coldMovement);
      writer.WriteAttributeIf(_grenzlast, "Grenzlast", 1);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_tuersystembezeichner), "TuerSystemBezeichner", _tuersystembezeichner);
      writer.WriteAttributeIf(_aufgleisenregisterpruefen, "AufgleisenRegisterpruefen", 1);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_fplBremsstellungTextvorgabe), "FplBremsstellungTextvorgabe", _fplBremsstellungTextvorgabe);
      writer.WriteAttributeIf(_energieVorgabe > 0, "EnergieVorgabe", _energieVorgabe);
    }

    //---------------------------------------------------------------------
    protected override void SaveElements(XmlWriter writer)
    {
      base.SaveElements(writer);
      _timetableFile?.Save(writer);
      _buchfahrplanRohDatei?.Save(writer);
      _buchfahrplanBMPDatei?.Save(writer);
      _anfangsbefehl?.Save(writer);
      _fahrplanEintraege.ForEach(fe => fe.Save(writer));
      _fahrzeugVarianten.Save(writer);
    }
  }
}
