using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ZusiStart.Data;

namespace ZusiStart.Converter
{
  public class StringFormatMultiConverterLng : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      StringBuilder sb = new StringBuilder();

      string parameter_str = parameter as string;

      parameter_str = LocalizationManager.Translate(parameter_str);

      string[] formats = (parameter_str).Split('|');
      for (int i = 0; i < formats.Length; i++)
      {
        string format = PluralConverter.Convert(formats[i], PluralConverter.NeedsPlural(values[i]));
        sb.AppendFormat(format, values[i]);
      }
      return sb.ToString().Replace('_', ' ');
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
