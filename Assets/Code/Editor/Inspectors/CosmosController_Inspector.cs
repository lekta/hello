using LH.Cosmos;
using UnityEditor;
using UnityEngine;

namespace LH.Dev {
    [CustomEditor(typeof(CosmosController))]
    public class CosmosController_Inspector : Editor {
        private CosmosController Controller => target as CosmosController;

        private static bool _showSceneGizmos;


        private void OnEnable() {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable() {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.RepaintAll();
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            EditorGUILayout.Space(3);
            DrawCameraPreview();
        }

        private void DrawCameraPreview() {
            using var horizontal = new EditorGUILayout.HorizontalScope();

            var icon = _showSceneGizmos ? DevGui.Icon.EyeOn : DevGui.Icon.EyeOff;

            if (GUILayout.Button(icon, EditorStyles.miniButton, GUILayout.Width(28), GUILayout.Height(20))) {
                _showSceneGizmos = !_showSceneGizmos;
                SceneView.RepaintAll();
            }
            EditorGUILayout.LabelField("Показать границы в сцене");
        }


        private void OnSceneGUI(SceneView sceneView) {
            if (!_showSceneGizmos)
                return;
            if (Controller == null || Controller.Camera == null || Controller.Config == null)
                return;

            var cam = Controller.Camera;
            var camParam = Controller.CameraConfig.Move;
            float screenWidth = cam.orthographicSize * 2f * cam.aspect;
            float fieldRadius = Controller.Config.FieldRadius;
            float hardLimit = fieldRadius + screenWidth * camParam.BoundaryOvershoot;

            DrawFieldRadius(fieldRadius);
            DrawHardLimit(camParam, fieldRadius, hardLimit, screenWidth);
        }

        private void DrawFieldRadius(float fieldRadius) {
            Handles.color = new Color(0.3f, 0.9f, 0.3f, 0.5f);
            Handles.DrawWireDisc(Vector3.zero, Vector3.forward, fieldRadius);
            Handles.Label(new Vector3(0f, fieldRadius + 0.5f, 0f), "Поле звёзд", EditorStyles.whiteLabel);
        }

        private void DrawHardLimit(CameraMoveParams camParam, float fieldRadius, float hardLimit, float screenWidth) {
            Handles.color = new Color(0.9f, 0.3f, 0.2f, 0.5f);
            EditorGUI.BeginChangeCheck();

            float newHardLimit = Handles.RadiusHandle(Quaternion.identity, Vector3.zero, hardLimit);

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(Controller, "Change Boundary Overshoot");
                camParam.BoundaryOvershoot = Mathf.Max(0f, (newHardLimit - fieldRadius) / screenWidth);
                EditorUtility.SetDirty(Controller);
            }

            Handles.Label(new Vector3(0f, hardLimit + 0.5f, 0f), "Жёсткий лимит", EditorStyles.whiteLabel);
        }
    }
}