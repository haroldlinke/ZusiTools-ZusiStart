using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZusiKlassenLib2
{
  // 2.6 - 14.08.2018
  // Fahrplan.Zug: StartSpeed hinzugefügt

  // 2.7 - 22.08.2018
  // Zusi.IsDemo und Zusi.IsInstalled hinzugefügt

  // 2.8 - 24.08.2018
  // Zug.ReadBuchfahrplan mit Rückgabewert
  // Attribut FahrplanEintrag.Betrst hinzugefügt

  // 2.9 - 28.08.2018
  // Zusi.IsDemo wieder entfernt
  // für LSB-freie Modelle Mesh-Erstellung aus Vertex und Face hinzugefügt

  // 2.10 - 06.09.2018
  // FahrzeugInfo: VariantenIndex hinzugefügt
  // Zug.Journey-Time immer positiv
  // TimeTableInfoControl.Duration immer positiv

  // 2.10.0.1 - 07.09.2018
  // Landschaft: Log-Falle "subsets read" ausgebaut
  // Landschaft: Loglevel für falsch verknüpfte Animationen auf WARN zurückgesetzt

  // 2.11.0.0 - 09.09.2018
  // IntAttribute: NullReferenceException beseitigt
  // StringHelper.ToColor: komplett neu, verarbeitet auch 0FFFFFFFF als ABGR
  // TextureImageCache veröffentlicht, Attribut UseBuiltinDDSDecoder hinzugefügt

  // 2.11.0.1 - 14.09.2018
  // Fahrplan.Zug: Bremsstellung hinzugefügt

  // 2.11.1.0 - 14.09.2018
  // ZusiDocumentBase: Save(WriteFlags) hinzugefügt

  // 3.0.0.0 - 16.09.2018
  // alle Objekte in Fahrplan und TimeTable schreibbar

  // 3.1.0.0 - 28.09.2018
  // Zug: Auf- und Abgleiszeit hinzugefügt

  // 3.1.1.0 - 04.10.2018
  // AutorEintrag: alle Attribute als public property verfügbar gemacht

  // 3.1.1.1 - 04.10.2018
  // Landschaft: Dreiecke nach LOD zählbar

  // 3.2 - 06.10.2018
  // ZusiDocumentBase: Parse(bool throwOnError) hizugefügt

  // 3.2.0.1 - 06.10.2018
  // Nuget-Paketfehler

  // 3.3 - 07.10.2018
  // Verknuepfte: Flags unklar

  // 3.3.1 - 08.10.2018
  // Anpassung: SovomaLib v3
  // TextureCache in eigener Bibliothek

  // 3.3.2 - 08.10.2018
  // Anpassung an erneuerte ZusiTextureLib

  // 3.3.3 - 09.10.2018
  // Anpassung an erneuerte ZusiTextureLib

  // 3.4. - 10.10.2018
  // fehlende Attribute ergänzt

  // 3.5. - 11.10.2018
  // Vertex: Vorgabe für U und V von NaN auf 0f geändert

  // 3.6. - 12.10.2018
  // aufgeräumt (ctor angepasst)
  // ZugdatenIndusiAnalog und // ZugdatenIndusiRechner hinzugefügt

  // 3.6.1 - 16.10.2018
  // fehlende Elemente hinzugefügt

  // 3.7 - 16.10.2018
  // Zug und FahrzeugVarianten: original FahrzeugVarianten können von ZugReihung ersetzt werden

  // 3.7.0.1 - 17.10.2018
  // bin-Ordner bereinigt, app.config entfernt

  // 3.8. - 17.10.2018
  // Bugfix: FahrzeugInfo.VariantenIndex == 0 wird nicht geschrieben
  //         FahrzeugInfo.BremsstellungFahrzeug wird nicht geschrieben

  // 3.8.0.1 - 02.11.2018
  // Bibliotheken erneuert

  // 3.8.0.2 - 02.05.2019
  // Strecke: Properties veröffentlicht
  // Fahrzeug: Türsystem 'SBahn' hinzugefügt

  // 3.8.1 - 06.05.2019
  // Bibliotheken erneuert

  // 3.9 - 09.05.2019
  // EnumTimeTables und EnumVehicles: Fahrpläne und Fahrzeuge können aus Verzeichnissen außerhalb der Zusi-Struktur eingelesen werden

  // 3.9.1 - 14.05.2019
  // FahrzeugInfo und Zugreihung: Logausgabe von relativem Dateinamenauf vollen Dateinamen umgestellt

  // 3.10 - 15.05.2019
  // Registry-Pfad DatenVerzeichnis schreibbar via Zusi.UpdateDataFolder

  // 3.11 - 20.05.2019
  // Registry-Pfad DatenVerzeichnis schreibbar wieder entfernt
  // Zusi.ZusiAlternateDataPath hinzugefügt
  // Datei: Auflösung relativer Pfade: a) ZusiDataPath und wenn a) nicht existiert b) ZusiAlternateDataPath

  // 3.11.1 - 25.05.2019
  // ZusiSettings.IsMaximized hinzugefügt

  // 4.0.0.1 - 30.06.2019/25.08.2019
  // Anpassung der Zusi-Verzeichnisse an Zusi 3.3
  // neue Elemente und Attribute für Fahrplan und FahrzeugInfo
  // Länge und Masse eines Fahrzeuges werden nicht mehr korrigiert (aufgerundet bzw. in t umgerechnet)
  // MgBremse in Fahrzeug und FahrzeugVariante hinzugefügt
  // IstBremsstellungVerfuegbar beachtet separate Magnetbremse
  // Animationen nach Typ mit Zeit abrufbar

  // 4.0.1 - 31.08.2019
  // Unterstrich hinter Zusi.ZusiDataPath und Zusi.ZusiAlternateDataPath entfernt

  // 4.0.2 - 01.09.2019
  // VS2019
  // .Net-Zielversion 4.7.2

  // 4.0.3 - 30.09.2019
  // Signatur/Timestamp

  // 4.0.4 - 03.10.2019
  // abhängige Bibliotheken erneuert

  // 4.0.5 - 03.10.2019
  // abhängige Bibliotheken erneuert

  // 4.1 - 15.10.2019
  // Zusi-Pfade an Steam-Version angepasst

  // 4.2 - 19.10.2019
  // Vehicle.Fahrzeug: BR450 Wagenerkennung hinzugefügt
  // Vehicle.Fahrzeug: BR612 Wagenerkennung hinzugefügt

  // 4.3 - 01.01.2021
  // integrierte Fahrpläne hinzugefügt (<trn>-Element in Fahrplandateien)

  // 4.4 - 05.01.2021
  // neue Attribute zu FstTextur hinzugefügt

  // 4.5 - 08.01.2021
  // neue Funktion in Zusi: GetRelativePathOf

  // 4.6 - 09.01.2021
  // ZusiTextureLib integriert

  // 4.6.1 - 10.01.2021
  // Bibliotheken erneuert

  // 4.6.2 - 12.01.2021
  // ZusiDocumentBase/ZusiDocument: 
  //      Fehler beim Schreiben des Encodings beseitigt
  //      variables Start-Element eingeführt

  // 4.7 - 24.01.2021
  // Zusi.GetRelativePathOf erweitert: gibt den Typ des verwendeten Datenpfades zurück

  // 4.7.1 - 31.01.2021
  // Überarbeitung

  // 4.7.2 - 02.02.2021
  // SovomaLib erneuert

  // 4.8.0 - 27.02.2021
  // Bibliotheken erneuert
  // C#-Sprachversion 9.0

  // 4.9.0 - 27.02.2021
  // Fahrplan-Konverter hinzugefügt (Einzel-Dateien --> integrierter Fahrplan)

  // 4.9.1 - 01.03.2021
  // Bibliotheken erneuert

  // 4.9.1 - 04.03.2021
  // ZusiDocument: IsDocumentValid hinzugefügt
  // Zusi: GetAbsolutePath hinzugefügt

  // 4.10 - 27.02.2021
  // Bibliotheken erneuert

  // 4.10.1 - 08.03.2021
  // Zug: Bremsstellung kommt nun von Attribut BremsstellungZug

  // 4.10.2 - 08.03.2021
  // Buchfahrplan: Attribut Gattung veröffentlicht

  // 4.10.3 - 12.03.2021
  // Bibliotheken erneuert

  // 4.11 - 23.03.2021
  // Anpassung an Zusi 3.4.3
  //  FahrzeugBeladung
  //  NVR-Nummer
  //  Anzahl Achsen

  // 4.12 - 07.04.2021
  // Buchfahrplan und Elemente teilweise von ZusiGenericObject auf ZusiObject umgestellt
  //  viele Eigenschaften öffentlich verfügbar gemacht

  // 4.13 - 07.04.2021
  // Buchfahrplan: neue Attribute und Elemente hinzugefügt
  // FahrzeugVarianten: neue Attribute und Elemente hinzugefügt

  // 4.13.0.1 - 08.04.2021
  // FplZeit: FplEintrag als enum

  // 4.14 - 15.04.2021
  // kleine Anpassungen für Zugvorbereiter und DBServer

  // 4.15 - 16.04.2021
  // Bugfix: Verknuepfte.CreateModel beachtete Skalierung nicht (_sk)

  // 4.15.0.1 - 16.04.2021
  // Bugfix für Bugifx: Verknuepfte.CreateModel: Optionale Elemente (_p, _phi, _sk) wurden nicht beachtet

  // 4.16 - 23.04.2021
  // Anpassungen an Zusi 3.4.3.7
  // Handbremse hinzugefügt

  // 4.17 - 25.04.2021
  // String- und Pathfunktionen mit SovomaLib zusammengeführt

  // 4.17.1 - 25.04.2021
  // String- und Pathfunktionen mit SovomaLib zusammengeführt

  // 4.18 - 25.04.2021
  // FahrzeugVariante: Führerstandsmodus hinzugefügt

  // 4.19 - 28.04.2021
  // Bugfix: Zusi: HKCU\Software\Zusi3\Einstellungen ist ein optionaler Key

  // 4.20 - 01.05.2021
  // Zusi: Property InfrastructureDataPath hinzugefügt
  // Zusi: Logging erweitert

  // 4.20.1 - 01.05.2021
  // Zusi: Logging verbessert

  // 4.21 - 05.05.2021
  // Bugfix: FahrzeugVariante: Datei "2. Führerstand" ist optional
  // Bugfix: Zug: fehlendes Feld "FahrplanDatei führte zu einem ungültigen Dateinamen

  // 4.22 - 12.06.2021
  // DataPath ohne Arbeitsverzeichnisse

  // 4.22.0.1 - 13.06.2021
  // Zusi: geringfügige Logging-Änderungen

  // 4.23.0.0 - 13.06.2021
  // FahrzeugDatei, Bremssystem, Handbremse: Anpassung an Zusi 3.4.4.4

  // 4.24.0.0 - 22.06.2021
  // Bugfix: Registry-Values der Steam-Version wurden nicht richtig gelesen
  // ZusiSettings als eigene Datei

  // 4.24.1.0 - 22.06.2021
  // weitere Fehlersuche für Steam-Version

  // 4.25 - 02.08.2021
  // neue Attribute
  //  Buchfahrplan: LaengeLoks
  //  FplSignaltyp: FplHilfssignal

  // 4.26 - 02.08.2021
  // Debug-Logging in Zusi wieder reduziert

  // 4.27 - 22.09.2021
  // Anpassung an Zusi 3.4.5.8

  // 4.27.1 - 23.09.2021
  // FplvMaxReduzierungen war nich serialisierbar

  // 4.28 - 01.10.2021
  // Vereinfachung für DateiFührerstand in Fahrzeugvariante (Vorn/Hinten) hinzugefügt

  // 5.0.0 - 20.01.2022
  // Umstellung auf .NET 5

  // 5.1.0 - 10.02.2022
  // Target .NET-Framework 4.7.2 aufgenommen

  // 5.1.0.1 - 18.03.2022
  // Spezial-Version für Michael Groß

  // 6.4.2 - 16.11.2024
  // .net 6
  // FplMasse
  // FplZugLaenge
  // Luftpresser:  "DruckAngleicher" und "NurWennHSEinDieselLaeuft"
  // Bremse_KE_GP: "AutomLastabh"
  // 'Nachbremsfunktion' of node 'Bremse_KE_Tm'
  // Element 'BremseRrot' of 'Bremse_KE_GPR_Scheibe'
  // Attribute 'StartAntriebIndex' of node 'FahrzeugInfo'
  // Attribute 'TuerSystemBezeichner' of node 'Zug'
  // BremsePE und BremseRE
  // AntriebsmodellDieselElektrischDrehstrom
  // "AutoAngleichenNachLoesen" und "AngleicherAutomatik"
  // Attribute 'MaxDruck' of node 'BremseLuftDirekt'
  // fahrstrStreckela, fahrstrStreckestrukturnummer
  // Attribute 'FahrstrStrecke' of node 'FplSignaltyp'
  // Attribute 'AufgleisenRegisterpruefen' of node 'Zug'
  // Attribute 'AutomLastabh' of node 'Bremse_KE_GPR_Klotz'
  // Attribute 'Grenzlast' of node 'Buchfahrplan'
  // Attribute 'Verkehrstage' of node 'Buchfahrplan'
  // Attribute 'FplvMax' of node 'FplvMaxReduzierungen'
  // Attribute 'FplBremsstellungTextvorgabe' of node 'Zug'

  // 6.5.1
  // Attribute 'FplBremsstellungTextvorgabe' of node 'Buchfahrplan'

  // 8.0.13
  // concurrency problem in VehicleCache fixed
}
