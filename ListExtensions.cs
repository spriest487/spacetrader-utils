using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;
using UnityRandom = UnityEngine.Random;

namespace SpaceTrader.Util {
    public static class ListUtility {
        public static bool Contains<T>(
            in this ReadOnlySpan<T> items, 
            T item, 
            [CanBeNull] EqualityComparer<T> comparer = null
        ) {
            comparer ??= EqualityComparer<T>.Default;

            for (var i = 0; i < items.Length; i += 1) {
                if (comparer.Equals(items[i], item)) {
                    return true;
                }
            }

            return false;
        }
        
        public static T Random<T>([NotNull] this IReadOnlyList<T> source) {
            Debug.Assert(source != null && source.Count > 0);

            var randomIndex = UnityRandom.Range(0, source.Count);
            return source[randomIndex];
        }

        public static T Random<T>([NotNull] this IReadOnlyList<T> source, Random random) {
            Debug.Assert(source != null && source.Count > 0);

            var randomIndex = random.Next(source.Count);
            return source[randomIndex];
        }
        
        public static T Random<T>(in this ReadOnlySpan<T> source) {
            Debug.Assert(source != null && source.Length > 0);

            var randomIndex = UnityRandom.Range(0, source.Length);
            return source[randomIndex];
        }
        
        public static T Random<T>(in this ReadOnlySpan<T> source, Random random) {
            Debug.Assert(source != null && source.Length > 0);

            var randomIndex = random.Next(source.Length);
            return source[randomIndex];
        }

        public static T RandomWeighted<T>(
            [NotNull] this IReadOnlyList<T> source,
            [NotNull] Func<T, int> weightSelector
        ) {
            var weightRange = source.Sum(weightSelector);
            var value = UnityRandom.Range(0, weightRange);

            var weightAcc = 0;
            foreach (var item in source) {
                weightAcc += weightSelector(item);
                if (weightAcc >= value) {
                    return item;
                }
            }

            return default; // unreachable
        }

        public static T RandomWeighted<T>(
            [NotNull] this IReadOnlyList<T> source,
            [NotNull] Random random,
            [NotNull] Func<T, int> weightSelector
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

        public static void Shuffle<T>([NotNull] this IList<T> list) {
            var n = list.Count;
            while (n > 1) {
                n--;
                var k = UnityRandom.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public static bool TopologicalSort<T>(
            [NotNull] this IEnumerable<T> nodes,
            [NotNull] Func<T, IEnumerable<T>> neighborFunc,
            [NotNull] List<T> results
        ) {
            var states = new Dictionary<T, bool>();

            foreach (var node in nodes) {
                if (!Visit(node)) {
                    results.Clear();
                    return false;
                }
            }

            return true;

            bool Visit(T node) {
                if (states.TryGetValue(node, out var state)) {
                    return state;
                }

                states[node] = false;
                foreach (var neighbor in neighborFunc(node)) {
                    if (!Visit(neighbor)) {
                        return false;
                    }
                }

                states[node] = true;

                results.Add(node);
                return true;
            }
        }
    }
}
