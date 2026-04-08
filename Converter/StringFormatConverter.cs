using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using log4net;

namespace ZusiStart.Converter
{
  public static class PluralConverter
  {
    private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    public static string Convert(string s, bool plural)
    {
      _log.Debug($"Convert '{s}' with plural={plural}");
      if (string.IsNullOrEmpty(s))
        return s;

      StringBuilder sb = new StringBuilder();
      Regex regex = new Regex(@"\[.*?\]");
      int startIndex = 0;
      foreach (Match m in regex.Matches(s))
      {
        //System.Diagnostics.Debug.WriteLine(m.Value);

        string[] ss = m.Value.Substring(1, m.Value.Length - 2).Split(':');
        sb.Append(s.Substring(startIndex, m.Index - startIndex));
        sb.Append(plural ? ss[1] : ss[0]);
        startIndex = m.Index + m.Length;
      }

      sb.Append(s.Substring(startIndex));

      //System.Diagnostics.Debug.WriteLine(sb.ToString());
      return sb.ToString();
    }

    public static bool NeedsPlural(object value)
    {
      try
      {
        return System.Convert.ToInt32(value) != 1;
      }
      catch
      {
        return false;
      }
    }
  }

  public class StringFormatConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string format = parameter as string;
      format = PluralConverter.Convert(format, PluralConverter.NeedsPlural(value));
      return string.Format(format, value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
