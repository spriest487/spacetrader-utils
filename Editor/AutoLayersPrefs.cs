using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SpaceTrader.Util.EditorUtil {
    public static class AutoLayersPrefs {
        private const string DefaultOutputPath = "Assets/AutoLayers/Layers.cs";

        public static string OutputPath {
            get => GetPrefs()?.OutputPath ?? DefaultOutputPath;
            set {
                var prefs = GetPrefs() ?? new AutoLayersPrefsData();
                prefs.OutputPath = value ?? "";
                SetPrefs(prefs);
            }
        }

        public static string Namespace {
            get => GetPrefs()?.Namespace ?? GetScriptNamespace();
            set {
                var prefs = GetPrefs() ?? new AutoLayersPrefsData();
                prefs.Namespace = value ?? "";
                SetPrefs(prefs);
            }
        }

        private static string GetScriptNamespace() {
            var settingsAsset = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/EditorSettings.asset");
            using (var settingsObj = new SerializedObject(settingsAsset))
            using (var nsProp = settingsObj.FindProperty("m_ProjectGenerationRootNamespace")) {
                return nsProp.stringValue;
            }
        }

        private const string PrefsPath = "AutoLayers/AutoLayersSettings.json";

        private static AutoLayersPrefsData GetPrefs() {
            var file = new FileInfo(Path.Combine(Application.dataPath, PrefsPath));
            if (!file.Exists) {
                return null;
            }

            try {
                var json = File.ReadAllText(file.FullName);
                return JsonUtility.FromJson<AutoLayersPrefsData>(json);
            } catch {
                return null;
            }
        }

        private static void SetPrefs(AutoLayersPrefsData prefs) {
            var file = new FileInfo(Path.Combine(Application.dataPath, PrefsPath));

            try {
                file.Directory?.Create();

                var json = JsonUtility.ToJson(prefs);

                File.WriteAllText(file.FullName, json);
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }
    }
    
    [Serializable]
    internal class AutoLayersPrefsData {
        public string OutputPath;
        public string Namespace;
    }
}
