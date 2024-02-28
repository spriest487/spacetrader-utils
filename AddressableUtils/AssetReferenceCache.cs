#if UNITY_ADDRESSABLES

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace SpaceTrader.Util.AddressableUtils {
    public class AssetReferenceCache<TAsset, TRef> : IDisposable 
        where TRef : AssetReference
        where TAsset : class {
        private readonly Dictionary<object, AsyncOperationHandle<TAsset>> handleCache;

        public AssetReferenceCache() {
            this.handleCache = new Dictionary<object, AsyncOperationHandle<TAsset>>();
        }

        public void Dispose() {
            this.Clear();
        }

        public void Clear() {
            foreach (var (_, handle) in this.handleCache) {
                if (handle.IsValid()) {
                    Addressables.Release(handle);
                }
            }
            
            this.handleCache.Clear();
        }

        [ItemCanBeNull]
        public ValueTask<TAsset> Get(TRef assetRef) {
            if (this.handleCache.TryGetValue(assetRef.RuntimeKey, out var handle)) {
                if (handle.IsDone) {
                    return new ValueTask<TAsset>(handle.Result);
                }

                if (handle.IsValid()) {
                    return new ValueTask<TAsset>(handle.Task);
                }
            }

            return new ValueTask<TAsset>(this.LoadAsync(assetRef));
        }

        [ItemCanBeNull]
        private async Task<TAsset> LoadAsync(TRef assetRef) {
            var handle = Addressables.LoadAssetAsync<TAsset>(assetRef);
            this.handleCache[assetRef.RuntimeKey] = handle;

            await handle.Task;
            
            if (handle.OperationException != null) {
                Debug.LogException(handle.OperationException);
            }


            Debug.LogFormat("cached asset: {0}", handle.Result switch {
                null => "null",
                Object asset => asset.name,
                object asset => asset.ToString(),
            });

            return handle.Result;
        }

        public bool GetCachedAsset(object runtimeKey, [MaybeNullWhen(false)] out TAsset asset) {
            if (!this.handleCache.TryGetValue(runtimeKey, out var handle)) {
                asset = default;
                return false;
            }

            asset = handle.Result;
            return asset != null;
        }
    }
}

#endif
