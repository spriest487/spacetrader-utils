using UnityEditor;
using UnityEngine;

namespace SpaceTrader.Util.EditorUtil {
    public class AutoLayersEditorWindow : EditorWindow {
        [MenuItem("Tools/SpaceTrader/Auto Layers...")]
        private static void ShowAutoLayersEditorWindow() {
            var window = GetWindow<AutoLayersEditorWindow>(true);

            window.Show();
        }

        private void OnGUI() {
            AutoLayersPrefs.OutputPath = EditorGUILayout.TextField("Output Path", AutoLayersPrefs.OutputPath);
            AutoLayersPrefs.Namespace = EditorGUILayout.TextField("Namespace", AutoLayersPrefs.Namespace);

            if (GUILayout.Button("Generate")) {
                AutoLayersSaveProcessor.GenerateScript();
            }
        }
    }
}
