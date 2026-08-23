/*Copyright 2017 Holger Maaß
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
*you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 *Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using Sovoma;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Fahrplan
{
  //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
  public enum TimeTableItemType
  {
    Default,
    Helper,
    StopOnDemand,
    ServiceStop,
  }

  //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
  public enum TrainSetActionType
  {
    Default,
    TurnTrain,
    CabChange
  }

  //---------------------------------------------------------------------
  [Serializable]
  public class FahrplanEintrag : ZusiObject
  {
    #region attributes & elements
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "Ank",
            "Abf",
            "Betrst",
            "FplEintrag",
            "FzgVerbandAktion",
            "FzgVerbandWendeSignalabstand",
            "FzgVerbandAktionWendesignal",
            "ErsatzsignalzeilePlus1",
            "Signalvorlauf",
            "KuerzungLoeschen"
        };

    private static readonly string[] _knownElems =
    {
            "FahrplanSignalEintrag",
            "FahrplanFahrstrasseEintrag",
            "Ereignis",
            "FahrplanVorgangFahrstrasse",
            "FahrplanVorgangEreignisse"
        };
#pragma warning restore IDE0052
    #endregion

    private readonly DateTime? _arrival;
    private readonly DateTime? _departure;
    private readonly string _betrst;
    private TimeTableItemType _fplEintrag;
    private TrainSetActionType _fzgVerbandAktion;
    private readonly string _fzgVerbandWendeSignalabstand;
    private readonly string _fzgVerbandAktionWendesignal;
    private readonly string _ersatzsignalzeilePlus1;
    private readonly string _signalvorlauf;
    private readonly string _kuerzungLoeschen;
    private readonly bool _possiblyStop;

    private readonly List<FahrplanSignalEintrag> _fahrplanSignalEintraege = new();
    private readonly List<FahrplanFahrstrasseEintrag> _fahrplanFahrstrasseEintraege = new();
    private readonly List<Ereignis> _ereignisse = new();
    private readonly List<FahrplanVorgangFahrstrasse> _fahrplanVorgaengeFahrstrasse = new();
    private readonly List<FahrplanVorgangEreignisse> _fahrplanVorgangEreignisse = new();

    public DateTime? Arrival => _arrival;
    public string Bestrst => _betrst;
    public DateTime? Departure => _departure;
    public TimeTableItemType FplEintrag
    {
      get => _fplEintrag;
      set
      {
        if (_fplEintrag != value)
        {
          _fplEintrag = value;
          IsDirty = false;
          RaisePropertyChanged(nameof(FplEintrag));
        }
      }
    }
    //public TrainSetActionType FzgVerbandAktion => _fzgVerbandAktion; **HLI added get and set methods
    public TrainSetActionType FzgVerbandAktion
    {
      get => _fzgVerbandAktion;
      set
      {
        _fzgVerbandAktion = value;
        RaisePropertyChanged(nameof(FzgVerbandAktion));
      }
    }

    public List<FahrplanSignalEintrag> FahrplanSignalEintraege => _fahrplanSignalEintraege;

    public bool IsPossiblyStop => _possiblyStop;

    public FahrplanEintrag(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    {
      _arrival = ZusiDate.Parse(x.GetAttrValue("Ank", (string)null));
      _departure = ZusiDate.Parse(x.GetAttrValue("Abf", (string)null));
      _betrst = x.GetAttrValue("Betrst", (string)null);
      _fplEintrag = x.GetAttrValue("FplEintrag", TimeTableItemType.Default);
      _fzgVerbandAktion = x.GetAttrValue("FzgVerbandAktion", TrainSetActionType.Default);
      _fzgVerbandWendeSignalabstand = x.GetAttrValue("FzgVerbandWendeSignalabstand", (string)null);
      _fzgVerbandAktionWendesignal = x.GetAttrValue("FzgVerbandAktionWendesignal", (string)null);
      _ersatzsignalzeilePlus1 = x.GetAttrValue("ErsatzsignalzeilePlus1", (string)null);
      _signalvorlauf = x.GetAttrValue("Signalvorlauf", (string)null);
      _kuerzungLoeschen = x.GetAttrValue("KuerzungLoeschen", (string)null);

      _possiblyStop = (_arrival != null && _departure != null && (_fplEintrag == TimeTableItemType.Default || _fplEintrag == TimeTableItemType.StopOnDemand));

      foreach (XElement xx in x.Elements("FahrplanSignalEintrag"))
      {
        _fahrplanSignalEintraege.Add(new FahrplanSignalEintrag(this, xx));
      }
      foreach (XElement xx in x.Elements("FahrplanFahrstrasseEintrag"))
      {
        _fahrplanFahrstrasseEintraege.Add(new FahrplanFahrstrasseEintrag(this, xx));
      }
      foreach (XElement xx in x.Elements("Ereignis"))
      {
        _ereignisse.Add(new Ereignis(this, xx));
      }
      foreach (XElement xx in x.Elements("FahrplanVorgangFahrstrasse"))
      {
        _fahrplanVorgaengeFahrstrasse.Add(new FahrplanVorgangFahrstrasse(this, xx));
      }
      foreach (XElement xx in x.Elements("FahrplanVorgangEreignisse"))
      {
        _fahrplanVorgangEreignisse.Add(new FahrplanVorgangEreignisse(this, xx));
      }
    }

    public FahrplanEintrag(IZusiObjectParent parent, FahrplanEintrag source)
        : base(parent, source)
    {
      _arrival = source._arrival;
      _departure = source._departure;
      _betrst = source._betrst;
      _fplEintrag = source._fplEintrag;
      _fzgVerbandAktion = source._fzgVerbandAktion;
      _fzgVerbandWendeSignalabstand = source._fzgVerbandWendeSignalabstand;
      _fzgVerbandAktionWendesignal = source._fzgVerbandAktionWendesignal;
      _ersatzsignalzeilePlus1 = source._ersatzsignalzeilePlus1;
      _signalvorlauf = source._signalvorlauf;
      _kuerzungLoeschen = source._kuerzungLoeschen;

      foreach (FahrplanSignalEintrag fse in source._fahrplanSignalEintraege)
      {
        _fahrplanSignalEintraege.Add(new FahrplanSignalEintrag(this, fse));
      }
      foreach (FahrplanFahrstrasseEintrag ffe in source._fahrplanFahrstrasseEintraege)
      {
        _fahrplanFahrstrasseEintraege.Add(new FahrplanFahrstrasseEintrag(this, ffe));
      }
      foreach (Ereignis e in source._ereignisse)
      {
        _ereignisse.Add(new Ereignis(this, e));
      }
      foreach (FahrplanVorgangFahrstrasse fvf in source._fahrplanVorgaengeFahrstrasse)
      {
        _fahrplanVorgaengeFahrstrasse.Add(new FahrplanVorgangFahrstrasse(this, fvf));
      }
      foreach (FahrplanVorgangEreignisse fve in source._fahrplanVorgangEreignisse)
      {
        _fahrplanVorgangEreignisse.Add(new FahrplanVorgangEreignisse(this, fve));
      }
    }

    protected override void SaveAttributes(XmlWriter writer)
    {
      base.SaveAttributes(writer);
      writer.WriteAttributeDateTimeIf(_arrival != null, "Ank", _arrival.GetValueOrDefault(), "yyyy-MM-dd HH:mm:ss");
      writer.WriteAttributeDateTimeIf(_departure != null, "Abf", _departure.GetValueOrDefault(), "yyyy-MM-dd HH:mm:ss");
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_betrst), "Betrst", _betrst);
      writer.WriteAttributeIf(_fplEintrag != TimeTableItemType.Default, "FplEintrag", (int)_fplEintrag);
      writer.WriteAttributeIf(_fzgVerbandAktion != TrainSetActionType.Default, "FzgVerbandAktion", (int)_fzgVerbandAktion);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_fzgVerbandWendeSignalabstand), "FzgVerbandWendeSignalabstand", _fzgVerbandWendeSignalabstand);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_fzgVerbandAktionWendesignal), "FzgVerbandAktionWendesignal", _fzgVerbandAktionWendesignal);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_ersatzsignalzeilePlus1), "ErsatzsignalzeilePlus1", _ersatzsignalzeilePlus1);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_signalvorlauf), "Signalvorlauf", _signalvorlauf);
      writer.WriteAttributeIf(!string.IsNullOrEmpty(_kuerzungLoeschen), "KuerzungLoeschen", _kuerzungLoeschen);
    }

    protected override void SaveElements(XmlWriter writer)
    {
      base.SaveElements(writer);
      _fahrplanSignalEintraege.ForEach(fse => fse.Save(writer));
      _fahrplanFahrstrasseEintraege.ForEach(ffe => ffe.Save(writer));
      _ereignisse.ForEach(e => e.Save(writer));
      _fahrplanVorgaengeFahrstrasse.ForEach(fvf => fvf.Save(writer));
      _fahrplanVorgangEreignisse.ForEach(fve => fve.Save(writer));
    }
  }
}
