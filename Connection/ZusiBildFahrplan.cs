using IpcCommLib;
using log4net;
using Sovoma;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ZusiStart.Data;
using ZusiStart.Miscellaneous;

namespace ZusiStart.Connection
{
  class ZusiBildFahrplan
  {
    private const string _ZusiProcessName = "TimetableGraphProject";

    //const int SW_HIDE = 0;
    //const int SW_SHOWNORMAL = 1;
    //const int SW_SHOWMINIMIZED = 2;
    const int SW_MAXIMIZE = 3;
    //const int SW_SHOWMAXIMIZED = 3;
    //const int SW_SHOWNOACTIVATE = 4;
    //const int SW_SHOW = 5;
    const int SW_MINIMIZE = 6;
    //const int SW_SHOWMINNOACTIVE = 7;
    //const int SW_SHOWNA = 8;
    const int SW_RESTORE = 9;
    //const int SW_SHOWDEFAULT = 10;
    //const int SW_FORCEMINIMIZE = 11;

    class NativeMethods
    {
      [DllImport("User32.dll")]
      public static extern bool BringWindowToTop(IntPtr hWnd);

      [DllImport("User32.dll")]
      public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }

    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly Lazy<ZusiBildFahrplan> _instance = new(() => new ZusiBildFahrplan());

    private readonly BildFahrplan _runprog; // = new(BildFahrplanState.Minimized);
    private bool _pendingStart;
    private readonly ThreadSafeVar<Process> _zusi = new();

    //---------------------------------------------------------------------
    public static ZusiBildFahrplan Instance => _instance.Value;
    public static bool CanStart => Instance._runprog.CanStart;
    public static bool IsStarted => Instance._zusi.Value != null;

    //---------------------------------------------------------------------
    public static event EventHandler Started;
    public static event EventHandler Terminated;

    //---------------------------------------------------------------------
    private ZusiBildFahrplan()
    {
      string Bildfahrplanfilename = DataManager.Instance.BildfahrplanExePath;
      _runprog = new BildFahrplan(Bildfahrplanfilename, BildFahrplanState.Minimized);
      _runprog.Finished += Sim_Finished;
    }

    //---------------------------------------------------------------------
    private void Sim_Finished(object sender, BackgroundProcessFinishedEventArgs e)
    {
      _zusi.Value = null;
      Terminated?.Invoke(this, EventArgs.Empty);
    }

    //---------------------------------------------------------------------
    private void Maximize_impl(bool bringToTop)
    {
      if (_zusi.Value != null)
      {
        NativeMethods.ShowWindow(_zusi.Value.MainWindowHandle, SW_MAXIMIZE);
        if (bringToTop)
        {
          NativeMethods.BringWindowToTop(_zusi.Value.MainWindowHandle);
        }
      }
    }

    //---------------------------------------------------------------------
    private void Minimize_impl()
    {
      if (_zusi.Value != null)
      {
        NativeMethods.ShowWindow(_zusi.Value.MainWindowHandle, SW_MINIMIZE);
      }
    }

    //---------------------------------------------------------------------
    private void Quit_impl()
    {
      try
      {
        if (_runprog != null)
        {
          _runprog.Stop();
          //  if (_zusi.Value != null)
          //{
          //  if (_zusi.Value.CloseMainWindow())
          //  {
          //    _zusi.Value.Close();
          //  }
          //  else
          //  {
          //    _zusi.Value.Kill();
          //  }

          //  _log.Debug("ZusiBildFahrplan has been terminated");
          //  Terminated?.Invoke(this, EventArgs.Empty);
        }
      }
      catch { };
    }

    //---------------------------------------------------------------------
    private void Restore_impl(bool bringToTop)
    {
      if (_zusi.Value != null)
      {
        NativeMethods.ShowWindow(_zusi.Value.MainWindowHandle, SW_RESTORE);
        if (bringToTop)
        {
          NativeMethods.BringWindowToTop(_zusi.Value.MainWindowHandle);
        }
      }
    }

    //---------------------------------------------------------------------
    private void Start_impl(string filename, string parameter)
    {
      StartZusiAsync(filename, parameter);
      _log.Debug("starting ZusiBildFahrplan has been requested");
    }

    //---------------------------------------------------------------------
    private async void StartZusiAsync(string filename, string parameter)
    {
      if (!_pendingStart)
      {
        _pendingStart = true;
        _zusi.Value = await Task.Run(() => RunZusi(filename, parameter));
        _pendingStart = false;
        _log.Debug("ZusiBildFahrplan has been started");
        Started?.Invoke(this, EventArgs.Empty);

        await Task.Run(() => _runprog.WaitFor());
      }
    }

    //---------------------------------------------------------------------
    private Process RunZusi(string filename, string parameter)
    {
      string ZusiProgfilename = filename.QuoteIf(filename.Contains(" "));
      _runprog.Executable = ZusiProgfilename;

      int n = 0;
      Process[] processes = Process.GetProcessesByName(_ZusiProcessName);

      if (processes != null && processes.Length > 0)
      {
        processes[0].Kill();

        while (true)
        {
          Thread.Sleep(1000);
          if (n++ >= 120)
          {
            throw new InvalidOperationException("ZusiBildFahrplan has not been closed within 120 seconds.");
          }

          processes = Process.GetProcessesByName(_ZusiProcessName);
          if (processes == null || processes.Length == 0)
          {
            break;
          }
        }
      }

      processes = Process.GetProcessesByName(_ZusiProcessName);

      if (processes == null || processes.Length == 0)
      {
        if (string.IsNullOrEmpty(parameter))
        {
          _runprog.Start(parameter);
        }
        else
        {
          _runprog.Start(parameter);
        }

        while (true)
        {
          Thread.Sleep(1000);
          if (n++ >= 120)
          {
            //throw new InvalidOperationException("ZusiBildFahrplan has not been started within 120 seconds.");
          }

          processes = Process.GetProcessesByName(_ZusiProcessName);
          if (processes != null && processes.Length > 0)
          {
            break;
          }
        }
        n = 0;
        while (true)
        {
          Thread.Sleep(1000);
          if (n++ >= 10)
          {

            break;
          }
        }
      }

      return processes[0];
    }

    //---------------------------------------------------------------------
    public static void Maximize(bool bringToTop)
    {
      Instance.Maximize_impl(bringToTop);
    }

    //---------------------------------------------------------------------
    public static void Minimize()
    {
      Instance.Minimize_impl();
    }

    //---------------------------------------------------------------------
    public static void Quit()
    {
      Instance.Quit_impl();
    }

    //---------------------------------------------------------------------
    public static void Restore(bool bringToTop)
    {
      Instance.Restore_impl(bringToTop);
    }

    //---------------------------------------------------------------------
    public static void Start(string filename, string parameter)
    {
      Instance.Start_impl(filename, parameter);
    }
  }
}