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
 * 
 * 4.3 - 01.01.2021
 * Bugfix: Element p und phi sind optional
*/

using System;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Route;

namespace ZusiKlassenLib2.TimeTable
{
    //---------------------------------------------------------------------
    [Serializable]
    public class StrModul : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownElems =
        {
            "Datei",
            "p",
            "phi"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly Datei _datei;
        private readonly p _p;
        private readonly phi _phi;
        private Strecke _route;

        public Datei Datei => _datei;

        public Strecke Strecke => GetRoute();

        //---------------------------------------------------------------------
        public StrModul(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _datei = new(this, x.Element("Datei"));
            _p = x.GetOptionalElement(this, "p", (pa, xx) => new p((ZusiObject)pa, xx));
            _phi = x.GetOptionalElement(this, "phi", (pa, xx) => new phi((ZusiObject)pa, xx));
        }

        //---------------------------------------------------------------------
        public StrModul(IZusiObjectParent parent, StrModul source)
            : base(parent, source)
        {
            _datei = new Datei(this, source._datei);
            if (source._p != null)
            {
                _p = new p(this, source._p);
            }
            if (source._phi != null)
            {
                _phi = new phi(this, source._phi);
            }
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);
            _datei.Save(writer);
            _p?.Save(writer);
            _phi?.Save(writer);
        }

        //---------------------------------------------------------------------
        private Strecke GetRoute()
        {
            if (_route == null)
            {
                try
                {
                    StreckenDatei sd = new(_datei.FullPath);
                    sd.Parse(true);
                    _route = sd.Root;
                }
                catch { }
            }
            return _route;
        }
    }
}
