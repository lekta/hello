using LH.Cosmos;
using UnityEditor;
using UnityEngine;

namespace LH.Dev {
    public static class CosmosGizmos {
        public static void DrawHidden(HiddenObjectData hidden) {
            Vector3 pos = new Vector3(hidden.Position.x, hidden.Position.y, 0f);

            // Радиус воздействия
            Handles.color = new Color(1f, 0.3f, 0.8f, 0.5f);
            Handles.DrawWireDisc(pos, Vector3.forward, hidden.Radius);

            // Зона обнаружения
            Handles.color = new Color(1f, 0.3f, 0.8f, 0.2f);
            Handles.DrawWireDisc(pos, Vector3.forward, hidden.Radius * 0.5f);

            // Крестик
            float cross = Mathf.Min(hidden.Radius * 0.05f, 10f);
            Handles.DrawLine(pos + Vector3.left * cross, pos + Vector3.right * cross);
            Handles.DrawLine(pos + Vector3.down * cross, pos + Vector3.up * cross);

            Handles.Label(pos + Vector3.up * (hidden.Radius + 5f), $"Hidden #{hidden.Index}", EditorStyles.whiteBoldLabel);
        }
    }
}