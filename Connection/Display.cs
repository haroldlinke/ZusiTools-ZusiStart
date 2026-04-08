using IpcCommLib;
using Sovoma;
using System.IO;
using System.Linq;
using ZusiKlassenLib2;

namespace ZusiStart.Connection
{
    enum DisplayState
    {
        Normal,
        Minimized,
        Maximized
    }

    class Display : BackgroundProcess
    {
    // static string Bildfahrplanfilename = @"C:\Program Files\Zusi3\_Tools\ZUSIBildfahrplan\TimetableGraphProject.exe";
    //---------------------------------------------------------------------
    public Display(string Bildfahrplanfilename, DisplayState state)
            : base(Bildfahrplanfilename, Path.GetDirectoryName(Bildfahrplanfilename), false)
        {
            if (state == DisplayState.Minimized)
            {
                IsMinimized = true;
            }
            if (state == DisplayState.Maximized)
            {
                IsMaximized = true;
            }
            UseShellExecute = true;
        }

        //---------------------------------------------------------------------
        public override int Start(string arg)
        {
            //return base.Start(arg.QuoteIf(arg.Contains(' ')));
      return base.Start( arg);

    }
  }
}
