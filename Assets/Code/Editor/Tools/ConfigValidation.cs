using System.Collections.Generic;
using System.Linq;
using LH.Cosmos;
using LH.Imprint;
using UnityEditor;
using Object = UnityEngine.Object;

namespace LH.Dev {
    public static class ConfigValidation {
        public static List<ConfigIssue> ValidateAll() {
            var issues = new List<ConfigIssue>();

            foreach (var cosmos in FindAllAssets<CosmosConfig>())
                ValidateCosmos(cosmos, issues);

            foreach (var camera in FindAllAssets<CameraConfig>())
                ValidateCamera(camera, issues);

            foreach (var imprints in FindAllAssets<ImprintsConfig>())
                ImprintUtils.ValidateImprints(imprints, issues);

            return issues;
        }

        public static List<ConfigIssue> ValidateCosmos(CosmosConfig cfg) {
            var issues = new List<ConfigIssue>();
            ValidateCosmos(cfg, issues);
            return issues;
        }


        private static void ValidateCosmos(CosmosConfig cfg, List<ConfigIssue> issues) {
            if (cfg.Star == null)
                issues.Add(new ConfigIssue { Message = "Star prefab not set", Severity = IssueSeverity.Error, Asset = cfg, PropertyPath = "Star" });

            if (cfg.HiddenObject == null)
                issues.Add(new ConfigIssue { Message = "HiddenObject prefab not set", Severity = IssueSeverity.Error, Asset = cfg, PropertyPath = "HiddenObject" });

            if (cfg.StarCount <= 0)
                issues.Add(new ConfigIssue { Message = "StarCount <= 0", Severity = IssueSeverity.Error, Asset = cfg, PropertyPath = "StarCount",
                    AutoFix = () => AutofixSetField(cfg, "StarCount", 600) });

            if (cfg.FieldRadius <= 0f)
                issues.Add(new ConfigIssue { Message = "FieldRadius <= 0", Severity = IssueSeverity.Error, Asset = cfg, PropertyPath = "FieldRadius",
                    AutoFix = () => AutofixSetField(cfg, "FieldRadius", 3000f) });

            var idSet = new HashSet<int>();
            for (int i = 0; i < cfg.Hiddens.Count; i++) {
                var h = cfg.Hiddens[i];
                string prefix = $"Hiddens[{i}]";

                if (!idSet.Add(h.Id))
                    issues.Add(new ConfigIssue { Message = $"Hidden #{h.Id}: duplicate ID", Severity = IssueSeverity.Error, Asset = cfg, PropertyPath = prefix });

                if (h.Behaviors == null || h.Behaviors.Count == 0)
                    issues.Add(new ConfigIssue { Message = $"Hidden #{h.Id}: no behaviors", Severity = IssueSeverity.Warning, Asset = cfg, PropertyPath = $"{prefix}.Behaviors",
                        AutoFix = () => AutofixAddDefaultBehavior(cfg, i) });

                if (h.Content == null)
                    issues.Add(new ConfigIssue { Message = $"Hidden #{h.Id}: no content", Severity = IssueSeverity.Warning, Asset = cfg, PropertyPath = $"{prefix}.Content" });

                if (h.Dependencies != null) {
                    for (int d = 0; d < h.Dependencies.Count; d++) {
                        int depId = h.Dependencies[d];
                        if (!cfg.Hiddens.Any(other => other.Id == depId))
                            issues.Add(new ConfigIssue { Message = $"Hidden #{h.Id}: dependency {depId} not found", Severity = IssueSeverity.Error, Asset = cfg, PropertyPath = $"{prefix}.Dependencies",
                                AutoFix = () => AutofixRemoveBadDependency(cfg, i, depId) });
                    }
                }
            }
        }

        private static void ValidateCamera(CameraConfig cfg, List<ConfigIssue> issues) {
            if (cfg.Move.DeadZone >= cfg.Move.MaxSpeedZone)
                issues.Add(new ConfigIssue { Message = "DeadZone >= MaxSpeedZone", Severity = IssueSeverity.Warning, Asset = cfg, PropertyPath = "Move.DeadZone" });
        }


        // ---- Autofixes ----

        private static void AutofixSetField(Object asset, string fieldName, object value) {
            Undo.RecordObject(asset, $"Autofix {fieldName}");
            var field = asset.GetType().GetField(fieldName);
            if (field != null)
                field.SetValue(asset, value);
            EditorUtility.SetDirty(asset);
        }

        private static void AutofixAddDefaultBehavior(CosmosConfig cfg, int hiddenIndex) {
            if (hiddenIndex >= cfg.Hiddens.Count) return;
            Undo.RecordObject(cfg, "Autofix add default behavior");
            cfg.Hiddens[hiddenIndex].Behaviors ??= new();
            cfg.Hiddens[hiddenIndex].Behaviors.Add(new TremorBehavior());
            EditorUtility.SetDirty(cfg);
        }

        private static void AutofixRemoveBadDependency(CosmosConfig cfg, int hiddenIndex, int badDepId) {
            if (hiddenIndex >= cfg.Hiddens.Count) return;
            Undo.RecordObject(cfg, "Autofix remove bad dependency");
            cfg.Hiddens[hiddenIndex].Dependencies?.Remove(badDepId);
            EditorUtility.SetDirty(cfg);
        }


        private static T[] FindAllAssets<T>() where T : Object {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(a => a != null)
                .ToArray();
        }
    }
}
