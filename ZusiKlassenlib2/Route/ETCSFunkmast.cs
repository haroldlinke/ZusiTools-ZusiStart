using Sovoma;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Route
{
  public class ETCSFunkmast : ZusiObject
  {
    #region attributes & elements
#pragma warning disable IDE0052
    private static readonly string[] _knownAttribs =
    {
            "ETCSSenderadius",
            "ETCSRBCNummer",
            "ETCSRBCTelNummer",
            "ETCSRBCID",
            "ETCSRBCLand",
            "ETCSGSMRNetz",
            "ETCSFSUebergangOS",
            "ETCSTransitionErsatzsignalSR",
            "ETCSReleaseSpeedDefault",
            "ETCSFunkausfallToleranz"

        };

    private static readonly string[] _knownElems =
    {
            "p"
        };
#pragma warning restore IDE0052
    #endregion

    private readonly int _senderadius;
    private readonly string _rbcNummer;
    private readonly string _rbcTelNummer;
    private readonly string _rbcId;
    private readonly string _rbcLand;
    private readonly string _gsmRNetz;
    private readonly string _fsUebergangOS;
    private readonly string _transitionErsatzsignalSR;
    private readonly string _releaseSpeedDefault;
    private readonly string _funkausfallToleranz;
    private readonly p _p;

    public int Senderadius => _senderadius;
    public string RBCNummer => _rbcNummer;
    public string RBCTelNummer => _rbcTelNummer;
    public string RBCId => _rbcId;
    public string RBCLand => _rbcLand;
    public string GSMRNetz => _gsmRNetz;
    public string FSUebergangOS => _fsUebergangOS;
    public string TransitionErsatzsignalSR => _transitionErsatzsignalSR;
    public string ReleaseSpeedDefault => _releaseSpeedDefault;
    public string FunkausfallToleranz => _funkausfallToleranz;
    public p Location => _p;

    //---------------------------------------------------------------------
    public ETCSFunkmast(IZusiObjectParent parent, XElement x)
        : base(parent, x)
    {
      _senderadius = x.GetAttrValue("ETCSSenderadius", 0);
      _rbcNummer = x.GetAttrValue("ETCSRBCNummer", string.Empty);
      _rbcTelNummer = x.GetAttrValue("ETCSRBCTelNummer", string.Empty);
      _rbcId = x.GetAttrValue("ETCSRBCID", string.Empty);
      _rbcLand = x.GetAttrValue("ETCSRBCLand", string.Empty);
      _gsmRNetz = x.GetAttrValue("ETCSGSMRNetz", string.Empty);
      _fsUebergangOS = x.GetAttrValue("ETCSFSUebergangOS", string.Empty);
      _transitionErsatzsignalSR = x.GetAttrValue("ETCSTransitionErsatzsignalSR", string.Empty);
      _releaseSpeedDefault = x.GetAttrValue("ETCSReleaseSpeedDefault", string.Empty);
      _funkausfallToleranz = x.GetAttrValue("ETCSFunkausfallToleranz", string.Empty);

      _p = GetOptionalObject<p>(this, x.Element("p"));
    }

    //---------------------------------------------------------------------
    protected override void SaveAttributes(XmlWriter writer)
    {
      base.SaveAttributes(writer);

      writer.WriteAttributeIf(_senderadius > 0, "ETCSSenderadius", _senderadius);
      writer.WriteAttributeStringIfNotEmpty("ETCSRBCNummer", _rbcNummer);
      writer.WriteAttributeStringIfNotEmpty("ETCSRBCTelNummer", _rbcTelNummer);
      writer.WriteAttributeStringIfNotEmpty("ETCSRBCID", _rbcId);
      writer.WriteAttributeStringIfNotEmpty("ETCSRBCLand", _rbcLand);
      writer.WriteAttributeStringIfNotEmpty("ETCSGSMRNetz", _gsmRNetz);
      writer.WriteAttributeStringIfNotEmpty("ETCSFSUebergangOS", _fsUebergangOS);
      writer.WriteAttributeStringIfNotEmpty("ETCSTransitionErsatzsignalSR", _transitionErsatzsignalSR);
      writer.WriteAttributeStringIfNotEmpty("ETCSReleaseSpeedDefault", _releaseSpeedDefault);
      writer.WriteAttributeStringIfNotEmpty("ETCSFunkausfallToleranz", _funkausfallToleranz);
    }

    //---------------------------------------------------------------------
    protected override void SaveElements(XmlWriter writer)
    {
      base.SaveElements(writer);

      _p.Save(writer);
    }
  }
}
