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
  [Serializable]
  public class Zugdaten : ZusiObject
  {
    #region attributes & elements
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "ZugsicherungHS",
            "Lufthahn",
            "PZBStoerschalter",
            "IndusiZugart",
            "BRA",
            "BRH"
        };
#pragma warning restore IDE0052
    #endregion

    private ZusiTriState _zugsicherungHS;
    private ZusiTriState _lufthahn;
    private ZusiTriState _pzbStoerschalter;
    private Zugart _indusiZugart;
    private readonly IntAttribute _brh;
    private readonly IntAttribute _bra;

    //---------------------------------------------------------------------
    public ZusiTriState ZugsicherungHS
    {
      get => _zugsicherungHS;
      set => _zugsicherungHS = value;
    }

    //---------------------------------------------------------------------
    public ZusiTriState Lufthahn
    {
      get => _lufthahn;
      set => _lufthahn = value;
    }

    //---------------------------------------------------------------------
    public ZusiTriState PZBStoerschalter
    {
      get => _pzbStoerschalter;
      set => _pzbStoerschalter = value;
    }

    //---------------------------------------------------------------------
    public Zugart IndusiZugart
    {
      get => _indusiZugart;
      set => _indusiZugart = value;
    }

    //---------------------------------------------------------------------
    public int BRH
    {
      get => _brh;
      set => _brh.Value = value;
    }

    //---------------------------------------------------------------------
    public int BRA
    {
      get => _bra;
      set => _bra.Value = value;
    }

    //---------------------------------------------------------------------
    public Zugdaten()
    { }

    //---------------------------------------------------------------------
    public Zugdaten(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    {
      _zugsicherungHS = TriStateConverter.Convert(x.GetAttrValue("ZugsicherungHS", ""));
      _lufthahn = TriStateConverter.Convert(x.GetAttrValue("Lufthahn", ""));
      _pzbStoerschalter = TriStateConverter.Convert(x.GetAttrValue("PZBStoerschalter", ""));
      _bra = new IntAttribute(x, "BRA");
      _brh = new IntAttribute(x, "BRH");
      string s = x.GetAttrValue("IndusiZugart", "");
      if (!string.IsNullOrEmpty(s))
      {
        _indusiZugart = (Zugart)int.Parse(s);
      }
    }

    //---------------------------------------------------------------------
    public Zugdaten(IZusiObjectParent parent, Zugdaten source)
        : base(parent, source)
    {
      _zugsicherungHS = source._zugsicherungHS;
      _lufthahn = source._lufthahn;
      _pzbStoerschalter = source._pzbStoerschalter;
      _bra = new IntAttribute(source._bra);
      _brh = new IntAttribute(source._brh);
      _indusiZugart = source._indusiZugart;
    }

    //---------------------------------------------------------------------
    protected override void SaveAttributes(XmlWriter writer)
    {
      base.SaveAttributes(writer);

      writer.WriteAttributeIf(_zugsicherungHS != ZusiTriState.Undefined, "ZugsicherungHS", (int)_zugsicherungHS);
      writer.WriteAttributeIf(_lufthahn != ZusiTriState.Undefined, "Lufthahn", (int)_lufthahn);
      writer.WriteAttributeIf(_pzbStoerschalter != ZusiTriState.Undefined, "PZBStoerschalter", (int)_pzbStoerschalter);
      writer.WriteAttributeIf(_indusiZugart != Zugart.None, "IndusiZugart", (int)_indusiZugart);
      _bra.Write(writer);
      _brh.Write(writer);
    }
  }
}
