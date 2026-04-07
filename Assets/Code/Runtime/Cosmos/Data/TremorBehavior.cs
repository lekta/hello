using System;
using UnityEngine;

namespace LH.Cosmos {
    [Serializable]
    public class TremorBehavior : IHiddenBehavior {
        [Tooltip("Амплитуда дрожи звёзд")]
        public float Magnitude = 0.3f;
    }
}
