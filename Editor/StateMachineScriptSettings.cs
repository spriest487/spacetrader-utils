using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace SpaceTrader.Util {
    public enum StateMachineScriptFallbackMode {
        None,
        Allow,
        Warn,
        Prompt,
    }

    [CreateAssetMenu(
        menuName = "SpaceTrader/Editor/StateMachine Script Settings",
        fileName = nameof(StateMachineScriptSettings)
    )]
    public class StateMachineScriptSettings : ScriptableObject {
        [Serializable]
        private class BuildReportCache {
            [field: SerializeField]
            public StateMachineScriptBuildReport[] BuildReports { get; set; }
        }

        private const string BuildReportCacheDir = "Temp/SpaceTrader/StateMachineScriptSettings/";
        private const string BuildReportCachePath = BuildReportCacheDir + "/BuildReport.json";

        private static Dictionary<string, StateMachineScriptBuildReport> buildReports;

        [field: SerializeField, HideInInspector]
        private string StateTypeName { get; set; }

        [BoxGroup("Settings")]
        [ShowInInspector, ValueDropdown("@StateMachineScriptGenerator.GetStateTypes()")]
        public Type StateType {
            get {
                if (!StateMachineScriptGenerator.GetStateType(this.StateTypeName, out var ty)) {
                    return null;
                }

                return ty;
            }
            set => this.StateTypeName = value?.Name ?? "";
        }

        [field: BoxGroup("Settings")]
        [field: SerializeField]
        private string[] internalNamespaceWhitelist;

        [field: BoxGroup("Output")]
        [field: SerializeField]
        public string GeneratedClassName { get; set; }

        [field: BoxGroup("Output")]
        [field: SerializeField, Sirenix.OdinInspector.FilePath]
        public string ScriptFilename { get; set; }

        [field: BoxGroup("Output")]
        [field: SerializeField]
        public string Namespace { get; set; }

        [field: BoxGroup("Reflection Fallbacks")]
        [field: SerializeField]
        public StateMachineScriptFallbackMode FallbackMode { get; set; } = StateMachineScriptFallbackMode.None;

        [field: BoxGroup("Reflection Fallbacks")]
        [field: SerializeField]
        public StateMachineScriptFallbackMode FallbackModeEditor { get; set; } = StateMachineScriptFallbackMode.Prompt;

        [field: BoxGroup("Profiling")]
        [field: SerializeField]
        public bool GenerateProfilerMarkers { get; set; }

        public ReadOnlySpan<string> InternalNamespaceWhitelist => this.internalNamespaceWhitelist;

        [UsedImplicitly]
        private bool IsConfigValid {
            get {
                return !string.IsNullOrWhiteSpace(this.ScriptFilename)
                    && !string.IsNullOrWhiteSpace(this.GeneratedClassName)
                    && this.StateType != null;
            }
        }

        [EnableGUI]
        [ShowInInspector, ShowIf("@HasBuildReport()")]
        [InfoBox("@GetBuildReportWarnings()", InfoMessageType.Warning, "@BuildReportHasWarnings()")]
        public StateMachineScriptBuildReport LastBuildReport => this.GetBuildReport();

        private StateMachineScriptBuildReport GetBuildReport() {
            if (buildReports == null) {
                LoadCachedBuildReports();
            }

            var assetPath = AssetDatabase.GetAssetPath(this);

            if (string.IsNullOrWhiteSpace(assetPath) || !buildReports!.TryGetValue(assetPath, out var buildReport)) {
                buildReport = new StateMachineScriptBuildReport { AssetPath = assetPath };
            }

            return buildReport;
        }

        private static void LoadCachedBuildReports() {
            try {
                var json = File.ReadAllText(BuildReportCachePath);
                var cached = JsonUtility.FromJson<BuildReportCache>(json);

                if (cached.BuildReports != null) {
                    buildReports = cached.BuildReports.ToDictionary(x => x.AssetPath);
                }
            } catch (Exception e) {
                if (e is not FileNotFoundException or DirectoryNotFoundException) {
                    Debug.LogWarningFormat(
                        "{0} - loading cached build reports failed: {1}",
                        nameof(StateMachineScriptSettings),
                        e
                    );
                }

                buildReports = new Dictionary<string, StateMachineScriptBuildReport>();
            }
        }

        private static void SaveBuildReports() {
            if (buildReports == null) {
                return;
            }

            try {
                var json = JsonUtility.ToJson(
                    new BuildReportCache {
                        BuildReports = buildReports.Values.ToArray(),
                    }
                );

                if (!Directory.Exists(BuildReportCacheDir)) {
                    Directory.CreateDirectory(BuildReportCacheDir);
                }

                File.WriteAllText(BuildReportCachePath, json);
            } catch (Exception e) {
                Debug.LogWarningFormat(
                    "{0} - saving build report cache failed: {1}",
                    nameof(StateMachineScriptSettings),
                    e
                );
            }
        }

        [UsedImplicitly]
        private bool HasBuildReport() {
            var rules = this.GetBuildReport().Rules;
            return rules is { Length: > 0 };
        }

        [UsedImplicitly]
        private bool BuildReportHasWarnings() {
            if (!this.HasBuildReport()) {
                return false;
            }

            foreach (var rule in this.GetBuildReport().Rules) {
                if (rule.RuleMethodReflected || rule.ConditionReflected) {
                    return true;
                }
            }

            return false;
        }

        [UsedImplicitly]
        private string GetBuildReportWarnings() {
            var warnings = new StringBuilder();

            var rules = this.GetBuildReport().Rules;
            foreach (var rule in rules) {
                if (rule.RuleMethodReflected || rule.ConditionReflected) {
                    warnings.AppendLine($"{rule.Name} references inaccessible members and is using reflection");
                }
            }

            return warnings.ToString();
        }

        [Button("Bake Script"), EnableIf("IsConfigValid")]
        public void BakeScript() {
            var script = new StringBuilder();
            var generator = new StateMachineScriptGenerator(this);

            generator.WriteScript(script);

            File.WriteAllText(this.ScriptFilename, script.ToString());

            AssetDatabase.ImportAsset(this.ScriptFilename);

            var assetPath = AssetDatabase.GetAssetPath(this);
            var buildReport = new StateMachineScriptBuildReport {
                AssetPath = assetPath,
                Rules = generator.BuildReportRules.ToArray(),
            };
            buildReports[assetPath] = buildReport;

            SaveBuildReports();
        }

        

        [UsedImplicitly]
        public static void PromptRegenerate(string className, string transitionName, string sourceAssetPath) {
            var asset = AssetDatabase.LoadAssetAtPath<StateMachineScriptSettings>(sourceAssetPath);
            if (!asset) {
                Debug.LogErrorFormat(
                    "{0} - failed to load script source for {1} asset at {2}",
                    nameof(StateMachineScriptSettings),
                    className,
                    sourceAssetPath
                );

                return;
            }

            var msg =
                $"The StateMachine script {className} encountered a fallback to a dynamic invocation for the transition {transitionName}. Please re-bake the script.";

            if (EditorUtility.DisplayDialog("Regenerate StateMachine Script", msg, "OK", "Skip")) {
                if (EditorApplication.isPlaying) {
                    EditorApplication.isPlaying = false;
                }

                asset.BakeScript();
            }
        }
    }
}
