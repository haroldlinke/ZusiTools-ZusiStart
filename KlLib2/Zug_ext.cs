/*
 * Copyright 2026 Harold Linke
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
 *
 * 
 */

using ZusiCLIProject.FileLibrary.Zusi3.ZusiFdl;

namespace ZusiStart.KlLib2
{

  //---------------------------------------------------------------------
  [Serializable]
  public static class ZugExtensions
  {
    //---------------------------------------------------------------------
    public static DateTime? GetAbgleiszeit(this ZusiCLIProject.FileLibrary.Zusi3.Zug zug)
    {
      //ZusiCLIProject.FileLibrary.Zusi3.Zug.FahrplanEintrag fpe = zug.Eintraege.LastOrDefault(f => f.Abf != null);
      ZusiCLIProject.FileLibrary.Zusi3.Zug.FahrplanEintrag fpe = zug.Eintraege[zug.Eintraege.Length - 1];
      if (fpe.Abf == null ||fpe.Abf == DateTime.MinValue)
      {
        for (int i = (zug.Eintraege.Length - 1); i >= 0; i--)
        {
          fpe = zug.Eintraege[i];
          if (fpe.Abf != null && fpe.Abf != DateTime.MinValue)
          {
            return fpe.Abf;
          }
        }
      }

      return fpe?.Abf;

      //ZusiCLIProject.FileLibrary.Zusi3.Zug.FahrplanEintrag[] all = zug.Eintraege;
      //if (all.Count() > 0)
      //{
      //  foreach (ZusiCLIProject.FileLibrary.Zusi3.Zug.FahrplanEintrag fpe in all.ToArray().Reverse())
      //  {
      //    if (fpe.Ank != null)
      //    {
      //      return fpe.Ank;
      //    }
      //    if (fpe.Abf != null)
      //    {
      //      return fpe.Abf;
      //    }
      //  }
      //}

      //return null;
    }

    //---------------------------------------------------------------------
    public static DateTime? GetAufgleiszeit(this ZusiCLIProject.FileLibrary.Zusi3.Zug zug)
    {
      ZusiCLIProject.FileLibrary.Zusi3.Zug.FahrplanEintrag fpe = zug.Eintraege.FirstOrDefault(f => f.Ank != null);
      return fpe?.Ank;

      //ZusiCLIProject.FileLibrary.Zusi3.Zug.FahrplanEintrag fpe = zug.Eintraege.FirstOrDefault(f => f.Abf != null);
      //if (fpe == null)
      //{
      //  //Log.DebugFormat("Book-Timetable {0} has no departure time", FindParent<ZusiDocumentBase>().Filename);
      //  fpe = zug.Eintraege.FirstOrDefault(f => f.Ank != null);
      //  return fpe?.Ank;
      //}
      //else
      //{
      //  return fpe.Abf;
      //}
    }

    //---------------------------------------------------------------------
    public static DateTime? GetStartTime(this ZusiCLIProject.FileLibrary.Zusi3.Zug zug)
    {
      DateTime? start = null;

      ZusiCLIProject.FileLibrary.Zusi3.Zug.FahrplanEintrag fpe = zug.Eintraege.FirstOrDefault(f => f.Abf != null);
      if (fpe == null)
      {
        //Log.DebugFormat("Book-Timetable {0} has no departure time", FindParent<ZusiDocumentBase>().Filename);
        fpe = zug.Eintraege.FirstOrDefault(f => f.Ank != null);
        start = fpe?.Ank;
      }
      else
      {
        start = fpe.Abf;
      }

      return start;
    }

    //---------------------------------------------------------------------
    public static DateTime? GetEndTime(this ZusiCLIProject.FileLibrary.Zusi3.Zug zug)
    {
      ZusiCLIProject.FileLibrary.Zusi3.Zug.FahrplanEintrag[] all = zug.Eintraege;
      if (all.Count() > 0)
      {
        foreach (ZusiCLIProject.FileLibrary.Zusi3.Zug.FahrplanEintrag fpe in all.ToArray().Reverse())
        {
          if (fpe.Ank != null)
          {
            return fpe.Ank;
          }
          if (fpe.Abf != null)
          {
            return fpe.Abf;
          }
        }
      }

      return null;
    }

    //---------------------------------------------------------------------
    public static bool IsDecoTrain(this ZusiCLIProject.FileLibrary.Zusi3.Zug zug)
    {
      bool _decoTrain = false;

      if (!zug.Dekozug)
      {
        _decoTrain = zug.HasDecoVehicle();
      }
      else
      {
        _decoTrain = true;
      }
      return _decoTrain;
    }

    //---------------------------------------------------------------------
    public static bool HasDecoVehicle(this ZusiCLIProject.FileLibrary.Zusi3.Zug zug)
    {
      return false;
    }

    public static ZusiCLIProject.FileLibrary.Zusi3.Zug.FahrzeugContainer[] Reihung2Container(this ZusiCLIProject.FileLibrary.Zusi3.Zug.FahrzeugContainer[] fahrzeugcontainer, ZusiKlassenLib2.Fahrplan.ZugReihung reihung)
    {
      if (reihung != null)
      {
        //convert reihung to array
      }
      return fahrzeugcontainer;

    }

    //---------------------------------------------------------------------
    public static void ReplaceTrain(this ZusiCLIProject.FileLibrary.Zusi3.Zug zug, ZusiKlassenLib2.Fahrplan.ZugReihung reihung)
    {
      if (reihung != null)
      {
        //if (_fahrzeugVariantenOrg == null)
        //{
        //  _fahrzeugVariantenOrg = _fahrzeugVarianten;
        //}
        zug.FahrzeugContainers = zug.FahrzeugContainers.Reihung2Container(reihung);
        // _fahrzeugVarianten = new FahrzeugVarianten(this, _fahrzeugVariantenOrg, reihung);
      }
      else
      {
        //_fahrzeugVarianten = _fahrzeugVariantenOrg;
        //_fahrzeugVariantenOrg = null;
      }
    }

    //---------------------------------------------------------------------
    private static bool IsValidName(string name)
    {
      if (!string.IsNullOrEmpty(name))
      {
        string s = name.ToLower();
        if (s.StartsWith("- zbf") ||
            s.StartsWith("- zf") ||
            s.StartsWith("- kein ") ||
            s.StartsWith("sbk") ||
            s.StartsWith("va") ||
            s.StartsWith("ve") ||
            s.StartsWith("abzw") ||
            s.StartsWith("esig") ||
            s.StartsWith("asig") ||
            s.StartsWith("avsig") ||
            s.StartsWith("bksig") ||
            s.StartsWith("zsig") ||
            s.StartsWith("zvsig") ||
            s.StartsWith("bü") ||
            s.StartsWith("üs") ||
            s.StartsWith("lzb") ||
            s.StartsWith("- eingl") ||
            s.StartsWith("betriebs") ||
            s.StartsWith("strende") ||
            s.StartsWith("streckenende") ||
            s.StartsWith("ende") ||
            s.StartsWith("ri.") ||
            s.StartsWith("von ") ||
            s.StartsWith("nach ") ||
            s.StartsWith("aufgl"))
        {
          return false;
        }

        return true;
      }
      return false;
    }

    public static List<string> getBetriebstellen(this ZusiCLIProject.FileLibrary.Zusi3.Zug zug)
    {
      List<string> ziel = new List<string>();
      bool first_entry = true;
      string s = "dummy";
      foreach (var fe in zug.Eintraege)
      {
        s = fe.Betrst;
        if (!string.IsNullOrEmpty(s))
        {
          s = s.Trim();
        }

        if (!IsValidName(s) && !first_entry)
          continue;
        first_entry = false;
        ziel.Add(s); // oder s.ToUpper(), je nach Vergleichslogik
      }
      ziel.Add(s); // the last entry
      return ziel;
    }

    public static List<string> checkBetriebstellen(this ZusiCLIProject.FileLibrary.Zusi3.Zug zug, List<string> testBetriebsstellen)
    {
      return zug.getBetriebstellen().Where(bs => testBetriebsstellen.Contains(bs)).ToList();
    }

    private static void add_module_and_neighbors(List<string> usedmodules, string modulename, ZusiCLIProject.FileLibrary.Zusi3.Strecke.Referenz.Eintrag startpunkt, bool includeneigboringmodules)
    {
      if (!usedmodules.Contains(modulename))
      {
        usedmodules.Add(modulename);
        if (includeneigboringmodules)
        {
          foreach (ZusiCLIProject.FileLibrary.Zusi3.Strecke.NachbarModul nachbarmodul in startpunkt.Destination.ParentBuffer.NachbarModule)
          {
            string nb_modulename = nachbarmodul.Datei.NameOnly;
            if (!usedmodules.Contains(nb_modulename))
            {
              usedmodules.Add(nb_modulename);
            }
          }
        }
      }
    }

    private static void add_module(List<string> usedmodules, string modulename)
    {
      if (!usedmodules.Contains(modulename))
      {
        usedmodules.Add(modulename);
      }
    }

    public static List<string> getusedmodules(this ZusiCLIProject.FileLibrary.Zusi3.Zug zug, ZusiFdl2 fdl2, DateTime? aufgleiszeit, DateTime? abgleiszeit, out List<string> all_used_modules, bool includeneigboringmodules = false)
    {
        List<string> usedmodules = new List<string>();
      all_used_modules = new List<string>();

      if (fdl2 == null)
      {
        return usedmodules;
      }

      bool lastmoduleinuse = false;
      bool moduleusedintimeframe = false;

      ZugDispoState dispo = new ZugDispoState(fdl2, zug);
      List<KeyValuePair<ZusiCLIProject.FileLibrary.Zusi3.Zug.FahrplanEintrag, ZusiCLIProject.FileLibrary.Zusi3.Strecke.Fahrstrasse>> hauptpfad = dispo.CalculateHauptbuchfplFstr();

      foreach (var fahrstrasse in hauptpfad)
      {
        //string modulename = fahrstrasse.Value.Startpunkte.FirstOrDefault().Datei.NameOnly;
        ZusiCLIProject.FileLibrary.Zusi3.Strecke.Referenz.Eintrag startpunkt = fahrstrasse.Value.Startpunkte.FirstOrDefault();
        string modulename = startpunkt.Datei.NameOnly;
        add_module(all_used_modules, modulename);

        string zielmodulname = fahrstrasse.Value.Zielpunkte.FirstOrDefault().Datei.NameOnly;
        add_module(all_used_modules, zielmodulname);

        if (fahrstrasse.Key != null)
        {
          if ((fahrstrasse.Key.Ank == DateTime.MinValue) && (fahrstrasse.Key.Abf == DateTime.MinValue))
          {
            // if there is no FahrplanEintrag, we assume that the module is used if the last one was used

            if (lastmoduleinuse)
            {
              add_module_and_neighbors(usedmodules, modulename, startpunkt, includeneigboringmodules);
              //if (!usedmodules.Contains(modulename))
              //{
              //  usedmodules.Add(modulename);

              //  if (includeneigboringmodules)
              //  {
              //    foreach (ZusiCLIProject.FileLibrary.Zusi3.Strecke.NachbarModul nachbarmodul in startpunkt.Destination.ParentBuffer.NachbarModule)
              //    {
              //      string nb_modulename = nachbarmodul.Datei.NameOnly;
              //      if (!usedmodules.Contains(nb_modulename))
              //      {
              //        usedmodules.Add(nb_modulename);
              //      }
              //    }
              //  }
              //}
            }
          }
          else
          {
            moduleusedintimeframe = ((fahrstrasse.Key.Ank != DateTime.MinValue) && (fahrstrasse.Key.Ank <= abgleiszeit)) && ((fahrstrasse.Key.Abf != DateTime.MinValue) && (fahrstrasse.Key.Abf >= aufgleiszeit));

            if ((fahrstrasse.Key.Ank == DateTime.MinValue) && ((fahrstrasse.Key.Abf != DateTime.MinValue) && (fahrstrasse.Key.Abf >= aufgleiszeit) && (fahrstrasse.Key.Abf <= abgleiszeit)))
            {
              moduleusedintimeframe = true;
            }
            else
            {
              if ((fahrstrasse.Key.Abf == DateTime.MinValue) && ((fahrstrasse.Key.Ank != DateTime.MinValue) && (fahrstrasse.Key.Ank >= aufgleiszeit) && (fahrstrasse.Key.Ank <= abgleiszeit)))
              {
                moduleusedintimeframe = true;
              }
              else
              {
                if (((fahrstrasse.Key.Ank != DateTime.MinValue) && (fahrstrasse.Key.Ank >= aufgleiszeit) && (fahrstrasse.Key.Ank <= abgleiszeit)) || ((fahrstrasse.Key.Abf != DateTime.MinValue) && (fahrstrasse.Key.Abf >= aufgleiszeit) && (fahrstrasse.Key.Abf <= abgleiszeit)))
                {
                  moduleusedintimeframe = true;
                }
              }
            }

            if (moduleusedintimeframe)
            {
              lastmoduleinuse = true;

              add_module_and_neighbors(usedmodules, modulename, startpunkt, includeneigboringmodules);
              //if (!usedmodules.Contains(modulename))
              //{
              //  usedmodules.Add(modulename);

              //  if (includeneigboringmodules)
              //  {
              //    foreach (ZusiCLIProject.FileLibrary.Zusi3.Strecke.NachbarModul nachbarmodul in startpunkt.Destination.ParentBuffer.NachbarModule)
              //    {
              //      string nb_modulename = nachbarmodul.Datei.NameOnly;
              //      if (!usedmodules.Contains(nb_modulename))
              //      {
              //        usedmodules.Add(nb_modulename);
              //      }
              //    }
              //  }
              //}
              if (!usedmodules.Contains(zielmodulname))
              {
                usedmodules.Add(zielmodulname);
              }
            }
            else
            {
              lastmoduleinuse = false;
            }
          }
        }
        else // if there is no FahrplanEintrag, we assume that the module is used if the last one was used
        {
          if (lastmoduleinuse)
          {
            add_module_and_neighbors(usedmodules, modulename, startpunkt, includeneigboringmodules);
            //if (!usedmodules.Contains(modulename))
            //{
            //  usedmodules.Add(modulename);

            //  if (includeneigboringmodules)
            //  {
            //    foreach (ZusiCLIProject.FileLibrary.Zusi3.Strecke.NachbarModul nachbarmodul in startpunkt.Destination.ParentBuffer.NachbarModule)
            //    {
            //      string nb_modulename = nachbarmodul.Datei.NameOnly;
            //      if (!usedmodules.Contains(nb_modulename))
            //      {
            //        usedmodules.Add(nb_modulename);
            //      }
            //    }
            //  }
            //}
          }
        }
      }
      return usedmodules;
    }

    public static List<string> getStreckenElemente(this ZusiCLIProject.FileLibrary.Zusi3.Zug zug, ZusiFdl2 fdl2)
    {
      List<string> usedStreckenElements = new List<string>();

      ZugDispoState dispo = new ZugDispoState(fdl2, zug);
      List<KeyValuePair<ZusiCLIProject.FileLibrary.Zusi3.Zug.FahrplanEintrag, ZusiCLIProject.FileLibrary.Zusi3.Strecke.Fahrstrasse>> hauptpfad = dispo.CalculateHauptbuchfplFstr();

      foreach (var fahrstrasse in hauptpfad)
      {
        // only check for modules that are actually used by the train

        //string modulname = item.Start.ParentBuffer.ParentBuffer?.LandschaftsDatei?.NameOnly ?? "";
        if (false)
        {
          string modulename = fahrstrasse.Value.Startpunkte.FirstOrDefault().Datei.NameOnly;
          if (!usedStreckenElements.Contains(modulename))
          {
            usedStreckenElements.Add(modulename);
          }
        }
        else
        {
          //check for Streckenelemente does not work yet

          usedStreckenElements.AddRange(fahrstrasse.Value.Startpunkte.Select(sp => sp.ReferenzNr.ToString()));
        }
      }
      return usedStreckenElements;
    }
  }
}
