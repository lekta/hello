using System.Collections.Generic;
using LH.Cosmos;
using UnityEditor;
using UnityEngine;

namespace LH.Dev {
    [CustomEditor(typeof(CosmosConfig))]
    public class CosmosConfig_Editor : Editor {
        private static Material _glMaterial;

        private readonly List<CosmicBodyData> _cachedStars = new();
        private readonly List<HiddenObjectData> _cachedHidden = new();
        private int _cachedSeed;
        private int _cachedCount;
        private float _cachedRadius;
        private int _cachedHiddenCount;


        private void OnEnable() {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable() {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            if (GUI.changed)
                SceneView.RepaintAll();
        }

        private void OnSceneGUI(SceneView sceneView) {
            if (Application.isPlaying || Event.current.type != EventType.Repaint)
                return;

            var cfg = (CosmosConfig)target;
            if (cfg == null)
                return;

            DrawFieldBoundary(cfg.FieldRadius);
            EnsureCachedStars(cfg);
            DrawStarsGL();
            DrawHiddenObjects();
        }

        private static void DrawFieldBoundary(float radius) {
            Handles.color = new Color(0.3f, 0.9f, 0.3f, 0.4f);
            Handles.DrawWireDisc(Vector3.zero, Vector3.forward, radius);
            Handles.Label(new Vector3(0f, radius + 0.5f, 0f), "Поле звёзд", EditorStyles.whiteLabel);
        }

        private void EnsureCachedStars(CosmosConfig cfg) {
            if (_cachedSeed == cfg.Seed
                && _cachedCount == cfg.BodyCount
                && Mathf.Approximately(_cachedRadius, cfg.FieldRadius)
                && _cachedStars.Count == cfg.BodyCount
                && _cachedHiddenCount == cfg.HiddenObjectCount
               )
                return;

            _cachedSeed = cfg.Seed;
            _cachedCount = cfg.BodyCount;
            _cachedRadius = cfg.FieldRadius;
            _cachedHiddenCount = cfg.HiddenObjectCount;

            CosmicBodiesManager.GenerateField(cfg.Seed, cfg.BodyCount, cfg.FieldRadius, _cachedStars);
            HiddenObjectsManager.GenerateField(cfg.Seed, cfg.HiddenObjectCount, cfg.FieldRadius, _cachedStars, _cachedHidden);
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
                GL.Color(new Color(1f, 1f, 0.9f, 0.3f + brightness * 0.7f));

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

        private void DrawHiddenObjects() {
            if (_cachedHidden.Count == 0)
                return;

            for (int i = 0; i < _cachedHidden.Count; i++) {
                CosmosGizmos.DrawHidden(_cachedHidden[i]);
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