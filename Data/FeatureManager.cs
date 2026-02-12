using log4net;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ZusiStart.Data
{

  public static class FeatureManager
  {
    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public enum Features
    {
      Tracking = 0,
      StartLocation = 1,
      RouteGraph = 2,
    }

    private static readonly List<Features> _features = new List<Features>();

    public static void initFeatures(List<Features> features)
    {
      _features.AddRange(features);
    }

    public static bool feature_enabled(Features feature)
    {
      return _features.Contains(feature);
    }
  }
    
}