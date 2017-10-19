using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace SpaceTrader.Util
{
    public static class ListUtility
    {
        public static T Random<T>(this IReadOnlyList<T> source, Random random)
        {
            Debug.Assert(source != null && source.Count > 0);

            var randomIndex = random.Next(source.Count);
            return source[randomIndex];
        }

        public static T RandomWeighted<T>(this IReadOnlyList<T> source, 
            Func<T, int> weightFunc,
            Random random)
        {
            Debug.Assert(source != null && source.Count > 0);

            var totalWeight = source.Sum(weightFunc);
            Debug.Assert(totalWeight > 0);

            var targetWeight = random.Next(totalWeight);
            foreach (var item in source)
            {
                var itemWeight = weightFunc(item);
                if (targetWeight < itemWeight)
                {
                    return item;
                }

                targetWeight -= itemWeight;
            }

            return default(T);
        }
    }
}
