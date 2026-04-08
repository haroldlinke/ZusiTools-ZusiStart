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

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    //---------------------------------------------------------------------
    [Serializable]
    public class Kombischalter : Schalter
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Grundstellung",
            "NameSchalter",
            "FedertUnten",
            "FedertOben",
            "Mittelposition"
        };

        private static readonly string[] _knownElems =
        {
            "Raste"
        };
#pragma warning restore IDE0052

        // NameSchalter="Schalter Führerbremsventil_5" FktName="D5-Regler" Tastaturzuordnung="4">
        private readonly bool _federtOben;
        private readonly bool _federtUnten;
        private readonly int _grundStellung;
        private readonly int _mittelposition;

        public bool FedertOben { get { return _federtOben; } }
        public bool FedertUnten { get { return _federtUnten; } }
        public int GrundStellung { get { return _grundStellung; } }
        public int Mittelposition { get { return _mittelposition; } }

        public List<Raste> Rasten { get { return Objects<Raste>(); } }

        public Kombischalter(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            string s;

            s = Attribute("FedertOben");
            _federtOben = !string.IsNullOrEmpty(s) && int.Parse(s) != 0;

            s = Attribute("FedertUnten");
            _federtUnten = !string.IsNullOrEmpty(s) && int.Parse(s) != 0;

            s = Attribute("Grundstellung");
            _grundStellung = string.IsNullOrEmpty(s) ? 0 : int.Parse(s);

            s = Attribute("Mittelposition");
            _mittelposition = string.IsNullOrEmpty(s) ? 0 : int.Parse(s);
        }
    }
}
