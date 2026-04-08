using IpcCommLib;
using Sovoma;
using System.IO;
using System.Linq;
using ZusiKlassenLib2;

namespace ZusiStart.Connection
{
    enum BildFahrplanState
    {
        Normal,
        Minimized,
        Maximized
    }

    class BildFahrplan : BackgroundProcess
    {
    // static string Bildfahrplanfilename = @"C:\Program Files\Zusi3\_Tools\ZUSIBildfahrplan\TimetableGraphProject.exe";
    //---------------------------------------------------------------------
    public BildFahrplan(string Bildfahrplanfilename, BildFahrplanState state)
            : base(Bildfahrplanfilename, Path.GetDirectoryName(Bildfahrplanfilename), false)
        {
            if (state == BildFahrplanState.Minimized)
            {
                IsMinimized = true;
            }
            if (state == BildFahrplanState.Maximized)
            {
                IsMaximized = true;
            }
            UseShellExecute = false;
        }

        //---------------------------------------------------------------------
        public override int Start(string arg)
        {
            //return base.Start(arg.QuoteIf(arg.Contains(' ')));
      return base.Start( arg);

    }
  }
}
