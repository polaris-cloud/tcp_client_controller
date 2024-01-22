using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Bee.Core.Utils;

namespace Bee.Core.Converters
{
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {


            if (value is Enum myEnum)
            {
                var display = myEnum.GetDisplay();
                return !string.IsNullOrEmpty(display) ? display : myEnum.ToString();
            }
            return DependencyProperty.UnsetValue;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }

    }
}
