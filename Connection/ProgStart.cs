using IpcCommLib;
using Sovoma;
using System.IO;
using System.Linq;
using ZusiKlassenLib;

namespace ZusiStart.Connection
{
  enum ProgDisplayState
  {
    Normal,
    Minimized,
    Maximized
  }

  class ProgStart : BackgroundProcess
  {
    // static string Bildfahrplanfilename = @"C:\Program Files\Zusi3\_Tools\ZUSIBildfahrplan\TimetableGraphProject.exe";
    //---------------------------------------------------------------------
    public ProgStart(string Bildfahrplanfilename, ProgDisplayState state)
            : base(Bildfahrplanfilename, Path.GetDirectoryName(Bildfahrplanfilename), false)
    {
      if (state == ProgDisplayState.Minimized)
      {
        IsMinimized = true;
      }
      if (state == ProgDisplayState.Maximized)
      {
        IsMaximized = true;
      }
      UseShellExecute = false;
    }

    //---------------------------------------------------------------------
    public override int Start(string arg)
    {
      //return base.Start(arg.QuoteIf(arg.Contains(' ')));
      return base.Start(arg);

    }
  }
}
