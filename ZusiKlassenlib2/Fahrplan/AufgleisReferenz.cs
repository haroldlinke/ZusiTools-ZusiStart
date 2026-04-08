/*
 * Copyright 2018 Holger Maaß
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
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.TimeTable
{
    //---------------------------------------------------------------------
    [Serializable]
    public class AufgleisReferenz : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "ReferenzNr"
        };

        private static readonly string[] _knownElems =
        {
            "Datei"
        };
#pragma warning restore IDE0052
        #endregion

        private int _referenzNummer;
        private Datei _datei;

        //---------------------------------------------------------------------
        public Datei Datei
        {
            get => _datei;
            set => _datei = value;
        }

        //---------------------------------------------------------------------
        public int ReferenzNr
        {
            get => _referenzNummer;
            set => _referenzNummer = value;
        }

        //---------------------------------------------------------------------
        public AufgleisReferenz()
        { }

        //---------------------------------------------------------------------
        public AufgleisReferenz(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _referenzNummer = x.GetAttrValue(_knownAttribs[0], 0);
            _datei = new Datei(this, x.Element("Datei"));
        }

        //---------------------------------------------------------------------
        public AufgleisReferenz(IZusiObjectParent parent, AufgleisReferenz source)
            : base(parent, source)
        {
            _referenzNummer = source._referenzNummer;
            _datei = new Datei(this, source._datei);
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);
            writer.WriteAttributeIf(_referenzNummer > 0, _knownAttribs[0], _referenzNummer);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);
            _datei.Save(writer);
        }
    }
}
