using Catel.MVVM.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PsoPatchEditor.Converter
{
    public class Uint32ToHexStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(string) && value is UInt32)
            {
                return string.Format(@"{0:x}", value);
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(UInt32) && value is string)
            {
                UInt32 result;
                if (UInt32.TryParse(value as string, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result))
                {
                    return result;
                }
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
