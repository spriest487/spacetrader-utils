#if ZENJECT

using UnityEngine;
using Zenject;

namespace SpaceTrader.Util {
    public static class ZenjectUtility {
        public static T InstantiatePrefabForComponent<T>(
            this IInstantiator instantiator,
            T prefab
        ) where T : Component {
            return instantiator.InstantiatePrefabForComponent<T>(
                prefab.gameObject
            );
        }

        public static T InstantiatePrefabForComponent<T>(
            this IInstantiator instantiator,
            T prefab,
            Transform parent
        ) where T : Component {
            return instantiator.InstantiatePrefabForComponent<T>(
                prefab.gameObject,
                parent
            );
        }
    }
}

#endif