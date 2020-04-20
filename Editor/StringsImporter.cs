using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.AssetImporters;

namespace SpaceTrader.Util.EditorUtil {
    [ScriptedImporter(1, "ini")]
    public class StringsImporter : ScriptedImporter {
        [OnOpenAsset]
        private static bool HandleOpen(int instanceID, int line) {
            var strings = EditorUtility.InstanceIDToObject(instanceID) as Strings;
            if (strings == null) {
                return false;
            }

            var stringsPath = AssetDatabase.GetAssetPath(strings);
            if (string.IsNullOrEmpty(stringsPath)) {
                return false;
            }

            EditorUtility.OpenWithDefaultApp(stringsPath);
            return true;
        }

        private string ReadEntry(StreamReader stream) {
            StringBuilder entryString = null;

            bool newlineEscaped;
            string line;
            do {
                line = stream.ReadLine();
                if (line == null) {
                    break;
                }

                newlineEscaped = line.EndsWith("\\");

                if (newlineEscaped) {
                    line = line.Substring(0, line.Length - 1);
                }

                if (entryString == null) {
                    entryString = new StringBuilder(line);
                } else if (line.All(char.IsWhiteSpace)) {
                    entryString.AppendLine();
                } else {
                    entryString.Append(line);
                }
            } while (newlineEscaped);

            return entryString?.ToString();
        }

        public override void OnImportAsset(AssetImportContext ctx) {
            var stringsMap = new Dictionary<string, string>();

            try {
                using (var stringsFile = File.OpenText(ctx.assetPath)) {
                    string entry;
                    string category = null;

                    while ((entry = this.ReadEntry(stringsFile)) != null) {
                        entry = entry.TrimEnd();

                        if (entry.StartsWith("[") && entry.EndsWith("]")) {
                            category = entry.Substring(1, entry.Length - 2);
                            continue;
                        }

                        // comments can be escaped with \;
                        var commentPos = entry.IndexOf(';');
                        if (commentPos > 0 && entry[commentPos - 1] != '\\') {
                            commentPos = -1;
                        }

                        if (commentPos >= 0) {
                            entry = entry.Substring(0, commentPos);
                        }

                        // ignore whitespace or completely commented lines
                        if (entry.All(char.IsWhiteSpace)) {
                            continue;
                        }

                        var splitPos = entry.IndexOf('=');
                        if (splitPos == -1) {
                            ctx.LogImportWarning($"missing delimited in line '{entry}'");
                            continue;
                        }

                        var key = entry.Substring(0, splitPos);
                        var value = entry.Substring(splitPos + 1).Trim();

                        if (category != null) {
                            key = $"{category}.{key}";
                        }

                        if (stringsMap.ContainsKey(key)) {
                            ctx.LogImportWarning($"duplicate key '{key}'");
                        }

                        stringsMap.Add(key, value);
                    }
                }
            } catch (Exception e) {
                stringsMap.Clear();
                ctx.LogImportError(e.ToString());
            }

            var strings = Strings.Create(stringsMap);

            ctx.AddObjectToAsset(Path.GetFileName(ctx.assetPath), strings);
        }
    }
}