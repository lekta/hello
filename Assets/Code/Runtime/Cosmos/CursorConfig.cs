using UnityEngine;

namespace LH {
    public class CursorConfig : ScriptableObject {
        [Tooltip("Скорость прокрутки камеры (ширин экрана в секунду)")]
        public float CameraScrollSpeed = 1.2f;
    }
}
