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

using Sovoma;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2
{
    [Serializable]
    public class Info : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "DateiKategorie",
            "DateiTyp",
            "Version",
            "MinVersion",
            "Beschreibung",
            "EinsatzAb",
            "EinsatzBis",
            // eher fehlerhafte Attribute
            "Autor",
            //"Lizenz"
        };

        private static readonly string[] _knownElems =
        {
            "AutorEintrag",
            "Datei"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly string _dateiTyp;
        private readonly string _version;
        private readonly string _minVersion;
        private readonly string _beschreibung;
        private readonly string _einsatzAb;
        private readonly string _einsatzBis;
        private readonly List<AutorEintrag> _autorEintraege = new();
        private readonly List<Datei> _files = new();

        public List<AutorEintrag> AutorEintraege => _autorEintraege;
        public string Beschreibung => _beschreibung;
        public string DateiTyp => _dateiTyp;
        public string EinsatzAb => _einsatzAb;
        public string EinsatzBis => _einsatzBis;
        public List<Datei> Files => _files;
        public string MinVersion => _minVersion;
        public string Version => _version;

        //---------------------------------------------------------------------
        public Info(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _dateiTyp = x.GetAttrValue("DateiTyp", "");
            _version = x.GetAttrValue("Version", "");
            _minVersion = x.GetAttrValue("MinVersion", "");
            _beschreibung = x.GetAttrValue("Beschreibung", "");
            _einsatzAb = x.GetAttrValue("EinsatzAb", "");
            _einsatzBis = x.GetAttrValue("EinsatzBis", "");

            foreach (XElement xa in x.Elements("AutorEintrag"))
            {
                _autorEintraege.Add(new AutorEintrag(this, xa));
            }

            foreach (XElement xd in x.Elements("Datei"))
            {
                _files.Add(new Datei(this, xd));
            }
        }

        //---------------------------------------------------------------------
        public void AddMyAuthorEntry()
        {
            if (_autorEintraege.FirstOrDefault(a => a.Id == 80) == null)
            {
                _autorEintraege.Add(AutorEintrag.MyAuthorEntry);
                IsDirty = true;
            }
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeStringIfNotEmpty("DateiTyp", _dateiTyp);
            writer.WriteAttributeStringIfNotEmpty("Version", _version);
            writer.WriteAttributeStringIfNotEmpty("MinVersion", _minVersion);
            writer.WriteAttributeStringIfNotEmpty("Beschreibung", _beschreibung);
            writer.WriteAttributeStringIfNotEmpty("EinsatzAb", _einsatzAb);
            writer.WriteAttributeStringIfNotEmpty("EinsatzBis", _einsatzBis);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            foreach (AutorEintrag ae in _autorEintraege)
            {
                ae.Save(writer);
            }

            foreach(Datei d in _files)
            {
                d.Save(writer);
            }
        }
    }
}
