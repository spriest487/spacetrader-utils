using System.IO;
using JetBrains.Annotations;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Teragram.Util.EditorUtil {
    [CustomEditor(typeof(LocalizationStrings))]
    public class LocalizationStringsEditor : OdinEditor {
        public override void OnInspectorGUI() {
            this.Tree.BeginDraw(true);
            this.Tree.Draw();
            this.Tree.EndDraw();
            
            if (GUILayout.Button("Create Script")) {
                var scriptPath = ((LocalizationStrings)this.target).scriptPath; 
                var scriptNamespace = ((LocalizationStrings)this.target).scriptNamespace; 
                this.CreateScript(scriptPath, scriptNamespace);
            }
        }

        private void CreateScript([NotNull] string path, [CanBeNull] string scriptNamespace) {
            var className = Path.GetFileNameWithoutExtension(path);
            
            using var writer = File.CreateText(path);
            
            writer.WriteLine("// ReSharper disable UnusedMember.Global");
            writer.WriteLine("// ReSharper disable RedundantVerbatimPrefix");

            var hasNamespace = !string.IsNullOrWhiteSpace(scriptNamespace); 
            if (hasNamespace) {
                writer.WriteLine("namespace {0} {{", scriptNamespace);
            }

            writer.WriteLine("public static class {0} {{", className);

            var localization = (LocalizationStrings)this.target;
            if (localization && localization.DefaultLanguage) {
                foreach (var (key, defaultString) in localization.DefaultLanguage) {
                    if (!string.IsNullOrWhiteSpace(defaultString)) {
                        writer.WriteLine("/// default value: {0}", defaultString);
                    }
                    
                    var name = key.Replace(" ", "")
                        .Replace(".", "");

                    writer.WriteLine("public const string @{0} = \"{1}\";", name, key);
                }
            }
            
            writer.WriteLine("}");

            if (hasNamespace) {
                writer.WriteLine("}");
            }

            AssetDatabase.Refresh();
        }
    }
}
