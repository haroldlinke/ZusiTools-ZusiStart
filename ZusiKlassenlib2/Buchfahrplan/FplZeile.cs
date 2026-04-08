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
  public enum FplRglGgl
  {
    Arbitrary,
    Single,
    Regular,
    Wrong
  }

  //---------------------------------------------------------------------
  [Serializable]
  public class FplZeile : ZusiObject
  {
    //private static readonly ILog _log = LogManager.GetLogger(typeof(FplZeile));

    #region attributes & elements
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "FplLaufweg",
            "FplRglGgl",
            "FahrstrStrecke",
            "FahrstrStreckeLa",
            "FahrstrStreckeStrukturnummer"
        };

    private static readonly string[] _knownElems =
    {
            "p",
            "FplIcon",
            "FplName",
            "FplvMax",
            "Fplkm",
            "FplAnk",
            "FplAbf",
            "FplNameRechts",
            "FplSignaltyp",
            "FplRichtungswechsel",
            "FplSaegelinien",
            "FplTunnel",
            "FplvMaxGNT",
            "FplvMaxReduzierungen"
        };
#pragma warning restore IDE0052
    #endregion

    private readonly double _laufweg;
    private readonly FplRglGgl _rglGgl;
    private readonly string _fahrstrStrecke;
    private readonly string _fahrstrStreckela;
    private readonly string _fahrstrStreckestrukturnummer;
    private readonly ZPoint3D _p;
    private readonly FplIcon _icon;
    private readonly FplName _name;
    private readonly FplvMax _vMax;
    private readonly Fplkm _km;
    private readonly FplAnk _ank;
    private readonly FplAbf _abf;
    private readonly FplNameRechts _nameRechts;
    private readonly FplSignaltyp _signalTyp;
    private readonly FplRichtungswechsel _richtungswechsel;
    private readonly FplSaegelinien _saegelinien;
    private readonly FplTunnel _tunnel;
    private readonly FplvMaxGNT _vMaxGNT;
    private readonly FplvMaxReduzierungen _maxReduzierungen;

    public double Laufweg => _laufweg;
    public FplRglGgl RglGgl => _rglGgl;
    public string FahrstrStrecke => _fahrstrStrecke;
    public string FahrstrStreckeLa => _fahrstrStreckela;
    public string FahrstrStreckeStrukturnummer => _fahrstrStreckestrukturnummer;
    public FplIcon Icon => _icon;
    public FplName Name => _name;
    public FplvMax VMax => _vMax;
    public Fplkm? Km => _km;
    public FplAnk? Ankunft => _ank;
    public FplAbf? Abfahrt => _abf;
    public FplNameRechts NameRechts => _nameRechts;
    public FplSignaltyp SignalTyp => _signalTyp;
    public FplRichtungswechsel Richtungswechsel => _richtungswechsel;
    public FplSaegelinien Saegelinien => _saegelinien;
    public FplTunnel Tunnel => _tunnel;
    public FplvMaxGNT VMaxGNT => _vMaxGNT;
    public ZPoint3D Location => _p;
    public FplvMaxReduzierungen MaxReduzierungen => _maxReduzierungen;

    public FplZeile(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    {
      _laufweg = GetAttrValueDouble(x, "FplLaufweg", 0.0);
      _rglGgl = x.GetAttrEnum<FplRglGgl>("FplRglGgl", FplRglGgl.Arbitrary);
      _fahrstrStrecke = x.GetAttrValue("FahrstrStrecke", string.Empty);
      _fahrstrStreckela = x.GetAttrValue("FahrstrStreckeLa", string.Empty);
      _fahrstrStreckestrukturnummer = x.GetAttrValue("FahrstrStreckeStrukturnummer", string.Empty);

      foreach (XElement xe in x.Elements())
      {
        if (xe.Name.LocalName == "Fplkm")
        {
          Fplkm km = new(this, xe);
          if (_km != null)
          {
            //ZusiDocumentBase doc = GetDocument();
            //_log.Debug($"Multiple node 'Fplkm' found in file {doc.Filename}, FplZeile.Laufweg {_laufweg}");
          }
          else
          {
            _km = km;
          }
        }
        else if (xe.Name.LocalName == "FplvMax")
        {
          _vMax = GetOptionalObject<FplvMax>(this, x.Element("FplvMax"));
        }
        else if (xe.Name.LocalName == "FplvMaxGNT")
        {
          _vMaxGNT = new FplvMaxGNT(this, xe);
        }
        else
        {
          if (xe.Name.LocalName == "p")
          {
            _p = new p(this, xe);
          }
          else if (xe.Name.LocalName == "FplIcon")
          {
            _icon = new FplIcon(this, xe);
          }
          else if (xe.Name.LocalName == "FplName")
          {
            _name = new FplName(this, xe);
          }
          else if (xe.Name.LocalName == "FplNameRechts")
          {
            _nameRechts = new FplNameRechts(this, xe);
          }
          else if (xe.Name.LocalName == "FplAnk")
          {
            _ank = new FplAnk(this, xe);
          }
          else if (xe.Name.LocalName == "FplAbf")
          {
            _abf = new FplAbf(this, xe);
          }
          else if (xe.Name.LocalName == "FplSignaltyp")
          {
            _signalTyp = new FplSignaltyp(this, xe);
          }
          else if (xe.Name.LocalName == "FplRichtungswechsel")
          {
            _richtungswechsel = new FplRichtungswechsel(this, xe);
          }
          else if (xe.Name.LocalName == "FplSaegelinien")
          {
            _saegelinien = new FplSaegelinien(this, xe);
          }
          else if (xe.Name.LocalName == "FplTunnel")
          {
            _tunnel = new FplTunnel(this, xe);
          }
          else if (xe.Name.LocalName == "FplvMaxReduzierungen")
          {
            _maxReduzierungen = new FplvMaxReduzierungen(this, xe);
          }
        }
      }
    }

    protected override void SaveAttributes(XmlWriter writer)
    {
      base.SaveAttributes(writer);

      writer.WriteAttributeDoubleIf(_laufweg != 0, "FplLaufweg", _laufweg, 4);
      writer.WriteAttributeIf(_rglGgl != FplRglGgl.Arbitrary, "FplRglGgl", (int)_rglGgl);
      writer.WriteAttributeStringIfNotEmpty("FahrstrStrecke", _fahrstrStrecke);
      writer.WriteAttributeStringIfNotEmpty("FahrstrStreckeLa", _fahrstrStreckela);
      writer.WriteAttributeStringIfNotEmpty("FahrstrStreckeStrukturnummer", _fahrstrStreckestrukturnummer);

    }

    protected override void SaveElements(XmlWriter writer)
    {
      base.SaveElements(writer);

      _p?.Save(writer);
      _icon?.Save(writer);
      _name?.Save(writer);
      _nameRechts?.Save(writer);
      _signalTyp?.Save(writer);
      _vMax?.Save(writer);
      _vMaxGNT?.Save(writer);
      _km?.Save(writer);
      _ank?.Save(writer);
      _abf?.Save(writer);
      _richtungswechsel?.Save(writer);
      _saegelinien?.Save(writer);
      _tunnel?.Save(writer);
    }
  }
}
