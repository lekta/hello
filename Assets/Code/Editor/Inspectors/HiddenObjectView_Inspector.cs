using LH.Cosmos;
using UnityEditor;
using UnityEngine;

namespace LH.Dev {
    [CustomEditor(typeof(HiddenObjectView))]
    public class HiddenObjectView_Inspector : Editor {
        private HiddenObjectView View => (HiddenObjectView)target;
        
        private void OnEnable() {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable() {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            GUILayout.Space(3);

            var data = View.Data;
            if (data == null) {
                GUILayout.Label("No data");
            } else {
                GUILayout.Label($"#{data.Index}; revealed {data.Revealed}");
            }
        }

        private void OnSceneGUI(SceneView sceneView) {
            if (Event.current.type != EventType.Repaint)
                return;
            
            if (View.Data == null)
                return;
            
            CosmosGizmos.DrawHidden(View.Data);
        }
    }
}
