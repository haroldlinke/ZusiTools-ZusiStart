using IpcCommLib;
using Sovoma;
using System.IO;
using System.Linq;
using ZusiKlassenLib2;

namespace ZusiStart.Connection
{
  
  class ProgMeter : BackgroundProcess
  {
    // static string Bildfahrplanfilename = @"C:\Program Files\Zusi3\_Tools\ZUSIBildfahrplan\TimetableGraphProject.exe";
    //---------------------------------------------------------------------
    public ProgMeter(string Bildfahrplanfilename, DisplayState state)
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
      return base.Start(arg);

    }
  }
}
