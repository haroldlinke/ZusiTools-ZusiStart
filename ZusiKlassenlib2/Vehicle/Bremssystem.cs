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
  public enum Bremsstellung
  {
    Unknown,    // 0
    G,          // 1
    P,          // 2
    P_Mg,       // 3
    R,          // 4
    R_Mg        // 5
  }

  //---------------------------------------------------------------------
  public enum Bremsbauart
  {
    Undefined,
    Disc,
    GreyIronShoe,   // Grauguss
    K_Shoe,
    LL_Shoe,
    Matrossow
  }

  //---------------------------------------------------------------------
  [Serializable]
  public class Bremssystem : ZusiObject
  {
    //private static readonly ILog Log = LogManager.GetLogger(typeof(Bremssystem));

    #region attributes & elements
#pragma warning disable IDE0052
    //---------------------------------------------------------------------
    private static readonly string[] _knownAttribs =
    {
            "VolumenR",
            "VolumenA",
            "VolumenB",
            "VolumenZylinder",
            "FBremsuebersetzung",
            "HBLAnschluss",
            "AnzahlAchsen",
            "Bremsbauart",
            "Loesezugtyp",
            "Umstellgewicht",
            "AnzahlAbsperrhaehne"
        };

    //---------------------------------------------------------------------
    private static readonly string[] _knownElems =
    {
            "BremsenKennungV",
            "BremseP",
            "BremseG",
            "BremseR",
            "BremseRE",
            "BremsePE",
            "BremsePH",
            "BremseBeladenP",
            "BremseBeladenG"
        };
#pragma warning restore IDE0052
    #endregion

    private readonly int _nAxles;
    private readonly string _volumenR;
    private readonly string _volumenA;
    private readonly string _volumenB;
    private readonly string _volumenZylinder;
    private readonly string _fBremsuebersetzung;
    private readonly string _hBLAnschluss;
    private readonly int _loesezugtyp;
    private readonly int _umstellgewicht;
    private readonly int _anzahlAbsperrhaehne;
    private readonly Bremsbauart _brakeModel;

    private readonly BremsenKennungV _bremsenKennungV;
    private readonly BremseP _bremseP;
    private readonly BremseBeladenP _bremseBeladenP;
    private readonly BremseG _bremseG;
    private readonly BremseBeladenG _bremseBeladenG;
    private readonly BremseR _bremseR;
    private readonly BremseRE _bremseRE;
    private readonly BremsePE _bremsePE;
    private readonly BremsePH _bremsePH;

    public Bremsbauart BrakeModel => _brakeModel;
    public BremsenKennungV BremsenKennungV => _bremsenKennungV;
    public BremseG BremseG => _bremseG;
    public BremseP BremseP => _bremseP;
    public BremseBeladenG BremseBeladenG => _bremseBeladenG;
    public BremseBeladenP BremseBeladenP => _bremseBeladenP;
    public BremseR BremseR => _bremseR;
    public BremseRE BremseRE => _bremseRE;
    public BremsePE BremsePE => _bremsePE;
    public BremsePH BremsePH => _bremsePH;
    public int CountAxles => _nAxles;
    public int Loesezugtyp => _loesezugtyp;
    public int Umstellgewicht => _umstellgewicht;
    public int AnzahlAbsperrhaehne => _anzahlAbsperrhaehne;

    public virtual bool IsEinloesig => false;
    public virtual bool IsKlotzBremse => false;
    public virtual bool IsScheibenBremse => false;

    //---------------------------------------------------------------------
    public static string BremsstellungToString(Bremsstellung bremsstellung)
    {
      return bremsstellung switch
      {
        Bremsstellung.G => "G",
        Bremsstellung.P => "P",
        Bremsstellung.R => "R",
        Bremsstellung.P_Mg => "P+Mg",
        Bremsstellung.R_Mg => "R+Mg",
        _ => "Unbekannt"
      };
    }

    //---------------------------------------------------------------------
    public Bremssystem(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    {
      _volumenA = x.GetAttrValue("VolumenA", "");
      _volumenR = x.GetAttrValue("VolumenR", "");
      _volumenZylinder = x.GetAttrValue("VolumenZylinder", "");
      _fBremsuebersetzung = x.GetAttrValue("FBremsuebersetzung", "");
      _hBLAnschluss = x.GetAttrValue("HBLAnschluss", "");
      _nAxles = x.GetAttrValue("AnzahlAchsen", 0);
      _brakeModel = x.GetAttrEnum<Bremsbauart>("Bremsbauart", Bremsbauart.Undefined);
      _loesezugtyp = x.GetAttrValue("Loesezugtyp", 0);
      _umstellgewicht = x.GetAttrValue("Umstellgewicht", 0);
      _anzahlAbsperrhaehne = x.GetAttrValue("AnzahlAbsperrhaehne", 0);
      _bremsenKennungV = new BremsenKennungV(this, x.Element("BremsenKennungV"));
      _bremseP = x.GetOptionalElement(this, "BremseP", (p, c) => new BremseP(p, c));
      _bremseG = x.GetOptionalElement(this, "BremseG", (p, c) => new BremseG(p, c));
      _bremseBeladenP = x.GetOptionalElement(this, "BremseBeladenP", (p, c) => new BremseBeladenP(p, c));
      _bremseBeladenG = x.GetOptionalElement(this, "BremseBeladenG", (p, c) => new BremseBeladenG(p, c));
      _bremseR = x.GetOptionalElement(this, "BremseR", (p, c) => new BremseR(p, c));
      _bremseRE = x.GetOptionalElement(this, "BremseRE", (p, c) => new BremseRE(p, c)); //#todo#
      _bremsePE = x.GetOptionalElement(this, "BremsePE", (p, c) => new BremsePE(p, c)); //#todo#
      _bremsePH = x.GetOptionalElement(this, "BremsePH", (p, c) => new BremsePH(p, c)); //#todo#
    }

    //---------------------------------------------------------------------
    public double BremsGewicht(Bremsstellung bremsStellung)
    {
      switch (bremsStellung)
      {
        case Bremsstellung.G:
          if (_bremseG != null) return _bremseG.BremsGewicht;
          break;
        case Bremsstellung.P:
        case Bremsstellung.P_Mg:
          if (_bremseP != null) return _bremseP.BremsGewicht;
          break;
        case Bremsstellung.R:
        case Bremsstellung.R_Mg:
          if (_bremseR != null) return _bremseR.BremsGewicht;
          break;
      }

      return 0.0;
    }

    //---------------------------------------------------------------------
    public bool IstBremsstellungVerfuegbar(Bremsstellung bremsstellung)
    {
      return bremsstellung switch
      {
        Bremsstellung.G => _bremseG != null,
        Bremsstellung.P or Bremsstellung.P_Mg => _bremseP != null,
        Bremsstellung.R or Bremsstellung.R_Mg => _bremseR != null,
        _ => false,
      };
    }

    //---------------------------------------------------------------------
    protected override void SaveAttributes(XmlWriter writer)
    {
      base.SaveAttributes(writer);

      writer.WriteAttributeStringIfNotEmpty("VolumenR", _volumenR);
      writer.WriteAttributeStringIfNotEmpty("VolumenA", _volumenA);
      writer.WriteAttributeStringIfNotEmpty("VolumenB", _volumenB);
      writer.WriteAttributeStringIfNotEmpty("VolumenZylinder", _volumenZylinder);
      writer.WriteAttributeStringIfNotEmpty("FBremsuebersetzung", _fBremsuebersetzung);
      writer.WriteAttributeStringIfNotEmpty("HBLAnschluss", _hBLAnschluss);
      writer.WriteAttributeIf(_nAxles > 0, "AnzahlAchsen", _nAxles);
      writer.WriteAttributeIf(_loesezugtyp > 0, "Loesezugtyp", _loesezugtyp);
      writer.WriteAttributeIf(_umstellgewicht > 0, "Umstellgewicht", _umstellgewicht);
      writer.WriteAttributeIf(_anzahlAbsperrhaehne > 0, "AnzahlAbsperrhaehne", _anzahlAbsperrhaehne);
      writer.WriteAttributeIf(_brakeModel > Bremsbauart.Undefined, "Bremsbauart", (int)_brakeModel);
    }

    //---------------------------------------------------------------------
    protected override void SaveElements(XmlWriter writer)
    {
      base.SaveElements(writer);

      _bremsenKennungV.Save(writer);
      _bremseP?.Save(writer);
      _bremseG?.Save(writer);
      _bremseBeladenP?.Save(writer);
      _bremseBeladenG?.Save(writer);
      _bremseR?.Save(writer);
      _bremseRE?.Save(writer);
      _bremsePE?.Save(writer);
      _bremsePH?.Save(writer);
    }
  }
}
