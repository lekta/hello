using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace LH.Cosmos {
    public class StarsManager {
        private CosmosController _cosmos;
        private StarsCreationParams _starsParams;

        private float _fieldRadius;
        private Transform _starsHolder;
        private readonly List<StarData> _datas = new();
        private readonly List<StarView> _views = new();

        public IReadOnlyList<StarData> Datas => _datas;


        public void Init(CosmosController cosmos, float fieldRadius) {
            _cosmos = cosmos;
            _starsParams = cosmos.Config.StarsParams;
            _fieldRadius = fieldRadius;
            var sw = Stopwatch.StartNew();

            _starsHolder = new GameObject("stars").transform;
            _starsHolder.SetParent(_cosmos.transform);

            RecreateStars();

            Debug.Log($"Stars ({_datas.Count}) generated in {sw.ElapsedMilliseconds} ms");
        }

        private void RecreateStars() {
            for (int i = 0; i < _views.Count; i++) {
                _views[i].TurnOff();
            }

            var cfg = _cosmos.Config;
            var prefab = cfg.Star;
            while (_views.Count < cfg.StarCount) {
                var view = Object.Instantiate(prefab, _starsHolder);
                _views.Add(view);
            }

            GenerateField(cfg.Seed, cfg.StarCount, _fieldRadius, _datas, _starsParams, cfg.ColorZones);

            for (int i = 0; i < _datas.Count; i++) {
                _views[i].Setup(_datas[i]);
            }
        }

        public static void GenerateField(int seed, int count, float radius, List<StarData> outDatas, StarsCreationParams stars, ColorZone[] colorZones = null) {
            outDatas.Clear();
            var rng = new Random(seed);

            var clusterCenters = new Vector2[stars.ClusterCount];
            for (int i = 0; i < stars.ClusterCount; i++) {
                clusterCenters[i] = RandomInDisc(rng, radius * .8f);
            }

            for (int i = 0; i < count; i++) {
                Vector2 pos = NextStarPosition(rng, radius, clusterCenters, stars);
                float scale = stars.RandomSize(rng);

                outDatas.Add(new StarData {
                    Index = i,
                    AnchorPosition = pos,
                    AnchorScale = scale,
                });
            }

            // После позиции/размера — визуал и поведение (порядок rng важен)
            for (int i = 0; i < count; i++) {
                var data = outDatas[i];
                data.Color = NextStarColor(rng, data.AnchorScale, data.AnchorPosition, stars, colorZones);
                data.TwinkleSpeed = stars.TwinkleSpeedRange.x + (float)rng.NextDouble() * (stars.TwinkleSpeedRange.y - stars.TwinkleSpeedRange.x);
                data.TwinklePhase = (float)(rng.NextDouble() * Math.PI * 2.0);
                data.BlinkSpeed = stars.BlinkSpeedRange.x + (float)rng.NextDouble() * (stars.BlinkSpeedRange.y - stars.BlinkSpeedRange.x);
                data.BlinkPhase = (float)(rng.NextDouble() * Math.PI * 2.0);
                data.TremorSensitivity = (float)rng.NextDouble();
            }
        }

        private static Vector2 NextStarPosition(Random rng, float radius, Vector2[] clusterCenters, StarsCreationParams stars) {
            if (rng.NextDouble() < stars.ClusteredRatio) {
                var center = clusterCenters[rng.Next(stars.ClusterCount)];
                Vector2 offset = RandomGaussian2D(rng) * (radius * .18f);
                Vector2 pos = center + offset;

                float mag = pos.magnitude;
                if (mag > radius)
                    pos *= radius / mag;

                return pos;
            } else {
                return RandomInDisc(rng, radius);
            }
        }

        public void Update(IReadOnlyList<HiddenObject> hiddens) {
            float time = Time.time;

            foreach (var data in _datas) {
                AttractToAnchors(data);
            }

            foreach (var data in _datas) {
                ApplyTwinkle(data, time);
            }

            foreach (var hidden in hiddens) {
                if (hidden.Revealed)
                    continue;

                foreach (var starIdx in hidden.AffectedStars.Keys) {
                    var data = _datas[starIdx];
                    ApplyHiddenTeasering(data, hidden, time);
                }
            }

            Vector2 cursorPos = _cosmos.CursorWorldPos;
            float activity = _cosmos.CursorActivity;
            foreach (var data in _datas) {
                ApplyCursorFocus(data, cursorPos, activity, time);
            }

            foreach (var view in _views) {
                view.ManualUpdate();
            }
        }

        private static Color NextStarColor(Random rng, float scale, Vector2 position, StarsCreationParams stars, ColorZone[] zones) {
            float temperature = Mathf.Clamp01(scale / stars.TemperatureScaleDivisor + (float)rng.NextDouble() * stars.TemperatureRandomness);
            Color baseColor = stars.TemperatureGradient.Evaluate(temperature);

            if (zones != null) {
                for (int z = 0; z < zones.Length; z++) {
                    var zone = zones[z];
                    float dist = (position - zone.Position).magnitude;
                    if (dist >= zone.Radius)
                        continue;

                    float influence = (1f - dist / zone.Radius);
                    influence *= influence * zone.Strength;
                    baseColor = Color.Lerp(baseColor, zone.Tint * baseColor.maxColorComponent, influence);
                }
            }

            return baseColor;
        }

        // Равномерная точка внутри диска: sqrt компенсирует смещение к центру
        private static Vector2 RandomInDisc(Random rng, float radius) {
            float angle = (float)(rng.NextDouble() * Math.PI * 2.0);
            float r = radius * (float)Math.Sqrt(rng.NextDouble());
            return new Vector2((float)Math.Cos(angle) * r, (float)Math.Sin(angle) * r);
        }

        // Box-Muller: 2D вектор с нормальным распределением (σ=1)
        private static Vector2 RandomGaussian2D(Random rng) {
            float u1 = 1f - (float)rng.NextDouble();
            float u2 = (float)rng.NextDouble();
            float mag = (float)Math.Sqrt(-2.0 * Math.Log(u1));

            return new Vector2(
                mag * (float)Math.Cos(2.0 * Math.PI * u2),
                mag * (float)Math.Sin(2.0 * Math.PI * u2)
            );
        }

        private void ApplyTwinkle(StarData data, float time) {
            float subtle = 1f - _starsParams.TwinkleSubtle * Mathf.Sin(time * data.TwinkleSpeed + data.TwinklePhase);
            float blink = Mathf.Pow(Mathf.Abs(Mathf.Sin(time * data.BlinkSpeed + data.BlinkPhase)), _starsParams.BlinkSharpness);
            data.Brightness = subtle * blink;
        }

        private void ApplyHiddenTeasering(StarData data, HiddenObject hidden, float time) {
            if (hidden.BlackoutCoef > 0f) {
                data.Brightness *= 1f - hidden.BlackoutCoef;
            }

            // DO: ослаблять эффект к краям
            float tx = Mathf.Sin(time * 101f + data.Index * 2.3f) * hidden.TremorCoef;
            float ty = Mathf.Sin(time * 97f + data.Index * 3.1f) * hidden.TremorCoef;
            data.Position += new Vector2(tx, ty);
        }


        private void ApplyCursorFocus(StarData data, Vector2 cursorPos, float activity, float time) {
            if (activity < .001f)
                return;

            Vector2 toStar = data.AnchorPosition - cursorPos;
            float dist = toStar.magnitude;
            if (dist >= _starsParams.FocusRadius)
                return;

            float t = dist / _starsParams.FocusRadius;
            float blend = _starsParams.FocusTransitionSpeed * Time.deltaTime * activity;

            // Масштаб: увеличение ближе к центру
            float scaleFade = (1f - t) * (1f - t);
            float targetScale = data.AnchorScale * (1f + (_starsParams.FocusMaxScale - 1f) * scaleFade);
            data.Scale = Mathf.Lerp(data.Scale, targetScale, blend);

            if (dist < .001f) {
                data.Position = Vector2.Lerp(data.Position, cursorPos, blend);
                return;
            }

            Vector2 dir = toStar / dist;

            if (dist < _starsParams.FocusSnapRadius) {
                float snapT = 1f - dist / _starsParams.FocusSnapRadius;
                data.Position = Vector2.Lerp(data.Position, cursorPos, snapT * snapT * .8f * blend);
            } else {
                float pushZone = _starsParams.FocusRadius - _starsParams.FocusSnapRadius;
                float pushT = (dist - _starsParams.FocusSnapRadius) / pushZone;
                float remapped = Mathf.Pow(pushT, _starsParams.FocusFisheyePower);
                float newDist = _starsParams.FocusSnapRadius + remapped * pushZone;
                Vector2 fisheyePos = cursorPos + dir * newDist;
                data.Position = Vector2.Lerp(data.Position, fisheyePos, blend);
            }

            // Дрожь: усиливается ближе к центру фокуса, зависит от чувствительности звезды
            float tremorStrength = data.TremorSensitivity * activity * (1f - t) * _starsParams.TremorAmplitude;
            float tx = Mathf.Sin(time * (170f - 153f * t) + data.TwinklePhase * 3.7f) * tremorStrength;
            float ty = Mathf.Sin(time * (130f - 117f * t) + data.BlinkPhase * 2.3f) * tremorStrength;
            data.Position += new Vector2(tx, ty);
        }

        private void AttractToAnchors(StarData data) {
            float dt = Time.deltaTime;

            var delta = data.AnchorPosition - data.Position;
            float distance = delta.magnitude;

            if (distance > .01f) {
                float speed = distance * 5.0f;
                Vector2 shift = delta.normalized * speed * dt;

                if (shift.magnitude > distance) {
                    data.Position = data.AnchorPosition;
                } else {
                    data.Position += shift;
                }
            } else {
                data.Position = data.AnchorPosition;
            }

            var sizeDelta = data.AnchorScale - data.Scale;
            if (Mathf.Abs(sizeDelta) > .01f) {
                float sizeSpeed = sizeDelta * 2f * dt;

                data.Scale += sizeSpeed;
            } else {
                data.Scale = data.AnchorScale;
            }
        }
    }
}