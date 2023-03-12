using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SpaceTrader.Util.EditorUtil {
    public class AutoLayersSaveProcessor : UnityEditor.AssetModificationProcessor {
        private static string SanitizeIdentifier(string name) {
            name = name.Trim();
            if (string.IsNullOrEmpty(name)) {
                return "";
            }

            var sanitized = new StringBuilder();
            var validChars = name.SkipWhile(c => !char.IsLetter(c) || c == '_')
                .Where(c => char.IsLetterOrDigit(c) || c == '_');

            foreach (var c in validChars) {
                sanitized.Append(c);
            }

            return sanitized.ToString();
        }

        private static IEnumerable<KeyValuePair<int, string>> ValidLayers() {
            for (var i = 0; i < 32; ++i) {
                var layerName = SanitizeIdentifier(LayerMask.LayerToName(i));
                if (layerName.Length > 0) {
                    yield return new KeyValuePair<int, string>(i, layerName);
                }
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> ValidTags() {
            foreach (var tag in InternalEditorUtility.tags) {
                var tagName = SanitizeIdentifier(tag);
                if (tagName.Length > 0) {
                    yield return new KeyValuePair<string, string>(tag, tagName);
                }
            }
        }

        public static void GenerateScript() {
            var saveFile = new FileInfo(Path.Combine(Application.dataPath, AutoLayersPrefs.OutputPath));
            saveFile.Directory?.Create();

            var layers = ValidLayers().ToList();

            var ns = AutoLayersPrefs.Namespace;

            using (var writer = saveFile.CreateText()) {
                writer.WriteLine("// ReSharper disable EnumUnderlyingTypeIsInt");
                writer.WriteLine("// ReSharper disable UnusedMember.Global");
                writer.WriteLine("// ReSharper disable RedundantVerbatimPrefix");
                
                if (!string.IsNullOrWhiteSpace(ns)) {
                    writer.WriteLine("namespace {0} {{", ns);
                }

                writer.WriteLine("\tpublic enum Layer : int {");
                foreach (var (layer, layerIdent) in layers) {
                    writer.WriteLine("\t\t@{0} = {1},", layerIdent, layer);
                }

                writer.WriteLine("\t}");
                writer.WriteLine();

                writer.WriteLine("\tpublic static class LayerExtensions {");

                writer.WriteLine("\t\tpublic static int Mask(this Layer layer) {");
                writer.WriteLine("\t\t\treturn 1 << (int)layer;");
                writer.WriteLine("\t\t}");
                writer.WriteLine();

                writer.WriteLine("\t\tpublic static string Name(this Layer @this) {");
                writer.WriteLine("\t\t\tswitch (@this) {");
                foreach (var (layer, layerIdent) in layers) {
                    var name = LayerMask.LayerToName(layer);
                    writer.WriteLine("\t\t\t\tcase Layer.@{0}: return \"{1}\";", layerIdent, name);
                }

                writer.WriteLine("\t\t\t\tdefault: return \"\";");
                writer.WriteLine("\t\t\t}");
                writer.WriteLine("\t\t}");
                writer.WriteLine("\t}");

                writer.WriteLine("\tpublic static class Tag {");
                foreach (var (tag, tagIdent) in ValidTags()) {
                    writer.WriteLine("\t\tpublic const string @{0} = \"{1}\";", tagIdent, tag);
                }

                writer.WriteLine("\t}");

                if (!string.IsNullOrWhiteSpace(ns)) {
                    writer.WriteLine("}");
                }
            }

            AssetDatabase.Refresh();
        }

        private static string[] OnWillSaveAssets(string[] paths) {
            if (paths.Contains("ProjectSettings/TagManager.asset")) {
                GenerateScript();
            }

            return paths;
        }
    }
}
