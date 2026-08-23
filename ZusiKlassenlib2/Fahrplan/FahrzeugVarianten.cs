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

using log4net;
using Newtonsoft.Json;
using Sovoma;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.Vehicle;

namespace ZusiKlassenLib2.Fahrplan
{
  //---------------------------------------------------------------------
  [Serializable]
  public class FahrzeugVarianten : ZusiObject, ITrainAssembly
  {
#pragma warning disable IDE0052
    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


    private static readonly string[] _knownAttribs =
    {
            "Bezeichnung",
            "ZufallsWert",
            "PerZufallUebernehmen",
            "FzgPosition",
            "spZugNiedriger"
        };

    private static readonly string[] _knownElems =
    {
            "FahrzeugInfo",
            "FahrzeugVarianten",
            "Datei"
        };
#pragma warning restore IDE0052

    private readonly string _bezeichnung;
    private readonly float _zufallsWert;
    private readonly bool _perZufallUebernehmen;
    private readonly int _fzgPosition;
    private readonly double _spZugNiedriger;

    private readonly List<FahrzeugVarianten> _gruppen = new();
    private readonly List<FahrzeugInfo> _fahrzeuge = new();
    private Datei _datei;
    //private FahrzeugVariantenLink _datei;

    public int FzgPosition => _fzgPosition;

    public List<FahrzeugVarianten> FahrzeugGruppen => _gruppen;

    public List<FahrzeugInfo> Fahrzeuge => _fahrzeuge;

    public Datei Datei => _datei;
   
    //public FahrzeugVariantenLink Datei { get { return _datei; } }

    public bool PerZufallUebernehmen => _perZufallUebernehmen;
    public double SpZugNiedriger => _spZugNiedriger;

    public FahrzeugVarianten(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    {
      _bezeichnung = x.GetAttrValue("Bezeichnung", "");
      _zufallsWert = GetAttrValueFloat(x, "ZufallsWert", 0.0f);
      _perZufallUebernehmen = x.GetAttrValue("PerZufallUebernehmen", false);
      _fzgPosition = x.GetAttrValue("FzgPosition", 0);
      _spZugNiedriger = GetAttrValueDouble(x, "spZugNiedriger", 0.0);

      foreach (XElement xfv in x.Elements("FahrzeugVarianten"))
      {
        _gruppen.Add(new FahrzeugVarianten(this, xfv));
      }

      foreach (XElement xfi in x.Elements("FahrzeugInfo"))
      {
        _fahrzeuge.Add(new FahrzeugInfo(this, xfi));
      }

      //_datei = (string)x.Element("Datei");
      _datei = x.GetOptionalElement(this, "Datei", (p, c) => new Datei((ZusiObject)p, c));
      //_datei = x.GetOptionalElement(this, "Datei", (p, c) => new FahrzeugVariantenLink((ZusiObject)p, c));
      if (_datei != null)
      {
        string filename = _datei.FullPath;
        try
        {
          FahrzeugVariantenDatei fvDatei = new FahrzeugVariantenDatei(this, filename);
          fvDatei.Parse();
          FahrzeugVarianten fv = fvDatei.Root;
          _gruppen.Add(new FahrzeugVarianten(this, fv));
          _datei = null; // Datei-Element wird nicht mehr benötigt, da die Daten jetzt in _gruppen enthalten sind
        }
        catch (Exception ex)
        {
            _log.Error($"FahrzeugVarianten: Fehler beim Laden der Datei '{filename}'", ex);
        }
      }

    }

    public FahrzeugVarianten(IZusiObjectParent parent, FahrzeugVarianten source)
        : base(parent, source)
    {
      _bezeichnung = source._bezeichnung;
      _zufallsWert = source._zufallsWert;
      _perZufallUebernehmen = source._perZufallUebernehmen;
      _fzgPosition = source._fzgPosition;
      _spZugNiedriger = source._spZugNiedriger;
      _datei = source._datei;

      foreach (FahrzeugVarianten fv in source._gruppen)
      {
        _gruppen.Add(new FahrzeugVarianten(this, fv));
      }

      foreach (FahrzeugInfo fi in source._fahrzeuge)
      {
        _fahrzeuge.Add(new FahrzeugInfo(this, fi));
      }
      _datei = source._datei;
    }

    public FahrzeugVarianten(IZusiObjectParent parent, FahrzeugVarianten source, ZugReihung reihung)
        : base(parent, source)
    {
      _bezeichnung = source._bezeichnung;
      _zufallsWert = source._zufallsWert;
      _perZufallUebernehmen = source._perZufallUebernehmen;
      _fzgPosition = source._fzgPosition;
      _spZugNiedriger = source._spZugNiedriger;
      _datei = source._datei;

      LinkedListNode<FahrzeugInfo> node = reihung.First;
      while (node != null)
      {
        _fahrzeuge.Add(node.Value);
        node = node.Next;
      }
    }

    public void BuildTrain(LinkedList<FahrzeugInfo> zugReihung)
    {
      List<ITrainAssembly> tmp = new();
      _fahrzeuge.ForEach(f => tmp.Add(f));
      try
      {
        _gruppen.ForEach(g => tmp.Insert(g.FzgPosition, g));
      }
      catch // FahrzeugPosition falsch, außerhalb der Liste
      {
        _gruppen.ForEach(g => tmp.Insert(0, g));
      }

      if (_perZufallUebernehmen && tmp.Count>0)
      {
        ITrainAssembly ita = tmp[Randomizer.Next(tmp.Count - 1)];
        ita.BuildTrain(zugReihung);
      }
      else
      {
        tmp.ForEach(t => t.BuildTrain(zugReihung));
      }
    }

    public void CollectVehicles(List<ITrainAssembly> vehicles)
    {
      List<ITrainAssembly> tmp = new();
      _fahrzeuge.ForEach(f => tmp.Add(f));
      //try
      //{
      //  _gruppen.ForEach(g => tmp.Insert(g.FzgPosition, g));
      //}
      //catch // FahrzeugPosition falsch, außerhalb der Liste
      //{
      //  _log.Error($"FahrzeugVarianten.CollectVehicles: FahrzeugPosition falsch, außerhalb der Liste. Alle Gruppen werden am Anfang eingefügt. {tmp.ToString(): {_gruppen.ForEach(g => _log.Error(g.FzgPosition))}");
      //  _gruppen.ForEach(g => tmp.Insert(0, g));
      //}
      foreach (var g in _gruppen)
      {
        try
        {
          tmp.Insert(g.FzgPosition, g);
        }
        catch (Exception ex)
        {
          //_log.Error($"FahrzeugVarianten.CollectVehicles: FahrzeugPosition falsch, außerhalb der Liste. Alle Gruppen werden am Anfang eingefügt.: Position={g.FzgPosition}", ex);
          string tmpJson = JsonConvert.SerializeObject(tmp, Newtonsoft.Json.Formatting.Indented);

          _log.Error(
              $"FahrzeugVarianten.CollectVehicles: FahrzeugPosition falsch, außerhalb der Liste.: Position={g.FzgPosition}, Objekt={g}. " +
              $"tmp-Inhalt:\n{tmpJson}",
              ex);
          tmp.Insert(0, g);
        }
      }

      vehicles.AddRange(tmp);
    }

    public bool ContainsVehicle(IEnumerable<FahrzeugVariante> variants)
    {
      foreach (FahrzeugVariante fv in variants)
      {
        if (ContainsVehicle(fv.BR, fv.IDHaupt, fv.IDNeben))
          return true;
      }

      return false;
    }

    public bool ContainsVehicle(string vclass, int idMajor = -1, int idMinor = -1)
    {
      foreach (FahrzeugInfo fi in _fahrzeuge)
      {
        if (fi.Fahrzeug != null)
        {
          var x = fi.Fahrzeug.Varianten.Where(v => v.BR == vclass && (idMajor == -1 || (v.IDHaupt == idMajor && v.IDNeben == idMinor)));
          if (x.Any())
            return true;
        }
      };

      foreach (FahrzeugVarianten fv in _gruppen)
      {
        if (fv.ContainsVehicle(vclass, idMajor, idMinor))
          return true;
      }

      return false;
    }

    public bool HasDecoVehicle()
    {
      foreach (FahrzeugInfo fi in _fahrzeuge)
      {
        if (fi.Fahrzeug != null && fi.Fahrzeug.Varianten.Where(v => v.Dekozug).Any())
          return true;
      };

      foreach (FahrzeugVarianten fv in _gruppen)
      {
        if (fv.HasDecoVehicle())
          return true;
      }

      return false;
    }

    protected override void SaveAttributes(XmlWriter writer)
    {
      base.SaveAttributes(writer);

      writer.WriteAttributeStringIfNotEmpty("Bezeichnung", _bezeichnung);
      writer.WriteAttributeFloatIf(_zufallsWert != 0, "ZufallsWert", _zufallsWert, -1);
      writer.WriteAttributeIf(_perZufallUebernehmen, "PerZufallUebernehmen", _perZufallUebernehmen ? 1 : 0);
      writer.WriteAttributeIf(_fzgPosition != 0, "FzgPosition", _fzgPosition);
      writer.WriteAttributeDoubleIf(_spZugNiedriger > 0, "spZugNiedriger", _spZugNiedriger, 4);
    }

    protected override void SaveElements(XmlWriter writer)
    {
      base.SaveElements(writer);

      _gruppen.ForEach(v => v.Save(writer));
      _fahrzeuge.ForEach(f => f.Save(writer));
      _datei?.Save(writer);
    }
  }
}
