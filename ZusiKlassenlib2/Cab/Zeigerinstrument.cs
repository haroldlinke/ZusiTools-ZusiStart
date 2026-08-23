using Sovoma.WPF.MathEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Cab
{
    [Serializable]
    public class Zeigerinstrument : GraphicElement
    {
        public static bool ExtendedCheck { get; set; }

        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "Traegheit",
            "Zeigertyp",
            "Segmente",
            "Balkenursprung"
        };

        private static readonly string[] _knownElems =
        {
            "ZeigerStuetzpunkt"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly int _zeigerTyp;
        private readonly List<ZeigerStuetzpunkt> _points = new();

        public bool IsAscendingArcSequenceBroken
        {
            get
            {
                bool broken = false;
                if (_zeigerTyp == 0 && InstrumentName.StartsWith("Uhr"))
                {
                    for (int i = 0, j = 1; j < _points.Count; i++, j++)
                    {
                        ZeigerStuetzpunkt z1 = _points[i];
                        ZeigerStuetzpunkt z2 = _points[j];
                        float dxValue = 1.0f;
                        if (ExtendedCheck)
                        {
                            dxValue = Math.Abs(z1.Value - z2.Value);
                        }
                        if (dxValue > 0.02f && z2.Arc < z1.Arc)
                        {
                            z2.IsBreak = true;
                            broken = true;
                        }
                        else
                        {
                            z2.IsBreak = false;
                        }
                    }
                }
                return broken;
            }
        }

        public List<ZeigerStuetzpunkt> Points { get { return _points; } }

        public static string[] KnownAttribs => _knownAttribs;

        public Zeigerinstrument(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            if (_attributes.ContainsKey("Zeigertyp"))
            {
                _zeigerTyp = int.Parse(_attributes["Zeigertyp"]);
            }

            foreach (ZusiObject zo in _objects.Where(e => e is ZeigerStuetzpunkt))
            {
                _points.Add((ZeigerStuetzpunkt)zo);
            }
        }

        public void RepairArcSequence(bool preview)
        {
            if (preview)
            {
                RepairArcSequencePreview();
            }
            else
            {
                if (IsAscendingArcSequenceBroken)
                {
                    float c360 = Math2D.Radians(360.0f);
                    float c = -c360;
                    while (DoRepairArcSequence(c))
                    {
                        c += c360;
                        if (c == 0) c += c360;
                        if (c >= (4 * c360))
                        {
                            throw new Exception("Die Reihe der Winkel kann nicht repariert werden.");
                        }
                        if (!preview)
                        {
                            if (!IsAscendingArcSequenceBroken)
                                break;
                        }
                    }
                }
            }
        }

        private void RepairArcSequencePreview()
        {
            float c360 = Math2D.Radians(360.0f);
            float c = -c360;
            while (DoRepairArcSequencePreview(c))
            {
                c += c360;
                if (c == 0) c += c360;
                if (c >= (4 * c360))
                {
                    throw new Exception("Die Reihe der Winkel kann nicht repariert werden.");
                }
            }
        }

        private bool DoRepairArcSequence(float c)
        {
            for (int i = 0, j = 1; j < _points.Count; i++, j++)
            {
                ZeigerStuetzpunkt z1 = _points[i];
                ZeigerStuetzpunkt z2 = _points[j];
                if (z2.Arc < z1.Arc)
                {
                    if (c < 0)
                    {
                        for (int k = i; k >= 0; k--)
                        {
                            _points[k].Arc += c;
                        }
                    }
                    else
                    {
                        for (int k = j; k < _points.Count; k++)
                        {
                            _points[k].Arc += c;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private bool DoRepairArcSequencePreview(float c)
        {
            for (int i = 0, j = 1; j < _points.Count; i++, j++)
            {
                ZeigerStuetzpunkt z1 = _points[i];
                ZeigerStuetzpunkt z2 = _points[j];
                if (z2.ArcPreview < z1.ArcPreview)
                {
                    if (c < 0)
                    {
                        for (int k = i; k >= 0; k--)
                        {
                            _points[k].ArcPreview += c;
                        }
                    }
                    else
                    {
                        for (int k = j; k < _points.Count; k++)
                        {
                            _points[k].ArcPreview += c;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
