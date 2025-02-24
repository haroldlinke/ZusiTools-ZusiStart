using log4net;
using System;
using System.Windows;
using ZusiKlassenLib;

namespace ZusiStart
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(App));

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#if WIN32
#if false
            AppDomain.CurrentDomain.AssemblyResolve += Resolver;
            InitializeCefSharp();
#else
            CefSettings settings = new CefSettings();
            settings.BrowserSubprocessPath = @"x86\BrowserSubprocess.exe";
            Cef.Initialize(settings, false, null);
#endif
#endif
            // setup log4net
            //GlobalContext.Properties["LogPath"] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //log4net.Config.XmlConfigurator.Configure();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!Zusi.IsInstalled)
            {
                throw new InvalidOperationException("Dieses Programm kann nicht ausgeführt werden, da die Vollversion des Zusi nicht installiert ist.");
            }

            base.OnStartup(e);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _log.Fatal(e.ExceptionObject.ToString());

            if (e.ExceptionObject is Exception ex)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(ex.Message, "Dieser Fehler lässt sich nicht gerade biegen", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(e.ExceptionObject.ToString(), "Dieser Fehler lässt sich nicht gerade biegen", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

#if WIN32
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializeCefSharp()
        {
            var settings = new CefSettings
            {
                // Set BrowserSubProcessPath based on app bitness at runtime
                BrowserSubprocessPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                                     Environment.Is64BitProcess ? "x64" : "x86",
                                                     "CefSharp.BrowserSubprocess.exe")
            };

            // Make sure you set performDependencyCheck false
            Cef.Initialize(settings, performDependencyCheck: false, browserProcessHandler: null);
        }

        // Will attempt to load missing assembly from either x86 or x64 subdir
        // Required by CefSharp to load the unmanaged dependencies when running using AnyCPU
        private static Assembly Resolver(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("CefSharp"))
            {
                string assemblyName = args.Name.Split(new[] { ',' }, 2)[0] + ".dll";
                string archSpecificPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                                                       Environment.Is64BitProcess ? "x64" : "x86",
                                                       assemblyName);

                return File.Exists(archSpecificPath)
                           ? Assembly.LoadFile(archSpecificPath)
                           : null;
            }

            return null;
        }
#endif
    }
}

