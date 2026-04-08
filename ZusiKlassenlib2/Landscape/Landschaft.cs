using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib2.Common;

namespace ZusiKlassenLib2.Landscape
{
    public class Landschaft : ZusiObject, ILandscapeObject, I3DModel
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Landschaft));

        #region attributes & elements
#pragma warning disable IDE0052
        private static readonly string[] _knownElems =
        {
            "Verknuepfte",
            "lsb",
            "SubSet",
            "Animation",
            "VerknAnimation",
            "MeshAnimation",
            "Ankerpunkt",
            "LandschaftSound"
        };
#pragma warning restore IDE0052
        #endregion

        private readonly string _path;
        private readonly List<Animation> _localAnimations = new();
        private readonly List<Verknuepfte> _tiles = new();
        private readonly List<Verknuepfte> _details = new();
        private readonly List<Verknuepfte> _objects = new();
        private readonly Datei _lsb;
        private readonly List<SubSet> _subSets = new();
        private readonly List<MeshAnimation> _verknAnimations = new();
        private readonly List<MeshAnimation> _meshAnimations = new();
        private readonly LandschaftSound _sound;

        public Animation[] Animations => GetAnimations();
        public List<Verknuepfte> Details => Details1;
        public List<Verknuepfte> Objects => _objects;
        public List<SubSet> SubSets => _subSets;
        public List<Verknuepfte> Tiles => _tiles;

        public List<Verknuepfte> Details1 => _details;

        #region ctor

        //---------------------------------------------------------------------
        public Landschaft(ZusiDocumentBase parent, string path, XElement x)
            : base(parent, x)
        {
            _path = path;

            //Regex regex = new Regex(@"(.*?)\.lod(?<lod>[0-3])\.ls3");
            //Match m = regex.Match(parent.Filename);

            if (!string.IsNullOrEmpty(path))
            {
                foreach (XElement xv in x.Elements("Verknuepfte"))
                {
                    Verknuepfte v = new(this, xv);
                    if (v.Flags.HasFlag(LoadFlagsType.TileFile))
                    {
                        _tiles.Add(v);
                    }
                    else if (v.Flags.HasFlag(LoadFlagsType.TileDetailFile))
                    {
                        _details.Add(v);
                    }
                    else
                    {
                        _objects.Add(v);
                    }
                }

                _lsb = x.GetOptionalElement(this, "lsb", (p, e) => new Datei(p, e));

                string lsbfile = Path.ChangeExtension(parent.Filename, "lsb");
                if (File.Exists(lsbfile))
                {
                    using FileStream fs = new(lsbfile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using BinaryReader lsbReader = new(fs);

                    foreach (XElement xs in x.Elements("SubSet"))
                    {
                        _subSets.Add(new SubSet(this, xs, lsbReader));
                    }

                    // Die BR 614/914 hat ein Problem im 3D-Objekt. Dadurch kommt es zur halbseitigen Überlagerung der Fenster.
                    // Der Grund dafür ist, dass das Mesh für die Beleuchtung der Fenster exakt an der gleichen Position liegt,
                    // wie die Fenster selber. DirectX (respektive Zusi) kommt damit klar, WPF nicht.
                    // Meine derzeitige Lösung ist, das entsprechende Mesh auf beiden Seiten 0,1 mm schmaler zu machen.

                    Regex regex = new(@"614_ob[45]??(.*?)\.lod[0-3]\.lsb|614_pop4\.lod0\.lsb");
                    Match m = regex.Match(lsbfile);
                    if (m.Success)
                    {
                        _subSets[1].Hack();
                    }
                    regex = new Regex(@"914_(ob|pop)\.lod0\.lsb");
                    m = regex.Match(lsbfile);
                    if (m.Success)
                    {
                        _subSets[0].Hack();
                        _subSets[1].Hack();
                    }
                }
                else
                {
                    foreach (XElement xs in x.Elements("SubSet"))
                    {
                        _subSets.Add(new SubSet(this, xs, null));
                    }
                }
            }

            foreach (XElement xa in x.Elements("Animation"))
            {
                _localAnimations.Add(new Animation(this, xa));
            }

            foreach (XElement xva in x.Elements("VerknAnimation"))
            {
                MeshAnimation ma = new(null, xva);

                Animation ani = _localAnimations.FirstOrDefault(a => a.Numbers.Contains(ma.AniNr));
                if (ani == null)
                {
                    Log.WarnFormat("file {0}{1}   linked animation: animation #{2} not declared",
                        GetDocument().Filename, Environment.NewLine, ma.AniNr);
                }
                else
                {
                    if (ma.AniIndex >= _objects.Count)
                    {
                        Log.WarnFormat("file {0}{1}   linked animation for link #{2} requested, but only {3} links exists",
                            GetDocument().Filename, Environment.NewLine, ma.AniIndex, _objects.Count);
                    }
                    else
                    {
                        ma.ChangeParent(ani);
                        _verknAnimations.Add(ma);
                        _objects[ma.AniIndex].LinkedAnimation = ma;
                    }
                }
            }

            foreach (XElement xma in x.Elements("MeshAnimation"))
            {
                MeshAnimation ma = new(this, xma);

                Animation ani = _localAnimations.FirstOrDefault(a => a.Numbers.Contains(ma.AniNr));
                if (ani == null)
                {
                    Log.WarnFormat("file {0}{1}   mesh animation: animation #{2} not declared",
                        GetDocument().Filename, Environment.NewLine, ma.AniNr);
                }
                else
                {
                    if (ma.AniIndex >= _subSets.Count)
                    {
                        Log.WarnFormat("file {0}{1}   mesh animation for subset #{2} requested, but only {3} subsets exists",
                            GetDocument().Filename, Environment.NewLine, ma.AniIndex, _subSets.Count);
                    }
                    else
                    {
                        ma.ChangeParent(ani);
                        _meshAnimations.Add(ma);
                        _subSets[ma.AniIndex].Animation = ma;
                    }
                }
            }

            _sound = GetOptionalObject<LandschaftSound>(this, x.Element("LandschaftSound"));
        }

        //---------------------------------------------------------------------
        public Landschaft(ZusiDocumentBase parent, XElement x)
            : this(parent, null, x)
        { }

        #endregion

        //---------------------------------------------------------------------
        public void GetLoDInfo(LoDInfo lodInfo)
        {
            foreach (SubSet s in _subSets)
            {
                lodInfo.AddMeshInfo(s.GetMeshInfo());
            }
        }

        //---------------------------------------------------------------------
        public ObjectInfo GetObjectInfo()
        {
            ObjectInfo objectInfo = new();
            foreach (var v in _objects)
            {
                List<LoDInfo> lodInfos = v.GetLodInfo();
                lodInfos.ForEach(li =>
                {
                    int lod = li.LoD;
                    objectInfo[lod] = li;
                });
            }
            return objectInfo;
        }

        //---------------------------------------------------------------------
        public void DumpLSB(string filename)
        {
            using FileStream fs = new(filename, FileMode.Create, FileAccess.Write, FileShare.Read);
            using StreamWriter sw = new(fs, Encoding.UTF8);
            foreach (SubSet s in _subSets)
            {
                s.Dump(sw);
            }
            sw.Flush();
        }

        //---------------------------------------------------------------------
        public Model3D CreateModel()
        {
            return CreateModel(15);
        }

        //---------------------------------------------------------------------
        public Model3D CreateModel(float distance, params AnimationInfo[] infos)
        {
            Model3DGroup model = new();

            foreach (var v in _objects.Where(vv => vv.IsVisibleAt(distance)))
            {
                try
                {
                    Model3D m = v.CreateModel(distance, infos);
                    if (m != null)
                    {
                        model.Children.Add(m);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }

            foreach (SubSet s in _subSets.OrderBy(s => s.RenderWeightning))
            {
                model.Children.Add(s.CreateModel(/*0, */infos));
            }

            return model;
        }

        //---------------------------------------------------------------------
        public void CollectRelatedFiles(List<string> files)
        {
            _objects.ForEach(o => o.CollectRelatedFiles(files));
            _subSets.ForEach(s => s.CollectRelatedFiles(files));
            if (_lsb != null)
            {
                files.Add(_lsb.FullPath);
            }
        }

        //---------------------------------------------------------------------
        protected override void SaveElements(XmlWriter writer)
        {
            base.SaveElements(writer);

            foreach (Verknuepfte v in _tiles)
            {
                v.Save(writer);
            }
            foreach (Verknuepfte v in Details1)
            {
                v.Save(writer);
            }
            foreach (Verknuepfte v in _objects)
            {
                v.Save(writer);
            }

            if (_lsb != null)
            {
                if (_subSets.Count > 0)
                {
                    _lsb.Save(writer);

                    string d = System.IO.Path.GetDirectoryName(_path);
                    string f = System.IO.Path.GetFileNameWithoutExtension(_path);
                    string p = string.Format("{0}\\{1}.lsb", d, f);

                    using FileStream lsb = new(p, FileMode.Create, FileAccess.Write, FileShare.Read);
                    foreach (SubSet s in _subSets)
                    {
                        s.Save(writer);
                        s.SaveMesh(lsb);
                    }

                    lsb.Flush();
                }
            }

            _sound?.Save(writer);
        }

        //---------------------------------------------------------------------
        private class AnimationByTypeComparer : IEqualityComparer<Animation>
        {
            public bool Equals(Animation x, Animation y)
            {
                return x.AniID == y.AniID;
            }

            public int GetHashCode(Animation obj)
            {
                return (int)obj.AniID;
            }
        }

        //---------------------------------------------------------------------
        private static readonly AnimationByTypeComparer _animationByTypeComparer = new();

        //---------------------------------------------------------------------
        private Animation[] GetAnimations()
        {
            List<Animation> tmp = new(_localAnimations);
            foreach (Verknuepfte v in _objects)
            {
                tmp.AddRange(v.LinkedLandscape.Animations);
            }

            return tmp.Distinct(_animationByTypeComparer).ToArray();
        }
    }
}
