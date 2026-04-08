using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LH.Dev {
    public static partial class DevGui {
        private static readonly GUIContent _tempContent = new();
        public static string ProjectRootPath => Application.dataPath[..^7]; // минус длина "/Assets"


        public static GUIContent GetContentWarning(string label, string tooltip = null) {
            _tempContent.text = label;
            _tempContent.image = Icon.ConsoleWarningSmall.image;
            _tempContent.tooltip = tooltip ?? string.Empty;
            return _tempContent;
        }

        public static GUIContent GetContent(string label, Texture icon, string tooltip = null) {
            _tempContent.text = label;
            _tempContent.image = icon;
            _tempContent.tooltip = tooltip ?? string.Empty;
            return _tempContent;
        }

        public static GUIContent GetContent(string label, string tooltip = null) {
            _tempContent.text = label;
            _tempContent.image = null;
            _tempContent.tooltip = tooltip;
            return _tempContent;
        }

        public static GUIContent GetContent(Texture icon, string tooltip = null) {
            _tempContent.text = null;
            _tempContent.image = icon;
            _tempContent.tooltip = tooltip;
            return _tempContent;
        }

        public static void HorizontalLine(int height = 1, int padding = 0) {
            Rect rect = EditorGUILayout.GetControlRect(false, GUILayout.Height(padding + height));
            rect.height = height;
            rect.y += padding * 0.5f;
            EditorGUI.DrawRect(rect, new Color(.5f, .5f, .5f, 1));
        }

        /// <summary>
        /// Рисует блок инлайн-валидации. Вызывать в конце OnInspectorGUI.
        /// issues обновляются только при dirty=true (OnEnable / GUI.changed).
        /// </summary>
        public static void DrawInlineValidation(ref List<ConfigIssue> issues, ref bool dirty, Func<List<ConfigIssue>> validate, SerializedObject serializedObject = null) {
            if (dirty) {
                issues = validate();
                dirty = false;
            }

            if (issues == null || issues.Count == 0)
                return;

            GUILayout.Space(6);
            HorizontalLine();
            EditorGUILayout.LabelField($"Validation ({issues.Count})", EditorStyles.boldLabel);

            foreach (var issue in issues) {
                var msgType = issue.Severity == IssueSeverity.Error ? MessageType.Error : MessageType.Warning;
                string path = string.IsNullOrEmpty(issue.PropertyPath) ? "" : $"  [{issue.PropertyPath}]";

                using (new GUILayout.HorizontalScope()) {
                    EditorGUILayout.HelpBox($"{path} {issue.Message}", msgType);

                    if (issue.HasAutoFix && GUILayout.Button("Fix", GUILayout.Width(40))) {
                        issue.AutoFix.Invoke();
                        serializedObject?.Update();
                        dirty = true;
                    }
                }
            }
        }
    }
}