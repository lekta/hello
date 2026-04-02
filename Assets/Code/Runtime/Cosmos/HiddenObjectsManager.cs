using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace LH.Cosmos {
    public class HiddenObjectsManager {
        private const float ANOMALY_TREMOR = .1f;
        private const float ANOMALY_DRIFT = 15f;
        private const float BLACKOUT_DURATION = 0.5f;
        private const float BLACKOUT_INTERVAL_MIN = 40f;
        private const float BLACKOUT_INTERVAL_MAX = 50f;
        private const float REVEAL_FOCUS_TIME = 1.5f;

        private CosmosController _cosmos;
        private Transform _holder;
        private readonly List<HiddenObjectData> _datas = new();
        private readonly List<HiddenObjectView> _views = new();
        private readonly float[] _blackoutTimers = new float[10];
        private readonly float[] _blackoutIntervals = new float[10];
        private float[] _focusTimers;

        // DO: в ооп блет
        // Для каждого скрытого объекта — список индексов затронутых звёзд
        private List<int>[] _affectedStarIndices;

        // Для каждой звезды — индекс скрытого объекта (-1 если нет)
        private int[] _starAnomalyOwner;

        // Нормализованная сила аномалии (0..1) для каждой звезды
        private float[] _starAnomalyStrength;

        // Drift offsets per star (seeded)
        private Vector2[] _starDriftOffsets;


        public void Init(CosmosController cosmos, float fieldRadius, List<CosmicBodyData> starDatas) {
            _cosmos = cosmos;
            var sw = Stopwatch.StartNew();

            _holder = new GameObject("hidden").transform;
            _holder.SetParent(cosmos.transform);

            var cfg = cosmos.Config;
            GenerateHiddenObjects(cfg.Seed + 7777, cfg.HiddenObjectCount, fieldRadius, starDatas);
            CreateViews(cfg.HiddenObject);

            Debug.Log($"Cosmic hidden ({_datas.Count}) generated in {sw.ElapsedMilliseconds} ms");
        }

        private void GenerateHiddenObjects(int seed, int count, float fieldRadius, List<CosmicBodyData> stars) {
            _datas.Clear();
            var rng = new Random(seed);

            _starAnomalyOwner = new int[stars.Count];
            _starAnomalyStrength = new float[stars.Count];
            _starDriftOffsets = new Vector2[stars.Count];
            _affectedStarIndices = new List<int>[count];
            _focusTimers = new float[count];

            for (int i = 0; i < stars.Count; i++)
                _starAnomalyOwner[i] = -1;

            for (int i = 0; i < count; i++) {
                var data = PlaceHiddenObject(rng, fieldRadius, stars, i);
                _datas.Add(data);

                float interval = BLACKOUT_INTERVAL_MIN + (float)rng.NextDouble() * (BLACKOUT_INTERVAL_MAX - BLACKOUT_INTERVAL_MIN);
                _blackoutIntervals[i] = interval;
                _blackoutTimers[i] = (float)rng.NextDouble() * interval; // stagger

                // Drift offsets for affected stars
                foreach (int si in _affectedStarIndices[i]) {
                    float angle = (float)(rng.NextDouble() * Math.PI * 2.0);
                    float drift = ANOMALY_DRIFT * _starAnomalyStrength[si];
                    _starDriftOffsets[si] = new Vector2(Mathf.Cos(angle) * drift, Mathf.Sin(angle) * drift);
                }
            }
        }

        private HiddenObjectData PlaceHiddenObject(Random rng, float fieldRadius, List<CosmicBodyData> stars, int index) {
            // Пробуем разместить так, чтобы минимум 3 звезды попали в радиус
            for (int attempt = 0; attempt < 200; attempt++) {
                float angle = (float)(rng.NextDouble() * Math.PI * 2.0);
                float r = fieldRadius * 0.7f * (float)Math.Sqrt(rng.NextDouble());
                Vector2 pos = new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
                float radius = 80f + (float)rng.NextDouble() * 120f;

                var affected = new List<int>();
                for (int si = 0; si < stars.Count; si++) {
                    if (_starAnomalyOwner[si] >= 0)
                        continue; // уже занята другим объектом
                    float dist = (stars[si].AnchorPosition - pos).magnitude;
                    if (dist < radius)
                        affected.Add(si);
                }

                if (affected.Count >= 3) {
                    _affectedStarIndices[index] = affected;
                    foreach (int si in affected) {
                        _starAnomalyOwner[si] = index;
                        float dist = (stars[si].AnchorPosition - pos).magnitude;
                        _starAnomalyStrength[si] = 1f - dist / radius;
                    }
                    return new HiddenObjectData { Index = index, Position = pos, Radius = radius };
                }
            }

            // Fallback: просто ставим где есть звёзды, с большим радиусом
            Vector2 fallbackPos = stars[rng.Next(stars.Count)].AnchorPosition;
            float fallbackRadius = 200f;
            var fallbackAffected = new List<int>();
            for (int si = 0; si < stars.Count; si++) {
                if (_starAnomalyOwner[si] >= 0)
                    continue;
                float dist = (stars[si].AnchorPosition - fallbackPos).magnitude;
                if (dist < fallbackRadius)
                    fallbackAffected.Add(si);
            }
            _affectedStarIndices[index] = fallbackAffected;
            foreach (int si in fallbackAffected) {
                _starAnomalyOwner[si] = index;
                float dist = (stars[si].AnchorPosition - fallbackPos).magnitude;
                _starAnomalyStrength[si] = 1f - dist / fallbackRadius;
            }
            return new HiddenObjectData { Index = index, Position = fallbackPos, Radius = fallbackRadius };
        }

        private void CreateViews(HiddenObjectView prefab) {
            for (int i = 0; i < _datas.Count; i++) {
                var view = Object.Instantiate(prefab, _holder);
                view.Setup(_datas[i]);
                _views.Add(view);
            }
        }

        public void Update(List<CosmicBodyData> starDatas) {
            float time = Time.time;
            float dt = Time.deltaTime;
            Vector2 cursorPos = _cosmos.CursorWorldPos;
            bool isFocus = _cosmos.CursorState == CursorState.Focus;

            // DO: это должно быть в манагере звёзд
            for (int hi = 0; hi < _datas.Count; hi++) {
                var hidden = _datas[hi];
                if (hidden.Revealed)
                    continue;

                UpdateBlackout(hi, starDatas, time, dt);
                ApplyAnomalyTremor(hi, starDatas, time);
                ApplyAnomalyDrift(hi, starDatas);
                UpdateReveal(hi, cursorPos, isFocus, dt);
            }

            // DO: сделать подписку вьюхи на стейт-ченжед
            foreach (var view in _views)
                view.UpdateManual();
        }

        private void UpdateBlackout(int hi, List<CosmicBodyData> starDatas, float time, float dt) {
            _blackoutTimers[hi] += dt;
            if (_blackoutTimers[hi] < _blackoutIntervals[hi])
                return;

            float blackoutPhase = _blackoutTimers[hi] - _blackoutIntervals[hi];
            if (blackoutPhase > BLACKOUT_DURATION) {
                _blackoutTimers[hi] = 0f;
                return;
            }

            // Все звёзды в зоне гаснут
            foreach (int si in _affectedStarIndices[hi])
                starDatas[si].Brightness = 0f;
        }

        private void ApplyAnomalyTremor(int hi, List<CosmicBodyData> starDatas, float time) {
            foreach (int si in _affectedStarIndices[hi]) {
                float strength = _starAnomalyStrength[si] * ANOMALY_TREMOR;
                var data = starDatas[si];
                float tx = Mathf.Sin(time * 11f + si * 2.3f) * strength;
                float ty = Mathf.Sin(time * 9f + si * 3.1f) * strength;
                data.Position += new Vector2(tx, ty);
            }
        }

        private void ApplyAnomalyDrift(int hi, List<CosmicBodyData> starDatas) {
            foreach (int si in _affectedStarIndices[hi]) {
                starDatas[si].Position += _starDriftOffsets[si];
            }
        }

        // DO: это остаётся тут, но вместо массивов ебанутых нужен ООО (объектно-ориентированный объект) скрытого
        private void UpdateReveal(int hi, Vector2 cursorPos, bool isFocus, float dt) {
            var hidden = _datas[hi];

            // DO: добавить логику анхайда - когда ещё не раскрыт фокусом, но под лупой уже "виднеется"
            float dist = (cursorPos - hidden.Position).magnitude;

            if (isFocus && dist < hidden.Radius * 0.5f) {
                _focusTimers[hi] += dt;
                if (_focusTimers[hi] >= REVEAL_FOCUS_TIME)
                    hidden.Revealed = true;
            } else {
                _focusTimers[hi] = Mathf.MoveTowards(_focusTimers[hi], 0f, dt * 2f);
            }
        }

        public static void GenerateField(int seed, int count, float fieldRadius, List<CosmicBodyData> stars, List<HiddenObjectData> outHidden) {
            outHidden.Clear();
            var rng = new Random(seed + 7777);
            var taken = new bool[stars.Count];

            for (int i = 0; i < count; i++) {
                for (int attempt = 0; attempt < 200; attempt++) {
                    float angle = (float)(rng.NextDouble() * Math.PI * 2.0);
                    float r = fieldRadius * 0.7f * (float)Math.Sqrt(rng.NextDouble());
                    Vector2 pos = new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
                    float radius = 80f + (float)rng.NextDouble() * 120f;

                    int affected = 0;
                    for (int si = 0; si < stars.Count; si++) {
                        if (taken[si])
                            continue;
                        if ((stars[si].AnchorPosition - pos).magnitude < radius)
                            affected++;
                    }

                    if (affected >= 3) {
                        for (int si = 0; si < stars.Count; si++) {
                            if (!taken[si] && (stars[si].AnchorPosition - pos).magnitude < radius)
                                taken[si] = true;
                        }
                        outHidden.Add(new HiddenObjectData { Index = i, Position = pos, Radius = radius });
                        break;
                    }
                }
            }
        }
    }
}