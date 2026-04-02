using UnityEngine;

namespace LH.Cosmos {
    public class CosmicBodyData {
        public int Index;

        public Vector2 AnchorPosition;
        public Vector2 Position;

        public float AnchorScale;
        public float Scale;

        // Визуал
        public Color Color;
        public float Brightness = 1f;

        // Мерцание (генерируются по сиду)
        public float TwinkleSpeed;
        public float TwinklePhase;
        public float BlinkSpeed;
        public float BlinkPhase;

        // Дрожь при фокусе (0..1, генерируется по сиду)
        public float TremorSensitivity;
    }
}