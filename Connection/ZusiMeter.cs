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
  class ZusiMeter
  {
    private const string _ZusiProcessName = "ZusiMeter";

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
    private static readonly Lazy<ZusiMeter> _instance = new(() => new ZusiMeter());

    private readonly ProgMeter _runprog;
    private bool _pendingStart;
    private readonly ThreadSafeVar<Process> _zusi = new();

    //---------------------------------------------------------------------
    public static ZusiMeter Instance => _instance.Value;
    public static bool CanStart => Instance._runprog.CanStart;
    public static bool IsStarted => Instance._zusi.Value != null;

    //---------------------------------------------------------------------
    public static event EventHandler Started;
    public static event EventHandler Terminated;

    //---------------------------------------------------------------------
    private ZusiMeter()
    {
      string ZusiMeterfilename = DataManager.Instance.options.ZusiMeter_Exe;
      ZusiMeterfilename = ZusiMeterfilename.QuoteIf(ZusiMeterfilename.Contains(" "));
      _runprog = new ProgMeter(ZusiMeterfilename, DisplayState.Normal);
      _runprog.Finished += Prog_Finished;
    }

    //---------------------------------------------------------------------
    private void Prog_Finished(object sender, BackgroundProcessFinishedEventArgs e)
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
          _log.Debug("ZusiMeter has been terminated");
          Terminated?.Invoke(this, EventArgs.Empty);
        }
      }
      catch {
        _log.Error("ZusiMeter Error terminating");
      };
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
      StartProgAsync(filename, parameter);
      _log.Debug("starting ZusiMeter has been requested");
    }

    //---------------------------------------------------------------------
    private async void StartProgAsync(string filename, string parameter)
    {
      if (!_pendingStart)
      {
        _pendingStart = true;
        _zusi.Value = await Task.Run(() => RunProg(filename, parameter));
        _pendingStart = false;
        _log.Debug("ZusiMeter has been started");
        Started?.Invoke(this, EventArgs.Empty);

        await Task.Run(() => _runprog.WaitFor());
      }
    }

    //---------------------------------------------------------------------
    private Process RunProg(string filename, string parameter)
    {
      string ZusiMeterfilename = filename.QuoteIf(filename.Contains(" "));
      _runprog.Executable = ZusiMeterfilename;
      if (string.IsNullOrEmpty(parameter))
      {
        _runprog.Start(parameter);
      }
      else
      {
        parameter = parameter.QuoteIf(parameter.Contains(" "));
        _runprog.Start(parameter);
      }

      return null;
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