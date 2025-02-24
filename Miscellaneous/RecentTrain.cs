#define WITH_TIMETABLE

using Sovoma;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ZusiKlassenLib;
using ZusiKlassenLib.Common;
using ZusiKlassenLib.Fahrplan;
using ZusiKlassenLib.TimeTable;

namespace ZusiStart.Miscellaneous
{
    public sealed class RecentTrain : INotifyPropertyChanged
    {
        private readonly string _timeTableName;
        private readonly Zug _train;
        private int _used;

        public string TimeTableName { get => _timeTableName; }
        public Zug Train { get => _train; }
        public int Used
        {
            get => _used;
            private set
            {
                if (_used != value)
                {
                    _used = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Used)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //---------------------------------------------------------------------
        public RecentTrain(Zug train, string timeTableName)
            : this(train, timeTableName, 1)
        { }

        //---------------------------------------------------------------------
        internal RecentTrain(Zug train, string timeTableName, int used)
        {
            _timeTableName = timeTableName;
            _train = train;
            _used = used;
        }

        //---------------------------------------------------------------------
        public void IncUsed()
        {
            Used += 1;
        }

        //---------------------------------------------------------------------
        public void Save(XmlWriter writer)
        {
            writer.WriteStartElement("recenttrain");
            writer.WriteAttribute("used", _used);

            // train
            writer.WriteStartElement("train");

            ZusiDocumentBase doc = _train.GetDocument();
            DataPathType dpt = DataPathType.Unknown;
            string s = Zusi.GetRelativePathOf(doc.Filename, ref dpt);
            writer.WriteString(s);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }

    public sealed class RecentTrainsCollection : ObservableCollection<RecentTrain>
    {
        private string _path;
        private bool _dirty;

        public static readonly string FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ZusiStart", "recenttrains.dat");

        public bool IsDirty { get => _dirty; }

        //---------------------------------------------------------------------
        public new void Add(RecentTrain item)
        {
            RecentTrain r = this.FirstOrDefault(x => x.Train.ID == item.Train.ID);
            if (r != null)
            {
                r.IncUsed();

                bool swapped;
                do
                {
                    swapped = false;
                    for (int i = 0, j = 1; j < Count; i++, j++)
                    {
                        if (Items[i].Used < Items[j].Used)
                        {
                            RecentTrain swap = Items[i];
                            Items[i] = Items[j];
                            Items[j] = swap;
                            swapped = true;
                        }
                    }

                } while (swapped);

                _dirty = true;
            }
            else
            {
                int index = 0;
                for (; index < Count && Items[index].Used >= item.Used; index++)
                { }
                Insert(index, item);
            }
        }

        //---------------------------------------------------------------------
        public new void Clear()
        {
            base.Clear();
            _dirty = true;
        }

        //---------------------------------------------------------------------
        public new void Insert(int index, RecentTrain item)
        {
            base.Insert(index, item);
            _dirty = true;
        }

        //---------------------------------------------------------------------
        public new bool Remove(RecentTrain item)
        {
            _dirty |= base.Remove(item);
            return _dirty;
        }

        //---------------------------------------------------------------------
        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            _dirty = true;
        }

        //---------------------------------------------------------------------
        public void LoadFromFile(IEnumerable<Zug> trains, IEnumerable<TimeTable> timeTables)
        {
            LoadFromFile(FileName, trains, timeTables);
        }

        //---------------------------------------------------------------------
        public void LoadFromFile(string path, IEnumerable<Zug> trains, IEnumerable<TimeTable> timeTables)
        {
            _path = path;

            if (File.Exists(path))
            {
                using FileStream fs = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                try
                {
                    using XmlReader reader = XmlReader.Create(fs);
                    XDocument doc = XDocument.Load(reader);
                    LoadDocument(doc.Root, trains, timeTables);
                }
                catch { }
            }
        }

        //---------------------------------------------------------------------
        public void Save()
        {
            if (IsDirty)
            {
                XmlWriterSettings xmlWriterSettings = new()
                {
                    Indent = true,
                    OmitXmlDeclaration = false,
                };

                EnsureDirectory();

                using XmlWriter writer = XmlWriter.Create(_path, xmlWriterSettings);
                writer.WriteStartDocument(true);
                SaveDocument(writer);
                writer.WriteEndDocument();
            }
        }

        //---------------------------------------------------------------------
        public void SaveAs(string path)
        {
            _path = path;
            Save();
        }

        //---------------------------------------------------------------------
        private void EnsureDirectory()
        {
            string dir = Path.GetDirectoryName(_path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        //---------------------------------------------------------------------
        private void LoadDocument(XElement e, IEnumerable<Zug> trains, IEnumerable<TimeTable> timeTables)
        {
            List<RecentTrain> tmp = new();
            foreach (XElement xr in e.Elements("recenttrain"))
            {
                string trainFile = xr.Element("train").Value;
                Zug z = trains.FirstOrDefault(t =>
                {                    DataPathType dpt = DataPathType.Unknown;
                    string s = Zusi.GetRelativePathOf(t.GetDocument().Filename, ref dpt);
                  return s == trainFile; // dpt != DataPathType.Unknown;
                });
        //string trainFile = xr.Element("train").Value;
        //Zug z = trains.FirstOrDefault(t =>
        //{
        //  DataPathType dpt = DataPathType.Unknown;
        //  string s = Zusi.GetAbsolutePathOf(t.GetDocument().Filename, ref dpt);
        //  return dpt != DataPathType.Unknown;
        //});
        if (z != null)
                {
                    TimeTable timeTable = timeTables.FirstOrDefault(tt => tt.ID == z.BelongsToTimeTable);
                    ZusiDocumentBase doc = timeTable?.GetDocument();
                    string timeTableName = doc != null ? Path.GetFileNameWithoutExtension(doc.Filename) : null;

                    base.Add(new RecentTrain(z, timeTableName, xr.GetAttrValue("used", 1)));
                }
            }
            tmp.Sort((x, y) => x.Used > y.Used ? -1 : (x.Used < y.Used ? 1 : 0));
            tmp.ForEach(i => base.Add(i));
        }

        //---------------------------------------------------------------------
        private void SaveDocument(XmlWriter writer)
        {
            writer.WriteStartElement("recenttrains");
            foreach (RecentTrain item in Items)
            {
                item.Save(writer);
            }
            writer.WriteEndElement();
        }
    }
}
