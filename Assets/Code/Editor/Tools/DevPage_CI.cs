using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LH.Dev {
    public class DevPage_CI : DevWindowPage {
        public override string Label => "CI";

        private List<ConfigIssue> _issues;

        public override void Gui() {
            if (GUILayout.Button("Validate Configs", GUILayout.Width(200)))
                _issues = ConfigValidation.ValidateAll();

            if (_issues == null) {
                EditorGUILayout.HelpBox("Press 'Validate Configs' to run checks", MessageType.Info);
                return;
            }

            GUILayout.Space(4);

            if (_issues.Count == 0) {
                EditorGUILayout.HelpBox("All configs valid", MessageType.Info);
                return;
            }

            int errors = 0, warnings = 0;
            foreach (var issue in _issues) {
                if (issue.Severity == IssueSeverity.Error) errors++;
                else warnings++;
            }
            EditorGUILayout.LabelField($"{_issues.Count} issues: {errors} errors, {warnings} warnings", EditorStyles.boldLabel);

            GUILayout.Space(4);

            foreach (var issue in _issues)
                DrawIssue(issue);
        }

        private static void DrawIssue(ConfigIssue issue) {
            var icon = issue.Severity == IssueSeverity.Error ? MessageType.Error : MessageType.Warning;
            string assetName = issue.Asset != null ? issue.Asset.name : "?";
            string path = string.IsNullOrEmpty(issue.PropertyPath) ? "" : $"  [{issue.PropertyPath}]";

            using (new GUILayout.HorizontalScope()) {
                EditorGUILayout.HelpBox($"{assetName}{path}\n{issue.Message}", icon);

                using (new GUILayout.VerticalScope(GUILayout.Width(60))) {
                    if (issue.Asset != null && GUILayout.Button("Select", GUILayout.Width(56)))
                        Selection.activeObject = issue.Asset;

                    if (issue.HasAutoFix && GUILayout.Button("Fix", GUILayout.Width(56))) {
                        issue.AutoFix.Invoke();
                        // Перезапускаем валидацию не отсюда — пользователь нажмёт Validate снова
                    }
                }
            }
        }
    }
}
