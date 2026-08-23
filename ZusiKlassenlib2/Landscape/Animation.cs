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
using Sovoma;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Landscape
{
    //=========================================================================
    public enum AnimationType
    {
        /* AniID - type of animation
         *  0 undefiniert/signalgesteuert
         *  1 zeitlich kontinuierlich
         *  2 Geschwindigkeit (angetrieben, gebremst)
         *  3 Geschwindigkeit (gebremst)
         *  4 Geschwindigkeit (angetrieben)
         *  5 Geschwindigkeit
         *  6 Gleiskrümmung Fahrzeuganfang
         *  7 Gleiskrümmung Fahrzeugende
         *  8 Stromabnehmer A (1., vorderes Drehgestell)
         *  9 Stromabnehmer B (1., hinteres Drehgestell)
         * 10 Stromabnehmer C (2., vorderes Drehgestell)
         * 11 Stromabnehmer D (2., hinteres Drehgestell)
         * 12 Türen links
         * 13 Türen rechts
         * 14 Neigetechnik
         * 15 Türen links A
         * 16 Türen rechts A
         * 17 Türen links B
         * 18 Türen rechts B
         * 19 Türen links C
         * 20 Türen rechts C
         * 21 Türen links D
         * 22 Türen rechts D
         */
        All = -1,
        Undefined = 0,
        Continuous = 1,
        VelocityAD = 2,
        VelocityD = 3,
        VelocityA = 4,
        Velocity = 5,
        BogieFront = 6,
        BogieRear = 7,
        PantographFront1 = 8,
        PantographRear1 = 9,
        PantographFront2 = 10,
        PantographRear2 = 11,
        DoorsLeft = 12,
        DoorsRight = 13,
        Tilting = 14,
        DoorsLeftA,
        DoorsRightA,
        DoorsLeftB,
        DoorsRightB,
        DoorsLeftC,
        DoorsRightC,
        DoorsLeftD,
        DoorsRightD
    }

    //=========================================================================
    [Serializable]
    public class AniNrs : ZusiObject
    {
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "AniNr"
        };
#pragma warning restore IDE0052

        private readonly int _nr;

        //---------------------------------------------------------------------
        public AniNrs(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _nr = x.GetAttrValue("AniNr", 0);
        }

        //---------------------------------------------------------------------
        public AniNrs(IZusiObjectParent parent, int nr)
            : base(parent, new XElement("AniNrs"))
        {
            _nr = nr;
        }

        //---------------------------------------------------------------------
        public static implicit operator int(AniNrs obj)
        {
            return obj._nr;
        }
    }

    //=========================================================================
    [Serializable]
    public class Animation : ZusiObject
    {
        /* AniID - type of animation
         *  0 undefiniert/signalgesteuert
         *  1 zeitlich kontinuierlich
         *  2 Geschwindigkeit (angetrieben, gebremst)
         *  3 Geschwindigkeit (gebremst)
         *  4 Geschwindigkeit (angetrieben)
         *  5 Geschwindigkeit
         *  6 Gleiskrümmung Fahrzeuganfang
         *  7 Gleiskrümmung Fahrzeugende
         *  8 Stromabnehmer A (1., vorderes Drehgestell)
         *  9 Stromabnehmer B (1., hinteres Drehgestell)
         * 10 Stromabnehmer C (2., vorderes Drehgestell)
         * 11 Stromabnehmer D (2., hinteres Drehgestell)
         * 12 Türen links
         * 13 Türen rechts
         * 14 Neigetechnik
         * 15 Türen links A
         * 16 Türen rechts A
         * 17 Türen links B
         * 18 Türen rechts B
         * 19 Türen links C
         * 20 Türen rechts C
         * 21 Türen links D
         * 22 Türen rechts D
         */

        //---------------------------------------------------------------------
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "AniBeschreibung",
            "AniID",
            "AniLoopen"
        };

        //---------------------------------------------------------------------
        private static readonly string[] _knownElems =
        {
            "AniNrs"
        };
#pragma warning restore IDE0052

        private readonly int _id;
        private readonly string _beschreibung;
        private readonly bool _loop;
        private readonly List<int> _aniNrs = new();

        public AnimationType AniID => (AnimationType)_id;
        public string Description => _beschreibung;
        public bool IsLoop => _loop;
        public List<int> Numbers => _aniNrs;

        //---------------------------------------------------------------------
        public Animation(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _id = x.GetAttrValue("AniID", 0);
            _beschreibung = x.GetAttrValue("AniBeschreibung", "");
            _loop = x.GetAttrValue("AniLoopen", false);

            foreach (XElement xa in x.Elements("AniNrs"))
            {
                _aniNrs.Add(new AniNrs(this, xa));
            }

            if (_aniNrs.Count == 0)
            {
                _aniNrs.Add(new AniNrs(this, 0));
            }
        }
    }

    //=========================================================================
    [Serializable]
    public class MeshAnimation : ZusiObject
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MeshAnimation));

        //---------------------------------------------------------------------
#pragma warning disable IDE0052
        private static readonly string[] _knownAttribs =
        {
            "AniNr",
            "AniIndex",
            "AniGeschw"
        };

        //---------------------------------------------------------------------
        private static readonly string[] _knownElems =
        {
            "AniPunkt"
        };
#pragma warning restore IDE0052

        private readonly int _aniNr;
        private readonly int _aniIndex;
        private readonly float _aniGeschw;

        private readonly Transform3DGroup _transform;
        private readonly List<AniPunkt> _punkte = new();
        private readonly float _maxTime;
        private readonly float _minTime;

        public AnimationType AnimationType => Parent is Animation a ? a.AniID : AnimationType.Undefined;
        public float AniGeschw { get { return _aniGeschw; } }
        public int AniIndex { get { return _aniIndex; } }
        public int AniNr { get { return _aniNr; } }

        public List<AniPunkt> Punkte { get { return _punkte; } }
        public Transform3D Transform => _transform;

        //---------------------------------------------------------------------
        public MeshAnimation(IZusiObjectParent parent, XElement x)
            : base(parent, x)
        {
            _aniNr = x.GetAttrValue("AniNr", 0);
            _aniIndex = x.GetAttrValue("AniIndex", 0);
            _aniGeschw = GetAttrValueFloat(x,"AniGeschw", 0.0f);

            foreach (XElement ap in x.Elements("AniPunkt"))
            {
                _punkte.Add(new AniPunkt(this, ap));
            }
            _punkte.Sort((p1, p2) => p1.Zeit < p2.Zeit ? -1 : (p1.Zeit > p2.Zeit ? 1 : 0));
            if (_punkte.Count > 0)
            {
                _minTime = _punkte[0].Zeit;
#if NET48
                _maxTime = _punkte[_punkte.Count - 1].Zeit;
#else
                _maxTime = _punkte[^1].Zeit;
#endif
            }

            _transform = new Transform3DGroup();
            GetTransformComponents(0f, out Quaternion q, out Vector3D v);
            _transform.Children.Add(new RotateTransform3D(new QuaternionRotation3D(q)));
            _transform.Children.Add(new TranslateTransform3D(v));
        }

        //---------------------------------------------------------------------
        public void AnimateTo(double time)
        {
            AnimateTo((float)time);
        }

        //---------------------------------------------------------------------
        public void AnimateTo(float time)
        {
            GetTransformComponents(time, out Quaternion q, out Vector3D v);
            RotateTransform3D r = _transform.Children[0] as RotateTransform3D;
            (r.Rotation as QuaternionRotation3D).Quaternion = q;
            TranslateTransform3D t = _transform.Children[1] as TranslateTransform3D;
            t.OffsetX = v.X;
            t.OffsetY = v.Y;
            t.OffsetZ = v.Z;
        }

        //---------------------------------------------------------------------
        protected override void SaveAttributes(XmlWriter writer)
        {
            base.SaveAttributes(writer);

            writer.WriteAttributeIf(_aniNr != 0, "AniNr", _aniNr);
            writer.WriteAttributeIf(_aniIndex != 0, "AniIndex", _aniIndex);
            writer.WriteAttributeFloatIf(_aniGeschw != 0, "AniGeschw", _aniGeschw, -1);
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            _punkte.ForEach(p => p.Save(writer));
        }

        //---------------------------------------------------------------------
        private void GetTransformComponents(float time, out Quaternion rotation, out Vector3D translation)
        {
            if (_punkte.Count == 0)
            {
                rotation = new Quaternion();
                translation = new Vector3D();
                return;
            }

            time = Math.Min(Math.Max(time, _minTime), _maxTime);
            AniPunkt p0 = _punkte.LastOrDefault(p => time >= p.Zeit);
            if (p0.Zeit == time)
            {
                rotation = p0.Q;
                translation = p0.P.ToVector3D();
            }
            else
            {
                int i = _punkte.IndexOf(p0) + 1;
                if (i < _punkte.Count)
                {
                    AniPunkt p1 = _punkte[i];

                    //       1               x
                    // ------------- = -------------
                    // (p1.Z - p0.Z)   (time - p0.Z)
                    double f = (time - p0.Zeit) / (p1.Zeit - p0.Zeit);
                    rotation = Quaternion.Slerp(p0.Q, p1.Q, f);
                    translation = ZPoint3D.Interpolate(p0.P, p1.P, f);
                }
                else
                {
                    // time beyond limits
                    Log.Warn($"No point for time {time} found, an empty transform matrix will be returned.");

                    rotation = new Quaternion();
                    translation = new Vector3D();
                }
            }
        }
    }

    //=========================================================================
    public class AnimationInfo
    {
        private readonly List<MeshAnimation> _animations = new();

        public List<MeshAnimation> Animations => _animations;
        public bool IsMoving { get; set; }
        public AnimationType Type { get; set; }
        public float Time { get; set; }

        public void AnimateAllTo(double time)
        {
            AnimateAllTo((float)time);
        }

        public void AnimateAllTo(float time)
        {
            foreach (MeshAnimation ma in _animations)
            {
                ma.AnimateTo(time);
            }
        }
    }
}
