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
using System.Globalization;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Vehicle
{
  //---------------------------------------------------------------------
  [Serializable]
  public class FahrzeugGrunddaten : ZusiGenericObject
  {
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "RotationsZuschlag",
            "cwA",
            "Rollwiderstand",
            "Laenge",
            "spMax",
            "FesselAnfg",
            "FesselEnde",
            "StromabnHoehe",
            "Masse",
            "Achsstandsumme",
            "HBLVorhanden",
            "Schlingerfaktor",
            "Neigewinkel",
            "Spurweite",
            "AnzahlAchsen",
            "LokModus",
            "Verbundfahrzeug",
            "StromabnehmerA",
            "StromabnehmerB",
            "StromabnehmerC",
            "StromabnehmerD",
            "MaxZuladung",
            "AnzahlTueren",
            "Luftfederabsperrhaehne"
        };

    private static readonly string[] _knownElems =
    {
            "SchlussVorne",
            "SchlussHinten"
        };
#pragma warning restore IDE0052

    private readonly double _achsstandSumme;
    private readonly int _anzahlAchsen;
    private readonly double _fesselAnfg;
    private readonly double _fesselEnde;
    private readonly double _laenge;
    private readonly string _lokModus;
    private readonly double _masse;
    private readonly double _spMax;
    private readonly double _maxzuladung;
    private readonly int _anzahlTueren;
    private readonly int _luftfederabsperrhaehne;

    public double AchsstandSumme => _achsstandSumme;
    public int AnzahlAchsen => _anzahlAchsen;
    public double FesselAnfg => _fesselAnfg;
    public double FesselEnde => _fesselEnde;
    public double Laenge => _laenge;
    public string LokModus => _lokModus;
    public double Masse => _masse;
    public double MaxSpeed => _spMax;
    public double MaxZuladung => _maxzuladung;
    public int AnzahlTueren => _anzahlTueren;
    public int Luftfederabsperrhaehne => _luftfederabsperrhaehne;

    public FahrzeugGrunddaten(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    {
      string s = Attribute("Achsstandsumme");
      if (!string.IsNullOrEmpty(s))
      {
        double.TryParse(s, NumberStyles.Float, Cultures.EnUS, out _achsstandSumme);
      }

      s = Attribute("AnzahlAchsen");
      if (!string.IsNullOrEmpty(s))
      {
        _ = int.TryParse(s, out _anzahlAchsen);
      }

      s = Attribute("FesselAnfg");
      if (!string.IsNullOrEmpty(s))
      {
        double.TryParse(s, NumberStyles.Float, Cultures.EnUS, out _fesselAnfg);
      }

      s = Attribute("FesselEnde");
      if (!string.IsNullOrEmpty(s))
      {
        double.TryParse(s, NumberStyles.Float, Cultures.EnUS, out _fesselEnde);
      }

      s = Attribute("Laenge");
      if (!string.IsNullOrEmpty(s))
      {
        double.TryParse(s, NumberStyles.Float, Cultures.EnUS, out _laenge);
      }

      _lokModus = Attribute("LokModus");

      s = Attribute("Masse");
      if (!string.IsNullOrEmpty(s))
      {
        double.TryParse(s, NumberStyles.Float, Cultures.EnUS, out _masse);
      }

      s = Attribute("MaxZuladung");
      if (!string.IsNullOrEmpty(s))
      {
        double.TryParse(s, NumberStyles.Float, Cultures.EnUS, out _maxzuladung);
      }

      s = Attribute("spMax");
      if (!string.IsNullOrEmpty(s))
      {
        double.TryParse(s, NumberStyles.Float, Cultures.EnUS, out _spMax);
      }

      s = Attribute("AnzahlTüren");
      if (!string.IsNullOrEmpty(s))
      {
        _ = int.TryParse(s, out _anzahlTueren);
      }

      s = Attribute("Luftfederabsperrhaehne");
      if (!string.IsNullOrEmpty(s))
      {
        _ = int.TryParse(s, out _luftfederabsperrhaehne);
      }
    }
  }
}
