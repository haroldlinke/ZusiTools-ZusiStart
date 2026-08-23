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

namespace ZusiKlassenLib2.Common
{
    /*
     *       1 Signalgeschwindigkeit
     * 1000002 Ende Weichenbereich
     *       3 Signalhaltfall
     *       4 Fahrstraße auflösen
     *       6 Buchfahrplaneintrag
     * 1000007 Bahnsteiganfang rechts
     * 1000008 Bahnsteigmitte rechts
     * 1000009 Bahnsteigende rechts
     * 1000010 Bahnsteianfang links
     * 1000011 Bahnsteigmitte links
     * 1000012 Bahnsteigende links
     *      13 vorher keine Fahrstraße
     *      14 Zwangshalt
     *      15 Abrupthalt
     *      16 Zug entfernen
     *      17 Fahrt auf Sicht bis zum nächsten Hauptsignal
     *      18 Weiterfahrt nach Halt
     *      19 Beschreibung
     *      20 vorher keine Vorsignalverknüpfung
     *      21 keine Zug-Fahrstraße einrichten
     *      22 keine Rangier-Fahrstraße einrichten
     *      23 Hilfshauptsignal
     *      24 Türschließauftrag-Signal
     *      25 Abfahrsignal
     * 1000027 Bahnübergang öffnen
     *      27 Bahnübergang schließen
     *      28 Gegengleis kennzeichnen
     *      29 Richtungsvoranzeiger-Ziel
     *      30 Pfeifen
     *      31 Streckensound
     *      32 Befehl einblenden
     *      34 Register in Fahrstraße verknüpfen A
     *      35 Register in Fahrstraße verknüpfen B
     *      36 Weiche in Fahrstraße verknüpfen
     *      37 Signal in Fharstraße verknüpfen
     *      38 Richtungsvoranzeiger
     *      39 Regelgleis kennzeichnen
     *      40 eingleisige Strecke kennzeichnen
     *      41 Hauptschalter ausschalten
     *      42 Hauptschalter einschalten
     *      43 Stromabnehmer senken
     *      44 Stromabnehmer heben
     *      45 keine Anzeige-Fahrstraße einrichten
     *      46 Hauptschalter aus Ankündigung
     *      48 Signal umstellen
     *      49 Weiche auf Abzweig stellen
     *      50 Vorsignal in Fahrstraße verknüpfen
     *      52 Entgleisen
     *      53 Weiche in Grundstellung
     *      57 Buchfahrplan-Fehllänge
     *      58 MBrh/vMax-Reduzierung
     *      59 Anzahl Sägelinien
     *      60 Signal zeigt Kennlicht
     *      61 vMax anzeigegeführte Züge
     *      62 Stromabnehmer senken Ankündigung
     *      63 Zuordnung Grafikansicht-Streckenblickpunkt
     *     500 Indusi 500 Hz 
     *    1000 Indusi 1000 Hz
     *    2000 Indusi 2000 Hz
     * 1003001 LZB-Anfang
     *    3002 LZB-Ende
     *    3003 LZB-CIR-ELKE-Geschwindigkeit
     *    3011 ETCS-Level-Ankündigung
     *    3012 ETCS-Level-Quittierung
     *    3013 ETCS-Level wirksam
     *    3014 ETCS-Infill Balise
     *    3015 ETCS-Euroloop Beginn
     *    3017 ETCS-Geschwindigkeit
     *    3018 ETCS-Funkaufbau
     *    3019 ETCS-Stopmarker
     *    3020 ETCS-Ortungsbalise
     *    3021 ZBS-Betriebszustand
     *    3022 ZBS-Vorsignal
     *    3031 Fahrsperre aktiv
     *    4000 GNT (ZUB262) Geschwindigkeit
     *    4001 GNT (ZUB262) Anfang
     *    4002 GNT (ZUB262) Ende
     *    4003 GNT (ZUB262) Indusi-Unterdrückung
     *    4010 GNT (ZUB122) Geschwindigkeit
     *    4011 GNT (ZUB122) Anfang
     *    4012 GNT (ZUB122) Ende
     *    4013 GNT (ZUB122) Indusi-Unterdrückung
     */

    [Serializable]
    public class Ereignis : ZusiObject
    {
        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Er",
            "Wert",
            "Beschr"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly int _er;
        private readonly double _wert;
        private readonly string _beschr;

        public string Beschr => _beschr;
        public int Er => _er;
        public double Wert => _wert;

        //---------------------------------------------------------------------
        public Ereignis(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _er = x.GetAttrValue("Er", 0);
            _wert = GetAttrValueDouble(x, "Wert", 0.0);
            _beschr = x.GetAttrValue("Beschr", "");
        }

        //---------------------------------------------------------------------
        public Ereignis(IZusiObjectParent parent, Ereignis source)
            : base(parent, source)
        {
            _er = source._er;
            _wert = source._wert;
            _beschr = source._beschr;
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeIf(_er > 0, "Er", _er);
            writer.WriteAttributeDoubleIf(_wert > 0, "Wert", _wert, 4);
            writer.WriteAttributeStringIfNotEmpty("Beschr", _beschr);
        }
    }
}
