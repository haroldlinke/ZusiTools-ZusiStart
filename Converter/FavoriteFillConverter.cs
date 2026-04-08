using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using ZusiStart.Data;
using ZusiStart.Miscellaneous;
using System.Windows.Media;

namespace ZusiStart.Converter
{
  public class FavoriteFillConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var train = value as ZusiKlassenLib2.Fahrplan.Zug;
      string timeTablefilename = train.FahrplanDatei.FullPath;
      RecentTrain rt = new(train, timeTablefilename);
      var favorites = parameter as RecentTrainsCollection;
      if (train != null && favorites != null && favorites.Contains(rt))
        return Brushes.Gold; // Favorit
      return Brushes.Transparent; // Nicht Favorit
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }

}
