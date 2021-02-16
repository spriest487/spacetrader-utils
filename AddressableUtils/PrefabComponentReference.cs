using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

#if ADDRESSABLES

namespace SpaceTraderUtils.AddressableUtils {
    [Serializable]
    public class PrefabComponentReference<TComponent> : AssetReferenceGameObject where TComponent : Component {
        public PrefabComponentReference(string guid) : base(guid) {
        }

        public new AsyncOperationHandle<TComponent> LoadAssetAsync() {
            var loadPrefabOp = base.LoadAssetAsync();

            static AsyncOperationHandle<TComponent> ThenGetComponent(AsyncOperationHandle<GameObject> completedOp) {
                var component = completedOp.Result.GetComponent<TComponent>();
                return Addressables.ResourceManager.CreateCompletedOperation(component, null);
            }

            return Addressables.ResourceManager.CreateChainOperation(loadPrefabOp, ThenGetComponent);
        }

        public new AsyncOperationHandle<T> LoadAssetAsync<T>() where T : class {
            var loadAssetOp = this.LoadAssetAsync();

            static AsyncOperationHandle<T> ThenCast(AsyncOperationHandle<TComponent> completedOp) {
                var result = completedOp.Result as T;
                return Addressables.ResourceManager.CreateCompletedOperation(result, null);
            }

            return Addressables.ResourceManager.CreateChainOperation(loadAssetOp, ThenCast);
        }

        public override bool ValidateAsset(string path) {
#if UNITY_EDITOR
            var prefab = AssetDatabase.LoadMainAssetAtPath(path);
            return this.ValidateAsset(prefab);
#else
            return false;
#endif
        }

        public override bool ValidateAsset(Object obj) {
            if (!(obj is GameObject prefab)) {
                return false;
            }

            return prefab.TryGetComponent(out TComponent _);
        }
    }
}

#endif
