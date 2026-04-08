using System.Collections.Generic;
using System.IO;
using System.Linq;
using LH.Domain;
using LH.Imprint;
using UnityEditor;

namespace LH.Dev {
    public static class ImprintUtils {
        private static string IMPRINTS_SCENES_FOLDER => ImprintPaths.IMPRINTS_SCENES_FOLDER;
        private static string IMPRINTS_CONFIG_PATH => ImprintPaths.IMPRINTS_CONFIG;

        private static ImprintsConfig _cachedImprintsConfig;

        
        public static ImprintConfig[] GetAllImprints() {
            var config = GetImprintsConfig();
            
            return config.Imprints.Select(i => i).NotNull().ToArray();
        }

        private static ImprintsConfig GetImprintsConfig() {
            if (_cachedImprintsConfig == null) {
                _cachedImprintsConfig = AssetDatabase.LoadAssetAtPath<ImprintsConfig>(IMPRINTS_CONFIG_PATH);
            }
            return _cachedImprintsConfig;
        }

        public static string[] GetAllScenesNames() {
            var guids = AssetDatabase.FindAssets("t:Scene", new[] { IMPRINTS_SCENES_FOLDER });
            var names = new string[guids.Length];
            for (int i = 0; i < guids.Length; i++)
                names[i] = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guids[i]));
            return names;
        }

        
        public static List<ConfigIssue> ValidateImprint(ImprintConfig cfg) {
            var issues = new List<ConfigIssue>();
            ValidateImprint(cfg, issues);
            return issues;
        }

        public static void ValidateImprint(ImprintConfig cfg, List<ConfigIssue> issues) {
            if (cfg.Id <= 0)
                issues.Add(new ConfigIssue { Message = "Id not set", Severity = IssueSeverity.Error, Asset = cfg, PropertyPath = "Id" });

            if (string.IsNullOrEmpty(cfg.SceneName))
                issues.Add(new ConfigIssue { Message = "Scene not assigned", Severity = IssueSeverity.Warning, Asset = cfg, PropertyPath = "SceneName" });
            else if (!GetAllScenesNames().Contains(cfg.SceneName))
                issues.Add(new ConfigIssue { Message = $"Scene '{cfg.SceneName}' not found in {IMPRINTS_SCENES_FOLDER}", Severity = IssueSeverity.Error, Asset = cfg, PropertyPath = "SceneName" });
        }

        public static void ValidateImprints(ImprintsConfig cfg, List<ConfigIssue> issues) {
            if (cfg.Imprints == null || cfg.Imprints.Count == 0) {
                issues.Add(new ConfigIssue { Message = "No imprints configured", Severity = IssueSeverity.Warning, Asset = cfg });
                return;
            }

            var idSet = new HashSet<int>();
            for (int i = 0; i < cfg.Imprints.Count; i++) {
                var imprint = cfg.Imprints[i];
                string prefix = $"Imprints[{i}]";

                if (imprint == null) {
                    issues.Add(new ConfigIssue { Message = $"{prefix}: null reference", Severity = IssueSeverity.Error, Asset = cfg, PropertyPath = prefix });
                    continue;
                }

                if (!idSet.Add(imprint.Id))
                    issues.Add(new ConfigIssue { Message = $"{prefix}: duplicate Id {imprint.Id}", Severity = IssueSeverity.Error, Asset = cfg, PropertyPath = prefix });

                ValidateImprint(imprint, issues);
            }
        }
    }
}