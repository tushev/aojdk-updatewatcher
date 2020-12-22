using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AJ_UpdateWatcher
{
    class SuggestRemoveOrConvertBackVisibilityMultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(x => x == DependencyProperty.UnsetValue))
                return DependencyProperty.UnsetValue;

            bool is_autodiscovered = (bool)values[0];
            bool overrides_autodiscovered = (bool)values[1];
            //string type_text = (string)values[2]; //for debug
            bool negate = (parameter != null);

            // VISIBLE = NOT is_autodiscovered AND (overrides_autodiscovered XNOR negate)
            bool boolValue = (!is_autodiscovered && (overrides_autodiscovered == negate ));

            return boolValue ? Visibility.Visible : Visibility.Collapsed;

        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
