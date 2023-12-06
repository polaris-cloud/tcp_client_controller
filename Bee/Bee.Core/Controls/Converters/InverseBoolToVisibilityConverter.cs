using System;
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
    public class InverseBoolToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                throw new ArgumentException("Invalid argument type. Expected argument: bool.", nameof(value));
            if (targetType != typeof(Visibility))
                throw new ArgumentException("Invalid return type. Expected type: Visibility", nameof(targetType));
            if (!(bool)value)
                return (object)Visibility.Visible;
            return !(parameter is Visibility) ? (object)Visibility.Collapsed : parameter;
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
            return (object)((Visibility)value != 0);
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => (object)ConverterCreater.Get<InverseBoolToVisibilityConverter>();
    }
}
