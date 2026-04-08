using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LH.Cosmos;
using UnityEditor;
using UnityEngine;

namespace LH.Dev {
    [CustomPropertyDrawer(typeof(HiddenObjectData))]
    public class HiddenObjectData_Drawer : PropertyDrawer {
        private const string LOCKS_FIELD = "Locks";

        private const float PAD = 2f;
        private const float LINE = 20f;
        private const float REMOVE_BTN_WIDTH = 20f;


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            float height = EditorGUIUtility.singleLineHeight; // foldout

            var iter = property.Copy();
            var end = iter.GetEndProperty();
            if (iter.NextVisible(true)) {
                do {
                    if (SerializedProperty.EqualContents(iter, end))
                        break;

                    if (iter.name == LOCKS_FIELD) {
                        height += GetLocksHeight(iter) + PAD;
                    } else {
                        height += EditorGUI.GetPropertyHeight(iter, true) + PAD;
                    }
                } while (iter.NextVisible(false));
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Rect foldoutRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (!property.isExpanded)
                return;

            EditorGUI.indentLevel++;
            float y = position.y + EditorGUIUtility.singleLineHeight;

            var iter = property.Copy();
            var end = iter.GetEndProperty();
            if (iter.NextVisible(true)) {
                do {
                    if (SerializedProperty.EqualContents(iter, end))
                        break;

                    if (iter.name == LOCKS_FIELD) {
                        float h = GetLocksHeight(iter);
                        DrawLocks(new Rect(position.x, y, position.width, h), iter, property);
                        y += h + PAD;
                    } else {
                        float h = EditorGUI.GetPropertyHeight(iter, true);
                        EditorGUI.PropertyField(new Rect(position.x, y, position.width, h), iter, true);
                        y += h + PAD;
                    }
                } while (iter.NextVisible(false));
            }

            EditorGUI.indentLevel--;
        }


        // ---- Locks UI ----

        private static float GetLocksHeight(SerializedProperty locksProp) {
            if (!locksProp.isExpanded)
                return LINE;
            return LINE + locksProp.arraySize * (LINE + PAD) + LINE + PAD;
        }

        private static void DrawLocks(Rect position, SerializedProperty locksProp, SerializedProperty parentProp) {
            var config = locksProp.serializedObject.targetObject as CosmosConfig;
            int hiddenIndex = ParseHiddenIndex(parentProp.propertyPath);

            if (config == null || hiddenIndex < 0 || hiddenIndex >= config.Hiddens.Count) {
                EditorGUI.PropertyField(position, locksProp, true);
                return;
            }

            var currentHidden = config.Hiddens[hiddenIndex];

            Rect foldoutRect = new(position.x, position.y, position.width, LINE);
            locksProp.isExpanded = EditorGUI.Foldout(foldoutRect, locksProp.isExpanded, $"Locks ({locksProp.arraySize})", true);

            if (!locksProp.isExpanded)
                return;

            float indentPx = EditorGUI.indentLevel * 15f;
            float listX = position.x + indentPx;
            float listW = position.width - indentPx;

            float listContentH = locksProp.arraySize * (LINE + PAD) + LINE + PAD;
            Rect bgRect = new(listX, position.y + LINE, listW, listContentH);
            EditorGUI.DrawRect(bgRect, new Color(0f, 0f, 0f, 0.08f));

            float y = position.y + LINE;
            bool mouseOnAnyItem = false;

            for (int i = 0; i < locksProp.arraySize; i++) {
                var lck = currentHidden.Locks != null && i < currentHidden.Locks.Count ? currentHidden.Locks[i] : null;

                Rect rowRect = new(listX, y, listW, LINE);
                Rect labelRect = new(listX, y, listW - REMOVE_BTN_WIDTH - PAD, LINE);
                Rect removeRect = new(listX + listW - REMOVE_BTN_WIDTH, y, REMOVE_BTN_WIDTH, LINE);

                if (i % 2 == 1)
                    EditorGUI.DrawRect(rowRect, new Color(0f, 0f, 0f, 0.05f));

                int highlightId = lck is HiddenLock hl ? hl.HiddenId : -1;
                bool hovered = rowRect.Contains(Event.current.mousePosition);
                if (hovered && highlightId >= 0) {
                    mouseOnAnyItem = true;
                    if (CosmosGizmos.HighlightedHiddenId != highlightId) {
                        CosmosGizmos.HighlightedHiddenId = highlightId;
                        SceneView.RepaintAll();
                    }
                    EditorGUI.DrawRect(rowRect, new Color(1f, 1f, 0.2f, 0.12f));
                }

                EditorGUI.LabelField(labelRect, GetLockLabel(config, lck));

                if (GUI.Button(removeRect, "\u00d7")) {
                    locksProp.DeleteArrayElementAtIndex(i);
                    locksProp.serializedObject.ApplyModifiedProperties();
                    break;
                }

                y += LINE + PAD;
            }

            if (!mouseOnAnyItem && CosmosGizmos.HighlightedHiddenId != -1) {
                CosmosGizmos.HighlightedHiddenId = -1;
                SceneView.RepaintAll();
            }

            Rect addRect = new(listX, y, listW, LINE);

            // DO: сделать как у нормальных списков через "+" справа
            if (GUI.Button(addRect, "Add Lock (Hidden)")) {
                var available = GetAvailableHiddens(currentHidden, config.Hiddens);
                PopupWindow.Show(addRect, new HiddenPickerPopup(available, id => {
                    locksProp.arraySize++;
                    locksProp.serializedObject.ApplyModifiedProperties();
                    currentHidden.Locks ??= new();
                    currentHidden.Locks[currentHidden.Locks.Count - 1] = new HiddenLock { HiddenId = id };
                    EditorUtility.SetDirty(config);
                }));
            }
        }


        // ---- Helpers ----

        private static int ParseHiddenIndex(string propertyPath) {
            // DO: регекс закомпилить
            var match = Regex.Match(propertyPath, @"Hiddens\.Array\.data\[(\d+)\]");
            return match.Success ? int.Parse(match.Groups[1].Value) : -1;
        }

        private static string GetLockLabel(CosmosConfig config, ILockCondition lck) {
            if (lck == null)
                return "(null lock)";

            if (lck is HiddenLock hl) {
                foreach (var h in config.Hiddens) {
                    if (h.Id == hl.HiddenId) {
                        string info = h.Content != null ? h.Content.ToString() : "empty";
                        return $"H#{hl.HiddenId} - {info}";
                    }
                }
                return $"H#{hl.HiddenId} - (missing!)";
            }

            return lck.ToString();
        }

        private static List<HiddenObjectData> GetAvailableHiddens(HiddenObjectData current, List<HiddenObjectData> all) {
            var forbidden = GetForbiddenIds(current, all);
            var available = new List<HiddenObjectData>();
            foreach (var h in all)
                if (!forbidden.Contains(h.Id))
                    available.Add(h);
            return available;
        }

        /// Собирает запрещённые id: текущий + уже добавленные + все транзитивно зависящие от текущего (иначе цикл)
        private static HashSet<int> GetForbiddenIds(HiddenObjectData current, List<HiddenObjectData> all) {
            var forbidden = new HashSet<int> { current.Id };

            if (current.Locks != null)
                foreach (var lck in current.Locks)
                    if (lck is HiddenLock hl)
                        forbidden.Add(hl.HiddenId);

            // BFS по обратным рёбрам: все X где X -> ... -> current.Id
            var queue = new Queue<int>();
            queue.Enqueue(current.Id);
            while (queue.Count > 0) {
                int id = queue.Dequeue();
                foreach (var h in all) {
                    if (h.Locks == null) continue;
                    foreach (var lck in h.Locks) {
                        if (lck is HiddenLock hl && hl.HiddenId == id && forbidden.Add(h.Id))
                            queue.Enqueue(h.Id);
                    }
                }
            }

            return forbidden;
        }
    }


    public class HiddenPickerPopup : PopupWindowContent {
        private const float ROW_HEIGHT = 22f;
        private const float WIDTH = 280f;
        private const float MAX_VISIBLE = 10f;

        private readonly List<HiddenObjectData> _available;
        private readonly Action<int> _onSelect;
        private Vector2 _scroll;

        public HiddenPickerPopup(List<HiddenObjectData> available, Action<int> onSelect) {
            _available = available;
            _onSelect = onSelect;
        }

        public override Vector2 GetWindowSize() {
            if (_available.Count == 0)
                return new Vector2(WIDTH, ROW_HEIGHT);
            return new Vector2(WIDTH, Mathf.Min(_available.Count * ROW_HEIGHT, ROW_HEIGHT * MAX_VISIBLE));
        }

        public override void OnOpen() {
            editorWindow.wantsMouseMove = true;
        }

        public override void OnGUI(Rect rect) {
            if (_available.Count == 0) {
                EditorGUILayout.LabelField("Нет доступных скрытых");
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            int newHighlight = -1;
            for (int i = 0; i < _available.Count; i++) {
                var h = _available[i];
                Rect rowRect = EditorGUILayout.GetControlRect(false, ROW_HEIGHT);
                bool isHovered = rowRect.Contains(Event.current.mousePosition);

                if (isHovered) {
                    newHighlight = h.Id;
                    EditorGUI.DrawRect(rowRect, new Color(0.3f, 0.5f, 0.8f, 0.3f));
                }

                string info = h.Content != null ? h.Content.ToString() : "empty";
                if (GUI.Button(rowRect, $"  H#{h.Id}  —  {info}", EditorStyles.label)) {
                    _onSelect(h.Id);
                    editorWindow.Close();
                    return;
                }
            }

            EditorGUILayout.EndScrollView();

            if (newHighlight != CosmosGizmos.HighlightedHiddenId) {
                CosmosGizmos.HighlightedHiddenId = newHighlight;
                SceneView.RepaintAll();
            }

            if (Event.current.type == EventType.MouseMove)
                editorWindow.Repaint();
        }

        public override void OnClose() {
            CosmosGizmos.HighlightedHiddenId = -1;
            SceneView.RepaintAll();
        }
    }
}
