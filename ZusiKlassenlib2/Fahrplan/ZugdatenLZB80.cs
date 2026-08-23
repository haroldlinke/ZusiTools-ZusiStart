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
    //---------------------------------------------------------------------
    [Serializable]
    public class ZugdatenLZB80 : ZugdatenIndusiRechner
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "LZBGefuehrt",
            "LZBStoerschalter",
            "VMZ",
            "ZL"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly IntAttribute _lzbGefuehrt;
        private ZusiTriState _lzbStoerschalter;
        private readonly IntAttribute _vmz;
        private readonly IntAttribute _zl;

        //---------------------------------------------------------------------
        public int LZBGefuehrt
        {
            get => _lzbGefuehrt;
            set => _lzbGefuehrt.Value = value;
        }

        //---------------------------------------------------------------------
        public ZusiTriState LZBStoerschalter
        {
            get => _lzbStoerschalter;
            set => _lzbStoerschalter = value;
        }

        //---------------------------------------------------------------------
        public int VMZ
        {
            get => _vmz;
            set => _vmz.Value = value;
        }

        //---------------------------------------------------------------------
        public int ZL
        {
            get => _zl;
            set => _zl.Value = value;
        }

        //---------------------------------------------------------------------
        public ZugdatenLZB80()
        { }

        //---------------------------------------------------------------------
        public ZugdatenLZB80(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _lzbGefuehrt = new IntAttribute(x, "LZBGefuehrt");
            _lzbStoerschalter = TriStateConverter.Convert(x.GetAttrValue("LZBStoerschalter", ""));
            _vmz = new IntAttribute(x, "VMZ");
            _zl = new IntAttribute(x, "ZL");
        }

        //---------------------------------------------------------------------
        public ZugdatenLZB80(IZusiObjectParent parent, ZugdatenLZB80 source)
    : base(parent, source)
    {
            _lzbGefuehrt = new IntAttribute(source._lzbGefuehrt);
            _lzbStoerschalter = source._lzbStoerschalter;
            _vmz = new IntAttribute(source._vmz);
            _zl = new IntAttribute(source._zl);
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            _lzbGefuehrt.Write(writer);
            writer.WriteAttributeIf(_lzbStoerschalter != ZusiTriState.Undefined, "LZBStoerschalter", (int)_lzbStoerschalter);
            _vmz.Write(writer);
            _zl.Write(writer);
        }
    }
}
