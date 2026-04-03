using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace LH.Cosmos {
    public class HiddenObjectsManager {
        private CosmosController _cosmos;
        private Transform _holder;
        
        private readonly List<HiddenObject> _hiddens = new();
        public IReadOnlyList<HiddenObject> Hiddens => _hiddens;

        private readonly List<HiddenObjectView> _views = new();


        public void Init(CosmosController cosmos, float fieldRadius, IReadOnlyList<CosmicBodyData> starDatas) {
            _cosmos = cosmos;
            var sw = Stopwatch.StartNew();

            _holder = new GameObject("hidden").transform;
            _holder.SetParent(cosmos.transform);

            var cfg = cosmos.Config;
            GenerateHiddenObjects(cfg.Seed + 7777, cfg.HiddenObjectCount, fieldRadius, starDatas);
            CreateViews(cfg.HiddenObject);

            Debug.Log($"Cosmic hidden ({_hiddens.Count}) generated in {sw.ElapsedMilliseconds} ms");
        }

        private void GenerateHiddenObjects(int seed, int count, float fieldRadius, IReadOnlyList<CosmicBodyData> stars) {
            _hiddens.Clear();
            var rng = new Random(seed);


            for (int i = 0; i < count; i++) {
                var hidden = PlaceHiddenObject(rng, fieldRadius, stars, i);
                _hiddens.Add(hidden);
            }
        }

        private HiddenObject PlaceHiddenObject(Random rng, float fieldRadius, IReadOnlyList<CosmicBodyData> stars, int index) {
            var bestPos = Vector2.zero;
            var lastPos = Vector2.zero;
            var bestAffected = new List<(int, float)>();
            float bestRadius = 170f;

            // Пробуем разместить где больше звёзд
            for (int attempt = 0; attempt < 100; attempt++) {
                float randomAngle = (float)(rng.NextDouble() * Math.PI * 2.0);
                float randomRadius = fieldRadius * .7f * (float)Math.Sqrt(rng.NextDouble());
                Vector2 pos = new Vector2(Mathf.Cos(randomAngle) * randomRadius, Mathf.Sin(randomAngle) * randomRadius);

                float teaserRadius = 80f + (float)rng.NextDouble() * 120f;

                var affected = stars
                    .Select(s => (s.Index, Dist: (s.AnchorPosition - pos).magnitude))
                    .Where(t => t.Dist < teaserRadius)
                    .ToList();

                if (affected.Count > bestAffected.Count) {
                    bestAffected = affected;
                    bestPos = pos;
                    bestRadius = teaserRadius;
                }
                if (affected.Count > 5) {
                    break;
                }
                lastPos = pos;
            }
            if (bestPos == Vector2.zero) {
                bestPos = lastPos;
            }

            var data = new HiddenObjectData { Index = index, Position = bestPos, Radius = bestRadius };
            var hidden = new HiddenObject(data);
            hidden.Init(bestAffected, rng);

            return hidden;
        }

        private void CreateViews(HiddenObjectView prefab) {
            for (int i = 0; i < _hiddens.Count; i++) {
                var view = Object.Instantiate(prefab, _holder);
                view.Setup(_hiddens[i]);
                _views.Add(view);
            }
        }

        public void Update() {
            float dt = Time.deltaTime;

            Vector2 cursorPos = _cosmos.CursorWorldPos;
            bool isFocus = _cosmos.CursorState == CursorState.Focus;

            for (int hi = 0; hi < _hiddens.Count; hi++) {
                var hidden = _hiddens[hi];
                hidden.Update(dt, cursorPos, isFocus);
            }

            // DO: сделать подписку вьюхи на стейт-ченжед
            foreach (var view in _views)
                view.UpdateManual();
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