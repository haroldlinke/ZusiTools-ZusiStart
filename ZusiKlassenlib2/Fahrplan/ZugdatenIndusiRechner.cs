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

namespace ZusiKlassenLib2.Fahrplan
{
    [Serializable]
    public class ZugdatenIndusiRechner : Zugdaten
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "TfNummer",
        };
#pragma warning restore IDE0052
        #endregion

        private readonly StringAttribute _tfNummer;

        //---------------------------------------------------------------------
        public string TfNummer
        {
            get => _tfNummer;
            set => _tfNummer.Value = value;
        }

        //---------------------------------------------------------------------
        public ZugdatenIndusiRechner()
        { }

        //---------------------------------------------------------------------
        public ZugdatenIndusiRechner(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _tfNummer = new StringAttribute(x, "TfNummer");
        }

        //---------------------------------------------------------------------
        public ZugdatenIndusiRechner(IZusiObjectParent parent, ZugdatenIndusiRechner source)
            : base(parent, source)
        {
            _tfNummer = new StringAttribute(source._tfNummer);
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            _tfNummer.Write(writer);
        }
    }
}
