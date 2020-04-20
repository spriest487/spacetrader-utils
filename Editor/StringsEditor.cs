using UnityEditor;
using UnityEngine;

namespace SpaceTrader.Util.EditorUtil {
    [CustomEditor(typeof(Strings))]
    public class StringsEditor : Editor {
        public override void OnInspectorGUI() {
            var strings = this.target as Strings;
            if (strings == null) {
                return;
            }

            var style = new GUIStyle(EditorStyles.miniLabel) {
                wordWrap = true
            };

            foreach (var entry in strings) {
                EditorGUILayout.LabelField(entry.Key, entry.Value, style);
            }
        }
    }
}