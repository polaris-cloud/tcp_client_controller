using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Bee.Core.Controls.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object obj = value;
            if (obj is bool flag)
            {
                if (targetType == typeof(Visibility))
                {
                    if (flag)
                        return (object)Visibility.Visible;
                    return !(parameter is Visibility) ? (object)Visibility.Collapsed : parameter;
                }
            }
            else if (obj == null)
                return parameter is Visibility ? parameter : (object)Visibility.Collapsed;
            return (object)Visibility.Visible;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (!(value is Visibility))
                throw new ArgumentException("Invalid argument type. Expected argument: Visibility.", nameof(value));
            if (targetType != typeof(bool))
                throw new ArgumentException("Invalid return type. Expected type: bool", nameof(targetType));
            return (object)((Visibility)value == Visibility.Visible);
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => (object)ConverterCreater.Get<BoolToVisibilityConverter>();
    }
}
