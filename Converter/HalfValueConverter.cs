using System;
using System.Globalization;
using System.Windows.Data;
using log4net;

namespace ZusiStart.Converter
{
  public class HalfValueConverter : IValueConverter
  {
    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      _log.Debug($"HalfValueConverter: value={value}, parameter={parameter}");
      if (value is double doubleValue)
      {
        double offset = parameter != null ? System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture) : 0;
        return (doubleValue / 2) - offset;
      }
      return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}

