using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using ZusiStart.Data;

namespace ZusiStart.Converter
{
  public class LaNrVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return (value?.ToString() == DataManager.Instance.tab_title_La_Hdb || value?.ToString() == DataManager.Instance.tab_title_Ersatzfahrplan || value?.ToString() == DataManager.Instance.tab_title_StreBu) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

}
