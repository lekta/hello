using System;
using UnityEngine;

namespace LH.Cosmos {
    public class CameraConfig : ScriptableObject {
        public CameraMoveParams Move;
        public CameraShakeParams Shake;
    }

    [Serializable]
    public class CameraMoveParams {
        [Header("Зоны управления")]
        [Tooltip("Мёртвая зона (доля от центра до края экрана)")]
        [Range(0f, 1f)] public float DeadZone = .33f;

        [Tooltip("Зона максимальной скорости (доля от центра до края)")]
        [Range(0f, 1f)] public float MaxSpeedZone = .66f;

        [Tooltip("Скорость прокрутки (ширин экрана в секунду)")]
        public float ScrollSpeed = 1f;

        [Tooltip("Сглаживание скорости")]
        public float Smoothing = 3f;

        [Header("Границы")]
        [Tooltip("Жёсткость мягкой границы")]
        public float BoundaryStiffness = 9f;

        [Tooltip("Запас за границей (в ширинах экрана)")]
        public float BoundaryOvershoot = .1f;

        [Header("Визуал")]
        [Tooltip("Параллакс фона")]
        public float Parallax = .12f;
    }

    [Serializable]
    public class CameraShakeParams {
        [Tooltip("Максимальное смещение камеры (world units)")]
        public float MaxOffset = 4f;

        [Tooltip("Скорость затухания травмы")]
        public float Decay = 3f;

        [Tooltip("Частота шума")]
        public float Frequency = 25f;

        [Tooltip("Импульс от клика фокуса")]
        public float FocusImpulse = 1.4f;

        [Tooltip("Множитель тряски при раскрытии скрытого")]
        public float RevealShakeMax = .5f;
    }
}
