using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memento.SRS
{
    public static class Extension
    {
        public static TSource GetMaxElement<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.OrderBy(keySelector).First();
        }

        public static TSource GetMinElement<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.OrderByDescending(keySelector).First();
        }

        public static IEnumerable<TSource> ToEnumerable<TSource>(this TSource source)
        {
            return Enumerable.Repeat(source, 1);
        }

        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, TSource second)
        {
            return first.Concat(second.ToEnumerable());
        }

        public static IEnumerable<TSource> Concat<TSource>(this TSource first, IEnumerable<TSource> second)
        {
            return first.ToEnumerable().Concat(second);
        }
    }
}
