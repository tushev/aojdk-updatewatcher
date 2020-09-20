using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AJ_UpdateWatcher
{
    public class InstallationStatusToVisibilityMultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(x => x == DependencyProperty.UnsetValue))
                return DependencyProperty.UnsetValue;

            bool is_autodiscovered = (bool)values[0];
            bool check_for_updates_flag = (bool)values[1];
            bool show_shadowed = (bool)values[2];

            bool boolValue = true;
            if (is_autodiscovered && check_for_updates_flag == false)
                boolValue = show_shadowed;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;

        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}