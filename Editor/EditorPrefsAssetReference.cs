using UnityEditor;
using UnityEngine;

namespace SpaceTrader.Util.EditorUtil {
    public class EditorPrefsAssetReference<T> where T : Object {
        public string Name { get; }

        private readonly string pathKey;
        private T instance;

        public string Path {
            get => EditorPrefs.GetString(this.pathKey, "");
            set {
                EditorPrefs.SetString(this.pathKey, value);
                this.instance = null;
            }
        }

        public T Instance {
            get {
                if (!this.instance && !string.IsNullOrWhiteSpace(this.Path)) {
                    this.instance = AssetDatabase.LoadAssetAtPath<T>(this.Path);
                    if (!this.instance) {
                        this.Path = "";
                    }
                }

                return this.instance;
            }
        }

        public EditorPrefsAssetReference(string name) {
            this.Name = name;
            this.pathKey = $"EditorPrefsAssetReference.{name}";
        }
    }
}
