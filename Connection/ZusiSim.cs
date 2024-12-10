using IpcCommLib;
using log4net;
using Sovoma;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ZusiStart.Miscellaneous;

namespace ZusiStart.Connection
{
    class ZusiSim
    {
        private const string _ZusiProcessName = "ZusiSim.64";

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
        private static readonly Lazy<ZusiSim> _instance = new(() => new ZusiSim());

        private readonly Simulator _sim = new(SimulatorState.Minimized);
        private bool _pendingStart;
        private readonly ThreadSafeVar<Process> _zusi = new();

        //---------------------------------------------------------------------
        public static ZusiSim Instance => _instance.Value;
        public static bool CanStart => Instance._sim.CanStart;
        public static bool IsStarted => Instance._zusi.Value != null;

        //---------------------------------------------------------------------
        public static event EventHandler Started;
        public static event EventHandler Terminated;

        //---------------------------------------------------------------------
        private ZusiSim()
        {
            _sim.Finished += Sim_Finished;
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
            if (_zusi.Value != null)
            {
                if (_zusi.Value.CloseMainWindow())
                {
                    _zusi.Value.Close();
                }
                else
                {
                    _zusi.Value.Kill();
                }

                _log.Debug("Zusi has been terminated");
                Terminated?.Invoke(this, EventArgs.Empty);
            }
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
        private void Start_impl(string trainFile)
        {
            StartZusiAsync(trainFile);
            _log.Debug("starting Zusi has been requested");
        }

        //---------------------------------------------------------------------
        private async void StartZusiAsync(string trainFile)
        {
            if (!_pendingStart)
            {
                _pendingStart = true;
                _zusi.Value = await Task.Run(() => RunZusi(trainFile));
                _pendingStart = false;
                _log.Debug("Zusi has been started");
                Started?.Invoke(this, EventArgs.Empty);

                await Task.Run(() => _sim.WaitFor());
            }
        }

        //---------------------------------------------------------------------
        private Process RunZusi(string trainFile)
        {
            int n = 0;
            Process[] processes = Process.GetProcessesByName(_ZusiProcessName);
            if (processes == null || processes.Length == 0)
            {
                if (string.IsNullOrEmpty(trainFile))
                {
                    _sim.Start();
                }
                else
                {
                    _sim.Start(trainFile);
                }

                while (true)
                {
                    Thread.Sleep(1000);
                    if (n++ >= 120)
                    {
                        throw new InvalidOperationException("Zusi has not been started within 120 seconds.");
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
        public static void Start(string trainFile)
        {
            Instance.Start_impl(trainFile);
        }
    }
}