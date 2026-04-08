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
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Vehicle
{
  //---------------------------------------------------------------------
  [Serializable]
  public class Antriebszustand : ZusiObject
  {
    #region attributes & elements
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "Aktiv",
            "Typ"
        };
    
#pragma warning restore IDE0052
    #endregion

    private readonly bool _aktiv;
    private readonly int _typ;
    
    public Antriebszustand(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    {
      if (x != null)
      {
        _aktiv = x.GetAttrValue("Aktiv", false);
        _typ = x.GetAttrValue("Typ", 0);
      }
    }

    public Antriebszustand(IZusiObjectParent parent, Antriebszustand source)
        : base(parent, source)
    {
      _aktiv = source._aktiv;
      _typ = source._typ;
    }

    protected override void SaveAttributes(XmlWriter writer)
    {
      base.SaveAttributes(writer);

      writer.WriteAttributeIf(_aktiv, "Aktiv", _aktiv? 1:0);
      writer.WriteAttributeIf(_typ != 0, "typ", _typ);
      }
  
  }
}
