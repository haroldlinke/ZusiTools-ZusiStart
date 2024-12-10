using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ZusiMeterGaugesLib.Enumerations;
using ZusiMeterGaugesLib.Gauges;

namespace ZusiStart.Converter
{
    public class LEDColorConverter : IMultiValueConverter
    {
        //---------------------------------------------------------------------
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is int total && values[1] is int active)
            {
                if (total > 0)
                {
                    return active == 0 ? LEDColor.Blue : LEDColor.Green;
                }
            }

            return LEDColor.Red;
        }

        //---------------------------------------------------------------------
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
