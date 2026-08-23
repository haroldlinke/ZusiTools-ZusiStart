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
  public class Antriebskonfiguration: ZusiObject
  {
    #region attributes & elements
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "Bezeichnung"
        };

    private static readonly string[] _knownElems =
    {
            "AntriebsZustand"
        };
#pragma warning restore IDE0052
    #endregion

    private readonly string _bezeichnung;

    private readonly Antriebszustand _antriebsZustand;

    public Antriebskonfiguration(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    {
      if (x != null)
      {
        _bezeichnung = x.GetAttrValue("Bezeichnung", string.Empty);

       

        _antriebsZustand = new Antriebszustand(this, x.Element("AntriebsZustand"));
      }
    }

    public Antriebskonfiguration(IZusiObjectParent parent, Antriebskonfiguration source)
        : base(parent, source)
    {
      _bezeichnung = source._bezeichnung;

      _antriebsZustand = new Antriebszustand(this, source._antriebsZustand);
    }

    protected override void SaveAttributes(XmlWriter writer)
    {
      base.SaveAttributes(writer);

      writer.WriteAttributeIf(!string.IsNullOrEmpty(_bezeichnung), "Bezeichnung", _bezeichnung);
    }

    protected override void SaveElements(XmlWriter writer)
    {
      base.SaveElements(writer);

      _antriebsZustand.Save(writer);
    }
  }
}
