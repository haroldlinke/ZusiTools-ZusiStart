using Sovoma;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZusiKlassenLib2;
using ZusiKlassenLib2.Common;
using ZusiKlassenLib2.TimeTable;

namespace ZusiStart.Miscellaneous
{
    static class TimeTableHelper
    {
        public static string GetTitle(this TimeTable self)
        {
            ZusiDocumentBase doc = self.GetDocument();
            DataPathType dpt = DataPathType.Unknown;
            string s = Zusi.GetRelativePathOf(doc.Filename, ref dpt) + @"Timetables\";
            string[] ss = s.Split('\\');
            return string.Format("[{0}] {1}", ss[0], ss[1].Replace('_', '-'));
        }
    }
}
