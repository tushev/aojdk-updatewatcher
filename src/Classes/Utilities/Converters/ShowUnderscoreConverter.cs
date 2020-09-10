using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Adoptium_UpdateWatcher
{
    public class ShowUnderscoreConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is string text ? text.Replace("_", "__") : null;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is string text ? text.Replace("__", "_") : null;
    }
}
