using Microsoft.AspNetCore.Mvc;
using ZusiKlassenLib;
using ZusiKlassenLib.Fahrplan;
using ZusiStart.Data;
using System.Windows;
using System.Windows.Input;
using ZusiKlassenLib.Cab;


[ApiController]
[Route("api/commands")]
public class CommandsController : ControllerBase
{
  [HttpGet("show")]
  public IActionResult ShowMessage([FromQuery] string train)
  {
    try
    {
      train = Zusi.DataPath[0] + "Timetables\\" + train;
      ZugDatei zd = new ZugDatei(null, train);
      if (zd.Root == null)
      {
        zd.Parse();
      }

      Application.Current.Dispatcher.Invoke(() =>
      {
        DataManager.Instance.CurrentTrain = zd.Root;
        DataManager.Instance.CurrentTrainItem = null;
      });

      if (zd.Root == null)
        return Ok($"Message: {train} not found");
      else
        return Ok($"Message: {train}");

    }
    catch (Exception ex)
    {
      return Ok($"Message: {ex.Message}");
    }
  }

  [HttpGet("start")]
  public IActionResult StartTrain([FromQuery] string train)
  {
    try
    {
      train = Zusi.DataPath[0] + "Timetables\\" + train;
      ZugDatei zd = new ZugDatei(null, train);
      if (zd.Root == null)
      {
        zd.Parse();
      }

      Application.Current.Dispatcher.Invoke(() =>
      {
        DataManager.Instance.CurrentTrain = zd.Root;
      });

      if (zd.Root == null)
        return Ok($"Message: {train} not found");
      else
      {
        //starte aktuellen Zug:
        object _sender = null;
        ExecutedRoutedEventArgs _e = null;
        Application.Current.Dispatcher.Invoke(() =>
        {
          DataManager.Instance.main_window.OnStartTrain(_sender, _e);
        });
        return Ok($"Message: {train}");
      }

    }
    catch (Exception ex)
    {
      return Ok($"Message: {ex.Message}");
    }


  }
}
