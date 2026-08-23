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
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Fahrplan
{
    [Serializable]
    public class FahrplanSignalEintrag : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "FahrplanSignal"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly string _fahrplanSignal;

        public string FahrplanSignal => _fahrplanSignal;

        //---------------------------------------------------------------------
        public FahrplanSignalEintrag(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _fahrplanSignal = x.GetAttrValue("FahrplanSignal", string.Empty);
        }

        //---------------------------------------------------------------------
        public FahrplanSignalEintrag(IZusiObjectParent parent, FahrplanSignalEintrag source)
            : base(parent, source)
        {
            _fahrplanSignal = source._fahrplanSignal;
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);
            writer.WriteAttributeIf(!string.IsNullOrEmpty(_fahrplanSignal), "FahrplanSignal", _fahrplanSignal);
        }
    }
}
