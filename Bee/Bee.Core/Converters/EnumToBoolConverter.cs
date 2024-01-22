using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Bee.Core.Converters
{
    public class EnumToBoolConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            Type type=value.GetType();
            if (!type.IsEnum)
                throw new ArgumentException(@"value is not enum", nameof(value)); 
            if(parameter ==null)
                throw new ArgumentNullException(nameof(parameter));
            var parsed=Enum.Parse(value.GetType(), parameter.ToString()?? throw new ArgumentNullException(nameof(parameter)));
            return value.Equals(parsed); 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));
            return Enum.Parse(targetType, parameter.ToString() ?? throw new ArgumentException(nameof(parameter)));
        }
    }
}
