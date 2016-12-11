using Catel.MVVM.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace PsoPatchEditor.Converter
{
    public class ByteArrayToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(string) && value is byte[])
            {
                return _GetStringFromByteArray(value as byte[]);
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType == typeof(byte[]) && value is string)
            {
                return this._GetByteArrayFromString(value as string);
            }
            return DependencyProperty.UnsetValue;
        }

        private byte[] _GetByteArrayFromString(string p)
        {
            return Regex.Matches(p, @"[0-9a-f]{2}")
                .Cast<Match>()
                .Select(x =>
                {
                    byte output;
                    if (byte.TryParse(x.Value, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out output))
                    {
                        return output;
                    }
                    throw new Exception("Hex number cannot be parsed.");
                }).ToArray();
        }

        private string _GetStringFromByteArray(byte[] p)
        {
            return String.Join(" ", p.Select(x => String.Format(@"{0:x2}", x)));
        }
    }
}
