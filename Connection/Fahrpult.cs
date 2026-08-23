using log4net;
using Sovoma;
using Sovoma.WPF.Converter;
//using SovomaLib.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;

//using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using ZusiFahrpultLib;
using ZusiKlassenLib2;
using ZusiKlassenLib2.Common;
using ZusiStart.Data;
using static ZusiCLIProject.FileLibrary.Zusi3.Strecke;
using static ZusiKlassenLib2.Zusi;

namespace ZusiStart.Connection
{
  public enum StartTrainResult
  {
    Success,
    InvalidTrainFile,
    Disconnected,
    Pending
  }

  public struct TrainStartInfo
  {
    public string TimetableFile { get; set; }
    public string TrainNumber { get; set; }

    public override string ToString()
    {
      return $"{{{TimetableFile}, {TrainNumber}}}";
    }
  }

  public class Fahrpult : INotifyPropertyChanged, IDisposable
  {
    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private FahrpultClient _fahrpult;
    private TrainStartInfo? _pendingTrain;
    private readonly object _lock = new();
    private bool _pendingStart;
    private int _status = 0;

    //---------------------------------------------------------------------
    public bool IsReady => _status == 3;

    //---------------------------------------------------------------------
    public int Status
    {
      get => _status;
      private set
      {
        if (_status != value)
        {
          _status = value;
          OnPropertyChanged("Status");
          OnPropertyChanged("IsReady");
        }
      }
    }

    //---------------------------------------------------------------------
    public event PropertyChangedEventHandler PropertyChanged;
    //public event EventHandler Connected;
    //public event EventHandler Disonnected;

    //---------------------------------------------------------------------
    public Fahrpult()
    {
      ZusiSim.Started += ZusiSim_Started;
      ZusiSim.Terminated += ZusiSim_Terminated;
    }

    //---------------------------------------------------------------------
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public void SendKeyboardCommand(KeyboardCommand cmd)
    {
      _fahrpult.SendKeyboardCommand(cmd);
    }

    //---------------------------------------------------------------------
    public void SendCombiSwitchCommand(CombiSwitchCommand cmd)
    {
      _fahrpult.SendCombiSwitchCommand(cmd);
    }

    //---------------------------------------------------------------------
    public void SendControlCommand(ControlCommand cmd, params object[] attributes)
    {
      _fahrpult.SendControlCommand(cmd, attributes);
    }


    public void SetNeededData(IEnumerable<ZFtdID> ids)
    {
      _fahrpult.SetNeededData(ids);
    }

    //---------------------------------------------------------------------
    public void SetNeededData(bool needed)
    {
      _fahrpult.SetNeededData(needed);
    }

    //---------------------------------------------------------------------
    public void SetNeededData(IEnumerable<ZProgID> ids)
    {
      _fahrpult.SetNeededData(ids);
    }

    //---------------------------------------------------------------------
    public void SetNeededData(IEnumerable<ZFtdID> ftdIds, IEnumerable<ZProgID> progIds, bool operatingNeeded)
    {
      _fahrpult.SetNeededData(ftdIds, progIds, operatingNeeded);
    }




    //---------------------------------------------------------------------
    public StartTrainResult TryStartTrain(TrainStartInfo startInfo, bool zusiremote = false, bool onlymonitor=false)
    {
      _log.Debug($"TryStartTrain: {startInfo}");

      if (zusiremote)
      {
        _pendingTrain = startInfo;
        _pendingStart = true;
        StartFahrpultAsync();

        if (onlymonitor)
        {
          _pendingTrain = null;
          return StartTrainResult.Success;
        }

        StartTrain(startInfo);
        return StartTrainResult.Success;
      }
      else
      {
        if (!ZusiSim.IsStarted)
        {
          lock (_lock)
          {
            _pendingTrain = startInfo;
          }
          _pendingStart = true;
          ZusiSim.Start(null);
          return StartTrainResult.Pending;
        }

        if (_status != 3)
        {
          if (!_pendingStart)
          {
            _pendingStart = true;
            StartFahrpultAsync();
          }
          return StartTrainResult.Pending;
        }

        StartTrain(startInfo);
        return StartTrainResult.Success;
      }
    }

    //---------------------------------------------------------------------
    private void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_fahrpult != null)
        {
          Disposable.Dispose(ref _fahrpult);
        }
      }
    }

    //---------------------------------------------------------------------
    private async void StartFahrpultAsync()
    {
      System.Diagnostics.Debug.Assert(_fahrpult == null, "Das Fahrpult läuft noch!");

      _fahrpult = new(ParsingMode.Internal, AsmInfo.Product, AsmInfo.Version.ToString("%M.%m.%b"));
      _fahrpult.ClientConnected += Fahrpult_ClientConnected;
      _fahrpult.ClientConnected += DataManager.Instance.main_window.Fahrpult_Connected; ;
      _fahrpult.Disconnected += Fahrpult_Disconnected;
      _fahrpult.Disconnected += DataManager.Instance.main_window.Fahrpult_Disconnected;
      _fahrpult.FtdDataReceived += DataManager.Instance.main_window.Fahrpult_FtdDataReceived;
      _fahrpult.ProgDataReceived += DataManager.Instance.main_window.Fahrpult_ProgDataReceived;
      if (FeatureManager.feature_enabled(FeatureManager.Features.Tracking))
      {
        _fahrpult.SetNeededData([ZFtdID.UTM_RefX, ZFtdID.UTM_RefY, ZFtdID.UTM_Zone, ZFtdID.UTM_Zone2, ZFtdID.xKoordinate, ZFtdID.yKoordinate, ZFtdID.Drehwinkel_z_Achse]);
        _fahrpult.SetNeededData([ZProgID.Zugdatei]);
      }
#if DEBUG
      _fahrpult.LogSocketExceptions = true;
#endif
      await Task.Run(new Action(() =>
      {
        if (_fahrpult != null)
        {
          try
          {
            if (DataManager.Instance.options.RemoteZusi)
            {
              string remoteZusiIP = DataManager.Instance.options.RemoteZusiIP;

              _fahrpult.Open(remoteZusiIP);

            }
            else
            {
              _fahrpult.Open();
            }
          }
          catch (Exception ex)
          {
            _log.Error(ex);
          }
        }
      }));
    }

    //---------------------------------------------------------------------
    private void StartTrain(TrainStartInfo startInfo)
    {
      DataPathType dpt = DataPathType.Unknown;
      string relativeFileName = GetRelativePathOf(startInfo.TimetableFile, ref dpt);
      if (relativeFileName.Length == startInfo.TimetableFile.Length)
      {
        _log.Debug($"timetable file '{startInfo.TimetableFile}' is not relative to any zusi data directory");
        return;
      }

      _log.Debug($"StartTrain: {relativeFileName}, {startInfo.TrainNumber}");

      _fahrpult.SendControlCommand(ControlCommand.StartTrain, relativeFileName, startInfo.TrainNumber);
      if (ZusiSettings.IsMaximized)
      {
        ZusiSim.Maximize(true);
      }
      else
      {
        ZusiSim.Restore(true);
      }
    }

    //---------------------------------------------------------------------
    private void Fahrpult_Disconnected(object sender, EventArgs e)
    {
      _log.Debug("throttle disconnected");

      if (_fahrpult != null)
      {
        try
        {
          Disposable.Dispose(ref _fahrpult);
          _fahrpult = null;
        }
        catch (Exception ex)
        {
          _log.Error(ex);
        }

      }

      Status = 0;
    }

    //---------------------------------------------------------------------
    private void Fahrpult_ClientConnected(object sender, ClientConnectedEventArgs e)
    {
      _log.Debug($"throttle connected ({e.ClientAccepted}, {e.NeededDataAccepted})");

      if (e.ClientAccepted)
      {
        Status |= 1;
      }
      if (e.NeededDataAccepted)
      {
        Status |= 2;
      }

      lock (_lock)
      {
        _pendingStart = false;
        if (_pendingTrain != null)
        {
          StartTrain(_pendingTrain.Value);
          _pendingTrain = null;
        }
      }
    }



    private void Fahrpult_FtdDataReceived(object sender, FtdDataReceivedEventArgs e)
    {
      //_dataQueue.Enqueue((EventArgs)e);
      //_dataWakeUp.Set();

      //System.Windows.Point point = ToLatLon(e);
      //DataManager.Instance.main_window.UpdateMarkerPosition(point.Y, point.X);
    }

    private void Fahrpult_ProgDataReceived(object sender, ProgDataReceivedEventArgs e)
    {
      //this._dataQueue.Enqueue((EventArgs)e);
      //this._dataWakeUp.Set();
    }

    //---------------------------------------------------------------------
    private void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    //---------------------------------------------------------------------
    private void ZusiSim_Started(object sender, EventArgs e)
    {
      _log.Debug("Zusi started");

      StartFahrpultAsync();
    }

    //---------------------------------------------------------------------
    private void ZusiSim_Terminated(object sender, EventArgs e)
    {
      _log.Debug("Zusi terminated");
    }
  }
}
