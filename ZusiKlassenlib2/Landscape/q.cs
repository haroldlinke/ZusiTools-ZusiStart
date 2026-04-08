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
using System.Windows.Media.Media3D;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

#pragma warning disable IDE1006

namespace ZusiKlassenLib2.Landscape
{
    //=========================================================================
    public class q : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "X",
            "Y",
            "Z",
            "W"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly float _x;
        private readonly float _y;
        private readonly float _z;
        private readonly float _w;

        public float X => _x;
        public float Y => _y;
        public float Z => _z;
        public float W => _w;

        //---------------------------------------------------------------------
        public q(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _x = GetAttrValueFloat(x, "X", 0.0f);
            _y = GetAttrValueFloat(x, "Y", 0.0f);
            _z = GetAttrValueFloat(x, "Z", 0.0f);
            _w = GetAttrValueFloat(x, "W", 0.0f);
        }

        //---------------------------------------------------------------------
        public override string ToString()
        {
            return string.Format("[{0:N3}; {1:N3}; {2:N3}; {3:N3}]", _x, _y, _z, _w);
        }

        //---------------------------------------------------------------------
        public static implicit operator Quaternion(q o)
        {
            return new Quaternion(o._x, o._y, o._z, o._w);
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeFloatIf(_x != 0, "X", _x, 4);
            writer.WriteAttributeFloatIf(_y != 0, "Y", _y, 4);
            writer.WriteAttributeFloatIf(_z != 0, "Z", _z, 4);
            writer.WriteAttributeFloatIf(_w != 0, "W", _w, 4);
        }
    }
}
