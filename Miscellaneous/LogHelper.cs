using log4net;

namespace ZusiStart.Miscellaneous
{
  public static class LogHelper
  {
    private static readonly ILog log = LogManager.GetLogger(typeof(LogHelper));

    public static void LogException(Exception ex, string message = null)
    {
      var capturedStack = new System.Diagnostics.StackTrace(true);

      log.Error(
          $"{message}\nCaptured stack:\n{capturedStack}",
          ex
      );
    }
  }

}
