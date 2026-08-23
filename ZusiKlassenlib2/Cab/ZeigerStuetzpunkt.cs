using Sovoma;
using System;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class ZeigerStuetzpunkt : ZusiObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "ZeigerBreite",
            "ZeigerWinkel",
            "ZeigerWert",
            "ZeigerFarbe",
            "ZeigerLaenge",
            "ZeigerX",
            "ZeigerY"
        };
#pragma warning restore IDE0052

        private bool _break;
        private float _arcPreview;

        private readonly float _width;
        private float _arc;
        private float _value;
        private readonly float _length;
        private readonly float _x;
        private readonly float _y;
        private readonly uint _color;

        public bool IsBreak
        {
            get { return _break; }
            set
            {
                if (_break != value)
                {
                    _break = value;
                    RaisePropertyChanged(nameof(IsBreak));
                }
            }
        }

        public bool IsNegative { get { return _value < 0.0f; } }

        public float Arc
        {
            get { return _arc; }
            set
            {
                if (_arc != value)
                {
                    _arc = _arcPreview = value;
                    IsDirty = true;
                    RaisePropertyChanged(nameof(Arc));
                    RaisePropertyChanged(nameof(ArcPreview));
                    RaisePropertyChanged(nameof(IsNegative));
                }
            }
        }

        public float ArcPreview
        {
            get { return _arcPreview; }
            set
            {
                if (_arcPreview != value)
                {
                    _arcPreview = value;
                    RaisePropertyChanged("ArcPreView");
                    RaisePropertyChanged(nameof(IsNegative));
                }
            }
        }

        public float Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    RaisePropertyChanged(nameof(Value));
                }
            }
        }

        public ZeigerStuetzpunkt(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _width = GetAttrValueFloat(x, "ZeigerBreite", 0.0f);
            _arc = _arcPreview = GetAttrValueFloat(x, "ZeigerWinkel", 0.0f);
            _value = GetAttrValueFloat(x, "ZeigerWert", 0.0f);
            _length = GetAttrValueFloat(x, "ZeigerLaenge", 0.0f);
            _x = GetAttrValueFloat(x, "ZeigerX", 0.0f);
            _y = GetAttrValueFloat(x, "ZeigerY", 0.0f);
            _color = x.GetAttrValue("ZeigerFarbe", (uint)0x00000000);
        }

        protected override void SaveAttributes(XmlWriter writer)
        {
            writer.WriteAttributeFloat("ZeigerBreite", _width, 3);
            writer.WriteAttributeFloat("ZeigerWinkel", _arc, 4);
            writer.WriteAttributeFloat("ZeigerWert", _value, -1);
            writer.WriteAttributeFloat("ZeigerLaenge", _length, 4);
            writer.WriteAttributeFloat("ZeigerX", _x, -1);
            writer.WriteAttributeFloat("ZeigerY", _y, -1);
            writer.WriteAttributeColor("ZeigerFarbe", _color);
        }
    }
}
