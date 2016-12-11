using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Common.Converter
{
    public class Mapping
    {
        public object Input { get; set; }
        public object Output { get; set; }
    }
    public class MappingConverter : IValueConverter
    {
        public MappingConverter()
        {
            this.Mappings = new List<Mapping>();
        }
        public List<Mapping> Mappings { get; set; }


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var map = this.Mappings.FirstOrDefault(x => object.Equals(x.Input, value));
            if (map != null)
            {
                return map.Output;
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var map = this.Mappings.FirstOrDefault(x => object.Equals(x.Output, value));
            if (map != null)
            {
                return map.Input;
            }
            return DependencyProperty.UnsetValue;
        }
    }
}
