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

namespace ZusiKlassenLib2.Fahrplan
{
  //---------------------------------------------------------------------
  [Serializable]
  public class AbhAbhaengigkeit : ZusiObject
  {
    #region attributes & elements
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "AbhBedingung",
            "AbhParameter",
            "AbhAndererZug",
            "AbhOperator",
            "ETCSModus"
        };

    private static readonly string[] _knownElems =
    {
            "AbhAbhaengigkeit"
        };
#pragma warning restore IDE0052
    #endregion

    private readonly string _abhBedingung;
    private readonly string _abhParameter;
    private readonly string _abhAndererZug;
    private readonly string _abhOperator;
    private readonly string _etcsModus;

    private readonly List<AbhAbhaengigkeit> _abhaengigkeiten = new();

    public AbhAbhaengigkeit(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    {
      if (x != null)
      {
        _abhBedingung = x.GetAttrValue("AbhBedingung", string.Empty);
        _abhParameter = x.GetAttrValue("AbhParameter", string.Empty);
        _abhAndererZug = x.GetAttrValue("AbhAndererZug", string.Empty);
        _abhOperator = x.GetAttrValue("AbhOperator", string.Empty);
        _etcsModus = x.GetAttrValue("ETCSModus", string.Empty);

        foreach (XElement xx in x.Elements("AbhAbhaengigkeit"))
        {
          _abhaengigkeiten.Add(new AbhAbhaengigkeit(this, xx));
        }
      }
    }

    public AbhAbhaengigkeit(IZusiObjectParent parent, AbhAbhaengigkeit source)
        : base(parent, source)
    {
      _abhBedingung = source._abhBedingung;
      _abhParameter = source._abhParameter;
      _abhAndererZug = source._abhAndererZug;
      _abhOperator = source._abhOperator;
      _etcsModus = source._etcsModus;

      foreach (AbhAbhaengigkeit a in source._abhaengigkeiten)
      {
        _abhaengigkeiten.Add(new AbhAbhaengigkeit(this, a));
      }
    }

    protected override void SaveAttributes(XmlWriter writer)
    {
      base.SaveAttributes(writer);

      writer.WriteAttributeIf(!string.IsNullOrEmpty(_abhBedingung), "AbhBedingung", _abhBedingung);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_abhParameter), "AbhParameter", _abhParameter);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_abhAndererZug), "AbhAndererZug", _abhAndererZug);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_abhOperator), "AbhOperator", _abhOperator);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_abhOperator), "ETCSModus", _etcsModus);
    }

    protected override void SaveElements(XmlWriter writer)
    {
      base.SaveElements(writer);
      _abhaengigkeiten.ForEach(a => a.Save(writer));
    }
  }
}
