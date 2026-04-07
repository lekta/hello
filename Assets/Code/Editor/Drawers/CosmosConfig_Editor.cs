using System.Collections.Generic;
using LH.Cosmos;
using LH.Domain;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LH.Dev {
    [CustomEditor(typeof(CosmosConfig))]
    public class CosmosConfig_Editor : Editor {
        private const string HIDDENS_PROP = "Hiddens";

        private CosmosConfig Config => (CosmosConfig)target;

        private static Material _glMaterial;

        private readonly List<StarData> _cachedStars = new();
        private int _cachedSeed;
        private int _cachedCount;
        private float _cachedRadius;

        private readonly HashSet<int> _existingIdsCache = new(16);

        private static bool _showColorsGizmo;
        private static bool _editHiddenPositions;

        private SerializedProperty _hiddensProp;
        private ReorderableList _hiddensList;

        private int _selectedHiddenIndex = -1;
        private bool _hiddensExpanded = true;

        private List<ConfigIssue> _inlineIssues = new();


        private void OnEnable() {
            SceneView.duringSceneGui += OnSceneGUI;
            InitHiddens();
        }

        private void OnDisable() {
            SceneView.duringSceneGui -= OnSceneGUI;
            _hiddensProp = null;
        }

        private void InitHiddens() {
            _hiddensProp = serializedObject.FindProperty(HIDDENS_PROP);
            _hiddensList = new ReorderableList(serializedObject, _hiddensProp, true, false, true, true);

            _hiddensList.elementHeightCallback = index => {
                if (index >= _hiddensProp.arraySize)
                    return EditorGUIUtility.singleLineHeight;
                var elem = _hiddensProp.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(elem, true) + 4f;
            };

            _hiddensList.drawElementCallback = GuiHiddenElement;

            _hiddensList.onSelectCallback = list => {
                // DO: работает только если в фокус попал сам элемент, а не его поле
                _selectedHiddenIndex = list.index;
                SceneView.RepaintAll();
            };

            _hiddensList.onAddCallback = list => {
                _hiddensProp.arraySize++;
                serializedObject.ApplyModifiedProperties();

                var entry = Config.Hiddens[^1];
                entry.ResetToDefaults();
                entry.Behaviors.Add(new TremorBehavior());
                EditorUtility.SetDirty(Config);
            };
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "m_Script", HIDDENS_PROP);

            GuiHiddens();
            GuiEditorTools();

            CheckHiddenIds();
            ClampHiddenValues();

            if (GUI.changed) {
                serializedObject.ApplyModifiedProperties();
                SceneView.RepaintAll();
            }

            GuiInlineValidation();
        }

        private void GuiInlineValidation() {
            _inlineIssues = ConfigValidation.ValidateCosmos(Config);
            if (_inlineIssues.Count == 0)
                return;

            GUILayout.Space(6);
            DevGui.HorizontalLine();
            EditorGUILayout.LabelField($"Validation ({_inlineIssues.Count})", EditorStyles.boldLabel);

            foreach (var issue in _inlineIssues) {
                var msgType = issue.Severity == IssueSeverity.Error ? MessageType.Error : MessageType.Warning;
                string path = string.IsNullOrEmpty(issue.PropertyPath) ? "" : $"  [{issue.PropertyPath}]";

                using (new GUILayout.HorizontalScope()) {
                    EditorGUILayout.HelpBox($"{path} {issue.Message}", msgType);

                    if (issue.HasAutoFix && GUILayout.Button("Fix", GUILayout.Width(40))) {
                        issue.AutoFix.Invoke();
                        serializedObject.Update();
                    }
                }
            }
        }

        private void GuiHiddens() {
            var hiddensProp = serializedObject.FindProperty(HIDDENS_PROP);

            _hiddensExpanded = EditorGUILayout.Foldout(_hiddensExpanded, $"Hiddens ({hiddensProp.arraySize})", true);
            if (!_hiddensExpanded) {
                return;
            }
            _hiddensList.DoLayoutList();
        }

        private void GuiHiddenElement(Rect rect, int index, bool isActive, bool isFocused) {
            if (index >= _hiddensProp.arraySize)
                return;

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition) && _selectedHiddenIndex != index) {
                _selectedHiddenIndex = index;
                SceneView.RepaintAll();
            }

            using var indent = new EditorGUI.IndentLevelScope();

            var elem = _hiddensProp.GetArrayElementAtIndex(index);
            rect.y += 2f;
            rect.height -= 4f;

            EditorGUI.PropertyField(rect, elem, DevGui.GetContent(Config.Hiddens[index].GetShortInfo()), true);
        }

        private void GuiEditorTools() {
            GUILayout.Space(6);
            GUILayout.Label("  Editor");

            _showColorsGizmo = EditorGUILayout.ToggleLeft("Show color zones", _showColorsGizmo);
            _editHiddenPositions = EditorGUILayout.ToggleLeft("Edit hidden positions", _editHiddenPositions);
        }

        private void CheckHiddenIds() {
            bool changed = false;
            var hiddens = Config.Hiddens;
            _existingIdsCache.Clear();

            for (int i = 0; i < hiddens.Count; i++) {
                bool needReset = hiddens[i].Id == 0 || _existingIdsCache.Contains(hiddens[i].Id);
                if (needReset) {
                    hiddens[i].ResetToDefaults();
                    hiddens[i].Behaviors.Add(new TremorBehavior());
                    changed = true;

                    do {
                        hiddens[i].Id = Random.Range(1000, 999999);
                    } while (_existingIdsCache.Contains(hiddens[i].Id));
                }
                _existingIdsCache.Add(hiddens[i].Id);
            }

            if (changed) {
                EditorUtility.SetDirty(Config);
            }
        }

        private void ClampHiddenValues() {
            bool changed = false;
            float fieldRadius = Config.FieldRadius;

            foreach (var h in Config.Hiddens) {
                if (h.Radius < 10f) {
                    h.Radius = 10f;
                    changed = true;
                }

                if (h.Position.magnitude + h.Radius > fieldRadius) {
                    float maxDist = fieldRadius - h.Radius;
                    if (maxDist < 0f) maxDist = 0f;
                    if (h.Position.magnitude > maxDist) {
                        h.Position = h.Position.magnitude < .01f ? Vector2.zero : h.Position.normalized * maxDist;
                        changed = true;
                    }
                }
            }

            if (changed)
                EditorUtility.SetDirty(Config);
        }


        private void OnSceneGUI(SceneView sceneView) {
            if (Application.isPlaying)
                return;

            var cfg = (CosmosConfig)target;
            if (cfg == null)
                return;

            if (Event.current.type == EventType.Repaint) {
                DrawFieldBoundary(cfg.FieldRadius);
                DrawColorZones(cfg.ColorZones);
                EnsureCachedStars(cfg);
                DrawStarsGL();
            }

            DrawHiddenObjects(cfg);
        }

        private static void DrawFieldBoundary(float radius) {
            Handles.color = new Color(.3f, .9f, .3f, .4f);
            Handles.DrawWireDisc(Vector3.zero, Vector3.forward, radius);
            Handles.Label(new Vector3(0f, radius + .5f, 0f), "Поле звёзд", EditorStyles.whiteLabel);
        }

        private void EnsureCachedStars(CosmosConfig cfg) {
            if (_cachedSeed == cfg.Seed
                && _cachedCount == cfg.StarCount
                && Mathf.Approximately(_cachedRadius, cfg.FieldRadius)
                && _cachedStars.Count == cfg.StarCount
               )
                return;

            _cachedSeed = cfg.Seed;
            _cachedCount = cfg.StarCount;
            _cachedRadius = cfg.FieldRadius;

            StarsManager.GenerateField(cfg.Seed, cfg.StarCount, cfg.FieldRadius, _cachedStars, cfg.StarsParams, cfg.ColorZones);
        }

        private void DrawStarsGL() {
            if (_cachedStars.Count == 0)
                return;

            var mat = GetGLMaterial();
            mat.SetPass(0);

            GL.PushMatrix();
            GL.MultMatrix(Handles.matrix);
            GL.Begin(GL.QUADS);

            for (int i = 0; i < _cachedStars.Count; i++) {
                var star = _cachedStars[i];
                float size = star.AnchorScale;

                float brightness = Mathf.Clamp01(size / 12f);
                GL.Color(star.Color.WithAlpha(.3f + brightness * .7f));

                float half = 5f + size * .25f;
                float x = star.AnchorPosition.x;
                float y = star.AnchorPosition.y;
                GL.Vertex3(x - half, y - half, 0f);
                GL.Vertex3(x + half, y - half, 0f);
                GL.Vertex3(x + half, y + half, 0f);
                GL.Vertex3(x - half, y + half, 0f);
            }

            GL.End();
            GL.PopMatrix();
        }

        private static void DrawColorZones(ColorZone[] zones) {
            if (zones.IsEmpty() || !_showColorsGizmo)
                return;

            for (int i = 0; i < zones.Length; i++) {
                var zone = zones[i];
                Vector3 pos = new Vector3(zone.Position.x, zone.Position.y, 0f);
                Color color = zone.Tint;

                Handles.color = color.WithAlpha(.1f);
                Handles.DrawSolidDisc(pos, Vector3.forward, zone.Radius);

                Handles.color = color.WithAlpha(.4f);
                Handles.DrawWireDisc(pos, Vector3.forward, zone.Radius);
                Handles.Label(pos, $"Color {i}, ({zone.Strength:P0})", EditorStyles.whiteLabel);
            }
        }

        private void SelectHiddenInInspector(int index) {
            _selectedHiddenIndex = index;
            _hiddensList.index = index;
            _hiddensExpanded = true;

            if (_hiddensProp != null && index >= 0 && index < _hiddensProp.arraySize)
                _hiddensProp.GetArrayElementAtIndex(index).isExpanded = true;

            Repaint();
        }

        private void DrawHiddenObjects(CosmosConfig cfg) {
            if (cfg.Hiddens.Count == 0)
                return;

            var lookup = new Dictionary<int, HiddenObjectData>();
            foreach (var h in cfg.Hiddens)
                lookup[h.Id] = h;

            // Клик по гизмо — выбор скрытого
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                int closest = -1;
                float closestDist = float.MaxValue;

                for (int i = 0; i < cfg.Hiddens.Count; i++) {
                    Vector3 pos3 = new Vector3(cfg.Hiddens[i].Position.x, cfg.Hiddens[i].Position.y, 0f);
                    float dist = HandleUtility.DistanceToCircle(pos3, cfg.Hiddens[i].Radius);
                    if (dist < closestDist) {
                        closestDist = dist;
                        closest = i;
                    }
                }

                if (closest >= 0 && closestDist < 10f && closest != _selectedHiddenIndex) {
                    SelectHiddenInInspector(closest);
                    if (!_editHiddenPositions) {
                        Event.current.Use();
                    }
                }
            }

            for (int i = 0; i < cfg.Hiddens.Count; i++) {
                var hidden = cfg.Hiddens[i];
                bool selected = i == _selectedHiddenIndex;

                if (_editHiddenPositions) {
                    EditorGUI.BeginChangeCheck();
                    Vector3 pos3 = new Vector3(hidden.Position.x, hidden.Position.y, 0f);
                    Vector3 newPos = Handles.PositionHandle(pos3, Quaternion.identity);

                    if (EditorGUI.EndChangeCheck()) {
                        SelectHiddenInInspector(i);
                        Undo.RecordObject(cfg, "Move Hidden Object");
                        Vector2 pos = new Vector2(newPos.x, newPos.y);
                        if (pos.magnitude > cfg.FieldRadius)
                            pos = pos.normalized * cfg.FieldRadius;
                        hidden.Position = pos;
                        EditorUtility.SetDirty(cfg);
                    }
                }

                if (Event.current.type == EventType.Repaint) {
                    CosmosGizmos.DrawHidden(hidden, selected);
                    CosmosGizmos.DrawDependencies(hidden, lookup);
                }
            }
        }

        private static Material GetGLMaterial() {
            if (_glMaterial != null)
                return _glMaterial;

            _glMaterial = new Material(Shader.Find("Hidden/Internal-Colored")) {
                hideFlags = HideFlags.HideAndDontSave
            };
            _glMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _glMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _glMaterial.SetInt("_ZWrite", 0);
            _glMaterial.SetInt("_Cull", 0);

            return _glMaterial;
        }
    }
}