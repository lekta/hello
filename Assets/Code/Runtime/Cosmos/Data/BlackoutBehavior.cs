using System;
using UnityEngine;

namespace LH.Cosmos {
    [Serializable]
    public class BlackoutBehavior : IHiddenBehavior {
        [Tooltip("Длительность затемнения")]
        public float Duration = 0.5f;

        [Tooltip("Минимальный интервал между затемнениями")]
        public float IntervalMin = 40f;

        [Tooltip("Максимальный интервал между затемнениями")]
        public float IntervalMax = 50f;

        public override string ToString() => $"Blackout ({Duration}s)";
    }
}
