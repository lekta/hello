using UnityEngine;

namespace LH.Cosmos {
    public class StarData {
        public int Index;
        public Vector2 AnchorPosition;
        public float AnchorScale;

        public Color Color;

        // Мерцание (генерируются по сиду)
        public float TwinkleSpeed;
        public float TwinklePhase;
        public float BlinkSpeed;
        public float BlinkPhase;

        // Дрожь при фокусе (0..1)
        public float TremorSensitivity;
    }
}
