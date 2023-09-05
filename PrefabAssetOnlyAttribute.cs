using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpaceTrader.Util {
    public class PrefabAssetOnlyAttribute : AssetReferenceUIRestriction {
        public Type ComponentType { get; }
        
        public PrefabAssetOnlyAttribute(Type componentType = null) {
            this.ComponentType = componentType;
        }

        public override bool ValidateAsset(Object obj) {
#if UNITY_EDITOR
            if (obj is not GameObject gameObj) {
                return false;
            }

            if (!PrefabUtility.IsPartOfPrefabAsset(gameObj)) {
                return false;
            }

            if (this.ComponentType != null) {
                return gameObj.TryGetComponent(this.ComponentType, out _);
            }
#endif

            return true;
        }

        public override bool ValidateAsset(string path) {
#if UNITY_EDITOR
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (!this.ValidateAsset(asset)) {
                return false;
            }
#endif
            return true;
        }
    }
}
