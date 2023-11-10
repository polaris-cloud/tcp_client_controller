using System;
using System.Globalization;
using System.Windows.Data;
using Bee.Core.Utils;

namespace Bee.Core.Converters
{
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var myEnum = (Enum)value;
            if (myEnum == null) return null;
            var display = myEnum.GetDisplay();
            return !string.IsNullOrEmpty(display) ? display : myEnum.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }

    }
}
