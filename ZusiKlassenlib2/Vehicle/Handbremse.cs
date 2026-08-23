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

namespace ZusiKlassenLib2.Vehicle
{
  //---------------------------------------------------------------------
  [Serializable]
  public class Handbremse : Bremse
  {
    #region attributes & elements
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "Bodenbedienbar",
            "Bremsgewicht",
            "Bremskraft"
        };

    private static readonly string[] _knownElems =
    {
      "BremsenKennungV"
        };
#pragma warning restore IDE0052
    #endregion

    private readonly BremsenKennungV _bremsenKennungV;
    private readonly string _bodenbedienbar;


    //---------------------------------------------------------------------
    public Handbremse(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    {
      _bremsenKennungV = new BremsenKennungV(this, x.Element("BremsenKennungV"));
      _bodenbedienbar = x.GetAttrValue("Bodenbedienbar", "");
    }

    //---------------------------------------------------------------------
    public Handbremse(IZusiObjectParent parent, Handbremse source)
        : base(parent, source)
    {
      //_bremsenKennungV = new(this, source._bremsenKennungV);
      _bodenbedienbar = source._bodenbedienbar;

    }

    protected override void SaveAttributes(XmlWriter writer)
    {
      base.SaveAttributes(writer);

      writer.WriteAttributeStringIfNotEmpty("Bodenbedienbar", _bodenbedienbar);
    }

    //---------------------------------------------------------------------
    protected override void SaveElements(XmlWriter writer)
    {
      base.SaveElements(writer);

      _bremsenKennungV.Save(writer);
      
    }
  }
}
