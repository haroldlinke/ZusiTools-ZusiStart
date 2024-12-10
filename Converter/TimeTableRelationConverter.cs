using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ZusiStart.Data;

namespace ZusiStart.Converter
{
    [ValueConversion(typeof(TimeTableRelation), typeof(Visibility))]
    public class TimeTableRelationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeTableRelation ttr)
            {
                if (ttr.TimeTable != null)
                {
                    return Visibility.Visible;
                }
            }

            return (parameter is Visibility v) ? v : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
