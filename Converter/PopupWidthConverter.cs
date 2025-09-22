using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ZusiStart.Converter
{
    public class PopupWidthConverter : IMultiValueConverter
    {
        // values[0]: ActualWidth of parent element
        // values[1]: HorizontalOffset of popup
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Length == 2 &&
                values[0] is double w && !double.IsNaN(w) &&
                values[1] is double o && !double.IsNaN(o)
                ? Math.Max(w - o, 0.0) : 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
  
}
