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

namespace ZusiKlassenLib2.Landscape
{
    //---------------------------------------------------------------------
    public class AniPunkt : ZusiObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "AniZeit",
            "AniDimmung"
        };

        private static readonly string[] _knownElems =
        {
            "p",
            "q"
        };
#pragma warning restore IDE0052

        private readonly float _aniZeit;
        private readonly string _aniDimmung;
        private readonly ZPoint3D _p;
        private readonly q _q;

        public ZPoint3D P => _p;
        public q Q => _q;
        public float Zeit => _aniZeit;

        public AniPunkt(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _aniZeit = GetAttrValueFloat(x, "AniZeit", 0.0f);
            _aniDimmung = x.GetAttrValue("AniDimmung", "");
            _p = x.GetOptionalElement(this, "p", (p, c) => new ZPoint3D(p, c), ZPoint3D.Null);
            _q = new q(this, x.Element("q"));
        }

        public AniPunkt(IZusiObjectParent parent, AniPunkt source)
            : base(parent, source)
        {
#if false
            _aniZeit = source._aniZeit;
            _p = new Point3D(_p);
            _q = new q(_q);
#else
            throw new NotImplementedException();
#endif
        }

        public override string ToString()
        {
            return string.Format("{0}: P{1} Q{2}", _aniZeit, _p.ToString(), _q.ToString());
        }

        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeFloatIf(_aniZeit != 0, "AniZeit", _aniZeit, -1);
            writer.WriteAttributeStringIfNotEmpty("AniDimmung", _aniDimmung);
        }

        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _p?.Save(writer);
            _q.Save(writer);
        }
    }
}
