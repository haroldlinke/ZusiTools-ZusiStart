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

using System.Xml;
using System.IO;
using System.Data.Entity.Core.Mapping;
using ZusiStart.Data;

namespace ZusiStart.KlLib2
{

  //---------------------------------------------------------------------
  [Serializable]
  public static class StreckeExtensions
  {


    // Berechnet die Anzahl der vollen Monate zwischen zwei Datumsangaben.

    static int GetFullMonthsDifference(DateTime from, DateTime to)
    {
      // Falls das Enddatum vor dem Startdatum liegt, tauschen
      if (to < from)
      {
        var temp = from;
        from = to;
        to = temp;
      }

      int months = (to.Year - from.Year) * 12 + (to.Month - from.Month);

      // Falls der Tag im Enddatum kleiner ist als im Startdatum, einen Monat abziehen
      if (to.Day < from.Day)
        months--;

      return months;
    }


    //---------------------------------------------------------------------
    public static int GetAlter(this ZusiCLIProject.FileLibrary.Zusi3.Strecke.Element strecken_element)
    {

      int ageInMonths = 0;
      int alter = 0;

      var extAlter = strecken_element.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == "extAlter");
      if (extAlter is not null)
      {
        return int.Parse(extAlter.Value);
      }
      else
      {
        if (strecken_element.ParentBuffer is null || strecken_element.ParentBuffer.LandschaftsDatei is null)
        {
          return 0;
        }
        extAlter = strecken_element.ParentBuffer.LandschaftsDatei.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == "extAlter");
        if (extAlter is not null)
        {
          alter = int.Parse(extAlter.Value);
        }
        else
        {
          string filePath = strecken_element.ParentBuffer.LandschaftsDatei.GetFullPath();
          //DateTime creationDate = File.GetCreationTime(filePath);
          DateTime modificationDate = File.GetLastWriteTime(filePath);
          DateTime now = DateTime.Now;

          //ageInMonths = GetFullMonthsDifference(creationDate, now);
          ageInMonths = GetFullMonthsDifference(modificationDate, now);

          alter = (int)ageInMonths / 3;
          if (alter < 0)
            alter = 0;
          else if (alter > 6)
            alter = 6;
          strecken_element.ParentBuffer.LandschaftsDatei.SetAlter(alter);
        }
      }
      strecken_element.SetAlter(alter);

      return alter;
    }

    //---------------------------------------------------------------------
    public static void SetAlter(this ZusiCLIProject.FileLibrary.Zusi3.Strecke.Element strecken_element, int value)
    {
      var extAlter = strecken_element.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == "extAlter");
      if (extAlter is not null)
      {
        extAlter.Value = value.ToString();
      }
      else
      {

        XmlAttribute[] attributes = new XmlAttribute[strecken_element.Attributes.Count() + 1];
        // Copy old elements
        //for (int i = 0; i < strecken_element.Attributes.Length; i++)
        //{
        //  attributes[i] = strecken_element.Attributes[i];
        //}
        strecken_element.Attributes.CopyTo(attributes, 0);
        var newAttribute = new XmlDocument().CreateAttribute("extAlter");
        newAttribute.Value = value.ToString();
        attributes[strecken_element.Attributes.Count()] = newAttribute;
        strecken_element.Attributes = attributes;
      }
    }

    //---------------------------------------------------------------------
    public static void SetAlter(this ZusiCLIProject.FileLibrary.Zusi3.Datei landschaftsDatei, int value)
    {
      var extAlter = landschaftsDatei.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == "extAlter");
      if (extAlter is not null)
      {
        extAlter.Value = value.ToString();
      }
      else
      {

        XmlAttribute[] attributes = new XmlAttribute[landschaftsDatei.Attributes.Count() + 1];
        // Copy old elements
        //for (int i = 0; i < strecken_element.Attributes.Length; i++)
        //{
        //  attributes[i] = strecken_element.Attributes[i];
        //}
        landschaftsDatei.Attributes.CopyTo(attributes, 0);
        var newAttribute = new XmlDocument().CreateAttribute("extAlter");
        newAttribute.Value = value.ToString();
        attributes[landschaftsDatei.Attributes.Count()] = newAttribute;
        landschaftsDatei.Attributes = attributes;
      }
    }

    //---------------------------------------------------------------------
    public static void SetOptimisationStatus(this ZusiCLIProject.FileLibrary.Zusi3.Datei landschaftsDatei, int value)
    {
      var extOptstatus = landschaftsDatei.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == "extOptstatus");
      if (extOptstatus is not null)
      {
        extOptstatus.Value = value.ToString();
      }
      else
      {

        XmlAttribute[] attributes = new XmlAttribute[landschaftsDatei.Attributes.Count() + 1];
        // Copy old elements
        //for (int i = 0; i < strecken_element.Attributes.Length; i++)
        //{
        //  attributes[i] = strecken_element.Attributes[i];
        //}
        landschaftsDatei.Attributes.CopyTo(attributes, 0);
        var newAttribute = new XmlDocument().CreateAttribute("extOptstatus");
        newAttribute.Value = value.ToString();
        attributes[landschaftsDatei.Attributes.Count()] = newAttribute;
        landschaftsDatei.Attributes = attributes;
      }
    }

    //---------------------------------------------------------------------
    public static void SetOptimisationStatus(this ZusiCLIProject.FileLibrary.Zusi3.Strecke.Element strecken_element, int value)
    {
      var extOptstatus = strecken_element.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == "extOptstatus");
      if (extOptstatus is not null)
      {
        extOptstatus.Value = value.ToString();
      }
      else
      {

        XmlAttribute[] attributes = new XmlAttribute[strecken_element.Attributes.Count() + 1];
        // Copy old elements
        //for (int i = 0; i < strecken_element.Attributes.Length; i++)
        //{
        //  attributes[i] = strecken_element.Attributes[i];
        //}
        strecken_element.Attributes.CopyTo(attributes, 0);
        var newAttribute = new XmlDocument().CreateAttribute("extOptstatus");
        newAttribute.Value = value.ToString();
        attributes[strecken_element.Attributes.Count()] = newAttribute;
        strecken_element.Attributes = attributes;
      }
    }

    //---------------------------------------------------------------------
    public static int GetOptimisationStatus(this ZusiCLIProject.FileLibrary.Zusi3.Strecke.Element strecken_element)
    {
      int Optstatus = 1;
      var extOptstatus = strecken_element.Attributes.OfType<XmlAttribute>().FirstOrDefault(a => a.Name == "extOptstatus");
      if (extOptstatus is not null)
      {
        return int.Parse(extOptstatus.Value);
      }
      else
      {
        string modulname = strecken_element.ParentBuffer?.LandschaftsDatei?.NameOnly ?? "";

        modulname = modulname.Replace(".ls3", ".st3");

        if (DataManager.Instance.used_streckenmodule != null && DataManager.Instance.used_streckenmodule.Contains(modulname))
        {
          Optstatus = 0;
        }
        else
        {
          Optstatus = 1;
        }
        return Optstatus;
      }
    }
  }
}
