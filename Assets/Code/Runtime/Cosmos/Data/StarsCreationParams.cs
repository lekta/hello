using System;
using LH.Domain;
using UnityEngine;

namespace LH.Cosmos {
    [Serializable]
    public class StarsCreationParams {
        [Header("Размеры")]
        [Tooltip("Доля мелких звёзд")]
        [Range(0, 1)] public float SmallRatio = .55f;
        [Tooltip("Доля средних звёзд (крупных = 1 − мелких − средних)")]
        [Range(0, 1)] public float MediumRatio = .35f;

        [MinMaxRange(1, 30)]
        public Vector2 SmallSize = new(2, 4);
        [MinMaxRange(1, 30)]
        public Vector2 MediumSize = new(4, 9);
        [MinMaxRange(1, 30)]
        public Vector2 LargeSize = new(9, 13);

        [Header("Кластеры")]
        [Range(0, 1)] public float ClusteredRatio = .75f;
        [Range(1, 20)] public int ClusterCount = 7;

        [Header("Мерцание")]
        [Tooltip("Амплитуда мягкого мерцания")]
        public float TwinkleSubtle = .05f;
        [Tooltip("Резкость вспышек (меньше — резче)")]
        public float BlinkSharpness = .1f;
        [MinMaxRange(0, 10)]
        [Tooltip("Диапазон скорости мерцания")]
        public Vector2 TwinkleSpeedRange = new(2, 5);
        [MinMaxRange(0, 3)]
        [Tooltip("Диапазон скорости вспышек")]
        public Vector2 BlinkSpeedRange = new(.3f, .8f);

        [Header("Дрожь при фокусе")]
        public float TremorAmplitude = .3f;

        [Header("Фокусировка курсора")]
        public float FocusRadius = 300f;
        public float FocusSnapRadius = 30f;
        [Range(0, 1)] public float FocusFisheyePower = .1f;
        public float FocusMaxScale = 7.2f;
        public float FocusTransitionSpeed = 4f;

        [Header("Цвета по температуре")]
        [Tooltip("Градиент: холодные (лево) → горячие (право). Температура зависит от размера звезды")]
        public Gradient TemperatureGradient = new() {
            colorKeys = new[] {
                new GradientColorKey(new Color(.7f, .8f, 1f), 0f),
                new GradientColorKey(new Color(.85f, .9f, 1f), .4f),
                new GradientColorKey(new Color(1f, .95f, .85f), .7f),
                new GradientColorKey(new Color(1f, .75f, .5f), 1f),
            },
            alphaKeys = new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f),
            }
        };

        [Tooltip("Делитель шкалы для расчёта температуры (больше — холоднее)")]
        public float TemperatureScaleDivisor = 16f;
        [Tooltip("Случайный разброс температуры")]
        [Range(0, 1)] public float TemperatureRandomness = .25f;

        public float LargeRatio => Mathf.Max(0f, 1f - SmallRatio - MediumRatio);


        public float RandomSize(System.Random rng) {
            double roll = rng.NextDouble();
            if (roll < SmallRatio)
                return SmallSize.x + (float)(rng.NextDouble() * (SmallSize.y - SmallSize.x));
            if (roll < SmallRatio + MediumRatio)
                return MediumSize.x + (float)(rng.NextDouble() * (MediumSize.y - MediumSize.x));
            return LargeSize.x + (float)(rng.NextDouble() * (LargeSize.y - LargeSize.x));
        }
    }
}