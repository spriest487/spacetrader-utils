using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SpaceTraderUtils.AddressableUtils {
    public class PrefabAssetSpawner : MonoBehaviour {
        [field: SerializeField]
        public AssetReferenceT<GameObject> PrefabAsset { get; private set; }

        public event Action<PrefabAssetSpawner> Spawned;

        private IEnumerator Start() {
            var transform = this.transform;
            var op = this.PrefabAsset.InstantiateAsync(transform.position, transform.rotation);

            while (!op.IsDone) {
                yield return null;
            }

            if (!op.IsValid()) {
                if (op.OperationException is { } ex) {
                    Debug.LogException(ex);
                }

                Debug.LogErrorFormat("Core failed to load ({0})", op.Status);
            } else {
                this.Spawned?.Invoke(this);
            }

            Destroy(this.gameObject);
        }

#if UNITY_EDITOR
        private const string ActionName = "Replace Prefab with Asset Spawner";
        private const string ReplaceInstanceMenuItemName = "GameObject/" + ActionName;

        [MenuItem(ReplaceInstanceMenuItemName, validate = true)]
        private static bool ReplaceInstanceWithSpawnerIsValid() {
            return Selection.gameObjects.All(PrefabUtility.IsAnyPrefabInstanceRoot);
        }

        [MenuItem(ReplaceInstanceMenuItemName, priority = 20)]
        private static void ReplaceInstanceWithSpawner() {
            var selection = Selection.objects;

            for (var i = 0; i < selection.Length; i += 1) {
                var original = (GameObject)selection[i];

                var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(original);
                var guid = AssetDatabase.AssetPathToGUID(prefabPath);

                var spawnerGameObj = new GameObject(original.name);
                spawnerGameObj.SetActive(original.activeSelf);
                spawnerGameObj.transform.SetParent(original.transform.parent);
                spawnerGameObj.transform.SetSiblingIndex(original.transform.GetSiblingIndex());
                spawnerGameObj.transform.localPosition = original.transform.localPosition;
                spawnerGameObj.transform.localRotation = original.transform.localRotation;
                spawnerGameObj.transform.localScale = original.transform.localScale;

                var spawner = spawnerGameObj.AddComponent<PrefabAssetSpawner>();
                spawner.PrefabAsset = new AssetReferenceT<GameObject>(guid);

                Undo.RegisterCreatedObjectUndo(spawnerGameObj, ActionName);
                Undo.DestroyObjectImmediate(original);
            }

            Selection.objects = selection;
        }
#endif
    }
}
