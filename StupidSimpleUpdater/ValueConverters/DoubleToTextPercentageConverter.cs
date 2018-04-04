using System;
using System.Globalization;
using System.Windows.Data;

namespace StupidSimpleUpdater.ValueConverters
{
    public class DoubleToTextPercentageConverter : IValueConverter
    {
        #region converter methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double && targetType == typeof(string))
            {
                return string.Format("{0:0.0}%", (double)value);
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}
