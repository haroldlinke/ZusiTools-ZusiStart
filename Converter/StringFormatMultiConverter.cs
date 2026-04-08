using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using log4net;
namespace ZusiStart.Converter
{
  public class StringFormatMultiConverter : IMultiValueConverter
  {
    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      _log.Debug($"StringFormatMultiConverter: values={string.Join(", ", values)}, parameter={parameter}");
      StringBuilder sb = new StringBuilder();
      string[] formats = (parameter as string).Split('|');
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
