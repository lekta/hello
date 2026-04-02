using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace LH.Cosmos {
    public class CosmicBodiesManager {
        // DO: размеры звёзд и вероятности в конфиг
        private const float SMALL_STAR_RATIO = 0.55f;  // тусклые
        private const float MEDIUM_STAR_RATIO = 0.35f; // средние

        // Доля звёзд в кластерах vs случайный фон
        private const float CLUSTERED_RATIO = 0.75f;
        private const int CLUSTER_COUNT = 7;

        // Фокусировка курсора
        private const float FOCUS_RADIUS = 300f;
        private const float FOCUS_SNAP_RADIUS = 30f;
        private const float FOCUS_FISHEYE_POWER = 0.1f;
        private const float FOCUS_MAX_SCALE = 7.2f;
        private const float FOCUS_TRANSITION_SPEED = 4f;

        private CosmosController _cosmos;
        private float _fieldRadius;
        private Transform _bodiesHolder;
        private readonly List<CosmicBodyData> _datas = new();
        private readonly List<CosmicBodyView> _bodies = new();


        public void Init(CosmosController cosmos, float fieldRadius) {
            _cosmos = cosmos;
            _fieldRadius = fieldRadius;

            _bodiesHolder = new GameObject("bodies").transform;
            _bodiesHolder.SetParent(_cosmos.transform);

            RecreateBodies();
        }

        private void RecreateBodies() {
            for (int i = 0; i < _bodies.Count; i++) {
                _bodies[i].TurnOff();
            }

            var cfg = _cosmos.Config;
            var prefab = cfg.CosmicBody;
            while (_bodies.Count < cfg.BodyCount) {
                var body = Object.Instantiate(prefab, _bodiesHolder);
                _bodies.Add(body);
            }

            GenerateField(cfg.Seed, cfg.BodyCount, _fieldRadius, _datas);

            for (int i = 0; i < _datas.Count; i++) {
                _bodies[i].Setup(_datas[i]);
            }
        }

        public static void GenerateField(int seed, int count, float radius, List<CosmicBodyData> outDatas) {
            outDatas.Clear();
            var rng = new Random(seed);

            var clusterCenters = new Vector2[CLUSTER_COUNT];
            for (int i = 0; i < CLUSTER_COUNT; i++) {
                clusterCenters[i] = RandomInDisc(rng, radius * 0.8f);
            }

            for (int i = 0; i < count; i++) {
                Vector2 pos = NextStarPosition(rng, radius, clusterCenters);
                float scale = NextStarScale(rng);

                outDatas.Add(new CosmicBodyData {
                    Index = i,
                    AnchorPosition = pos,
                    AnchorScale = scale,
                });
            }
        }

        private static Vector2 NextStarPosition(Random rng, float radius, Vector2[] clusterCenters) {
            if (rng.NextDouble() < CLUSTERED_RATIO) {
                var center = clusterCenters[rng.Next(CLUSTER_COUNT)];
                Vector2 offset = RandomGaussian2D(rng) * (radius * 0.18f);
                Vector2 pos = center + offset;

                float mag = pos.magnitude;
                if (mag > radius)
                    pos *= radius / mag;

                return pos;
            } else {
                return RandomInDisc(rng, radius);
            }
        }

        public void Update() {
            foreach (var data in _datas) {
                AttractToAnchors(data);
            }

            Vector2 cursorPos = _cosmos.CursorWorldPos;
            float activity = _cosmos.CursorActivity;
            foreach (var data in _datas) {
                ApplyCursorFocus(data, cursorPos, activity);
            }

            foreach (var body in _bodies) {
                body.Apply();
            }
        }

        // Три группы: 55% мелкие (2–4), 35% средние (4–9), 10% яркие (9–13)
        private static float NextStarScale(Random rng) {
            double roll = rng.NextDouble();
            if (roll < SMALL_STAR_RATIO)
                return 2f + (float)(rng.NextDouble() * 2);
            if (roll < SMALL_STAR_RATIO + MEDIUM_STAR_RATIO)
                return 4f + (float)(rng.NextDouble() * 5);
            return 9f + (float)(rng.NextDouble() * 4);
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

        private static void ApplyCursorFocus(CosmicBodyData data, Vector2 cursorPos, float activity) {
            if (activity < 0.001f) return;

            Vector2 toBody = data.AnchorPosition - cursorPos;
            float dist = toBody.magnitude;
            if (dist >= FOCUS_RADIUS) return;

            float t = dist / FOCUS_RADIUS;
            float blend = FOCUS_TRANSITION_SPEED * Time.deltaTime * activity;

            // Масштаб: увеличение ближе к центру
            float scaleFade = (1f - t) * (1f - t);
            float targetScale = data.AnchorScale * (1f + (FOCUS_MAX_SCALE - 1f) * scaleFade);
            data.Scale = Mathf.Lerp(data.Scale, targetScale, blend);

            if (dist < 0.001f) {
                data.Position = Vector2.Lerp(data.Position, cursorPos, blend);
                return;
            }

            Vector2 dir = toBody / dist;

            if (dist < FOCUS_SNAP_RADIUS) {
                float snapT = 1f - dist / FOCUS_SNAP_RADIUS;
                data.Position = Vector2.Lerp(data.Position, cursorPos, snapT * snapT * 0.8f * blend);
            } else {
                float pushZone = FOCUS_RADIUS - FOCUS_SNAP_RADIUS;
                float pushT = (dist - FOCUS_SNAP_RADIUS) / pushZone;
                float remapped = Mathf.Pow(pushT, FOCUS_FISHEYE_POWER);
                float newDist = FOCUS_SNAP_RADIUS + remapped * pushZone;
                Vector2 fisheyePos = cursorPos + dir * newDist;
                data.Position = Vector2.Lerp(data.Position, fisheyePos, blend);
            }
        }

        private void AttractToAnchors(CosmicBodyData data) {
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