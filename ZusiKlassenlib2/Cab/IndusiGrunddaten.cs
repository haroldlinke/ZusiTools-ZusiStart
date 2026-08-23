using Sovoma;
using System;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    [Serializable]
    public class IndusiBasisZugdaten : ZusiObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "TfNummer",
            "BRA",
            "BRH",
            "VMZ",
            "ZL"
        };
#pragma warning restore IDE0052

        private int _brh;
        private uint _bra;
        private string _tfNummer;
        private string _vmz;
        private string _zl;

        //---------------------------------------------------------------------
        public int BRH
        {
            get => _brh;
            set => _brh = value;
        }

        //---------------------------------------------------------------------
        public uint BRA
        {
            get => _bra;
            set => _bra = value;
        }

        //---------------------------------------------------------------------
        public string TfNummer
        {
            get => _tfNummer;
            set => _tfNummer = value;
        }

        //---------------------------------------------------------------------
        public string VMZ
        {
            get => _vmz;
            set => _vmz = value;
        }

        //---------------------------------------------------------------------
        public string ZL
        {
            get => _zl;
            set => _zl = value;
        }

        //---------------------------------------------------------------------
        public IndusiBasisZugdaten(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _tfNummer = x.GetAttrValue("TfNummer", "");
            _bra = x.GetAttrValue("BRA", 0u);
            _brh = x.GetAttrValue("BRH", 0);
            _vmz = x.GetAttrValue("VMZ", "");
            _zl = x.GetAttrValue("ZL", "");
        }

        //---------------------------------------------------------------------
        public IndusiBasisZugdaten(IZusiObjectParent parent, IndusiBasisZugdaten source)
            : base(parent, source)
        {
            _tfNummer = source._tfNummer;
            _bra = source._bra;
            _brh = source._brh;
            _vmz = source._vmz;
            _zl = source._zl;
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeIf(!string.IsNullOrEmpty(_tfNummer), "TfNummer", _tfNummer);
            writer.WriteAttributeIf(_bra != 0, "BRA", _bra);
            writer.WriteAttributeIf(_brh != 0, "BRH", _brh);
            writer.WriteAttributeIf(!string.IsNullOrEmpty(_vmz), "VMZ", _vmz);
            writer.WriteAttributeIf(!string.IsNullOrEmpty(_zl), "ZL", _zl);
        }
    }

    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    [Serializable]
    public class IndusiGrunddaten : IndusiBasisZugdaten
    {
        public IndusiGrunddaten(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }

    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    [Serializable]
    public class IndusiErsatzzugdaten : IndusiBasisZugdaten
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "ZugsicherungHS",
            "Lufthahn",
            "PZBStoerschalter"
        };
#pragma warning restore IDE0052

        public IndusiErsatzzugdaten(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        { }
    }
}
