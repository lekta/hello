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

        private static readonly Color COLOR_DEFAULT = new(1f, 0.3f, 0.8f);
        private static readonly Color COLOR_SELECTED = new(0.4f, 0.9f, 1f);
        private static readonly Color COLOR_HIGHLIGHT = new(1f, 1f, 0.2f);

        public static int HighlightedHiddenId = -1;


        public static void DrawHidden(HiddenObjectData hidden, bool selected = false) {
            Vector3 pos = new Vector3(hidden.Position.x, hidden.Position.y, 0f);
            bool highlighted = HighlightedHiddenId == hidden.Id;
            Color tint = highlighted ? COLOR_HIGHLIGHT : selected ? COLOR_SELECTED : COLOR_DEFAULT;

            if (highlighted) {
                Handles.color = tint.WithAlpha(0.15f);
                Handles.DrawSolidDisc(pos, Vector3.forward, hidden.Radius);
            } else if (selected) {
                Handles.color = tint.WithAlpha(0.06f);
                Handles.DrawSolidDisc(pos, Vector3.forward, hidden.Radius);
            }

            // Радиус воздействия
            Handles.color = tint.WithAlpha(selected ? 0.8f : 0.5f);
            Handles.DrawWireDisc(pos, Vector3.forward, hidden.Radius);

            // Зона обнаружения
            Handles.color = tint.WithAlpha(selected ? 0.35f : 0.2f);
            Handles.DrawWireDisc(pos, Vector3.forward, hidden.Radius * 0.5f);

            // Крестик
            Handles.color = tint.WithAlpha(selected ? 0.9f : 0.5f);
            float cross = Mathf.Min(hidden.Radius * 0.05f, 10f);
            Handles.DrawLine(pos + Vector3.left * cross, pos + Vector3.right * cross);
            Handles.DrawLine(pos + Vector3.down * cross, pos + Vector3.up * cross);

            Handles.Label(pos + Vector3.up * (hidden.Radius + 5f), $"H#{hidden.Id}", EditorStyles.whiteLabel);
        }

        public static void DrawLocks(HiddenObjectData hidden, Dictionary<int, HiddenObjectData> lookup) {
            if (hidden.Locks.IsEmpty())
                return;

            Vector3 from = new Vector3(hidden.Position.x, hidden.Position.y, 0f);

            foreach (var lck in hidden.Locks) {
                if (lck is not HiddenLock hl)
                    continue;
                if (!lookup.TryGetValue(hl.HiddenId, out var dep))
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