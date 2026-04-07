using System;
using System.Collections.Generic;
using System.Linq;
using LH.Domain;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LH.Dev {
    [CustomPropertyDrawer(typeof(SubtypeSelectAttribute))]
    public class SubtypeSelect_Drawer : PropertyDrawer {
        private const float DROPDOWN_HEIGHT = 18f;
        private const float PAD = 2f;

        private static readonly Dictionary<string, Type[]> _typeCache = new();
        private readonly Dictionary<string, ReorderableList> _listCache = new();


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.propertyType == SerializedPropertyType.ManagedReference) {
                DrawSinglePicker(position, property, label);
            } else if (property.isArray) {
                DrawListPicker(position, property, label);
            } else {
                EditorGUI.LabelField(position, label.text, "Use [TypePicker] with [SerializeReference]");
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (property.propertyType == SerializedPropertyType.ManagedReference)
                return GetSingleHeight(property);

            if (property.isArray)
                return GetOrCreateList(property, label).GetHeight();

            return DROPDOWN_HEIGHT;
        }


        // ---- Single SerializeReference field ----

        private void DrawSinglePicker(Rect position, SerializedProperty property, GUIContent label) {
            var types = GetAssignableTypes(property.managedReferenceFieldTypename);
            Type current = GetManagedReferenceType(property);

            int selectedIndex = current == null ? 0 : Array.IndexOf(types, current) + 1;
            string[] names = BuildDisplayNames(types);

            Rect dropdownRect = new(position.x, position.y, position.width, DROPDOWN_HEIGHT);

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(dropdownRect, label.text, selectedIndex, names);
            if (EditorGUI.EndChangeCheck()) {
                property.managedReferenceValue = newIndex == 0 ? null : Activator.CreateInstance(types[newIndex - 1]);
                property.serializedObject.ApplyModifiedProperties();
            }

            if (property.managedReferenceValue != null) {
                EditorGUI.indentLevel++;
                Rect childRect = new(position.x, position.y + DROPDOWN_HEIGHT + PAD, position.width, position.height - DROPDOWN_HEIGHT - PAD);
                DrawChildProperties(childRect, property);
                EditorGUI.indentLevel--;
            }
        }

        private static float GetSingleHeight(SerializedProperty property) {
            float height = DROPDOWN_HEIGHT;
            if (property.managedReferenceValue != null) {
                var iter = property.Copy();
                var end = iter.GetEndProperty();
                if (iter.NextVisible(true)) {
                    do {
                        if (SerializedProperty.EqualContents(iter, end))
                            break;
                        height += EditorGUI.GetPropertyHeight(iter, true) + PAD;
                    } while (iter.NextVisible(false));
                }
            }
            return height;
        }


        // ---- List of SerializeReference ----

        private void DrawListPicker(Rect position, SerializedProperty property, GUIContent label) {
            var list = GetOrCreateList(property, label);
            list.DoList(position);
        }

        private ReorderableList GetOrCreateList(SerializedProperty property, GUIContent label) {
            string key = property.propertyPath;
            if (_listCache.TryGetValue(key, out var existing) && existing.serializedProperty.serializedObject == property.serializedObject)
                return existing;

            string baseTypeName = null;
            if (property.arraySize > 0) {
                var elem = property.GetArrayElementAtIndex(0);
                if (elem.propertyType == SerializedPropertyType.ManagedReference)
                    baseTypeName = elem.managedReferenceFieldTypename;
            }

            var reorderableList = new ReorderableList(property.serializedObject, property, true, true, true, true);

            reorderableList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, label.text);
            };

            reorderableList.elementHeightCallback = index => {
                if (index >= property.arraySize)
                    return DROPDOWN_HEIGHT;
                var elem = property.GetArrayElementAtIndex(index);
                return GetSingleHeight(elem) + PAD;
            };

            reorderableList.drawElementCallback = (rect, index, isActive, isFocused) => {
                if (index >= property.arraySize)
                    return;
                var elem = property.GetArrayElementAtIndex(index);
                rect.y += 1f;
                rect.height -= PAD;
                DrawSinglePicker(rect, elem, new GUIContent($"[{index}]"));
            };

            string capturedBaseType = baseTypeName;
            reorderableList.onAddDropdownCallback = (buttonRect, list) => {
                string typeName = capturedBaseType;
                if (typeName == null && property.arraySize > 0)
                    typeName = property.GetArrayElementAtIndex(0).managedReferenceFieldTypename;

                // Если список пустой и тип не известен, добавляем null-элемент — тип определится из поля
                if (typeName == null) {
                    property.arraySize++;
                    property.serializedObject.ApplyModifiedProperties();
                    return;
                }

                var types = GetAssignableTypes(typeName);
                var menu = new GenericMenu();
                for (int i = 0; i < types.Length; i++) {
                    var t = types[i];
                    menu.AddItem(new GUIContent(t.Name), false, () => {
                        property.arraySize++;
                        property.serializedObject.ApplyModifiedProperties();
                        var newElem = property.GetArrayElementAtIndex(property.arraySize - 1);
                        newElem.managedReferenceValue = Activator.CreateInstance(t);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.ShowAsContext();
            };

            _listCache[key] = reorderableList;
            return reorderableList;
        }


        // ---- Shared helpers ----

        private static void DrawChildProperties(Rect position, SerializedProperty property) {
            var iter = property.Copy();
            var end = iter.GetEndProperty();
            float y = position.y;

            if (iter.NextVisible(true)) {
                do {
                    if (SerializedProperty.EqualContents(iter, end))
                        break;
                    float h = EditorGUI.GetPropertyHeight(iter, true);
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, h), iter, true);
                    y += h + PAD;
                } while (iter.NextVisible(false));
            }
        }

        private static Type[] GetAssignableTypes(string managedRefFieldTypename) {
            if (_typeCache.TryGetValue(managedRefFieldTypename, out var cached))
                return cached;

            Type baseType = GetTypeFromManagedRefTypename(managedRefFieldTypename);
            if (baseType == null) {
                _typeCache[managedRefFieldTypename] = Array.Empty<Type>();
                return _typeCache[managedRefFieldTypename];
            }

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => {
                    try {
                        return a.GetTypes();
                    } catch {
                        return Array.Empty<Type>();
                    }
                })
                .Where(t => !t.IsAbstract && !t.IsInterface && baseType.IsAssignableFrom(t))
                .OrderBy(t => t.Name)
                .ToArray();

            _typeCache[managedRefFieldTypename] = types;
            return types;
        }

        private static Type GetManagedReferenceType(SerializedProperty property) {
            string fullTypename = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(fullTypename))
                return null;
            return GetTypeFromManagedRefTypename(fullTypename);
        }

        private static Type GetTypeFromManagedRefTypename(string typename) {
            var parts = typename.Split(' ');
            if (parts.Length != 2)
                return null;
            return Type.GetType($"{parts[1]}, {parts[0]}");
        }

        private static string[] BuildDisplayNames(Type[] types) {
            var names = new string[types.Length + 1];
            names[0] = "(None)";
            for (int i = 0; i < types.Length; i++)
                names[i + 1] = types[i].Name;
            return names;
        }
    }
}