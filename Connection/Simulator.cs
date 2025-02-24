using IpcCommLib;
using Sovoma;
using System.IO;
using System.Linq;
using ZusiKlassenLib;

namespace ZusiStart.Connection
{
  enum SimulatorState
  {
    Normal,
    Minimized,
    Maximized
  }

  class Simulator : BackgroundProcess
  {
    //---------------------------------------------------------------------
    public Simulator(SimulatorState state)
        : base(Zusi.Executable, Path.GetDirectoryName(Zusi.Executable), false)
    {
      if (state == SimulatorState.Minimized)
      {
        IsMinimized = true;
      }
      if (state == SimulatorState.Maximized)
      {
        IsMaximized = true;
      }
      UseShellExecute = false;
      CreateNoWindow = true;
    }

    //---------------------------------------------------------------------
    public override int Start(string arg)
    {
      return base.Start(arg.QuoteIf(arg.Contains(' ')));
    }
  }
}
