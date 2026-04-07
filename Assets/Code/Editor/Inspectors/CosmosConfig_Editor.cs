using System.Collections.Generic;
using System.Linq;
using LH.Cosmos;
using LH.Domain;
using UnityEditor;
using UnityEngine;

namespace LH.Dev {
    [CustomEditor(typeof(CosmosConfig))]
    public class CosmosConfig_Editor : Editor {
        private CosmosConfig Config => (CosmosConfig)target;

        private static Material _glMaterial;

        private readonly List<StarData> _cachedStars = new();
        private int _cachedSeed;
        private int _cachedCount;
        private float _cachedRadius;

        private HashSet<int> _existingIdsCache = new(16);

        private static bool _showColorsGizmo;
        private static bool _editHiddenPositions;


        private void OnEnable() {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable() {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawDefaultInspector();

            GUILayout.Space(6);
            GUILayout.Label("  Editor");

            _showColorsGizmo = EditorGUILayout.ToggleLeft("Show color zones", _showColorsGizmo);
            _editHiddenPositions = EditorGUILayout.ToggleLeft("Edit hidden positions", _editHiddenPositions);

            GUILayout.Space(6);
            CheckHiddenIds();

            if (GUI.changed) {
                serializedObject.ApplyModifiedProperties();
                SceneView.RepaintAll();
            }
        }

        private void CheckHiddenIds() {
            bool changed = false;
            var hiddens = Config.Hiddens;
            _existingIdsCache.Clear();

            for (int i = 0; i < hiddens.Count; i++) {
                while (hiddens[i].Id == 0 || _existingIdsCache.Contains(hiddens[i].Id)) {
                    hiddens[i].Id = Random.Range(1000, 999999);
                    changed = true;
                }
                _existingIdsCache.Add(hiddens[i].Id);
            }

            if (changed) {
                EditorUtility.SetDirty(Config);
            }
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
                Handles.Label(pos, $"Zone #{i} ({zone.Strength:P0})", EditorStyles.whiteBoldLabel);
            }
        }

        private void DrawHiddenObjects(CosmosConfig cfg) {
            if (cfg.Hiddens.Count == 0)
                return;

            var lookup = new Dictionary<int, HiddenObjectData>();
            foreach (var h in cfg.Hiddens)
                lookup[h.Id] = h;

            for (int i = 0; i < cfg.Hiddens.Count; i++) {
                var hidden = cfg.Hiddens[i];

                if (_editHiddenPositions) {
                    EditorGUI.BeginChangeCheck();
                    Vector3 pos3 = new Vector3(hidden.Position.x, hidden.Position.y, 0f);
                    Vector3 newPos = Handles.PositionHandle(pos3, Quaternion.identity);

                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RecordObject(cfg, "Move Hidden Object");
                        hidden.Position = new Vector2(newPos.x, newPos.y);
                        EditorUtility.SetDirty(cfg);
                    }
                }

                if (Event.current.type == EventType.Repaint) {
                    CosmosGizmos.DrawHidden(hidden);
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