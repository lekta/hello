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

    }
}