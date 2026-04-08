using LH.Domain;
using UnityEditor;
using UnityEngine;

namespace LH.Dev {
    [CustomPropertyDrawer(typeof(ImprintSelectAttribute))]
    public class ImprintSelect_Drawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.propertyType != SerializedPropertyType.Integer) {
                EditorGUI.LabelField(position, label.text, "[ImprintSelect] requires int");
                return;
            }

            var imprints = ImprintUtils.GetAllImprints();
            if (imprints.Length == 0) {
                EditorGUI.LabelField(position, label.text, "No imprint configs found");
                return;
            }

            int currentId = property.intValue;
            int selectedIndex = -1;
            for (int i = 0; i < imprints.Length; i++) {
                if (imprints[i].Id == currentId) {
                    selectedIndex = i;
                    break;
                }
            }

            var options = new string[imprints.Length + 1];
            options[0] = "(None)";
            for (int i = 0; i < imprints.Length; i++)
                options[i + 1] = $"{imprints[i].name} (#{imprints[i].Id})";

            int popupIndex = selectedIndex >= 0 ? selectedIndex + 1 : 0;

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(position, label.text, popupIndex, options);
            if (EditorGUI.EndChangeCheck())
                property.intValue = newIndex == 0 ? 0 : imprints[newIndex - 1].Id;
        }
    }
}