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
  public class Antriebsumschaltung: ZusiObject
  {
    #region attributes & elements
#pragma warning disable IDE0052
    
    private static readonly string[] _knownElems =
    {
            "Antriebskonfiguration"
        };
#pragma warning restore IDE0052
    #endregion

    
    private readonly Antriebskonfiguration _antriebskonfiguration;

    public Antriebsumschaltung(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    {
      if (x != null)
      {
        _antriebskonfiguration = new Antriebskonfiguration(this, x.Element("Antriebskonfiguration"));
      }
    }

    public Antriebsumschaltung(IZusiObjectParent parent, Antriebsumschaltung source)
        : base(parent, source)
    {
      _antriebskonfiguration = new Antriebskonfiguration(this, source._antriebskonfiguration);
    }

    protected override void SaveElements(XmlWriter writer)
    {
      base.SaveElements(writer);

      _antriebskonfiguration.Save(writer);
    }
  }
}
