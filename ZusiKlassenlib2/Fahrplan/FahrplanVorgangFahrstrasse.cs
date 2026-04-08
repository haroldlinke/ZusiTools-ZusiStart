/*
 * Copyright 2018 Holger Maaß
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
using Sovoma;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Fahrplan
{
  //---------------------------------------------------------------------
  [Serializable]
  public class FahrplanVorgangFahrstrasse : ZusiObject
  {
    private static readonly ILog _log = LogManager.GetLogger(typeof(FahrplanVorgangFahrstrasse));

    #region attributes & elements
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "Beschreibung"
        };

    private static readonly string[] _knownElems =
    {
            "AbhAbhaengigkeit"
        };
#pragma warning restore IDE0052
    #endregion

    private readonly string _beschreibung;

    //private readonly AbhAbhaengigkeit _abhAbhaengigkeit;
    private readonly List<AbhAbhaengigkeit> _abhaengigkeiten = new(); // multiple AbhAbhaengigkeit are possible

    //---------------------------------------------------------------------
    public FahrplanVorgangFahrstrasse(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    {
      _beschreibung = x.GetAttrValue("Beschreibung", string.Empty);

      //_abhAbhaengigkeit = new AbhAbhaengigkeit(this, x.Element("AbhAbhaengigkeit"));

      //foreach (XElement xx in x.Elements("AbhAbhaengigkeit"))
      //{
      //  _abhaengigkeiten.Add(new AbhAbhaengigkeit(this, xx));
      //}

      _abhaengigkeiten.AddRange(from XElement xe in x.Elements(_knownElems[0])
                                select new AbhAbhaengigkeit(this, xe));

      //if (_abhAbhaengigkeit.NodeName == null)
      //{ 
      //  _log.Error("FahrplanVorgangFahrstrasse create: " + _abhAbhaengigkeit.ToString());
      //}
    }

    //---------------------------------------------------------------------
    public FahrplanVorgangFahrstrasse(IZusiObjectParent parent, FahrplanVorgangFahrstrasse source)
        : base(parent, source)
    {
      _beschreibung = source._beschreibung;

      //_abhAbhaengigkeit = new AbhAbhaengigkeit(this, source._abhAbhaengigkeit);

      //foreach (AbhAbhaengigkeit a in source._abhaengigkeiten)
      //{
      //  _abhaengigkeiten.Add(new AbhAbhaengigkeit(this, a));
      //}

      _abhaengigkeiten.AddRange(from AbhAbhaengigkeit a in source._abhaengigkeiten
                                select new AbhAbhaengigkeit(this, a));
    }

    //---------------------------------------------------------------------
    protected override void SaveAttributes(XmlWriter writer)
    {
      base.SaveAttributes(writer);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_beschreibung), "Beschreibung", _beschreibung);
    }

    //---------------------------------------------------------------------
    protected override void SaveElements(XmlWriter writer)
    {
      base.SaveElements(writer);
      try
      {
        _abhaengigkeiten.ForEach(a => a.Save(writer));
        //if (_abhAbhaengigkeit.NodeName != null)
        //{
        //  _abhAbhaengigkeit.Save(writer);
        //}
        // NodeName == null is possible as it is possible to have no entry AbhAbhängigkeit
        //else
        //{
        //  _log.Error("FahrplanVorgangFahrstrasse save: _abhAbhaengigkeit.nodename == null");
        //}
      }
      catch (Exception ex)
      {
        //_log.Error("FahrplanVorgangFahrstrasse: " + ex.ToString() + "-" + _abhAbhaengigkeit.ToString());
        _log.Error("FahrplanVorgangFahrstrasse: " + ex.ToString() + "-" + _abhaengigkeiten.ToString());

        //throw (ex);
      }

    }
  }
}
