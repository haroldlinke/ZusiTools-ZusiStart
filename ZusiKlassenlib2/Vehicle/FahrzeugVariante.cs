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
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Fahrplan;
using ZusiKlassenLib2.Landscape;

namespace ZusiKlassenLib2.Vehicle
{
    public enum CabMode
    {
        Both = 0,
        None = 1,
        ForwardOnly = 2,
        BothDifferent = 3
    }

    //---------------------------------------------------------------------
    [Serializable]
    public class FahrzeugVariante : ZusiObject
    {
        public const int Front = 0;
        public const int Rear = 1;

        private static readonly ILog Log = LogManager.GetLogger(typeof(FahrzeugVariante));

#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "BR",
            "Beschreibung",
            "Farbgebung",
            "EinsatzAb",
            "EinsatzBis",
            "IDHaupt",
            "IDNeben",
            "Dekozug",
            "NVRNummer",
            "FzgGattung",
            "Fuehrerstandsmodus",
            "SitzeKlasseA",
            "SitzeKlasseB",
            "InterneNummer",
            "HistNummer"
        };

        private static readonly string[] _knownElems =
        {
            "DateiAussenansicht",
            "DateiFuehrerstand",
            "DateiFuehrerstandRueckwaerts",
            "ExterneDatei"
        };
#pragma warning restore IDE0052

        private List<FzgTuerSystemBasis> _doorSystems;
        private readonly string _br;
        private readonly string _beschreibung;
        private readonly string _farbgebung;
        private readonly DateTime? _einsatzAb;
        private readonly DateTime? _einsatzBis;
        private readonly int _idHaupt;
        private readonly int _idNeben;
        private readonly bool _dekozug;
        private string _nvrNumber;
        private readonly string _gattung;
        private readonly int _seatsA;
        private readonly int _seatsB;
        private readonly CabMode _cabMode;
        private readonly string _interneNummer;

        private readonly DateiAussenansicht _dateiAussenansicht;
        private readonly DateiFuehrerstand[] _dateiFuehrerstand = new DateiFuehrerstand[] { null, null };
        private readonly List<ExterneDatei> _externeDateien = new();
        private readonly List<Fahrzeug> _includes = new();

        [NonSerialized]
        private Landschaft _aussenansicht;

        public string BR => _br;
        public Bremscomputer Bremscomputer => GetBremscomputer();
        public string Beschreibung => _beschreibung;
        public string Farbgebung => _farbgebung;
        public DateTime? EinsatzAb => _einsatzAb;
        public DateTime? EinsatzBis => _einsatzBis;
        public int IDHaupt => _idHaupt;
        public int IDNeben => _idNeben;
        public int VID => _idHaupt * 100 + _idNeben;
        public bool Dekozug
        {
            get
            {
                Zug z = FindParent<Zug>();
                return (z != null && z.IsDecoTrain) || _dekozug;
            }
        }
        public Dynbremse DynamicBrake => GetDynamicBrake();

        public DateiAussenansicht DateiAussenansicht => _dateiAussenansicht;
        public DateiFuehrerstand[] DateiFuehrerstand => _dateiFuehrerstand;
        public DateiFuehrerstand DateiFuehrerstandVorn => _dateiFuehrerstand[Front];
        public DateiFuehrerstand DateiFuehrerstandHinten => _dateiFuehrerstand[Rear];
        public List<ExterneDatei> ExterneDateien => _externeDateien;
        public string Gattung => _gattung;
        public int SeatsClassA => _seatsA;
        public int SeatsClassB => _seatsB;
        public CabMode CabMode => _cabMode;
        public string InterneNummer => _interneNummer;

        public Landschaft Aussenansicht
        {
            get
            {
                if (_aussenansicht == null)
                {
                    if (!_dateiAussenansicht.IsEmpty)
                    {
                        LandschaftsDatei ld = new(_dateiAussenansicht.FullPath);
                        ld.Parse();
                        _aussenansicht = ld.Root;
                    }
                }
                return _aussenansicht;
            }
        }

        public string NVRNummer
        {
            get => _nvrNumber;
            set
            {
                if (string.Compare(_nvrNumber, value) != 0)
                {
                    _nvrNumber = value;
                    RaisePropertyChanged(nameof(NVRNummer));
                }
            }
        }

        // angehängte Eigenschaften
        public FahrzeugBeladung[] Beladungen => GetBeladungen();
        public Bremssystem[] Bremssysteme => GetBremssystems();
        public MgBremse RailBrake => GetRailBrake();
        public List<FzgTuerSystemBasis> DoorSystems => GetDoorSystems();
        public Antriebsmodell Drivetrain => GetDrivetrain();
        public FahrzeugGrunddaten Grunddaten => GetGrunddaten();
        public bool IsDriven => GetDrivetrain() != null;
        public double Masse => GetMasse();
        public Handbremse Handbrake => GetHandbrake();
        public bool HasKlotzBremse => GetHasKlotzBremse();
        public bool HasScheibenBremse => GetHasScheibenBremse();
        public bool HasSingleReleaseBrake => GetHasSingleReleaseBrake();

        //---------------------------------------------------------------------
        public FahrzeugVariante(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _br = x.GetAttrValue("BR", "");
            _beschreibung = x.GetAttrValue("Beschreibung", "");
            _farbgebung = x.GetAttrValue("Farbgebung", "");
            _einsatzAb = ZusiDate.Parse(x.GetAttrValue("EinsatzAb", ""));
            _einsatzBis = ZusiDate.Parse(x.GetAttrValue("EinsatzBis", ""));
            _idHaupt = x.GetAttrValue("IDHaupt", 0);
            _idNeben = x.GetAttrValue("IDNeben", 0);
            _dekozug = x.GetAttrValue("Dekozug", false);
            _nvrNumber = x.GetAttrValue("NVRNummer", string.Empty);
            _gattung = x.GetAttrValue("FzgGattung", string.Empty);
            _seatsA = x.GetAttrValue("SitzeKlasseA", 0);
            _seatsB = x.GetAttrValue("SitzeKlasseB", 0);
            _cabMode = x.GetAttrEnum<CabMode>("Fuehrerstandsmodus", CabMode.None);
            _interneNummer = x.GetAttrValue("InterneNummer", string.Empty);

            _dateiAussenansicht = new DateiAussenansicht(this, x.Element("DateiAussenansicht"));
            _dateiFuehrerstand[0] = new DateiFuehrerstand(this, x.Element("DateiFuehrerstand"));
            _dateiFuehrerstand[1] = GetOptionalObject<DateiFuehrerstand>(this, x.Element("DateiFuehrerstandRueckwaerts"), "DateiFuehrerstandRueckwaerts");

            foreach (XElement xed in x.Elements("ExterneDatei"))
            {
                ExterneDatei ed = new(this, xed);
                _externeDateien.Add(ed);
                if (string.Compare(Path.GetExtension(ed.Datei.Dateiname), ".fzg", true) == 0)
                {
                    try
                    {
                        Fahrzeug fzg = VehicleCache.GetFahrzeug(ed.Datei);
                        if (fzg != null)
                        {
                            _includes.Add(fzg);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        public double Bremsgewicht(Bremsstellung bremsstellung)
        {
            if (Bremssysteme is Bremssystem[] bb)
            {
                MgBremse mgb = RailBrake;

                foreach (Bremssystem bs in bb)
                {
                    if (bs.IstBremsstellungVerfuegbar(bremsstellung))
                    {
                        double bw = bs.BremsGewicht(bremsstellung);
                        if (mgb != null)
                        {
                            bw += mgb.BremsGewicht;
                        }
                        return bw;
                    }
                }
            }

            return 0;
        }

        //---------------------------------------------------------------------
        public bool IstBremsstellungVerfuegbar(Bremsstellung bremsstellung)
        {
            if (Bremssysteme is Bremssystem[] bb)
            {
                foreach (Bremssystem bs in bb)
                {
                    return bs.IstBremsstellungVerfuegbar(bremsstellung);
                }
            }

            return false;
        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            return string.Format("{0} ({1}.{2})", _br, _idHaupt, _idNeben);
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeStringIfNotEmpty("BR", _br);
            writer.WriteAttributeStringIfNotEmpty("Beschreibung", _beschreibung);
            writer.WriteAttributeStringIfNotEmpty("Farbgebung", _farbgebung);
            writer.WriteAttributeDateTimeIf(_einsatzAb != null, "EinsatzAb", _einsatzAb.Value, "yyyy-MM-dd");
            writer.WriteAttributeDateTimeIf(_einsatzBis != null, "EinsatzBis", _einsatzBis.Value, "yyyy-MM-dd");
            writer.WriteAttribute("IDHaupt", _idHaupt);
            writer.WriteAttribute("IDNeben", _idNeben);
            writer.WriteAttributeIf(_dekozug, "Dekozug", 1);
            writer.WriteAttributeStringIfNotEmpty("NVRNummer", _nvrNumber);
            writer.WriteAttributeStringIfNotEmpty("FzgGattung", _gattung);
            writer.WriteAttributeIf(_seatsA > 0, "SitzeKlasseA", _seatsA);
            writer.WriteAttributeIf(_seatsB > 0, "SitzeKlasseB", _seatsB);
            writer.WriteAttributeIf(_cabMode != CabMode.Both, "Fuehrerstandsmodus", (int)_cabMode);
            writer.WriteAttributeStringIfNotEmpty("InterneNummer", _interneNummer);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _dateiAussenansicht.Save(writer);
            foreach (DateiFuehrerstand d in _dateiFuehrerstand)
            {
                d.Save(writer);
            }
            _externeDateien.ForEach(e => e.Save(writer));
        }

        //---------------------------------------------------------------------
        private Bremscomputer GetBremscomputer()
        {
            return Parent is Fahrzeug fzg ? fzg.Bremscomputer : null;
        }

        //---------------------------------------------------------------------
        private FahrzeugBeladung[] GetBeladungen()
        {
            List<FahrzeugBeladung> list = new();

            foreach (Fahrzeug f in _includes)
            {
                list.AddRange(f.Beladungen);
            }

            if (Parent is Fahrzeug fzg)
            {
                list.AddRange(fzg.Beladungen);
            }

            return list.ToArray();
        }

        //---------------------------------------------------------------------
        private Bremssystem[] GetBremssystems()
        {
            List<Bremssystem> bss = new();

            foreach (Fahrzeug f in _includes)
            {
                bss.AddRange(f.Bremssysteme);
            }

            if (Parent is Fahrzeug fzg)
            {
                bss.AddRange(fzg.Bremssysteme);
            }

            return bss.ToArray();
        }

        //---------------------------------------------------------------------
        private List<FzgTuerSystemBasis> GetDoorSystems()
        {
            if (_doorSystems == null)
            {
                _doorSystems = new List<FzgTuerSystemBasis>();

                foreach (Fahrzeug f in _includes)
                {
                    _doorSystems.AddRange(f.DoorSystems);
                }

                if (Parent is Fahrzeug fzg)
                {
                    _doorSystems.AddRange(fzg.DoorSystems);
                }
            }

            return _doorSystems;
        }

        //---------------------------------------------------------------------
        private Antriebsmodell GetDrivetrain()
        {
            foreach (Fahrzeug f in _includes)
            {
                if (f.Drivetrain != null)
                {
                    return f.Drivetrain;
                }
            }

            return Parent is Fahrzeug fzg ? fzg.Drivetrain : null;
        }

        //---------------------------------------------------------------------
        private Dynbremse GetDynamicBrake()
        {
            foreach (Fahrzeug f in _includes)
            {
                if (f.DynamicBrake != null)
                {
                    return f.DynamicBrake;
                }
            }

            return Parent is Fahrzeug fzg ? fzg.DynamicBrake : null;
        }

        //---------------------------------------------------------------------
        private FahrzeugGrunddaten GetGrunddaten()
        {
            foreach (Fahrzeug f in _includes)
            {
                if (f.Grunddaten != null)
                {
                    return f.Grunddaten;
                }
            }

            Fahrzeug fzg = FindParent<Fahrzeug>();
            return fzg.Grunddaten;
        }

        //---------------------------------------------------------------------
        private double GetMasse()
        {
            return Grunddaten is FahrzeugGrunddaten g ? g.Masse : 0;
        }

        //---------------------------------------------------------------------
        private MgBremse GetRailBrake()
        {
            foreach (Fahrzeug f in _includes)
            {
                MgBremse b = f.RailBrake;
                if (b != null)
                {
                    return b;
                }
            }

            return Parent is Fahrzeug fzg ? fzg.RailBrake : null;
        }

        //---------------------------------------------------------------------
        private Handbremse GetHandbrake()
        {
            foreach (Fahrzeug f in _includes)
            {
                Handbremse b = f.Handbrake;
                if (b != null)
                {
                    return b;
                }
            }

            return Parent is Fahrzeug fzg ? fzg.Handbrake : null;
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

            return Parent is Fahrzeug fzg && fzg.HasKlotzBremse;
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

            return Parent is Fahrzeug fzg && fzg.HasScheibenBremse;
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

            return Parent is Fahrzeug fzg && fzg.HasSingleReleaseBrake;
        }
    }
}
