using System;
using System.Globalization;
using System.Windows.Data;
using ZusiKlassenLib;

namespace ZusiStart.Converter
{
    [ValueConversion(typeof(FrictionPreset), typeof(uint))]
    public class FrictionPresetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is FrictionPreset f ? (uint)f : 0u;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is uint u && u < 4 ? (FrictionPreset)u : FrictionPreset.Dry;
        }
    }
}
