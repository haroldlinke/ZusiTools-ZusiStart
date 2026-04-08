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
*/

using Sovoma;
using System;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Buchfahrplan
{
    //---------------------------------------------------------------------
    [Serializable]
    public class Fplkm : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "km",
            "FplSprung",
            "FplkmNeu"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly double _km;
        private readonly double _kmNeu;
        private readonly bool _sprung;

        public double Km => _km;
        public double KmNeu => _kmNeu;
        public bool Sprung => _sprung;

        //---------------------------------------------------------------------
        public Fplkm(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _km = GetAttrValueDouble(x,"km", 0.0);
            _sprung = x.GetAttrValue("FplSprung", false);
            _kmNeu = GetAttrValueDouble(x,"FplkmNeu", 0.0);
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeDoubleIf(_km != 0, "km", _km, 4);
            writer.WriteAttributeIf(_sprung, "FplSprung", 1);
            writer.WriteAttributeDoubleIf(_kmNeu != 0, "FplkmNeu", _kmNeu, 4);
        }
    }
}
