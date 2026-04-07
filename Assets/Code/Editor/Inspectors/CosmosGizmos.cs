using System.Collections.Generic;
using LH.Cosmos;
using LH.Domain;
using UnityEditor;
using UnityEngine;

namespace LH.Dev {
    public static class CosmosGizmos {
        private const float DASH_LENGTH = 20f;
        private const float GAP_LENGTH = 12f;
        private const float ARROW_SIZE = 18f;

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

            Handles.Label(pos + Vector3.up * (hidden.Radius + 5f), $"Hidden #{hidden.Id}", EditorStyles.whiteBoldLabel);
        }

        public static void DrawDependencies(HiddenObjectData hidden, Dictionary<int, HiddenObjectData> lookup) {
            if (hidden.Dependencies.IsEmpty())
                return;

            Vector3 from = new Vector3(hidden.Position.x, hidden.Position.y, 0f);

            foreach (int depId in hidden.Dependencies) {
                if (!lookup.TryGetValue(depId, out var dep))
                    continue;

                Vector3 to = new Vector3(dep.Position.x, dep.Position.y, 0f);
                DrawDashedArrow(from, to);
            }
        }

        private static void DrawDashedArrow(Vector3 from, Vector3 to) {
            Handles.color = new Color(1f, 0.8f, 0.2f, 0.6f);

            Vector3 dir = to - from;
            float totalLength = dir.magnitude;
            if (totalLength < 1f)
                return;

            Vector3 dirNorm = dir / totalLength;

            float drawn = 0f;
            bool dash = true;
            while (drawn < totalLength) {
                float segLen = dash ? DASH_LENGTH : GAP_LENGTH;
                float segEnd = Mathf.Min(drawn + segLen, totalLength);

                if (dash)
                    Handles.DrawLine(from + dirNorm * drawn, from + dirNorm * segEnd);

                drawn = segEnd;
                dash = !dash;
            }

            // Стрелка на конце
            Vector3 perp = new Vector3(-dirNorm.y, dirNorm.x, 0f);
            Vector3 arrowBase = to - dirNorm * ARROW_SIZE;
            Handles.DrawLine(to, arrowBase + perp * ARROW_SIZE * 0.4f);
            Handles.DrawLine(to, arrowBase - perp * ARROW_SIZE * 0.4f);
        }
    }
}
