using UnityEngine;

namespace LH.Domain {
    public static class VectorExtension {
        public static string ToIntString(this Vector2 vector) => $"({(int)vector.x}; {(int)vector.y})";
    }
}