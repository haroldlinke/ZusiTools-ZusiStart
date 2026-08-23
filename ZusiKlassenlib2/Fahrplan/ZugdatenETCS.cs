/*
 * Copyright 2019 Holger Maaß
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
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace ZusiKlassenLib2.Fahrplan
{
  //---------------------------------------------------------------------
  public class ZugdatenETCS : ZugdatenLZB80
  {
    #region attributes & elements
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "Zugkategorie",
            "Achslast",
            "ETCSLevel",
            "ETCSModus",
            "Startsystem",
            "ETCSLSS",
            "ETCSCEASchalter",
            "ETCSPassivschalter"
        };
#pragma warning restore IDE0052
    #endregion

    private readonly IntAttribute _zugkategorie;
    private readonly IntAttribute _achslast;
    private readonly IntAttribute _etcsLevel;
    private readonly IntAttribute _etcsModus;
    private readonly StringAttribute _startsystem;
    private readonly IntAttribute _etcsLSS;
    private readonly IntAttribute _etcsCEASchalter;
    private readonly IntAttribute _etcsPassivschalter;

    //---------------------------------------------------------------------
    public int Zugkategorie
    {
      get => _zugkategorie;
      set => _zugkategorie.Value = value;
    }

    //---------------------------------------------------------------------
    public int Achslast
    {
      get => _achslast;
      set => _achslast.Value = value;
    }
    //---------------------------------------------------------------------
    public int ETCSLevel
    {
      get => _etcsLevel;
      set => _etcsLevel.Value = value;
    }
    //---------------------------------------------------------------------
    public int ETCSModus
    {
      get => _etcsModus;
      set => _etcsModus.Value = value;
    }
    //---------------------------------------------------------------------
    public string Startsystem
    {
      get => _startsystem;
      set => _startsystem.Value = value;
    }
    //---------------------------------------------------------------------
    public int ETCSLSS
    {
      get => _etcsLSS;
      set => _etcsLSS.Value = value;
    }
    //---------------------------------------------------------------------
    public int ETCSCEASchalter
    {
      get => _etcsCEASchalter;
      set => _etcsCEASchalter.Value = value;
    }
    //---------------------------------------------------------------------
    public int ETCSPassivschalter
    {
      get => _etcsPassivschalter;
      set => _etcsPassivschalter.Value = value;
    }

    //---------------------------------------------------------------------
    public ZugdatenETCS()
    { }

    //---------------------------------------------------------------------
    public ZugdatenETCS(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    {
      _zugkategorie = new IntAttribute(x, "Zugkategorie");
      _achslast = new IntAttribute(x, "Achslast");
      _etcsLevel = new IntAttribute(x, "ETCSLevel");
      _etcsModus = new IntAttribute(x, "ETCSModus");
      _startsystem = new StringAttribute(x, "Startsystem");
      _etcsLSS = new IntAttribute(x, "ETCSLSS");
      _etcsCEASchalter = new IntAttribute(x, "ETCSCEASchalter");
      _etcsPassivschalter = new IntAttribute(x, "ETCSPassivschalter");

    }

    //---------------------------------------------------------------------
    public ZugdatenETCS(IZusiObjectParent parent, ZugdatenETCS source)
    : base(parent, source)
    {
      _zugkategorie = new IntAttribute(source._zugkategorie);
      _achslast = new IntAttribute(source._achslast);
      _etcsLevel = new IntAttribute(source._etcsLevel);
      _etcsModus = new IntAttribute(source._etcsModus);
      _startsystem = new StringAttribute(source._startsystem);
      _etcsLSS = new IntAttribute(source._etcsLSS);
      _etcsCEASchalter = new IntAttribute(source._etcsCEASchalter);
      _etcsPassivschalter = new IntAttribute(source._etcsPassivschalter);
    }

    //---------------------------------------------------------------------
    protected override void SaveAttributes(XmlWriter writer)
    {
      base.SaveAttributes(writer);

      _zugkategorie.Write(writer);
      _achslast.Write(writer);
      _etcsLevel.Write(writer);
      _etcsModus.Write(writer);
      _startsystem.Write(writer);
      _etcsLSS.Write(writer);
      _etcsCEASchalter.Write(writer);
      _etcsPassivschalter.Write(writer);
    }
  }
}
