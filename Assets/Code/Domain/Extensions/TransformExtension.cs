using UnityEngine;

namespace LH.Domain {
    public static class TransformExtension {
        public static void SetPositionXY(this Transform transform, Vector2 pos) => transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }
}