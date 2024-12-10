using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ZusiStart.Converter
{
    [ValueConversion(typeof(uint), typeof(string))]
    public class EpocheToStringConverter : IValueConverter
    {
        // I   1835 bis 1920
        // II  1921 bis 1948
        // III 1949 bis 1964
        // IV  1965 bis 1990
        // V   1991 bis 2006
        // VI  2007
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is uint)
            {
                switch ((uint)value)
                {
                    case 1:
                        return "1835-1920";
                    case 2:
                        return "1920-1950";
                    case 3:
                        return "1945-1970";
                    case 4:
                        return "1965-1990";
                    case 5:
                        return "1990-2006";
                    case 6:
                        return "ab 2007";
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
