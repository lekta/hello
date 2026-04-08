using LH.Domain;
using UnityEditor;
using UnityEngine;

namespace LH.Dev {
    [CustomPropertyDrawer(typeof(ImprintSceneSelectAttribute))]
    public class ImprintSceneSelect_Drawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.propertyType != SerializedPropertyType.String) {
                EditorGUI.LabelField(position, label.text, "[SceneDropdown] requires string");
                return;
            }

            var scenes = ImprintUtils.GetAllScenesNames();
            if (scenes.Length == 0) {
                EditorGUI.LabelField(position, label.text, "No scenes in Imprints folder");
                return;
            }

            string current = property.stringValue;
            int selectedIndex = System.Array.IndexOf(scenes, current);

            var options = new string[scenes.Length + 1];
            options[0] = "(None)";
            for (int i = 0; i < scenes.Length; i++)
                options[i + 1] = scenes[i];

            int popupIndex = selectedIndex >= 0 ? selectedIndex + 1 : 0;

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(position, label.text, popupIndex, options);
            if (EditorGUI.EndChangeCheck())
                property.stringValue = newIndex == 0 ? "" : options[newIndex];
        }
    }
}
