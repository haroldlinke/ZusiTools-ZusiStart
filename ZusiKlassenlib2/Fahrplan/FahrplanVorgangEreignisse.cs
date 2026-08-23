/*
 * Copyright 2019 Holger Maaß
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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Fahrplan
{
    //---------------------------------------------------------------------
    public class FahrplanVorgangEreignisse : ZusiObject
    {
        #region attributes & elements
        private static readonly string[] _knownAttribs =
        {
            "Beschreibung"
        };

        private static readonly string[] _knownElems =
        {
            "AbhAbhaengigkeit",
            "Ereignis"
        };
        #endregion

        private readonly string _beschreibung;

        private readonly List<AbhAbhaengigkeit> _abhaengigkeiten = new();
        private readonly List<Ereignis> _ereignisse = new();

        public FahrplanVorgangEreignisse(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _beschreibung = x.GetAttrValue(_knownAttribs[0], (string)null);

            _abhaengigkeiten.AddRange(from XElement xe in x.Elements(_knownElems[0])
                                      select new AbhAbhaengigkeit(this, xe));

            _ereignisse.AddRange(from XElement xe in x.Elements(_knownElems[1])
                                 select new Ereignis(this, xe));
        }

        public FahrplanVorgangEreignisse(IZusiObjectParent parent, FahrplanVorgangEreignisse source)
            : base(parent, source)
        {
            _beschreibung = source._beschreibung;

            _abhaengigkeiten.AddRange(from AbhAbhaengigkeit a in source._abhaengigkeiten
                                      select new AbhAbhaengigkeit(this, a));

            _ereignisse.AddRange(from Ereignis e in source._ereignisse
                                 select new Ereignis(this, e));
        }

        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeIf(!string.IsNullOrEmpty(_beschreibung), _knownAttribs[0], _beschreibung);
        }

        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _abhaengigkeiten.ForEach(a => a.Save(writer));
            _ereignisse.ForEach(e => e.Save(writer));
        }
    }
}
