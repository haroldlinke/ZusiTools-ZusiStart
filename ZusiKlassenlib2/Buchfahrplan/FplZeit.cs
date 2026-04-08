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
    public enum FplEintrag
    {
        Default,
        Helper,
        StopOnRequest,
        InternalStop
    }

    //---------------------------------------------------------------------
    [Serializable]
    public class FplZeit : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "FplEintrag"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly string _attrName;
        private readonly FplEintrag _fplEintrag;
        private readonly DateTime? _time;

        public FplEintrag FplEintrag => _fplEintrag;
        public DateTime? Time { get { return _time; } }

        //---------------------------------------------------------------------
        protected FplZeit(IZusiObjectParent parent, XElement x, string attrName)
            : base(parent, x)
        {
            _attrName = attrName;
            _fplEintrag = x.GetAttrEnum<FplEintrag>("FplEintrag", FplEintrag.Default);
            _time = ZusiDate.Parse(x.GetAttrValue(attrName, string.Empty));
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            if (_time.HasValue)
            {
                writer.WriteAttribute(_attrName, _time.Value.ToString(ZusiDate.LongDateFormat));
            }
            writer.WriteAttributeIf(_fplEintrag > 0, "FplEintrag", _fplEintrag);
        }
    }

    //---------------------------------------------------------------------
    [Serializable]
    public class FplAnk : FplZeit
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Ank",
            "FplWichtigkeit"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly int _importance;

        public int Importance => _importance;

        //---------------------------------------------------------------------
        public FplAnk(IZusiObjectParent parent, XElement x)
            : base(parent, x, "Ank")
        {
            _importance = x.GetAttrValue("FplWichtigkeit", 0);
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeIf(_importance > 0, "FplWichtigkeit", _importance);
        }
    }

    //---------------------------------------------------------------------
    [Serializable]
    public class FplAbf : FplZeit
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Abf"
        };
#pragma warning restore IDE0052
        #endregion

        //---------------------------------------------------------------------
        public FplAbf(IZusiObjectParent parent, XElement x)
            : base(parent, x, "Abf")
        { }
    }
}
