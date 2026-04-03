using System;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

namespace LH.Dev {
    public class DevGuiScroll {
        private readonly GUILayoutOption[] _options;
        private Vector2 _viewPos;

        public DevGuiScroll(params GUILayoutOption[] options) {
            _options = options;
        }

        public void Draw([NotNull] Action innerFrame) {
            _viewPos = EditorGUILayout.BeginScrollView(_viewPos, _options);
            {
                try {
                    innerFrame.Invoke();
                } catch (Exception ex) {
                    if (ex is ExitGUIException) {
                        throw;
                    }
                    GUILayout.TextArea($"Exception:\n{ex.Message}\n{ex.StackTrace}", GUILayout.ExpandHeight(true));
                    Debug.LogException(ex);
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
}