using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SpaceTrader.Util {
    public abstract class InitializeOnPlayAsset : ScriptableObject {
        private bool initialized;
        
        private void OnEnable() {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= this.OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += this.OnPlayModeStateChanged;

            if (!Application.isPlaying) {
                return;
            }
#endif
            if (!this.initialized) {
                this.initialized = true;
                this.Initialize();
            }
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(PlayModeStateChange stateChange) {
            switch (stateChange) {
                case PlayModeStateChange.EnteredPlayMode: {
                    if (this.CheckPreloadedAssetsList()) {
                        this.initialized = true;
                        this.Initialize();
                    }

                    break;
                }

                case PlayModeStateChange.ExitingPlayMode: {
                    this.initialized = false;
                    break;
                }
            }
        }

        private bool CheckPreloadedAssetsList() {
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(this, out var guid, out _)) {
                return false;
            }

            var ignoreStateKey = $"{this.GetType().Name} Ignore {guid}";
            var preloadedAssets = PlayerSettings.GetPreloadedAssets();
            if (preloadedAssets.Contains(this)) {
                return true;
            }

            if (SessionState.GetBool(ignoreStateKey, defaultValue: false)) {
                return false;
            }

            const string title = "Missing preloaded asset";
            var msg = $"{this.GetType()} ({this.name}) was loaded, "
                + "but it was not included in the preloaded assets list. Add it now?";

            if (!EditorUtility.DisplayDialog(title, msg, "OK", "Ignore")) {
                SessionState.SetBool(ignoreStateKey, true);
                return false;
            }

            Array.Resize(ref preloadedAssets, preloadedAssets.Length + 1);
            preloadedAssets[^1] = this;

            PlayerSettings.SetPreloadedAssets(preloadedAssets);
            return true;
        }
#endif

        protected abstract void Initialize();
    }
}
