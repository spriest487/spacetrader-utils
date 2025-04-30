using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace SpaceTrader.Util {
    public static class EnumerableExtensions {
        public static IEnumerable<(T Value, int Index)> Indexed<T>([NotNull] this IEnumerable<T> source) {
            var index = 0;
            foreach (var item in source) {
                yield return (item, index++);
            }
        }

        public static T MinBy<T, TKey>(
            [NotNull] this IEnumerable<T> source,
            [NotNull] Func<T, TKey> keySelector,
            [CanBeNull] Comparison<TKey> comparison = null
        ) {
            return source.ByComparison(keySelector, delta => delta < 0, comparison);
        }

        public static T MaxBy<T, TKey>(
            [NotNull] this IEnumerable<T> source,
            [NotNull] Func<T, TKey> keySelector,
            [CanBeNull] Comparison<TKey> comparison = null
        ) {
            return source.ByComparison(keySelector, static delta => delta > 0, comparison);
        }

        private static T ByComparison<T, TKey>(
            [NotNull] this IEnumerable<T> source,
            [NotNull] Func<T, TKey> keySelector,
            [NotNull] Predicate<int> resultSelector,
            [CanBeNull] Comparison<TKey> comparison
        ) {
            comparison ??= Comparer<TKey>.Default.Compare;
            using var items = source.GetEnumerator();

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
            }

            throw new ArgumentException("source must not be empty");
        }

        public static int FindIndex<T>(
            [NotNull] this IReadOnlyList<T> list,
            [NotNull] Predicate<T> predicate
        ) {
            foreach (var i in Enumerable.Range(0, list.Count)) {
                if (predicate(list[i])) {
                    return i;
                }
            }

            return -1;
        }

        public static int IndexOf<T>(
            [NotNull] this IReadOnlyList<T> list,
            [CanBeNull] T item,
            [CanBeNull] EqualityComparer<T> comparer = null
        ) {
            comparer ??= EqualityComparer<T>.Default;
            return list.FindIndex(each => comparer.Equals(item, each));
        }

        public static void ForEach<T>(
            this IReadOnlyList<T> list,
            [NotNull] Action<T> action
        ) {
            for (var i = 0; i < list.Count; ++i) {
                action(list[i]);
            }
        }

        public static int IndexOf<T>(
            in this ReadOnlySpan<T> source,
            T item,
            EqualityComparer<T> comparer = null
        ) {
            comparer ??= EqualityComparer<T>.Default;
            
            for (var i = 0; i < source.Length; i += 1) {
                if (comparer.Equals(item, source[i])) {
                    return i;
                }
            }

            return -1;
        }

        public static int FindIndex<T>(
            in this ReadOnlySpan<T> source,
            [NotNull] Predicate<T> predicate
        ) {
            for (var i = 0; i < source.Length; i += 1) {
                if (predicate(source[i])) {
                    return i;
                }
            }

            return -1;
        }

        public static bool SequenceEquals<T>(
            in this ReadOnlySpan<T> a,
            IReadOnlyList<T> b,
            [CanBeNull] EqualityComparer<T> comparer
        ) {
            if (a.Length != b.Count) {
                return false;
            }

            comparer ??= EqualityComparer<T>.Default;

            for (var i = 0; i < a.Length; i += 1) {
                if (!comparer.Equals(a[i], b[i])) {
                    return false;
                }
            }

            return true;
        }

        public static void Deconstruct<K, V>(this KeyValuePair<K, V> kvp, out K k, out V v) {
            k = kvp.Key;
            v = kvp.Value;
        }

        public static IEnumerable<K> Keys<K, V>([NotNull] this IEnumerable<KeyValuePair<K, V>> source) {
            foreach (var item in source) {
                yield return item.Key;
            }
        }

        public static IEnumerable<V> Values<K, V>([NotNull] this IEnumerable<KeyValuePair<K, V>> source) {
            foreach (var item in source) {
                yield return item.Value;
            }
        }

        public static void Deconstruct<K, V>(
            [NotNull] this IGrouping<K, V> group,
            out K k,
            [NotNull] out IEnumerable<V> v
        ) {
            k = group.Key;
            v = group;
        }

        public static IEnumerable<(T, T)> Windows2<T>([NotNull] this IEnumerable<T> items) {
            using var enumerator = items.GetEnumerator();
            if (!enumerator.MoveNext()) {
                yield break;
            }

            var prev = enumerator.Current;

            while (enumerator.MoveNext()) {
                yield return (prev, enumerator.Current);
                prev = enumerator.Current;
            }
        }

        public static IEnumerable<(T, T, T)> Windows3<T>([NotNull] this IEnumerable<T> items) {
            using var enumerator = items.GetEnumerator();

            if (!enumerator.MoveNext()) {
                yield break;
            }

            var prev0 = enumerator.Current;

            if (!enumerator.MoveNext()) {
                yield break;
            }

            var prev1 = enumerator.Current;

            while (enumerator.MoveNext()) {
                yield return (prev0, prev1, enumerator.Current);
                prev0 = prev1;
                prev1 = enumerator.Current;
            }
        }
    }
}
