using System;
using System.Globalization;
using System.Windows.Data;

namespace ZusiStart.Converter
{
  public class HalfValueConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
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

