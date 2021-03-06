﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Neodenit.Memento.Common
{
    public static class Extension
    {
        public static TSource GetMaxElement<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.OrderByDescending(keySelector).FirstOrDefault();
        }

        public static TSource GetMinElement<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.OrderBy(keySelector).FirstOrDefault();
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
