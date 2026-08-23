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
using log4net;
using ZusiCLIProject.FileLibrary.Zusi3;

namespace ZusiStart.KlLib2
{

  //---------------------------------------------------------------------
  [Serializable]
  public static class DateiExtensions
  {
    private static readonly ILog _log = LogManager.GetLogger(typeof(Datei));

    //---------------------------------------------------------------------
    public static void SaveAs(this Datei datei,string filename)
    {
      var bfplW2 = new System.Xml.XmlTextWriter(filename, System.Text.Encoding.UTF8);
      bfplW2.Formatting = System.Xml.Formatting.Indented;
      bfplW2.Indentation = 0;
      datei.Content.Serialize(bfplW2);
      bfplW2.Close();
    }

    public static bool Exists(this Datei datei)
    {
      ZusiKlassenLib2.DataPathType dtp = ZusiKlassenLib2.DataPathType.Unknown;
      string path = ZusiKlassenLib2.Zusi.GetAbsolutePathOf(datei.Dateiname, ref dtp);
      return !string.IsNullOrEmpty(path) && System.IO.File.Exists(path);
    }

    //---------------------------------------------------------------------
    public static string GetFullPath(this Datei datei)
    {
      string _fullPath = string.Empty;
      if (!string.IsNullOrEmpty(datei.Dateiname))
      {
        ZusiKlassenLib2.DataPathType dtp = ZusiKlassenLib2.DataPathType.Unknown;
        _fullPath = ZusiKlassenLib2.Zusi.GetAbsolutePathOf(datei.Dateiname, ref dtp);
      }

      if (string.IsNullOrEmpty(_fullPath))
      {
        _log.Warn($"Cannot resolve full path of {datei.Dateiname}");
        _fullPath = string.Empty;
      }
      return _fullPath;
    }

    public static bool IsEmpty(this Datei datei)
    {
      return string.IsNullOrEmpty(datei.Dateiname);
    }

    public static bool IsRelative(this Datei datei)
    {
      return Sovoma.PathHelper.IsPathRelative(datei.Dateiname);
    }
  }

}
    