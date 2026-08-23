/*
 * Copyright 2018-2021 Holger Maaß
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
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Fahrplan;

namespace ZusiKlassenLib2.TimeTable
{
    //---------------------------------------------------------------------
    [Serializable]
    public class TrainReference
    {
        public TrainLink Link { get; set; }
        public Zug Train { get; set; }

        //---------------------------------------------------------------------
        public TrainReference(TrainLink link)
        {
            Link = link;
            Train = null;
        }

        //---------------------------------------------------------------------
        public TrainReference(Zug train)
        {
            Link = null;
            Train = train;
        }

        //---------------------------------------------------------------------
        public TrainReference(IZusiObjectParent parent, TrainReference source)
        {
            if (source.Link != null)
            {
                Link = new TrainLink(parent, source.Link);
            }
            else
            {
                Train = new Zug(parent, source.Train);
            }
        }

        //---------------------------------------------------------------------
        public void Save(XmlWriter writer)
        {
            Link?.Save(writer);
            Train?.Save(writer);
        }
    }

    //---------------------------------------------------------------------
    /* .fpn-Datei
     */
    [Serializable]
    public class TimeTable : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "AnfangsZeit",
            "Zeitmodus",
            "trnDateien",
            "ChaosVorschlagen",
            "ChaosVorschlag"
        };

        private static readonly string[] _knownElems =
        {
            "BefehlsKonfiguration",
            "Begruessungsdatei",
            "Zug",
            "trn",
            "StrModul",
            "UTM",
            // werden nicht gelesen, keine entsprechenden Typen vorhanden
            "LaPDF",
            "StrebuPDF",
            "ErsatzfahrplaenePDF"
        };
#pragma warning restore IDE0052
        #endregion

        //private static readonly ILog _log = LogManager.GetLogger(typeof(TimeTable));

        [NonSerialized]
        private readonly bool _readonly;
        private DateTime? _startTime;
        private readonly string _zeitModus;
        [NonSerialized]
        private BefehlsKonfiguration _befehlsKonfiguration;
        private Begruessungsdatei _begruessungsdatei;
        [Obsolete("Property '_zuege' is obsolete now, due to the integrated schedules. Please use '_trains' now")]
        private readonly List<TrainLink> _zuege = new();
        private readonly List<TrainReference> _trains = new();
        [NonSerialized]
        private readonly List<StrModul> _strModules = new();
        [NonSerialized]
        private ZusiKlassenLib2.Common.UTM _utm;
        private LaPDF _laPDF;
    private StrebuPDF _strebuPDF;
    private ErsatzfahrplaenePDF _ersatzfahrplaenePDF;



    [NonSerialized]
        private readonly string _name;
        private readonly bool _referencedSchedules;
        private readonly bool _chaosVorschlagen;
        private readonly float _chaosVorschlag = float.NaN;

        // zum Konvertieren
        [NonSerialized]
        private List<Zug> _convertTrains;

        public DateTime? StartTime
        {
            get => _startTime;
            set => SetValue(ref _startTime, value);
        }

        public BefehlsKonfiguration BefehlsKonfiguration
        {
            get => _befehlsKonfiguration;
            set => SetValue(ref _befehlsKonfiguration, value);
        }

        public Begruessungsdatei Begruessungsdatei
        {
            get => _begruessungsdatei;
            set => SetValue(ref _begruessungsdatei, value);
        }

        [Obsolete("Property 'Zuege' is obsolete now, due to the integrated schedules. Please use 'Trains' now")]
        public List<TrainLink> Zuege { get { return _zuege; } }
        public List<TrainReference> Trains { get { return _trains; } }

        public List<StrModul> StrModules { get => _strModules; }

        public ZusiKlassenLib2.Common.UTM Utm
        {
            get => _utm;
            set => SetValue(ref _utm, value);
        }

    public LaPDF LaPDF
    {
      get => _laPDF;
      set => SetValue(ref _laPDF, value);
    }

    public StrebuPDF StrebuPDF
    {
      get => _strebuPDF;
      set => SetValue(ref _strebuPDF, value);
    }

    public ErsatzfahrplaenePDF ErsatzfahrplaenePDF
    {
      get => _ersatzfahrplaenePDF;
      set => SetValue(ref _ersatzfahrplaenePDF, value);
    }

    public string Name => _name;

        public bool HasIntegratedSchedules => !_referencedSchedules;

        //---------------------------------------------------------------------
        public TimeTable()
        { }

        //---------------------------------------------------------------------
        public TimeTable(ZusiDocumentBase parent, XElement x)
            : base(parent, x)
        {
            _readonly = true;

            _startTime = ZusiDate.Parse(x.GetAttrValue("AnfangsZeit", ""));
            _zeitModus = x.GetAttrValue("Zeitmodus", "");
            _referencedSchedules = x.GetAttrValue("trnDateien", false);
            _chaosVorschlagen = x.GetAttrValue("ChaosVorschlagen", false);
            _chaosVorschlag = x.GetAttrValue("ChaosVorschlag", float.NaN);

            _befehlsKonfiguration = x.GetOptionalElement(this, "BefehlsKonfiguration", (p, c) => new BefehlsKonfiguration((ZusiObject)p, c));
            _begruessungsdatei = x.GetOptionalElement(this, "Begruessungsdatei", (p, c) => new Begruessungsdatei((ZusiObject)p, c));

            foreach (XElement xx in x.Elements("Zug"))
            {
                _trains.Add(new TrainReference(new TrainLink(this, xx)));
            }
            if (_trains.Count > 0)
            {
                _referencedSchedules = true;
            }

            foreach (XElement xx in x.Elements("trn"))
            {
                _trains.Add(new TrainReference(new Zug(parent, xx)));
            }

            foreach (XElement xx in x.Elements("StrModul"))
            {
                _strModules.Add(new StrModul(this, xx));
            }

            _utm = x.GetOptionalElement(this, "UTM", (p, c) => new ZusiKlassenLib2.Common.UTM((ZusiObject)p, c));

      _laPDF = x.GetOptionalElement(this, "LaPDF", (p, c) => new LaPDF((ZusiObject)p, c));

      _strebuPDF = x.GetOptionalElement(this, "StrebuPDF", (p, c) => new StrebuPDF((ZusiObject)p, c));

      _ersatzfahrplaenePDF = x.GetOptionalElement(this, "ErsatzfahrplaenePDF", (p, c) => new ErsatzfahrplaenePDF((ZusiObject)p, c));



      _name = Path.GetFileNameWithoutExtension(parent.Filename).Replace('_', ' ');
        }

        //---------------------------------------------------------------------
        public TimeTable(IZusiObjectParent parent, TimeTable source, bool withTrains)
            : base(parent, source)
        {
            _readonly = false;

            _startTime = source._startTime;
            _zeitModus = source._zeitModus;
            _befehlsKonfiguration = source._befehlsKonfiguration;
            _begruessungsdatei = source._begruessungsdatei;
            _utm = source._utm;
            _name = source._name;
      _laPDF = source._laPDF;
      _strebuPDF = source._strebuPDF;
      _ersatzfahrplaenePDF = source._ersatzfahrplaenePDF;
      _referencedSchedules = source._referencedSchedules;
            _chaosVorschlagen = source._chaosVorschlagen;
            _chaosVorschlag = source._chaosVorschlag;

            if (withTrains)
            {
                foreach (TrainReference tref in source._trains)
                {
                    _trains.Add(new TrainReference(this, tref));
                }
            }
            foreach (StrModul sm in source._strModules)
            {
                _strModules.Add(new StrModul(this, sm));
            }
        }

        //---------------------------------------------------------------------
        public void ConvertToIntegratedTimetable(string timeTableName)
        {
            TimeTable tt = new(null, this, false)
            {
                _convertTrains = new List<Zug>()
            };

            DataPathType dpt = DataPathType.Unknown;
            string relativeName = Zusi.GetRelativePathOf(timeTableName, ref dpt);

            var q = from tr in _trains
                    where tr.Link != null
                    select tr.Link.Datei.FullPath;
            foreach (var path in q)
            {
                ZugDatei zd = new(null, path);
                zd.Parse();
                Zug z = zd.Root;
                z.NodeName = "trn";
                z.FahrplanDatei.Dateiname = relativeName;
                tt._convertTrains.Add(z);
            }

            TimeTableFile ttf = new(GetDocument() as TimeTableFile, tt);
            ttf.SaveAs(timeTableName);
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);
            writer.WriteAttributeDateTimeIf(_startTime != null, "AnfangsZeit", _startTime.Value, "yyyy-MM-dd HH:mm:ss");
            writer.WriteAttributeIf(!string.IsNullOrEmpty(_zeitModus), "Zeitmodus", _zeitModus);
            writer.WriteAttributeIf(_referencedSchedules, "trnDateien", 1);
            writer.WriteAttributeIf(_chaosVorschlagen, "ChaosVorschlagen", 1);
            writer.WriteAttributeFloatIf(!float.IsNaN(_chaosVorschlag), "ChaosVorschlag", _chaosVorschlag, 3);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);
            _befehlsKonfiguration?.Save(writer);
            _begruessungsdatei?.Save(writer);
            if (_convertTrains?.Count > 0)
            {
                _convertTrains.ForEach(t => t.Save(writer));
            }
            else
            {
                _trains.ForEach(t => t.Save(writer));
            }
            _strModules.ForEach(s => s.Save(writer));
            _utm?.Save(writer);
      _laPDF?.Save(writer);
      _strebuPDF?.Save(writer);
      _ersatzfahrplaenePDF?.Save(writer);
    }

        //---------------------------------------------------------------------
        private void SetValue<T>(ref T obj, T value)
        {
            if (_readonly)
            {
                throw new InvalidOperationException("Can't change value, due to timetable is readonly.");
            }
            obj = value;
        }
    }
}
