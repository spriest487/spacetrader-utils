using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SpaceTrader.Util {
    public class PrefabLoader : MonoBehaviour {
        [SerializeField]
        private AssetReferenceGameObject prefabAsset;

        [SerializeField]
        private string singletonTag;

        private IEnumerator Start() {
            if (!this.prefabAsset.RuntimeKeyIsValid()) {
                Debug.LogWarningFormat(this, "Skipping PrefabLoader {0}: invalid asset reference", this.name);
                yield break;
            }

            if (!string.IsNullOrEmpty(this.singletonTag) && GameObject.FindWithTag(this.singletonTag)) {
                Debug.LogFormat(this, "Skipping PrefabLoader {0}: singleton already exists", this.name);
                yield break;
            }

            var transform = this.transform;
            var position = transform.position;
            var rotation = transform.rotation;
            var parent = transform.parent;

            var instantiateOp = this.prefabAsset.InstantiateAsync(position, rotation, parent);

            while (!instantiateOp.IsDone) {
                yield return null;
            }

            if (instantiateOp.OperationException != null || !instantiateOp.Result) {
                Debug.LogErrorFormat(this, "prefab load failed: {0}", instantiateOp.OperationException);
            } else {
                var instance = instantiateOp.Result;

                if (!string.IsNullOrEmpty(this.singletonTag) && !instance.CompareTag(this.singletonTag)) {
                    Debug.LogErrorFormat(this, "loaded prefab asset is missing the expected singleton tag '{0}'", this.singletonTag);
                    instance.tag = this.singletonTag;
                }
            }

            Destroy(this.gameObject);
        }
    }
}
