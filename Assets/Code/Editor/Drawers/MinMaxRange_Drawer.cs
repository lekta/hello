using LH.Domain;
using UnityEditor;
using UnityEngine;

namespace LH.Dev {
    [CustomPropertyDrawer(typeof(MinMaxRangeAttribute))]
    public class MinMaxRange_Drawer : PropertyDrawer {
        private const float FIELD_WIDTH = 48f;
        private const float PAD = 4f;

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {
            var range = (MinMaxRangeAttribute)attribute;

            if (prop.propertyType != SerializedPropertyType.Vector2) {
                EditorGUI.LabelField(pos, label.text, "Use with Vector2");
                return;
            }

            float min = prop.vector2Value.x;
            float max = prop.vector2Value.y;

            Rect labelRect = new(pos.x, pos.y, EditorGUIUtility.labelWidth, pos.height);
            Rect minRect = new(labelRect.xMax, pos.y, FIELD_WIDTH, pos.height);
            Rect sliderRect = new(minRect.xMax + PAD, pos.y, pos.width - EditorGUIUtility.labelWidth - FIELD_WIDTH * 2 - PAD * 2, pos.height);
            Rect maxRect = new(sliderRect.xMax + PAD, pos.y, FIELD_WIDTH, pos.height);

            EditorGUI.LabelField(labelRect, label);

            EditorGUI.BeginChangeCheck();
            min = EditorGUI.FloatField(minRect, min);
            EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, range.Min, range.Max);
            max = EditorGUI.FloatField(maxRect, max);

            if (EditorGUI.EndChangeCheck()) {
                min = Mathf.Clamp(min, range.Min, max);
                max = Mathf.Clamp(max, min, range.Max);
                prop.vector2Value = new Vector2(min, max);
            }
        }
    }
}
