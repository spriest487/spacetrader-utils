using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;
using UnityRandom = UnityEngine.Random;

namespace SpaceTrader.Util {
    public static class ListUtility {
        public static T Random<T>(this IReadOnlyList<T> source) {
            Debug.Assert(source != null && source.Count > 0);

            var randomIndex = UnityRandom.Range(0, source.Count);
            return source[randomIndex];
        }

        public static T Random<T>(this IReadOnlyList<T> source, Random random) {
            Debug.Assert(source != null && source.Count > 0);

            var randomIndex = random.Next(source.Count);
            return source[randomIndex];
        }

        public static T RandomWeighted<T>(
            this IReadOnlyList<T> source,
            Func<T, int> weightSelector
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
