using IWshRuntimeLibrary; // Make sure to add the reference
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.Logging;
using Microsoft.Win32;
using Sovoma;
using Sovoma.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ZusiKlassenLib2;
using ZusiKlassenLib2.Fahrplan;
using ZusiStart.Data;
using Zusisuplib;
using System;
using System.Threading;
using Makaretu;


namespace ZusiStart
{



  public class ShortcutCreator
  {
    public static void CreateShortcutOnDesktop(string shortcutName, string targetPath, string iconLocation)
    {
      // add icon to desktop
      string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
      string shortcutLocation = Path.Combine(desktopPath, shortcutName + ".lnk");

      WshShell shell = new WshShell();
      IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

      shortcut.Description = shortcutName;
      shortcut.TargetPath = targetPath; // Path to the executable
      shortcut.IconLocation = iconLocation; // Path to the icon file
      shortcut.Save();
    }

    public static void CreateShortcutInQuickLaunch(string shortcutName, string targetPath, string iconLocation)
    {
      string quickLaunchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Internet Explorer\Quick Launch");
      string shortcutLocation = Path.Combine(quickLaunchPath, shortcutName + ".lnk");

      WshShell shell = new WshShell();
      IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

      shortcut.Description = "ZusiStart";
      shortcut.TargetPath = targetPath; // Path to the executable
      shortcut.IconLocation = iconLocation; // Path to the icon file
      shortcut.Save();
    }
  }




  class Startup
  {
    private static readonly string _appGuid = "5DED5276-60FF-419F-B64D-864637E44C6C";

    private static readonly ILog _log = LogManager.GetLogger(typeof(App));

    public static string[] commandlineargs;

    private static void create_Registry_entry_HKCU()
    {
      // Installation only: add ZusiMeter to ZUSI Menu
      Zusiaccess.CreateZUSIMenuEntry(Bezeichnertext: "&ZusiStart", Vatermenu: "SpTBXSubmenuItemSimulation", MenuIndex: 5);
    }

    private static void create_Registry_entry_HKUS()
    {
      // Installation only: add ZusiMeter to ZUSI Menu
      // add menu entry for all users - needs admin rights
      foreach (var userSid in Registry.Users.GetSubKeyNames())
      {
        CreateZUSIMenuEntryHKUsers(userid: userSid, Bezeichnertext: "&ZusiStart", Vatermenu: "SpTBXSubmenuItemSimulation", MenuIndex: 5);
      }
    }


    // Registry HKEY_Users for ´defined User
    static public void ZUSI_write_ext_menuval_to_Regkey_HKUS(string keyVal, int EntryIdx = 0, string BezeichnerSprache = "Deutsch", string Bezeichnertext = "", string Vatermenu = "", int MenuIndex = 5, string Datei = "", string? Parameter = "")
    {
      RegistryKey? key;

      try
      {
        key = Registry.Users.OpenSubKey(keyVal, true);
        if (key != null)
        {
          _log.Debug($"create_ZUSI_menu_entry key {keyVal} found");
        }
        else
          _log.Error($"create_ZUSI_menu_entry key {keyVal} NOT found");
        try
        {
          key = Registry.Users.CreateSubKey(keyVal);
          _log.Debug($"create_ZUSI_menu_entry key {keyVal} created");
        }
        catch (Exception e)
        {
          _log.Error($"Error in create_ZUSI_menu_entry {e}");
          return;
        }
      }
      catch
      {
        _log.Debug($"create_ZUSI_menu_entry key {keyVal} NOT found");
        try
        {
          key = Registry.Users.CreateSubKey(keyVal);
          _log.Debug($"create_ZUSI_menu_entry key {keyVal} created");
        }
        catch (Exception e)
        {
          _log.Error($"Error in create_ZUSI_menu_entry {e}");
          return;
        }
      }

      try
      {
        key.SetValue("BezeichnerSprache" + EntryIdx.ToString(), BezeichnerSprache, RegistryValueKind.String);
        key.SetValue("BezeichnerText" + EntryIdx.ToString(), Bezeichnertext, RegistryValueKind.String);
        key.SetValue("Vatermenu", Vatermenu, RegistryValueKind.String);
        key.SetValue("MenuIndex", MenuIndex, RegistryValueKind.DWord);
        key.SetValue("Datei", Datei, RegistryValueKind.String);
        if (!string.IsNullOrEmpty(Parameter))
          key.SetValue("Parameter", Parameter, RegistryValueKind.String);
        _log.Debug($"create_ZUSI_menu_entry added key data for Fahrplanerstellung {keyVal}");
      }
      catch (Exception e)
      {
        _log.Error($"Error in create_ZUSI_menu_entry_2 {e}");
      }
      finally
      {
        key?.Close();
      }
    }

    public static void CreateZUSIMenuEntryHKUsers(string userid = "", string BezeichnerSprache = "Deutsch", string Bezeichnertext = "", string Vatermenu = "", int MenuIndex = 5, string? Params = "")
    {
      bool nozusifound = false;
      bool zusisteamfound = false;
      bool zusi3found = false;
      string keyval = userid + "\\Software\\Zusi3\\Fahrsim\\Einstellungen";
      string text2 = Process.GetCurrentProcess().MainModule?.FileName;
      if (text2 == null)
      {
        return;
      }
      zusi3found = false;
      zusisteamfound = false;
      nozusifound = false;

      try  // check for Zusi3 entry
      {
        using (Registry.Users.OpenSubKey(keyval, writable: true))
        {
          _log.Debug("create_ZUSI_menu_entry key " + keyval + " found");
          zusi3found = true;
          nozusifound = true;
        }
      }
      catch
      {
        zusi3found = false;
        nozusifound = false;
      }

      if (!zusi3found)
      {
        try
        {
          keyval = userid + "\\Software\\Zusi3\\Fahrsimsteam\\Einstellungen";
          using (Registry.Users.OpenSubKey(keyval, writable: true))
          {
            _log.Debug("create_ZUSI_menu_entry key " + keyval + " found");
            zusisteamfound = true;
            nozusifound = false;
          }
        }
        catch
        {
          zusisteamfound = false;
          nozusifound = true;
        }
      }

      if (nozusifound)
      {
        _log.Error("create_ZUSI_menu_entry no ZUSI entry found");
        return;
      }


      string menu_keyVal = ((!zusisteamfound) ? (userid + "\\SOFTWARE\\Zusi3\\Fahrsim\\Einstellungen\\Menu" + Bezeichnertext) : (userid + "\\SOFTWARE\\Zusi3\\Fahrsimsteam\\Einstellungen\\Menu" + Bezeichnertext));

      ZUSI_write_ext_menuval_to_Regkey_HKUS(menu_keyVal, 0, "Deutsch", Bezeichnertext, Vatermenu, MenuIndex, text2, Params);

    }

    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseRouting();
      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }

    //---------------------------------------------------------------------
    [STAThread]
    static void Main(string[] args)
    {
      using SingleInstanceApplicationLock appLock = new(_appGuid);
      if (!appLock.TryAcquireExclusiveLock())
      {
        MessageBox.Show(LocalizationManager.Translate("ZusiStart wird bereits ausgeführt."), LocalizationManager.Translate("Hinweis"), MessageBoxButton.OK, MessageBoxImage.Exclamation);
        return;
      }
      // setup log4net

      string[] commandLineArgs = Environment.GetCommandLineArgs();
      bool testflag = false;

      GlobalContext.Properties["LogPath"] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      log4net.Config.XmlConfigurator.Configure();
      _log.Info(" ");
      _log.Info("**************************************************************************");
      _log.Info("*");
      _log.Info("* ZusiStart started - Version:" + AsmInfo.Version.ToString());
      _log.Info("*");
      _log.Info("*test*************************************************************************");
      _log.Debug("Debug level enabled");
      _log.Warn("Warning level enabled");
      _log.Info("Info level enabled");
      _log.Error("Error level enabled");
      _log.Fatal("Fatal level enabled");

      string? executablePath = Process.GetCurrentProcess().MainModule?.FileName;

      if ((commandLineArgs.Length == 2 && commandLineArgs[1] == "*Installation*") || testflag)
      {
        // Installation only: add ZusiStart to ZUSI Menu
        create_Registry_entry_HKCU();
        create_Registry_entry_HKUS(); // if program runs as administrator menu has to be added to all users

        // determine icon path
        string icon_path = Path.Combine(Path.GetDirectoryName(executablePath), @"Resources\zusistart.ico");

        // Call the method to create the shortcut
        ShortcutCreator.CreateShortcutOnDesktop("ZusiStart", executablePath, icon_path);
        ShortcutCreator.CreateShortcutInQuickLaunch("ZusiStart", executablePath, icon_path);
      }
      else
      {
        if ((commandLineArgs.Length == 2 && commandLineArgs[1] == "*CheckLanguage*") || testflag)
        {
          DataManager.CheckLanguage = true;
        }
        else
        {
          if ((commandLineArgs.Length == 2 && commandLineArgs[1] == "*Test*") || testflag)
          {
            FeatureManager.initFeatures(new List<FeatureManager.Features> { FeatureManager.Features.Tracking, FeatureManager.Features.StartLocation, FeatureManager.Features.RouteGraph });
          }
          else // standard features
          {
            FeatureManager.initFeatures(new List<FeatureManager.Features> { FeatureManager.Features.Tracking, FeatureManager.Features.RouteGraph });
          }
        }

        if (executablePath != null)
        {
          Directory.SetCurrentDirectory(Path.GetDirectoryName(executablePath));
        }

        // Create the mDNS service
        //    using (var mdns = new Makaretu.Dns.ServiceDiscovery())
        //    {
        //      // Define the service to advertise
        //      var service = new Makaretu.Dns.ServiceProfile(
        //          "MyTestService",   // Service instance name
        //          "_myservice._tcp", // Service type
        //          5000               // Port number
        //      );

        //      // Optional: Add TXT records (key-value metadata)
        //      service.AddProperty("version", "1.0");
        //      service.AddProperty("description", "Test Zeroconf service in C#");

        //      // Advertise the service
        //      mdns.Advertise(service);

        //      //Console.WriteLine("Service published. Press Ctrl+C to stop.");
        //      //Console.WriteLine($"Name: {service.InstanceName}");
        //      //Console.WriteLine($"Type: {service.QualifiedServiceName}");
        //      //Console.WriteLine($"Port: {service.Port}");
        //}
        try
        {
        App app = new();
        app.InitializeComponent();
        commandlineargs = commandLineArgs;

        _ = app.Run();
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
    }
  }
}
