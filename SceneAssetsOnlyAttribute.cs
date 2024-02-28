#if UNITY_ADDRESSABLES

using UnityEditor;
using UnityEngine;

namespace SpaceTrader.Util {
    public class SceneAssetsOnlyAttribute : AssetReferenceUIRestriction {
        public override bool ValidateAsset(Object obj) {
#if UNITY_EDITOR
            return this.ValidateAsset(AssetDatabase.GetAssetPath(obj));
#else
            return base.ValidateAsset(obj);
#endif
        }

        public override bool ValidateAsset(string path) {
#if UNITY_EDITOR
            var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
            return assetType == typeof(SceneAsset);
#else
            return base.ValidateAsset(path);
#endif
        }
    }
}

#endif
