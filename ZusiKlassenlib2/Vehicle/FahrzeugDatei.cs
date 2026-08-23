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
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Vehicle
{
  /* .fzg-Datei
   */
  [Serializable]
  public class FahrzeugDatei : ZusiDocument<Fahrzeug>
  {
    private DataPathType _dataPath;

    public DataPathType DataPath => _dataPath;

    public static bool ClassifyVehicle(string relativeVehicleFilename, out VehicleKind kind, out string country, out int epoch)
    {
      kind = VehicleKind.Unknown;
      country = null;
      epoch = 0;

      if (string.IsNullOrEmpty(relativeVehicleFilename))
      {
        return false;
      }

      // 1 Land
      // 2 Epoche
      // 3 Dampfloks
      //   Dieselloks
      //   Elektroloks
      //   Dieseltriebwagen
      //   Elektrotriebwagen
      //   Akkutriebwagen
      string[] ss = relativeVehicleFilename.ToLower().Split('\\');
      
      if (ss.Length < 4)
      {
        return false;
      }

      // epoche
      string p = ss[2].StripPrefixPath("epoche");
      if (p.Length == ss[2].Length)
      {
        // cesko
        p = ss[2].StripPrefixPath("epocha");
      }
      _ = int.TryParse(p.Trim(), out int e);

      // vehicle kind
      VehicleKind vk = VehicleKind.Unknown;
      if (ss[3].StartsWith("dampf"))
      {
        vk = VehicleKind.Steam | VehicleKind.Locomotive;
      }
      else if (ss[3].StartsWith("diesell"))
      {
        vk = VehicleKind.Diesel | VehicleKind.Locomotive;
      }
      else if (ss[3].StartsWith("elektrol"))
      {
        vk = VehicleKind.Electric | VehicleKind.Locomotive;
      }
            else if (ss[3].StartsWith("hybridl"))
      {
        vk = VehicleKind.Electric | VehicleKind.Locomotive;
      }
      else if (ss[3].StartsWith("dieselt"))
      {
        vk = VehicleKind.Diesel | VehicleKind.RailCar;
      }
      else if (ss[3].StartsWith("elektrot"))
      {
        vk = VehicleKind.Electric | VehicleKind.RailCar;
      }
      else if (ss[3].StartsWith("rames_electriques"))
      {
        vk = VehicleKind.Electric | VehicleKind.RailCar;
      }
      else if (ss[3].StartsWith("akkut"))
      {
        vk = VehicleKind.Battery | VehicleKind.RailCar;
      }
      else if (ss[3].StartsWith("gueter"))
      {
        vk = VehicleKind.FreightWagon;
      }
      else if (ss[3].StartsWith("reise") || ss[3].StartsWith("osobni"))
      {
        vk = VehicleKind.Coach;
      }
      else if (ss[3].StartsWith("bahnd"))
      {
        vk = VehicleKind.ServiceWagon;
      }
      else if (ss[3].StartsWith("blind"))
      {
        vk = VehicleKind.Special;
      }
      else if (ss[3].StartsWith("bau"))
      {
        vk = VehicleKind.Special;
      }

      kind = vk;
      country = ss[1].ToProper();
      epoch = e;

      return true;
    }

    //---------------------------------------------------------------------
    public FahrzeugDatei(string path)
        : base(null, path, null, "Fahrzeug")
    { }

    //---------------------------------------------------------------------
    protected override void ParseDocument(XDocument doc)
    {
      base.ParseDocument(doc);

      if (Root.Varianten.Count > 0)
      {
        string relPath = Zusi.GetRelativePathOf(Filename, ref _dataPath);
        if (ClassifyVehicle(relPath, out VehicleKind kind, out string country, out int epoch))
        {
          Root.Kind = kind;
          Root.Country = country;
          Root.Epoche = epoch;

#if DEBUG
          if (kind == VehicleKind.Unknown)
          {
            _log.Error($"{Filename}: unknown kind");
          }
#endif
        }
      }
    }
  }
}
