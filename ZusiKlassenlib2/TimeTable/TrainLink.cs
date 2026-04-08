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

using System;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Fahrplan;

namespace ZusiKlassenLib2.TimeTable
{
    //---------------------------------------------------------------------
    /* Verweis auf .trn-Datei
     */
    [Serializable]
    public class TrainLink : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownElems =
        {
            "Datei"
        };
#pragma warning restore IDE0052
        #endregion

        private Datei _datei;

        //---------------------------------------------------------------------
        public Datei Datei
        {
            get => _datei;
            set => _datei = value;
        }

        //---------------------------------------------------------------------
        public ZugDatei ZugDatei => _datei.Exists ? new ZugDatei(this, _datei.FullPath) : null;

        //---------------------------------------------------------------------
        public TrainLink()
        { }

        //---------------------------------------------------------------------
        public TrainLink(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _datei = new Datei(this, x.Element("Datei"));
        }

        //---------------------------------------------------------------------
        public TrainLink(IZusiObjectParent parent, TrainLink source)
            : base(parent, source)
        {
            _datei = new Datei(this, source._datei);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            _datei.Save(writer);
        }
    }
}
