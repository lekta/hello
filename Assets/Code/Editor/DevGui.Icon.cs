using UnityEditor;
using UnityEngine;

namespace LH.Dev {
    public static partial class DevGui {
        public static class Icon {
            public static GUIContent ScriptableObject => EditorGUIUtility.IconContent("d_ScriptableObject Icon");
            public static GUIContent ConsoleWarningSmall => EditorGUIUtility.IconContent("console.warnicon.sml");

            public static GUIContent Select => EditorGUIUtility.IconContent("Animation.FilterBySelection");
            public static GUIContent MoveTool => EditorGUIUtility.IconContent("Grid.MoveTool");
            
            public static GUIContent LockOpen => EditorGUIUtility.IconContent("LockIcon");
            public static GUIContent LockClosed => EditorGUIUtility.IconContent("LockIcon-On");
            public static GUIContent EyeOn => EditorGUIUtility.IconContent("animationvisibilitytoggleon");
            public static GUIContent EyeOff => EditorGUIUtility.IconContent("animationvisibilitytoggleoff");
        }
    }
}