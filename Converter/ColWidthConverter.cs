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
    [ValueConversion(typeof(object), typeof(GridLength))]
    class ColWidthConverter : IValueConverter
    {
        private static GridLength _glAuto = new GridLength(0, GridUnitType.Auto);
        private static GridLength _gl1Star = new GridLength(1, GridUnitType.Star);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? _gl1Star : _glAuto;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
