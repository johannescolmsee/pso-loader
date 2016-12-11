using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.EnumerableExtensions
{
    public static class IEnumerableExtension
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> values)
        {
            return new ObservableCollection<T>(values);
        }
    }
}
