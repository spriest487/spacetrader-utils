using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

#if ADDRESSABLES

namespace SpaceTraderUtils.AddressableUtils {
    [Serializable]
    public class PrefabComponentReference<TComponent> : AssetReferenceGameObject
        where TComponent : Component {
        public PrefabComponentReference(string guid) : base(guid) {
        }

        private static AsyncOperationHandle<T> ThenGetComponent<T>(AsyncOperationHandle<GameObject> completedOp)
            where T : class {
            if (completedOp.Status != AsyncOperationStatus.Succeeded) {
                var msg = completedOp.OperationException?.Message;
                return Addressables.ResourceManager.CreateCompletedOperation<T>(null, msg);
            }

            var component = completedOp.Result.GetComponent<T>();
            return Addressables.ResourceManager.CreateCompletedOperation(component, null);
        }

        public new AsyncOperationHandle<TComponent> LoadAssetAsync() {
            var loadPrefabOp = base.LoadAssetAsync();
            return Addressables.ResourceManager.CreateChainOperation(loadPrefabOp, ThenGetComponent<TComponent>);
        }

        public new AsyncOperationHandle<TComponent> InstantiateAsync(
            Transform parent = null,
            bool instantiateInWorldSpace = false
        ) {
            var instantiateOp = base.InstantiateAsync(parent, instantiateInWorldSpace);
            return Addressables.ResourceManager.CreateChainOperation(instantiateOp, ThenGetComponent<TComponent>);
        }

        public new AsyncOperationHandle<TComponent> InstantiateAsync(
            Vector3 position,
            Quaternion rotation,
            Transform parent = null
        ) {
            var instantiateOp = base.InstantiateAsync(position, rotation, parent);
            return Addressables.ResourceManager.CreateChainOperation(instantiateOp, ThenGetComponent<TComponent>);
        }

        private static AsyncOperationHandle<T> ThenCast<T>(AsyncOperationHandle<TComponent> completedOp)
            where T : class {
            if (completedOp.Status != AsyncOperationStatus.Succeeded) {
                var msg = completedOp.OperationException?.Message;
                return Addressables.ResourceManager.CreateCompletedOperation<T>(null, msg);
            }

            var result = completedOp.Result as T;
            return Addressables.ResourceManager.CreateCompletedOperation(result, null);
        }

        public new AsyncOperationHandle<T> LoadAssetAsync<T>()
            where T : class {
            return Addressables.ResourceManager.CreateChainOperation(this.LoadAssetAsync(), ThenCast<T>);
        }

        public AsyncOperationHandle<T> InstantiateAsync<T>(
            Transform parent = null,
            bool instantiateInWorldSpace = false
        )
            where T : class {
            return Addressables.ResourceManager.CreateChainOperation(
                this.InstantiateAsync(parent, instantiateInWorldSpace),
                ThenCast<T>
            );
        }

        public AsyncOperationHandle<T> InstantiateAsync<T>(
            Vector3 position,
            Quaternion rotation,
            Transform parent = null
        )
            where T : class {
            return Addressables.ResourceManager.CreateChainOperation(
                this.InstantiateAsync(position, rotation, parent),
                ThenCast<T>
            );
        }

        public void ReleaseInstance(TComponent instance) {
            Addressables.ReleaseInstance(instance.gameObject);
        }

        public void ReleaseInstance(AsyncOperationHandle<TComponent> instance) {
            Addressables.ReleaseInstance(instance);
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
