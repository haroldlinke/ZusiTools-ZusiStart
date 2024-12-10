using Microsoft.Win32;
using Sovoma.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ZusiStart
{
    class Startup
    {
        private static readonly string _appGuid = "5DED5276-60FF-419F-B64D-864637E44C6C";


        static public void ZUSI_write_ext_menuval_to_Regkey(string keyVal, int EntryIdx = 0, string BezeichnerSprache = "Deutsch", string Bezeichnertext = "", string Vatermenu = "", int MenuIndex = 5, string Datei = "", string Parameter = "")
        {
            RegistryKey key;

            try
            {
                key = Registry.CurrentUser.OpenSubKey(keyVal, true);
                if (key != null)
                {
                    Debug.WriteLine($"create_ZUSI_menu_entry key {keyVal} found");
                }
                else
                    Debug.WriteLine($"create_ZUSI_menu_entry key {keyVal} NOT found");
                try
                {
                    key = Registry.CurrentUser.CreateSubKey(keyVal);
                    Debug.WriteLine($"create_ZUSI_menu_entry key {keyVal} created");
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error in create_ZUSI_menu_entry {e}");
                    return;
                }
            }
            catch
            {
                Debug.WriteLine($"create_ZUSI_menu_entry key {keyVal} NOT found");
                try
                {
                    key = Registry.CurrentUser.CreateSubKey(keyVal);
                    Debug.WriteLine($"create_ZUSI_menu_entry key {keyVal} created");
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error in create_ZUSI_menu_entry {e}");
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
                key.SetValue("Parameter", Parameter, RegistryValueKind.String);
                Debug.WriteLine($"create_ZUSI_menu_entry added key data for Fahrplanerstellung {keyVal}");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error in create_ZUSI_menu_entry_2 {e}");
            }
            finally
            {
                key?.Close();
            }
        }

        static public void CreateZUSIMenuEntry(string execFilePathname)
        {
 
            // Check for ZUSI version
            bool noZusiEntry = false;
            bool zusiSteam = false;
            bool checkZusiSteam = false;
            string keyVal = @"Software\Zusi3\Fahrsim\Einstellungen";

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyVal, true))
                {
                    Debug.WriteLine($"create_ZUSI_menu_entry key {keyVal} found");
                    checkZusiSteam = false;
                    zusiSteam = false;
                }
            }
            catch
            {
                checkZusiSteam = true;
            }

            if (checkZusiSteam)
            {
                try
                {
                    keyVal = @"Software\Zusi3\Fahrsimsteam\Einstellungen";
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyVal, true))
                    {
                        Debug.WriteLine($"create_ZUSI_menu_entry key {keyVal} found");
                        zusiSteam = true;
                    }
                }
                catch
                {
                    zusiSteam = false;
                    noZusiEntry = true;
                }
            }

            if (noZusiEntry)
            {
                Debug.WriteLine("create_ZUSI_menu_entry no ZUSI entry found");
                return;
            }

            bool zusiKeyOk = true;
            string keyVal0;

            if (zusiSteam)
            {
                keyVal0 = @"Software\Zusi3\Fahrsimsteam\Einstellungen\MenuZusiStart";
            }
            else
            {
                keyVal0 = @"Software\Zusi3\Fahrsim\Einstellungen\MenuZusiStart";
            }

            if (zusiKeyOk)
            {
                ZUSI_write_ext_menuval_to_Regkey(keyVal0, 0, "Deutsch", "&ZusiStart", "SpTBXSubmenuItemSimulation", 5, execFilePathname, "-fpn \"_@@fpn@@\" -trn \"_@@trn@@\" -zn \"_@@#@@\" ");
            }
        }

 

    //---------------------------------------------------------------------
    [STAThread]
        static void Main()
        {
            using SingleInstanceApplicationLock appLock = new(_appGuid);
            if (!appLock.TryAcquireExclusiveLock())
            {
                MessageBox.Show("ZusiStart wird bereits ausgeführt.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            string executablePath = Process.GetCurrentProcess().MainModule.FileName;

            CreateZUSIMenuEntry(executablePath);

            App app = new();
            app.InitializeComponent();
            _ = app.Run();
        }
    }
}
