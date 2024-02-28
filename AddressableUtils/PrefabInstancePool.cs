#if UNITY_ADDRESSABLES

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace SpaceTrader.Util.AddressableUtils {
    public class PrefabInstancePool : IDisposable {
        private readonly AssetReferenceCache<GameObject, AssetReferenceGameObject> handleCache;

        private readonly Dictionary<object, ObjectPool<GameObject>> instancePools;
        private readonly Dictionary<GameObject, object> instanceKeys;

        private readonly GameObject poolRoot;

        public PrefabInstancePool(string rootName) {
            this.handleCache = new AssetReferenceCache<GameObject, AssetReferenceGameObject>();

            this.instancePools = new Dictionary<object, ObjectPool<GameObject>>();
            this.instanceKeys = new Dictionary<GameObject, object>();

            this.poolRoot = new GameObject(nameof(rootName));
            this.poolRoot.SetActive(false);
            Object.DontDestroyOnLoad(this.poolRoot);
        }

        public void Dispose() {
            this.handleCache.Dispose();
            this.Clear();

            Object.Destroy(this.poolRoot);
        }

        public void Clear() {
            this.handleCache.Clear();

            foreach (var (_, instancePool) in this.instancePools) {
                instancePool.Dispose();
            }

            this.instancePools.Clear();
            this.instanceKeys.Clear();
        }
        
        public ValueTask<GameObject> Get(AssetReferenceGameObject assetRef, Transform parent = null) {
            return this.Get(assetRef, Vector3.zero, Quaternion.identity, parent);
        }

        public ValueTask<GameObject> Get(
            AssetReferenceGameObject assetRef,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null
        ) {
            var loadTask = this.handleCache.Get(assetRef);
            if (!loadTask.IsCompleted) {
                var runtimeKey = assetRef.RuntimeKey;
                var instantiateAsync = this.InstantiateAsync(runtimeKey, loadTask, position, rotation, parent);
                return new ValueTask<GameObject>(instantiateAsync);
            }

            var instance = this.InstantiatePooled(assetRef.RuntimeKey, loadTask.Result, position, rotation, parent);
            return new ValueTask<GameObject>(instance);
        }

        public ValueTask<T> Get<T>(
            AssetReferenceGameObject assetRef,
            Transform parent = null
        )
            where T : Component {
            return this.Get<T>(assetRef, Vector3.zero, Quaternion.identity, parent);
        }

        public ValueTask<T> Get<T>(
            AssetReferenceGameObject assetRef,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null
        )
            where T : Component {
            var instantiate = this.Get(assetRef, position, rotation, parent);
            if (instantiate.IsCompleted) {
                return new ValueTask<T>(instantiate.Result.GetComponent<T>());
            }

            return new ValueTask<T>(WaitForInstantiate(instantiate));

            static async Task<T> WaitForInstantiate(ValueTask<GameObject> task) {
                var instance = await task;
                return instance.GetComponent<T>();
            }
        }

        public void Release(GameObject instance) {
            if (!instance || !this.instanceKeys.TryGetValue(instance, out var runtimeKey)) {
                Debug.LogErrorFormat(
                    instance,
                    "{0} ({1}): invalid instance passed to {2}",
                    nameof(PrefabInstancePool),
                    this.poolRoot.name,
                    nameof(this.Release)
                );
                return;
            }

            if (!this.instancePools.TryGetValue(runtimeKey, out var pool)) {
                Debug.LogWarningFormat(
                    "{0} ({1}: missing pool for {2}, destroying instead of releasing",
                    nameof(PrefabInstancePool),
                    this.poolRoot.name,
                    instance
                );

                Object.Destroy(instance.gameObject);
                return;
            }

            instance.transform.SetParent(this.poolRoot.transform, false);
            pool.Release(instance.gameObject);
        }

        private async Task<GameObject> InstantiateAsync(
            object runtimeKey,
            ValueTask<GameObject> loadTask,
            Vector3 position,
            Quaternion rotation,
            Transform parent
        ) {
            var prefab = await loadTask;
            var instance = this.InstantiatePooled(runtimeKey, prefab, position, rotation, parent);

            if (!instance) {
                Debug.LogErrorFormat(
                    "{0} ({1}): failed to instantiate prefab",
                    nameof(PrefabInstancePool),
                    this.poolRoot.name
                );
            }

            return instance;
        }

        private GameObject InstantiatePooled(
            object runtimeKey,
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent
        ) {
            if (!prefab) {
                return null;
            }

            var pool = this.GetPool(runtimeKey, prefab);
            var instance = pool.Get();

            var instanceTransform = instance.transform;
            instanceTransform.SetPositionAndRotation(position, rotation);
            instanceTransform.SetParent(parent, worldPositionStays: true);

            this.instanceKeys[instance] = runtimeKey;

            return instance;
        }

        private ObjectPool<GameObject> GetPool(object runtimeKey, GameObject prefab) {
            if (this.instancePools.TryGetValue(runtimeKey, out var pool)) {
                return pool;
            }

            pool = new ObjectPool<GameObject>(
                createFunc: () => Object.Instantiate(prefab),
                actionOnDestroy: Object.Destroy
            );

            this.instancePools.Add(runtimeKey, pool);

            return pool;
        }
    }
}

#endif
