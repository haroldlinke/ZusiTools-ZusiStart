using Sovoma;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Route
{
  public class Strecke : ZusiObject
  {
    #region attributes & elements
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "Himmelsmodell",
            "RekTiefe",
            "SaegelinienEreignisse"
        };

    private static readonly string[] _knownElems =
    {
            "Datei",
            "HintergrundDatei",
            "BefehlsKonfiguration",
            "Kachelpfad",
            "UTM",
            "Huellkurve",
            "SkyDome",
            "StreckenStandort",
            "ModulDateien",
            "ReferenzElemente",
            "StrElement",
            "Fahrstrasse",
            "LoeschFahrstrasse",
            "Beschreibung",
            "ETCSFunkmast",
            "PanoramaDatei"
        };
#pragma warning restore IDE0052
    #endregion

    private Bounds _bounds = null;
    private readonly int _himmelsModell;

    private readonly Datei _datei;
    private readonly Datei _hintergrundDatei;
    private readonly Datei _befehlsKonfiguration;
    private readonly Datei _kachelpfad;
    private readonly Datei _panoramaDatei;
    private readonly ZusiKlassenLib2.Common.UTM _utm;
    private readonly Huellkurve _huellkurve;
    private readonly SkyDome _skyDome;
    private readonly string _rekTiefe;
    private readonly string _saegelinienEreignisse;
    private readonly Beschreibung _beschreibung;
    private readonly List<StreckenStandort> _streckenStandorte = new();
    private readonly List<ModulDateien> _modulDateien = new();
    private readonly List<ReferenzElemente> _refElements = new();
    private readonly List<StrElement> _strElements = new();
    private readonly List<Fahrstrasse> _fahrStrassen = new();
    private readonly List<LoeschFahrstrasse> _loeschFahrStrassen = new();
    private readonly List<ETCSFunkmast> _etcsFunkmasten = new();

    public Bounds Bounds => GetBounds();
    public Datei Datei => _datei;
    public List<ETCSFunkmast> ETCSFunkmasten => _etcsFunkmasten;
    public List<Fahrstrasse> FahrStrassen => _fahrStrassen;
    public Datei Hintergrund => _hintergrundDatei;
    public Huellkurve Huellkurve => _huellkurve;
    public List<LoeschFahrstrasse> LoeschFahrStrassen => _loeschFahrStrassen;
    public List<ModulDateien> ModulDateien => _modulDateien;
    public Datei PanoramaDatei => _panoramaDatei;
    public List<ReferenzElemente> RefElements => _refElements;
    public string RekTiefe => _rekTiefe;
    public string SaegelinienEreignisse => _saegelinienEreignisse;
    public List<StreckenStandort> StreckenStandorte => _streckenStandorte;
    public List<StrElement> StrElements => _strElements;
    public ZusiKlassenLib2.Common.UTM Utm => _utm;

    //---------------------------------------------------------------------
    public Strecke(ZusiDocumentBase parent, XElement x)
        : base(parent, x)
    {
      _himmelsModell = x.GetAttrValue("Himmelsmodell", 0);
      _rekTiefe = x.GetAttrValue("RekTiefe", "");
      _saegelinienEreignisse = x.GetAttrValue("SaegelinienEreignisse", "");

      _datei = GetOptionalObject<Datei>(this, x.Element("Datei"));
      _hintergrundDatei = GetOptionalObject<Datei>(this, x.Element("HintergrundDatei"));
      _befehlsKonfiguration = GetOptionalObject<Datei>(this, x.Element("BefehlsKonfiguration"));
      _kachelpfad = GetOptionalObject<Datei>(this, x.Element("Kachelpfad"));
      _utm = GetOptionalObject<ZusiKlassenLib2.Common.UTM>(this, x.Element("UTM"));
      _huellkurve = GetOptionalObject<Huellkurve>(this, x.Element("Huellkurve"));
      _skyDome = GetOptionalObject<SkyDome>(this, x.Element("SkyDome"));
      _beschreibung = GetOptionalObject<Beschreibung>(this, x.Element("Beschreibung"));
      _panoramaDatei = GetOptionalObject<Datei>(this, x.Element("PanoramaDatei"));

      foreach (XElement xfs in x.Elements("LoeschFahrstrasse"))
      {
        _loeschFahrStrassen.Add(new LoeschFahrstrasse(this, xfs));
      }

      foreach (XElement xe in x.Elements("ETCSFunkmast"))
      {
        _etcsFunkmasten.Add(new ETCSFunkmast(this, xe));
      }

      foreach (XElement xs in x.Elements("StreckenStandort"))
      {
        _streckenStandorte.Add(new StreckenStandort(this, xs));
      }

      foreach (XElement xm in x.Elements("ModulDateien"))
      {
        _modulDateien.Add(new ModulDateien(this, xm));
      }

      foreach (XElement xre in x.Elements("ReferenzElemente"))
      {
        _refElements.Add(new ReferenzElemente(this, xre));
      }

      foreach (XElement xstr in x.Elements("StrElement"))
      {
        StrElement se = new(this, xstr);
        _strElements.Add(se);
      }

      foreach (XElement xfs in x.Elements("Fahrstrasse"))
      {
        _fahrStrassen.Add(new Fahrstrasse(this, xfs));
      }
    }

    //---------------------------------------------------------------------
    protected override void SaveAttributes(XmlWriter writer)
    {
      base.SaveAttributes(writer);

      writer.WriteAttributeIf(_himmelsModell != 0, "Himmelsmodell", _himmelsModell);
      writer.WriteAttributeStringIfNotEmpty("RekTiefe", _rekTiefe);
      writer.WriteAttributeStringIfNotEmpty("SaegelinienEreignisse", _saegelinienEreignisse);
    }

    //---------------------------------------------------------------------
    protected override void SaveElements(XmlWriter writer)
    {
      base.SaveElements(writer);

      _datei.Save(writer);

      foreach (LoeschFahrstrasse fs in _loeschFahrStrassen)
      {
        fs.Save(writer);
      }

      _hintergrundDatei?.Save(writer);
      _panoramaDatei?.Save(writer);
      _befehlsKonfiguration?.Save(writer);
      _kachelpfad?.Save(writer);
      _beschreibung?.Save(writer);
      _utm?.Save(writer);
      _huellkurve?.Save(writer);

      foreach (ETCSFunkmast e in _etcsFunkmasten)
      {
        e.Save(writer);
      }

      _skyDome?.Save(writer);

      foreach (StreckenStandort ss in _streckenStandorte)
      {
        ss.Save(writer);
      }

      foreach (ModulDateien md in _modulDateien)
      {
        md.Save(writer);
      }

      foreach (ReferenzElemente re in _refElements)
      {
        re.Save(writer);
      }

      foreach (StrElement se in _strElements)
      {
        se.Save(writer);
      }

      foreach (Fahrstrasse fs in _fahrStrassen)
      {
        fs.Save(writer);
      }
    }

    //---------------------------------------------------------------------
    private Bounds GetBounds()
    {
      if (_bounds == null)
      {
        _bounds = Bounds.CreateFrom(this);
      }
      return _bounds;
    }
  }
}
