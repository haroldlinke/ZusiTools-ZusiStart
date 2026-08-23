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

namespace ZusiKlassenLib2.Fahrplan
{
#pragma warning disable CA1069
    public enum Zugart
    {
        None = 0,
        // Zugarten allgemein
        Unknown = 1,
        U = 2,
        M = 3,
        O = 4,
        S = 5,
        // BRA
        BRA1 = 1,
        BRA8 = 8,
        // Zugarten für PZ80
        ZA16 = 1,
        ZA15 = 2,
        ZA14 = 3,
        ZA13 = 4,
        ZA12 = 5,
        ZA11 = 6,
        ZA10 = 7,
        ZA9 = 8,
        ZA8 = 9,
        ZA7 = 10,
        ZA6 = 11,
        ZA4 = 12,
        ZA1 = 13
    }
#pragma warning restore CA1069

    //=========================================================================
    [Serializable]
    public class ZugdatenIndusiAnalog_PZ80 : Zugdaten
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "IndusiZugart"
        };
#pragma warning restore IDE0052
        #endregion

        private Zugart _indusiZugart;

        //---------------------------------------------------------------------
        public Zugart IndusiZugart
        {
            get => _indusiZugart;
            set => _indusiZugart = value;
        }

        //---------------------------------------------------------------------
        public ZugdatenIndusiAnalog_PZ80()
        { }

        //---------------------------------------------------------------------
        public ZugdatenIndusiAnalog_PZ80(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            string s = x.GetAttrValue("IndusiZugart", "");
            if (!string.IsNullOrEmpty(s))
            {
                _indusiZugart = (Zugart)int.Parse(s);
            }
        }

        //---------------------------------------------------------------------
        public ZugdatenIndusiAnalog_PZ80(IZusiObjectParent parent, ZugdatenIndusiAnalog_PZ80 source)
            : base(parent, source)
        {
            _indusiZugart = source._indusiZugart;
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeIf(_indusiZugart != Zugart.None, "IndusiZugart", (int)_indusiZugart);
        }
    }

    //=========================================================================
    [Serializable]
    public class ZugdatenIndusiAnalog : ZugdatenIndusiAnalog_PZ80
    {
        //---------------------------------------------------------------------
        public ZugdatenIndusiAnalog()
        { }

        //---------------------------------------------------------------------
        public ZugdatenIndusiAnalog(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }

        //---------------------------------------------------------------------
        public ZugdatenIndusiAnalog(IZusiObjectParent parent, ZugdatenIndusiAnalog_PZ80 source)
            : base(parent, source)
        { }
    }

    //=========================================================================
    [Serializable]
    public class ZugdatenIndusiPZ80 : ZugdatenIndusiAnalog_PZ80
    {
        //---------------------------------------------------------------------
        public ZugdatenIndusiPZ80()
        { }

        //---------------------------------------------------------------------
        public ZugdatenIndusiPZ80(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }

        //---------------------------------------------------------------------
        public ZugdatenIndusiPZ80(IZusiObjectParent parent, ZugdatenIndusiAnalog_PZ80 source)
            : base(parent, source)
        { }
    }
}
