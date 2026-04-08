using Sovoma;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// Allgemeine Informationen über eine Assembly werden über die folgenden 
// Attribute gesteuert. Ändern Sie diese Attributwerte, um die Informationen zu ändern,
// die einer Assembly zugeordnet sind.
//[assembly: AssemblyTitle("ZusiStart")]
//[assembly: AssemblyDescription("Alternative Zugauswahl für Zusi")]
//[assembly: AssemblyConfiguration("")]
//[assembly: AssemblyCompany("")]
//[assembly: AssemblyProduct("Zusi•Zugauswahl")]
//[assembly: AssemblyCopyright("Copyright © 2018-2024 Holger Maaß/Harold Linke")]
//[assembly: AssemblyTrademark("")]
//[assembly: AssemblyCulture("")]
//[assembly: AssemblySupportEMail("service@zusi-tools.org")]

// Durch Festlegen von ComVisible auf "false" werden die Typen in dieser Assembly unsichtbar 
// für COM-Komponenten.  Wenn Sie auf einen Typ in dieser Assembly von 
// COM aus zugreifen müssen, sollten Sie das ComVisible-Attribut für diesen Typ auf "True" festlegen.
[assembly: ComVisible(false)]

//Um mit dem Erstellen lokalisierbarer Anwendungen zu beginnen, legen Sie 
//<UICulture>ImCodeVerwendeteKultur</UICulture> in der .csproj-Datei
//in einer <PropertyGroup> fest.  Wenn Sie in den Quelldateien beispielsweise Deutsch
//(Deutschland) verwenden, legen Sie <UICulture> auf \"de-DE\" fest.  Heben Sie dann die Auskommentierung
//des nachstehenden NeutralResourceLanguage-Attributs auf.  Aktualisieren Sie "en-US" in der nachstehenden Zeile,
//sodass es mit der UICulture-Einstellung in der Projektdatei übereinstimmt.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]

// log4net
[assembly: log4net.Config.XmlConfigurator(Watch = true)]

//[assembly: ThemeInfo(
//   ResourceDictionaryLocation.None, //Speicherort der designspezifischen Ressourcenwörterbücher
//                                     //(wird verwendet, wenn eine Ressource auf der Seite
//                                     // oder in den Anwendungsressourcen-Wörterbüchern nicht gefunden werden kann.)
//    ResourceDictionaryLocation.SourceAssembly //Speicherort des generischen Ressourcenwörterbuchs
//                                              //(wird verwendet, wenn eine Ressource auf der Seite, in der Anwendung oder einem 
//                                              // designspezifischen Ressourcenwörterbuch nicht gefunden werden kann.)
//)]

// 0.6 - 21.08.2018
// ZusiKnips entfällt
// ZSPrepare per IPC übernimmt

// 0.7 - 23.08.2018
// Fehler im HttpMiniServer beseitigt

// 0.8 - 28.08.2018
// Updater gefixt

// 0.9 - 03.09.2018
// Versionsnummer wegen Updater erhöht
// weitere Log-Fallen ausgelegt

// 0.9.1 - 06.09.2018
// FahrzeugInfo mit VariantenIndex führte zum Absturz des TimeTableInfoControl
// Zugliste sortierbar (Abfahrtzeit, Fahrzeit auf/absteigend)
// Auswahl Dekozug implementiert
// Anzeige "Dekozug" im TimeTableInfoControl
// Downscale, wenn Bildschirm kleiner als 1920x1080
// Anzeige der Züge mit dynamischen Spaltenbreiten
// Filterungen/Sortierungen auf CollectionViewSource umgestellt (alle?)

// 0.9.1.1 - 06.09.2018
// Sortierbutton um 30% vergrößert

// 0.9.1.2 - 06.09.2018
// EngageScaling: DeviceBounds statt WorkingArea

// 0.9.1.3 - 06.09.2018
// Fehlersuche Scaling

// 0.9.2.0 - 06.09.2018
// Scaling-Fehler beseitigt (Tippfehler 1980 statt 1920)

// 0.9.3.0 - 07.09.2018
// weiteren Scaling-Fehler beseitigt (ScaleTransform auf falschem Grid)
// ZSPrepare komplett neu (WPF)

// 0.9.4.0 - 07.09.2018
// weiteren Scaling-Fehler beseitigt (Window war auf feste Größen festgenagelt)

// 0.9.5.0 - 08.09.2018
// xaml-Fehler beseitigt (falsche Datenquelle)

// 0.9.6.0 - 09.09.2018
// Icon hinzugefügt
// Zugansicht: Abschnitt mit nur Dekozügen wird nicht mehr angezeigt bei "Dekozüge" aus
// Überarbeitung der Hintergrundbilder
// 2. Hintergrundbild

// 0.9.6.1 - 14.09.2018
// BeadsControl an Alwin angepasst ;)

// 0.9.7 - 15.09.2018
// Bugfix: Scaling beachtet DPI
// Prüfung, ob Cachedateien existieren eingebaut
// Liste RecentTrains wird neu sortiert bei IncUsed

// 0.9.7.1 - 16.09.2018
// Zugsuche: Vorschaubilder Fahrzeuge besser mittig angeordnet

// 0.9.8 - 18.09.2018
// CountToVisibilityConverter in die sovomaLib umgezogen

// 1.0 - 06.05.2019
// Bibliotheken erneuert

// 1.0.1 - 07.05.2019
// dem Miniserver ein weiteres Bild hinzugefügt

// 1.1 - 09.05.2019
// Hintergrundbild hinzugefügt
// Windows-Konformes "Schließen"-X hinzugefügt
// Fahrplancache deaktiviert, Fahrpläne werden bei jedem Start eingelesen

// 1.1.1 - 09.05.2019
// "Schließen"-X in den Vordergrund gesetzt, damit es auch bei kleineren Bildschirmauflösungen sichtbar ist
// About erscheint nicht im Zug-Suchen-Mode

// 1.1.2 - 10.05.2019
// Dekozug wird beim Laden der Züge wieder ausgewertet (war nur im Cachemode aktiv)

// 1.1.3 - 25.05.2019
// Simulatorstart beachtet Benutzereinstellung "maximiert"

// 1.2 - 25.08.2019
// Anpassung an Zusi 3.3

// 1.3 - 07.10.2019
// Zugstart via Fahrpult-Schnittstelle

// 1.3.1 - 08.10.2019
// Zugstart entsprechend Zusi-Version mit relativem Pfad <= 3.3.4.0 bzw. vollständiger Pfad > 3.3.4.0

// 1.4 - 08.10.2019
// Zusi-Start-Modus konfigurierbar

// 1.5 - 16.10.2019
// Steam-Version tauglich (ZusiKlassenLib2)
// Windows-Konformen "Minimize"-Button hinzugefügt

// 1.6 - 19.10.2019
// Bibliotheken erneuert
// Bildarchiv der Stammfahrzeuge im ZusiStart-Paket
// alternativer Pfad in <Benutzer>\AppData\Local\ZusiPicLib
// ZusiKnips/(Photographer) zum Erstellen der Bilder hinzugefügt
// classes.familiy nach <Benutzer>\AppData\Local\ZusiStart verschoben

// 1.7 - 03.11.2019
// Logging aufgeräumt
// Log-Datei jetzt in C:\Benutzer\<user>\Dokumente\Sovoma
// Bugfix: ZusiStart erscheint wieder nach Beendigung des Zusi

// 1.8 - 01.03.2021
// kleines (?) Disaster mit der TCP-Schnitstelle, vergessen wir mal lieber

// 1.9 - 01.03.2021
// Bibliotheken erneuert
// TCP-Protokoll zum Starten eines Zuges erwartet nun Fahrplan-Datei und Zugnummer

// 1.10 - 08.03.2021
// Bibliotheken erneuert
// Anpassung an neue Fahrzeuge
// Einstellungen für Bildbibliothek hinzugefügt
// Gleisbedingungen hinzugefügt

// 1.11 - 08.03.2021
// Laden der Daten erfolgt maximal parallel
// Fortschrittsbalken durch Spinner ersetzt

// 1.11.0.x - 13.06.2021
// Zwischenversion für Fehlersuche

// 1.12 - 22.06.2021
// Fehler in ZusiKlassenLib2 beseitigt (siehe dort)

// 1.12.1 - 22.06.2021
// weitere Fehlersuche für Steam-Version

// 2.0.1 - 12.08.2024
// Update für Zusi 3.5

// 2.1.1 - 20.08.2024
// added support for intergated timelines

//[assembly: AssemblyVersion("2.0.2.0")]
