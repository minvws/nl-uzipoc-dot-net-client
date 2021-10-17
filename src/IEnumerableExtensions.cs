using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UziClientPoc
{
    static class IEnumerableExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this T item)
        {
            yield return item;
        }
    }
}
