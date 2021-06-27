#if UNITY_MLAPI

using System;
using MLAPI;
using MLAPI.Spawning;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PSPSPSPS {
    public class Spawner<T> : IDisposable where T : NetworkBehaviour {
        private readonly NetworkManager networkManager;

        private T prefab;
        private ulong prefabHash;

        public event Action<T> Spawned;

        public Spawner(NetworkManager networkManager) {
            for (var i = 0; i < networkManager.NetworkConfig.NetworkPrefabs.Count; i++) {
                var networkPrefab = networkManager.NetworkConfig.NetworkPrefabs[i];
                if (!this.prefab && networkPrefab.Prefab.TryGetComponent(out T prefab)) {
                    this.prefabHash = NetworkSpawnManager.GetPrefabHashFromIndex(i);
                    this.prefab = prefab;

                    break;
                }
            }

            NetworkSpawnManager.RegisterSpawnHandler(this.prefabHash, this.OnPrefabSpawned);
        }

        public void Dispose() {
            NetworkSpawnManager.UnregisterSpawnHandler(this.prefabHash);
        }

        private NetworkObject OnPrefabSpawned(Vector3 position, Quaternion rotation) {
            var instance = Object.Instantiate(this.prefab, position, rotation);
            this.Spawned?.Invoke(instance);

            return instance.NetworkObject;
        }

        public T Spawn(
            Vector3? position = default,
            Quaternion? rotation = default,
            ulong? ownerID = 0,
            bool spawnAsPlayer = false,
            bool destroyWithScene = false
        ) {
            var spawnPos = position ?? Vector3.zero;
            var spawnRot = rotation ?? Quaternion.identity;

            if (this.networkManager.IsServer) {
                var instance = Object.Instantiate(this.prefab, spawnPos, spawnRot);

                if (ownerID != null) {
                    if (spawnAsPlayer) {
                        instance.NetworkObject.SpawnAsPlayerObject(ownerID.Value, destroyWithScene: destroyWithScene);
                    } else {
                        instance.NetworkObject.SpawnWithOwnership(ownerID.Value);
                    }
                } else {
                    instance.NetworkObject.Spawn(destroyWithScene: destroyWithScene);
                }

                return instance;
            }

            if (!this.networkManager.IsClient) {
                return Object.Instantiate(this.prefab, spawnPos, spawnRot);
            }

            return null;
        }
    }
}

#endif
