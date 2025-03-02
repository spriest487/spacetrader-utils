#if UNITY_ADDRESSABLES

using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace SpaceTrader.Util.AddressableUtils {
    public static class AddressablesExtensions {
        public static Awaitable ToAwaitable(this AsyncOperationHandle handle) {
            var completion = new AwaitableCompletionSource();
            handle.Completed += completed => {
                if (completed.Status == AsyncOperationStatus.Succeeded) {
                    completion.SetResult();
                } else if (completed.OperationException != null) {
                    completion.SetException(completed.OperationException);
                } else {
                    completion.SetException(new Exception("async operation failed to complete"));
                }
            };

            return completion.Awaitable;
        }

        public static Awaitable<T> ToAwaitable<T>(this AsyncOperationHandle<T> handle) {
            var completion = new AwaitableCompletionSource<T>();

            handle.Completed += completed => {
                if (completed.Status == AsyncOperationStatus.Succeeded && completed.Result != null) {
                    completion.SetResult(completed.Result);
                } else if (completed.OperationException != null) {
                    completion.SetException(completed.OperationException);
                } else {
                    completion.SetException(new Exception("async operation failed to complete"));
                }
            };

            return completion.Awaitable;
        }

        public static async Awaitable<T> GetOrLoadAsset<T>(this AssetReferenceT<T> reference) where T : Object {
            if (reference.Asset) {
                return (T)reference.Asset;
            }

            if (reference.IsValid()) {
                return await reference.OperationHandle.Convert<T>().ToAwaitable();
            }

            return await reference.LoadAssetAsync().ToAwaitable();
        }
    }
}

#endif
