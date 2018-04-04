using System;
using System.Globalization;
using System.Windows.Data;

namespace StupidSimpleUpdater.ValueConverters
{
    public class BoolToInverseValueConverter : IValueConverter
    {
        #region converter methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && targetType == typeof(bool))
            {
                return !((bool)value);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && targetType == typeof(bool))
            {
                return !((bool)value);
            }
            return false;
        }

        #endregion
    }
}
