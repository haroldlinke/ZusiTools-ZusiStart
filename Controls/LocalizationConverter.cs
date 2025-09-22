using System;
using System.Globalization;
using System.Windows.Data;
using ZusiStart.Data;

namespace ZusiStart.Converter
{

  public class LocalizationConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return LocalizationManager.Translate(value?.ToString() ?? "");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}