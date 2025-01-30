using System;
using System.Collections.Generic;

namespace Preliy.Flange
{
    public static class LinqExtension
    {
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer = null)
        {
            comparer ??= Comparer<TKey>.Default;
            return source.ArgBy(keySelector, lag => comparer.Compare(lag.Current, lag.Previous) < 0);
        }

        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer = null)
        {
            comparer ??= Comparer<TKey>.Default;
            return source.ArgBy(keySelector, lag => comparer.Compare(lag.Current, lag.Previous) > 0);
        }
        
        public static TSource ArgBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<(TKey Current, TKey Previous), bool> predicate)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            var value = default(TSource);
            var key = default(TKey);

            if (value == null)
            {
                foreach (var other in source)
                {
                    if (other == null) continue;
                    var otherKey = keySelector(other);
                    if (otherKey == null) continue;
                    if (value == null || predicate((otherKey, key)))
                    {
                        value = other;
                        key = otherKey;
                    }

                }
                return value;
            }
            
            var hasValue = false;
            foreach (var other in source)
            {
                var otherKey = keySelector(other);
                if (otherKey == null) continue;

                if (hasValue)
                {
                    if (predicate((otherKey, key))) 
                    {
                        value = other;
                        key = otherKey;
                    }
                }
                else
                {
                    value = other;
                    key = otherKey;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw new InvalidOperationException("Sequence2 contains no elements");
        }
    }
}
