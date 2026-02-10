using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceTrader.Util {
    public static class SceneExtensions {
        [CanBeNull]
        public static T FindObjectOfType<T>(
            this Scene scene,
            bool includeInactive = false,
            List<GameObject> cachedRootObjects = null
        )
        where T : Component {
            if (cachedRootObjects != null) {
                cachedRootObjects.Clear();
                cachedRootObjects.Capacity = scene.rootCount;

                scene.GetRootGameObjects(cachedRootObjects);

                foreach (var rootObj in cachedRootObjects) {
                    var child = rootObj.GetComponentInChildren<T>(includeInactive);
                    if (child) {
                        return child;
                    }
                }
            } else {
                foreach (var rootObj in scene.GetRootGameObjects()) {
                    var child = rootObj.GetComponentInChildren<T>(includeInactive);
                    if (child) {
                        return child;
                    }
                }
            }

            return null;
        }
    }
}
