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
using System.Xml.Linq;
using System.Xml;
using ZusiKlassenLib2.Common;
using System;

namespace ZusiKlassenLib2.Vehicle
{
    //---------------------------------------------------------------------
    [Serializable]
    public class Bremse : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "BremsGewicht"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly double _bremsGewicht;

        public double BremsGewicht => _bremsGewicht;

        public Bremse(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _bremsGewicht = GetAttrValueDouble(x, "BremsGewicht", 0.0);
        }

        public Bremse(IZusiObjectParent parent, Bremse source)
            : base(parent, source)
        { }

        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeDouble("BremsGewicht", _bremsGewicht, -1);
        }
    }
}
