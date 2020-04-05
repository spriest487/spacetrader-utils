using System;
using System.Linq;
using System.Collections.Generic;

namespace SpaceTrader.Util {
    public static class EnumerableExtensions {
        public static IEnumerable<(T Value, int Index)> Indexed<T>(
                this IEnumerable<T> source) {
            var index = 0;
            foreach (var item in source) {
                yield return (item, index++);
            }
        }

        public static T MinBy<T, TKey>(this IEnumerable<T> source,
                Func<T, TKey> keySelector,
                Comparison<TKey> comparison = null) {
            return source.ByComparison(keySelector, delta => delta < 0, comparison);
        }

        public static T MaxBy<T, TKey>(this IEnumerable<T> source,
                Func<T, TKey> keySelector,
                Comparison<TKey> comparison = null) {
            return source.ByComparison(keySelector, delta => delta > 0, comparison);
        }

        private static T ByComparison<T, TKey>(this IEnumerable<T> source,
                Func<T, TKey> keySelector,
                Func<int, bool> resultSelector,
                Comparison<TKey> comparison) {
            comparison = comparison ?? Comparer<TKey>.Default.Compare;
            var items = source.GetEnumerator();

            if (items.MoveNext()) {
                var result = items.Current;
                var minKey = keySelector(result);

                while (items.MoveNext()) {
                    var item = items.Current;
                    var key = keySelector(item);

                    if (resultSelector(comparison(key, minKey))) {
                        result = item;
                        minKey = key;
                    }
                }

                return result;
            } else {
                throw new ArgumentException("source must not be empty");
            }
        }

        public static int FindIndex<T>(this IReadOnlyList<T> list,
                Func<T, bool> predicate) {
            foreach (var i in Enumerable.Range(0, list.Count)) {
                if (predicate(list[i])) {
                    return i;
                }
            }
            return -1;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> list,
                T item) {
            return list.FindIndex(each => item.Equals(each));
        }

        public static void ForEach<T>(this IReadOnlyList<T> list,
                Action<T> action) {
            for (int i = 0; i < list.Count; ++i) {
                action(list[i]);
            }
        }

        public static void Deconstruct<K, V>(this KeyValuePair<K, V> kvp, out K k, out V v) {
            k = kvp.Key;
            v = kvp.Value;
        }

        public static T RandomWeighted<T>(
            this IReadOnlyList<T> source,
            Random random,
            Func<T, int> weightSelector
        ) {
            var weightRange = source.Sum(weightSelector);
            var value = random.Next(0, weightRange);

            var weightAcc = 0;
            foreach (var item in source) {
                weightAcc += weightSelector(item);
                if (weightAcc >= value) {
                    return item;
                }
            }
            return default; // unreachable
        }
    }
}