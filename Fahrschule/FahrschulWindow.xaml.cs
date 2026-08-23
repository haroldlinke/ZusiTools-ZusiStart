using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using ZusiCLIProject.Utils;
using log4net;

namespace ZusiCLIProject.Zusi3Fahrschule2
{
  /// <summary>
  /// Interaktionslogik für FahrschulWindow.xaml
  /// </summary>
  public partial class FahrschulWindow : Window
  {
    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public FahrschulWindow()
    {
      InitializeComponent();
      webBrowser.Navigate(new Uri("about:blank"));
      chbAutopause.IsChecked = true;
      string? executablePath = Process.GetCurrentProcess().MainModule?.FileName;

      string baseUri = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(executablePath), "Fahrschule", "webpage");
      //string baseUri = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Zusi3", "ZusiVerzeichnisDemo", "");
      //if (string.IsNullOrEmpty(baseUri))
      //  baseUri = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Zusi3", "ZusiVerzeichnisDemo", "");
      //if (string.IsNullOrEmpty(baseUri))
      //{
      //  MessageBox.Show("Pfad zur Zusi-Demoversion nicht gefunden.");
      //  _log.Debug("Pfad zur Zusi-Demoversion nicht gefunden.");
      //  return;
      //}
      baseUri = "file:///" + System.Web.HttpUtility.UrlPathEncode(baseUri + @"\_InstSetup\language\Deutsch\Demo\").Replace("\\\\", "\\").Replace("\\", "/");
      webBrowser.Navigate(new Uri(baseUri + "start.htm"));
      StopZp9Observer = false;
    }

    ZusiTcpSocket tcpSocket = null;
    public void BtnConnect_Click(object senderBtn, RoutedEventArgs eBtn)
    {
      if (tcpSocket == null)
      {
        //string baseUri = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Zusi3", "ZusiVerzeichnisDemo", "");
        //if (string.IsNullOrEmpty(baseUri))
        //  baseUri = (string)Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Zusi3", "ZusiVerzeichnisDemo", "");
        //if (string.IsNullOrEmpty(baseUri))
        //{
        //  MessageBox.Show("Pfad zur Zusi-Demoversion nicht gefunden.");
        //  _log.Debug("Pfad zur Zusi-Demoversion nicht gefunden.");
        //  return;
        //}
        string? executablePath = Process.GetCurrentProcess().MainModule?.FileName;
        string baseUri = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(executablePath), "Fahrschule", "webpage");
        baseUri = "file:///" + System.Web.HttpUtility.UrlPathEncode(baseUri + @"\_InstSetup\language\Deutsch\Demo\").Replace("\\\\", "\\").Replace("\\", "/");
        webBrowser.Navigate(new Uri(baseUri + "start.htm"));
        btnConnect.Content = "Trennen";
        StopZp9Observer = false;

        tcpSocket = new ZusiTcpSocket();
        var t = new System.Net.Sockets.TcpClient();
        t.Connect(txtIp.Text, int.Parse(txtPort.Text));
        tcpSocket.TcpConnection = t;
        tcpSocket.SyncContext = System.Threading.SynchronizationContext.Current;

        tcpSocket.WritingBeginBuffering();
        tcpSocket.WriteWord(new short[] { 0x0001, 0x0001 }, 0x0001, 2);
        tcpSocket.WriteWord(new short[] { 0x0001, 0x0001 }, 0x0002, 2);
        tcpSocket.WriteString(new short[] { 0x0001, 0x0001 }, 0x0003, System.Reflection.Assembly.GetEntryAssembly().GetName().Name);
        tcpSocket.WriteString(new short[] { 0x0001, 0x0001 }, 0x0004, System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
        tcpSocket.WritingEndBuffering();
        tcpSocket.WritingBeginBuffering();
        tcpSocket.WriteWord(new short[] { 0x0002, 0x0003, 0x00A }, 0x0001, 0x0001); //V
                                                                                    //tcpSocket.WriteWord(new short[] { 0x0002, 0x0003, 0x00A }, 0x0001, 0x0002); //Hll
        tcpSocket.WriteWord(new short[] { 0x0002, 0x0003, 0x00A }, 0x0001, 0x001B); //Schleudern
        tcpSocket.WriteWord(new short[] { 0x0002, 0x0003, 0x00A }, 0x0001, 0x0064); //Sifa
        tcpSocket.WriteWord(new short[] { 0x0002, 0x0003, 0x00A }, 0x0001, 0x0065); //Zugbeeinflussung
                                                                                    //tcpSocket.WriteWord(new short[] { 0x0002, 0x0003, 0x00A }, 0x0001, 0x002A); //Motorlast
        tcpSocket.WriteWord(new short[] { 0x0002, 0x0003, 0x00A }, 0x0001, 0x0066); //Türen
        tcpSocket.WriteWord(new short[] { 0x0002, 0x0003, 0x00A }, 0x0001, 0x0092); //Weichen
                                                                                    //tcpSocket.WriteWord(new short[] { 0x0002, 0x0003, 0x00A }, 0x0001, 0x008E); //Zugverband
                                                                                    //tcpSocket.WriteWord(new short[] { 0x0002, 0x0003, 0x00A }, 0x0001, 0x008D); //Fahrzeug

        tcpSocket.WriteWord(new short[] { 0x0002, 0x0003, 0x00A }, 0x0001, 0x0019); //Zurückgelegter Gesamtweg

        tcpSocket.WritingEndBuffering();

        //tcpSocket.WriteString(new short[] { 0x0002, 0x010B, 0x0003 }, 0x0001, @"Timetables\Deutschland\Demo\Demofahrplan_1986\D2640.trn");
        //tcpSocket.WriteString(new short[] { 0x0002, 0x010B, 0x0003 }, 0x0001, @"Timetables\_Docu\Bremsteststrecke\Testzug1.trn");

        var reachedDrivedMeterFiles = new List<string>();
        bool reachedTarget = false; //9650
        float recentDriven = 0;
        string indusiWsp = "";
        bool sifaLMWsp = false;
        bool sifaHuWsp = false;
        bool sifaZbWsp = false;
        bool abfahrtOhneAuftragWsp = false;
        bool schleudernWsp = false;
        tcpSocket.AttibuteRead += delegate (object sender, AttributeEventArgs e)
        {
          if (e.OpenNodes.SequenceEqual(new short[] { 0x0002, 0x000A }) && (e.Attribute == 0x0019) && (indusiWsp == ""))
          {
            var kvm = new List<KeyValuePair<float, string>>();
            kvm.Add(new KeyValuePair<float, string>(1040, "Driburg.htm"));
            kvm.Add(new KeyValuePair<float, string>(2565, "tunnel.htm"));
            kvm.Add(new KeyValuePair<float, string>(3060, "Reelsen_Ank.htm"));
            kvm.Add(new KeyValuePair<float, string>(4125, "Reelsen.htm"));
            kvm.Add(new KeyValuePair<float, string>(4450, "Langeland_Evsig.htm"));
            kvm.Add(new KeyValuePair<float, string>(5300, "Lf7_90.htm"));
            kvm.Add(new KeyValuePair<float, string>(5425, "Langeland_Esig.htm"));
            kvm.Add(new KeyValuePair<float, string>(6300, "Langeland_Avsigwdh.htm"));
            kvm.Add(new KeyValuePair<float, string>(6540, "Langeland_Asig.htm"));
            kvm.Add(new KeyValuePair<float, string>(7035, "EndeWeichenbereich.htm"));
            kvm.Add(new KeyValuePair<float, string>(7455, "Altenbeken_Evsig.htm"));
            kvm.Add(new KeyValuePair<float, string>(7845, "Altenbeken_Evsigwdh.htm"));
            kvm.Add(new KeyValuePair<float, string>(8515, "Altenbeken_Esig.htm"));
            kvm.Add(new KeyValuePair<float, string>(8700, "Altenbeken_Avsig.htm"));
            kvm.Add(new KeyValuePair<float, string>(9280, "Altenbeken_Halt.htm"));

            float value = e.ToSingle();
            recentDriven = value;
            foreach (KeyValuePair<float, string> k in kvm)
            {
              if (!reachedDrivedMeterFiles.Contains(k.Value) && value > k.Key)
              {
                reachedDrivedMeterFiles.Add(k.Value);
                webBrowser.Navigate(new Uri(baseUri + k.Value));
                if (chbAutopause.IsChecked.Value)
                  tcpSocket.WriteShortInt(new short[] { 0x0002, 0x010B, 0x0001 }, 0x0001, 1); //Pause ein
              }
            }
            if (recentDriven > 230) //227.740082 beim Start
              StopZp9Observer = true;
          }
          if (e.OpenNodes.SequenceEqual(new short[] { 0x0002, 0x000A }) && (e.Attribute == 0x0001))
          {
            float value = e.ToSingle();

            if (recentDriven > 9500 && value == 0 && !reachedTarget)
            {
              reachedTarget = true;
              webBrowser.Navigate(new Uri(baseUri + "Altenbeken_Angehalten.htm"));
              if (chbAutopause.IsChecked.Value)
                tcpSocket.WriteShortInt(new short[] { 0x0002, 0x010B, 0x0001 }, 0x0001, 1); //Pause ein
            }
            if (value > 0 && !Zp9Empfangen && !abfahrtOhneAuftragWsp)
            {
              abfahrtOhneAuftragWsp = true;
              webBrowser.Navigate(new Uri(baseUri + "AbfahrtVorAuftrag.htm"));
              if (chbAutopause.IsChecked.Value)
                tcpSocket.WriteShortInt(new short[] { 0x0002, 0x010B, 0x0001 }, 0x0001, 1); //Pause ein
            }
          }
          if (e.OpenNodes.SequenceEqual(new short[] { 0x0002, 0x000A, 0x0065, 0x0003 }) && (e.Attribute == 0x002D))
          {
            int value = e.ToByte();
            if (value == 1 /*&& !sifaLMWsp*/)
            {
              //sifaLMWsp = true;
              webBrowser.Navigate(new Uri(baseUri + "Indusi500Hz.htm"));
              if (chbAutopause.IsChecked.Value)
                tcpSocket.WriteShortInt(new short[] { 0x0002, 0x010B, 0x0001 }, 0x0001, 1); //Pause ein
            }
          }
          if (e.OpenNodes.SequenceEqual(new short[] { 0x0002, 0x000A, 0x0065, 0x0003 }) && (e.Attribute == 0x0003))
          {
            int value = e.ToWord();
            string ret = "";
            switch (value)
            {
              case 1:
                ret = "Indusi1000Hz_ZBwachsam.htm";
                break;
              case 2:
                ret = "Indusi1000Hz_ZBangeh.htm";
                break;
              case 3:
                ret = "Indusi500Hz_ZB.htm";
                break;
              case 4:
                ret = "Indusi2000Hz_ZB.htm";
                break;
            }
            if (ret != indusiWsp)
            {
              indusiWsp = ret;
              if (ret != "")
              {
                webBrowser.Navigate(new Uri(baseUri + ret));
                if (chbAutopause.IsChecked.Value)
                  tcpSocket.WriteShortInt(new short[] { 0x0002, 0x010B, 0x0001 }, 0x0001, 1); //Pause ein
              }
            }
          }
          if (e.OpenNodes.SequenceEqual(new short[] { 0x0002, 0x000A, 0x0064 }) && (e.Attribute == 0x0002))
          {
            int value = e.ToByte();
            if (value == 1 && !sifaLMWsp)
            {
              sifaLMWsp = true;
              webBrowser.Navigate(new Uri(baseUri + "Sifa_LM.htm"));
              if (chbAutopause.IsChecked.Value)
                tcpSocket.WriteShortInt(new short[] { 0x0002, 0x010B, 0x0001 }, 0x0001, 1); //Pause ein
            }
          }
          if (e.OpenNodes.SequenceEqual(new short[] { 0x0002, 0x000A, 0x0064 }) && (e.Attribute == 0x0003))
          {
            int value = e.ToByte();
            if (value == 1 && !sifaHuWsp)
            {
              sifaHuWsp = true;
              webBrowser.Navigate(new Uri(baseUri + "Sifa_Hupe.htm"));
              if (chbAutopause.IsChecked.Value)
                tcpSocket.WriteShortInt(new short[] { 0x0002, 0x010B, 0x0001 }, 0x0001, 1); //Pause ein
            }
            if (value == 2 && !sifaZbWsp)
            {
              sifaZbWsp = true;
              webBrowser.Navigate(new Uri(baseUri + "Sifa_ZB.htm"));
              if (chbAutopause.IsChecked.Value)
                tcpSocket.WriteShortInt(new short[] { 0x0002, 0x010B, 0x0001 }, 0x0001, 1); //Pause ein
            }
            else
              sifaZbWsp = false;
          }
          if (e.OpenNodes.SequenceEqual(new short[] { 0x0002, 0x000A }) && (e.Attribute == 0x001B))
          {
            float value = e.ToSingle();
            if (value >= 1 && !schleudernWsp)
            {
              schleudernWsp = true;
              webBrowser.Navigate(new Uri(baseUri + "Schleudern.htm"));
              if (chbAutopause.IsChecked.Value)
                tcpSocket.WriteShortInt(new short[] { 0x0002, 0x010B, 0x0001 }, 0x0001, 1); //Pause ein
            }
          }
        };
        Zp9EmpfangenEvent = delegate ()
        {
          webBrowser.Navigate(new Uri(baseUri + "Abfahrauftrag.htm"));
          if (chbAutopause.IsChecked.Value)
            tcpSocket.WriteShortInt(new short[] { 0x0002, 0x010B, 0x0001 }, 0x0001, 1); //Pause ein
        };
        tcpSocket.StartRead();
        PrepareZp9Observer();
      }
      else
      {
        //tcpSocket.EndRead();
        tcpSocket.TcpConnection.Close();
        tcpSocket.TcpConnection = null;
        tcpSocket = null;
        btnConnect.Content = "Verbinden";
        webBrowser.Navigate(new Uri("about:blank"));
      }

    }

    private void WebBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
    {
      string temp = e.Uri.ToString();
      if (temp.EndsWith(".htm"))
        return;
      if (tcpSocket == null)
        return;
      e.Cancel = true;
      if (temp.EndsWith("/zurueck"))
      {
        if (webBrowser.CanGoBack)
          webBrowser.GoBack();
        return;
      }
      if (temp.EndsWith("/vor"))
      {
        if (webBrowser.CanGoForward)
          webBrowser.GoForward();
        return;
      }
      if (temp.EndsWith("/pause"))
      {
        tcpSocket.WriteShortInt(new short[] { 0x0002, 0x010B, 0x0001 }, 0x0001, -1); //Pause umscahlten
        return;
      }
      if (temp.EndsWith("/TIMEWARP"))
      {
        tcpSocket.WriteShortInt(new short[] { 0x0002, 0x010B, 0x0007 }, 0x0001, -1); //Zeitsprung umscahlten
        return;
      }
      if (temp.EndsWith("/TIMELAPSE"))
      {
        tcpSocket.WriteShortInt(new short[] { 0x0002, 0x010B, 0x0008 }, 0x0001, -1); //Zeitraffer umschalten
        return;
      }
      if (temp.EndsWith("/autopilot"))
      {
        BackupSendViaWindowsMessages(System.Windows.Forms.Keys.F3);
        return;
      }
      if (temp.EndsWith("/ftd"))
      {
        BackupSendViaWindowsMessages(System.Windows.Forms.Keys.F5);
        return;
      }
      temp = temp;
    }
    private static void BackupSendViaWindowsMessages(System.Windows.Forms.Keys key)
    {
      var proc = System.Diagnostics.Process.GetProcessesByName("ZusiSim").Concat(System.Diagnostics.Process.GetProcessesByName("ZusiSim.64"));
      foreach (var p in proc)
      {
        var zusiWindows = ZusiCLIProject.Utils.WindowsMessageDataSending.GetDesktopWindowHandlesByProcess(p).Where(w => w.Name == "Zusi");
        foreach (var w in zusiWindows)
        {
          w.SendKeyState(key, false);
          w.SendKeyState(key, true);
        }
      }
    }
    private bool StopZp9Observer = false;
    private SynchronizationContext Sync;
    private bool Zp9Empfangen = false;
    private Action Zp9EmpfangenEvent;
    private void PrepareZp9Observer()
    {
      var proc = System.Diagnostics.Process.GetProcessesByName("ZusiSim").Concat(System.Diagnostics.Process.GetProcessesByName("ZusiSim.64"));
      Sync = SynchronizationContext.Current;
      foreach (var p in proc)
      {
        var zusiWindows = ZusiCLIProject.Utils.WindowsMessageDataSending.GetDesktopWindowHandlesByProcess(p).Where(w => w.Name == "Zusi");
        foreach (var w in zusiWindows)
        {
          var zp9Observer = new Zp9Observer();
          zp9Observer.Handle = w;
          var thr = new System.Threading.Thread(delegate ()
          {
            while (true)
            {
              if (Zp9Empfangen)
                break;
              if (StopZp9Observer)
                break;
              if (tcpSocket == null)
                break;
              System.Threading.Thread.Sleep(1000);
              System.Drawing.Rectangle mySize = new();
              Sync.Send(delegate (object o) { mySize = new System.Drawing.Rectangle((int)this.Left, (int)this.Top, (int)this.Width, (int)this.Height); }, null);
              if (!zp9Observer.Check(mySize))
                continue;
              Zp9Empfangen = true;
              Sync.Post(delegate (object o) { Zp9EmpfangenEvent(); }, null);
              break;
            }
          });
          thr.Start();
        }
      }
    }
    //Weitere ToDos:
    //ToDo: Abfahrauftrag.htm (Provisorisch)
    //ToDo: AbfahrtVorAuftrag.htm (Provisorisch)
  }
}
