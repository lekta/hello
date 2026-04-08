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
        private readonly List<Star> _stars = new();
        private readonly List<StarView> _views = new();

        public IReadOnlyList<StarData> Datas => _datas;


        public void Init(CosmosController cosmos, float fieldRadius) {
            _cosmos = cosmos;
            _starsParams = cosmos.Config.StarsParams;
            _fieldRadius = fieldRadius;
            var sw = Stopwatch.StartNew();

            _starsHolder = new GameObject("stars").transform;
            _starsHolder.SetParent(_cosmos.transform);
            _starsHolder.position = new Vector3(0, 0, 200);

            RecreateStars();

            Debug.Log($"Stars ({_stars.Count}) generated in {sw.ElapsedMilliseconds} ms");
        }

        private void RecreateStars() {
            for (int i = 0; i < _views.Count; i++)
                _views[i].TurnOff();
            _stars.Clear();

            var cfg = _cosmos.Config;
            GenerateField(cfg.Seed, cfg.StarCount, _fieldRadius, _datas, _starsParams, cfg.ColorZones);

            var prefab = cfg.Star;
            while (_views.Count < _datas.Count)
                _views.Add(Object.Instantiate(prefab, _starsHolder));

            for (int i = 0; i < _datas.Count; i++) {
                var star = new Star(_datas[i]);
                _stars.Add(star);
                _views[i].Setup(star);
            }
        }


        public void Update(IReadOnlyList<HiddenObject> hiddens) {
            var (xMin, xMax, yMin, yMax) = GetCameraBoundaries();

            float time = Time.time;
            float dt = Time.deltaTime;
            Vector2 cursorPos = _cosmos.CursorWorldPos;
            float activity = _cosmos.CursorActivity;

            // Основной Update видимых звёзд
            for (int i = 0; i < _stars.Count; i++) {
                var star = _stars[i];

                var anchor = star.Data.AnchorPosition;
                var offset = star.Position - anchor;

                // сильно отлетевшие всё равно пересчитываем
                bool isOutOfPos = Mathf.Abs(offset.x) > 100f || Mathf.Abs(offset.y) > 100f;

                if (!isOutOfPos && (anchor.x < xMin || anchor.x > xMax || anchor.y < yMin || anchor.y > yMax))
                    continue;

                star.Update(time, dt, _starsParams, cursorPos, activity);
            }

            // Эффекты скрытых
            foreach (var hidden in hiddens) {
                if (hidden.Revealed || !hidden.Active)
                    continue;

                foreach (var starIdx in hidden.AffectedStars.Keys)
                    _stars[starIdx].ApplyHiddenEffect(hidden, time);
            }

            // Вьюхи видимых звёзд
            for (int i = 0; i < _stars.Count; i++) {
                var pos = _stars[i].Position;
                bool isPosVisible = pos.x > xMin && pos.x < xMax && pos.y > yMin && pos.y < yMax;

                var viewPos = _views[i].LastPosition;
                bool isViewVisible = viewPos.x > xMin && viewPos.x < xMax && viewPos.y > yMin && viewPos.y < yMax;

                if (!isPosVisible && !isViewVisible)
                    continue;

                _views[i].Apply();
            }
        }

        private (float xMin, float xMax, float yMin, float yMax) GetCameraBoundaries() {
            var cam = _cosmos.Camera;
            float camH = cam.orthographicSize;
            float camW = camH * cam.aspect;
            Vector2 camPos = cam.transform.position;
            float safeZone = _starsParams.FocusRadius;

            float xMin = camPos.x - camW - safeZone;
            float xMax = camPos.x + camW + safeZone;
            float yMin = camPos.y - camH - safeZone;
            float yMax = camPos.y + camH + safeZone;

            return (xMin, xMax, yMin, yMax);
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

        private static Vector2 RandomInDisc(Random rng, float radius) {
            float angle = (float)(rng.NextDouble() * Math.PI * 2.0);
            float r = radius * (float)Math.Sqrt(rng.NextDouble());
            return new Vector2((float)Math.Cos(angle) * r, (float)Math.Sin(angle) * r);
        }

        private static Vector2 RandomGaussian2D(Random rng) {
            float u1 = 1f - (float)rng.NextDouble();
            float u2 = (float)rng.NextDouble();
            float mag = (float)Math.Sqrt(-2.0 * Math.Log(u1));

            return new Vector2(
                mag * (float)Math.Cos(2.0 * Math.PI * u2),
                mag * (float)Math.Sin(2.0 * Math.PI * u2)
            );
        }
    }
}