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

using log4net;
using Sovoma;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Vehicle;

namespace ZusiKlassenLib2.Buchfahrplan
{
  [Serializable]
  public class Buchfahrplan : ZusiObject
  {
    private static readonly ILog Log = LogManager.GetLogger(typeof(Buchfahrplan));

#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "Gattung",
            "Nummer",
            "Zuglauf",
            "BR",
            "Masse",
            "spMax",
            "Bremsh",
            "MBrh",
            "Laenge",
            "kmStart",
            "Bremsstellung",
            "GNTSpalte",
            "BremsstellungZug",
            "LaengeLoks",
            "Verkehrstage",
            "Grenzlast",
            "FplBremsstellungTextvorgabe",
            "Wirbelstrombremse"
        };

    private static readonly string[] _knownElems =
    {
            "Datei_fpn",
            "Datei_trn",
            "UTM",
            "FplZeile",
            "Fzg"
        };
#pragma warning restore IDE0052

    private readonly string _gattung;
    private readonly string _nummer;
    private readonly string _zuglauf;
    private readonly string _br;
    private readonly double _mass;
    private readonly double _spMax;
    private readonly int _bremsh;
    private readonly int _mbrh;
    private readonly double _length;
    private readonly double _kmStart;
    private readonly Bremsstellung _bremsstellung;
    private readonly string _gntSpalte;
    private readonly Bremsstellung _bremsstellungZug;
    private readonly Datei _fpn;
    private readonly Datei _trn;
    private readonly ZusiKlassenLib2.Common.UTM _utm;
    private readonly double _lengthLocos;
    private readonly string _verkehrstage;
    private readonly bool _grenzlast;
    private readonly string _fplBremsstellungTextvorgabe;
    private readonly string _wirbelstrombremse;

    private readonly List<FplZeile> _fplZeilen = new();
    private readonly List<Fzg> _fzgs = new();

    public Bremsstellung Bremsstellung => _bremsstellung;
    public Bremsstellung BremsstellungZug => _bremsstellungZug;
    public string BR => _br;
    public int BremsH => _bremsh;
    public List<FplZeile> FplZeilen => _fplZeilen;
    public string Gattung => _gattung;
    public string GNTSpalte => _gntSpalte;
    public double KmStart => _kmStart;
    public double Length => _length;
    public double LengthLocos => _lengthLocos;
    public double Mass => _mass;
    public double MaxSpeed => _spMax;
    public int MBrh => _mbrh;
    public string Number => _nummer;
    public string Zuglauf => _zuglauf;
    public ZusiKlassenLib2.Common.UTM UTM => _utm;
    public string Verkehrstage => _verkehrstage;
    public bool Grenzlast => _grenzlast;
    public string FplBremsstellungTextvorgabe => _fplBremsstellungTextvorgabe;
    public string Wirbelstrombremse => _wirbelstrombremse;

    //---------------------------------------------------------------------
    public Buchfahrplan(ZusiDocumentBase parent, XElement x)
        : base(parent, x)
    {
      _gattung = x.GetAttrValue("Gattung", string.Empty); 
      _nummer = x.GetAttrValue("Nummer", string.Empty);
      _zuglauf = x.GetAttrValue("Zuglauf", string.Empty);
      _br = x.GetAttrValue("BR", string.Empty);
      _mass = GetAttrValueDouble(x, "Masse", 0.0);
      _spMax = GetAttrValueDouble(x, "spMax", 0.0);
      _bremsh = (int)Math.Round(GetAttrValueDouble(x, "Bremsh", 0.0) * 100);
      _mbrh = (int)Math.Round(GetAttrValueDouble(x, "MBrh", 0.0) * 100);
      _length = GetAttrValueDouble(x, "Laenge", 0.0);
      _lengthLocos = GetAttrValueDouble(x, "LaengeLoks", 0.0);
      _kmStart = GetAttrValueDouble(x, "kmStart", 0.0);
      _bremsstellung = x.GetAttrEnum<Bremsstellung>("Bremsstellung", Bremsstellung.G);
      _gntSpalte = x.GetAttrValue("GNTSpalte", string.Empty);
      _bremsstellungZug = x.GetAttrEnum<Bremsstellung>("BremsstellungZug", Bremsstellung.G);
      _verkehrstage = x.GetAttrValue("Verkehrstage", string.Empty);
      _grenzlast = x.GetAttrValue("Grenzlast", false);
      _fplBremsstellungTextvorgabe = x.GetAttrValue("FplBremsstellungTextvorgabe", string.Empty);
      _wirbelstrombremse = x.GetAttrValue("Wirbelstrombremse", string.Empty);

      _fpn = GetOptionalObject<Datei>(this, x.Element("Datei_fpn"));
      _trn = GetOptionalObject<Datei>(this, x.Element("Datei_trn"));
      _utm = GetOptionalObject<ZusiKlassenLib2.Common.UTM>(this, x.Element("UTM"));

      _fzgs.AddRange(from XElement xe in x.Elements("Fzg")
                     select new Fzg(this, xe));

      _fplZeilen.AddRange(from XElement xe in x.Elements("FplZeile")
                          select new FplZeile(this, xe));
    }

    //---------------------------------------------------------------------
    public DateTime? GetStartTime()
    {
      DateTime? start = null;

      FplZeile? zz = FplZeilen.FirstOrDefault(z => z.Abfahrt != null);
      if (zz == null)
      {
        Log.DebugFormat("Book-Timetable {0} has no departure time", FindParent<ZusiDocumentBase>().Filename);
        zz = FplZeilen.FirstOrDefault(z => z.Ankunft != null);
        start = zz?.Ankunft.Time;
      }
      else
      {
        start = zz.Abfahrt.Time;
      }

      return start;
    }

    //---------------------------------------------------------------------
    public DateTime? GetEndTime()
    {
      List<FplZeile> all = FplZeilen;
      if (all.Count > 0)
      {
        foreach (FplZeile z in all.ToArray().Reverse())
        {
          if (z.Ankunft != null && z.Ankunft.Time != null)
          {
            return z.Ankunft.Time;
          }
          if (z.Abfahrt != null && z.Abfahrt.Time != null)
          {
            return z.Abfahrt.Time;
          }
        }
      }

      return null;
    }

    //---------------------------------------------------------------------
    protected override void SaveElements(XmlWriter writer)
    {
      base.SaveElements(writer);

      _fpn?.Save(writer);
      _trn?.Save(writer);
      _fzgs.ForEach(f => f.Save(writer));
      _fplZeilen.ForEach(f => f.Save(writer));
    }

    //---------------------------------------------------------------------
    protected override void SaveAttributes(XmlWriter writer)
    {
      base.SaveAttributes(writer);

      writer.WriteAttributeStringIfNotEmpty("Gattung", _gattung);
      writer.WriteAttributeStringIfNotEmpty("Nummer", _nummer);
      writer.WriteAttributeStringIfNotEmpty("Zuglauf", _zuglauf);
      writer.WriteAttributeStringIfNotEmpty("BR", _br);
      writer.WriteAttributeDoubleIf(_mass > 0, "Masse", _mass, 4);
      writer.WriteAttributeDoubleIf(_spMax > 0, "spMax", _spMax, 4);
      writer.WriteAttributeIf(_bremsh > 0, "Bremsh", _bremsh);
      writer.WriteAttributeIf(_mbrh > 0, "MBrh", _mbrh);
      writer.WriteAttributeDoubleIf(_length > 0, "Laenge", _length, 4);
      writer.WriteAttributeDoubleIf(_lengthLocos > 0, "LaengeLoks", _lengthLocos, 4);
      writer.WriteAttributeDoubleIf(_kmStart > 0, "kmStart", _kmStart, 4);
      writer.WriteAttributeIf(_bremsstellung != Bremsstellung.G, "Bremsstellung", (int)_bremsstellung);
      writer.WriteAttributeStringIfNotEmpty("GNTSpalte", _gntSpalte);
      writer.WriteAttributeIf(_bremsstellungZug != Bremsstellung.G, "BremsstellungZug", (int)_bremsstellungZug);
      writer.WriteAttributeStringIfNotEmpty("Verkehrstage", _verkehrstage);
      writer.WriteAttributeIf(_grenzlast, "Grenzlast", 1);
      writer.WriteAttributeStringIfNotEmpty("FplBremsstellungTextvorgabe", _fplBremsstellungTextvorgabe);
      writer.WriteAttributeStringIfNotEmpty("Wirbelstrombremse", _wirbelstrombremse);
    }
  }
}
