using Sovoma;
using Sovoma.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZusiKlassenLib;
using ZusiKlassenLib.Common;
using ZusiKlassenLib.Fahrplan;
using ZusiKlassenLib.TimeTable;
using ZusiStart.Miscellaneous;

namespace ZusiStart.Data
{
    //=========================================================================
    public class FoundTimeTable
    {
        public TimeTable TimeTable { get; set; }
        public List<Zug> Trains { get; set; }
    }

    //=========================================================================
    public class FoundTimeTableViewModel : BaseTreeViewViewModel<FoundTimeTableViewModel, FoundTimeTable>
    {
        //private static readonly string _ttPath = Zusi.ZusiDataPath + @"Timetables\";

        //---------------------------------------------------------------------
        public static FoundTimeTableViewModel BuildViewModel(IEnumerable<FoundTimeTable>? source)
        {
            FoundTimeTableViewModel root = new FoundTimeTableViewModel("root");

            foreach (var m in source.GroupBy(stt => stt.TimeTable.GetTitle()))
            {
                root.Children.Add(new FoundTimeTableViewModel(m));
            }

            root.Initialize();
            return root;
        }

        //---------------------------------------------------------------------
        public FoundTimeTableViewModel(FoundTimeTableViewModel? parent, bool expanded, bool selected)
            : base(parent, expanded, selected)
        { }

        //---------------------------------------------------------------------
        private FoundTimeTableViewModel(string caption)
            : this(null, true, false)
        {
            _displayName = caption;
        }

        //---------------------------------------------------------------------
        private FoundTimeTableViewModel(IGrouping<string, FoundTimeTable> g)
            : this(null, true, false)
        {
            _displayName = g.Key;
            IsBold = true;

            foreach (var stt in g.OrderBy(s => s.TimeTable.StartTime.Value))
            {
                Children.Add(new FoundTimeTableViewModel(stt));
            }
        }

        //---------------------------------------------------------------------
        private FoundTimeTableViewModel(FoundTimeTable stt)
            : this(null, false, false)
        {
            _displayName =stt.TimeTable.StartTime.Value.ToString(@"dd.MM.yyyy ab H U\hr") + " ("+ stt.TimeTable.Name + ")";
            _object = stt;
        }
    }
}
