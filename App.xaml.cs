using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic.Logging;
using Sovoma;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using ZusiKlassenLib2;
using ZusiStart.Data;

namespace ZusiStart
{
  /// <summary>
  /// Interaktionslogik für "App.xaml"
  /// </summary>
  public partial class App : Application
  {
    private static readonly ILog _log = LogManager.GetLogger(typeof(App));

    private IHost _host;

    public App()
    {
      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
      // setup log4net
      //GlobalContext.Properties["LogPath"] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      //log4net.Config.XmlConfigurator.Configure();
      DataManager.CurrentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
      DataManager.CurrentLanguage = DataManager.CurrentLanguage switch
      {
        "de" => "de",
        "en" => "en",
        "fr" => "fr",
        _ => "en",
      };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      if (!Zusi.IsInstalled)
      {
        throw new InvalidOperationException("Dieses Programm kann nicht ausgeführt werden, da die Vollversion des Zusi nicht installiert ist.\nFür Steam-Anwender: Zusi Dateiverwaltung als Administrator öffnen -> Verwaltung -> Generelle Zusi-Einstellungen öffnen und mit OK Abspeichern.");
      }
      string tmpBaseFolder = Zusi.DataPath[2] + @"Temp\";
      GlobalContext.Properties["LogPath"] = tmpBaseFolder;
#if DEBUG
      log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
#else
      log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.release.config"));
#endif
      _log.Info(" ");
      _log.Info(" ");
      _log.Info(" ");
      _log.Info("**************************************************************************");
      _log.Info("*");
      _log.Info("* ZusiStart started - Version:" + AsmInfo.Version.ToString());
      _log.Info("*");
      _log.Info("**************************************************************************");
      _log.Info("Get Dirs: ZusiExecutable: " + Zusi.Executable);
      _log.Info("Get Dirs: ZusiVerzeichnis:" + Zusi.ZusiPath);
      _log.Info("Get Dirs: ZusiDatenVerzeichnisOffiziell: " + Zusi.DataPath[0]);
      _log.Info("Get Dirs: ZusiDatenVerzeichnis:" + Zusi.DataPath[2]);
      _log.Debug("Debug level enabled");
      _log.Warn("Warning level enabled");
      _log.Info("Info level enabled");
      _log.Error("Error level enabled");
      _log.Fatal("Fatal level enabled");

      try
      {
        base.OnStartup(e);
      }
      catch (Exception ex)
      {
        _log.Error(ex.ToString());
        if (ex.InnerException != null)
        {
          _log.Fatal("Inner Exception:");
          _log.Fatal(ex.InnerException.ToString);
          _log.Fatal(ex.InnerException.StackTrace);
        }
        MessageBox.Show(ex.Message, LocalizationManager.Translate("Dieser Fehler lässt sich nicht gerade biegen"), MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    protected override void OnExit(ExitEventArgs e)
    {
      _host?.Dispose();
      base.OnExit(e);
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      _log.Fatal(e.ExceptionObject.ToString());

      if (e.ExceptionObject is Exception ex)
      {
        if (ex.InnerException != null)
        {
          _log.Fatal("Inner Exception:");
          _log.Fatal(ex.InnerException.ToString);
          _log.Fatal(ex.InnerException.StackTrace);
        }

        MessageBox.Show(ex.Message, LocalizationManager.Translate("Dieser Fehler lässt sich nicht gerade biegen"), MessageBoxButton.OK, MessageBoxImage.Error);
      }
      else
      {
        MessageBox.Show(e.ExceptionObject.ToString(), LocalizationManager.Translate("Dieser Fehler lässt sich nicht gerade biegen"), MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

  }
}

