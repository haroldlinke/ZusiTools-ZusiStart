using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ZusiStart.Converter
{
    public class GridSizeConverter : IMultiValueConverter
    {
        //private static GridLength _zero = new GridLength(0);

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double w = 0;
            if (values.Length == 2 && values[0] is int v1 && values[1] is int v2 && parameter is string s && int.TryParse(s, out int p))
            {
                if (p == 0)
                {
                    w = v2 > 0 ? 0.5 : double.NaN;
                }
                else if (p == 1)
                {
                    w = v1 > 0 ? 0.5 : double.NaN;
                }
            }
            return double.IsNaN(w) ? new GridLength(0, GridUnitType.Auto) : new GridLength(w, GridUnitType.Star);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
