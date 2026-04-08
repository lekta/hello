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

        private readonly Dictionary<int, HiddenObject> _lookup = new();
        private readonly Dictionary<int, List<HiddenObject>> _dependents = new();

        private readonly List<HiddenObjectView> _views = new();


        public void Init(CosmosController cosmos, float fieldRadius, IReadOnlyList<StarData> starDatas) {
            _cosmos = cosmos;
            var sw = Stopwatch.StartNew();

            _holder = new GameObject("hidden").transform;
            _holder.SetParent(cosmos.transform);
            _holder.position = new Vector3(0, 0, 500);

            var cfg = cosmos.Config;
            CreateHiddenObjects(cfg.Seed + 7777, cfg.Hiddens, starDatas);
            CreateViews(cfg.HiddenObject);

            Debug.Log($"Hidden objects ({_hiddens.Count}) generated in {sw.ElapsedMilliseconds} ms");
        }

        private void CreateHiddenObjects(int seed, List<HiddenObjectData> entries, IReadOnlyList<StarData> stars) {
            _hiddens.Clear();
            _lookup.Clear();
            _dependents.Clear();
            var rng = new Random(seed);

            for (int i = 0; i < entries.Count; i++) {
                var data = entries[i];
                var hidden = new HiddenObject(data);

                var affected = stars
                    .Select(s => (s.Index, Dist: (s.AnchorPosition - data.Position).magnitude))
                    .Where(t => t.Dist < data.Radius)
                    .ToList();

                hidden.Init(affected, rng);
                _hiddens.Add(hidden);
                _lookup[hidden.Id] = hidden;
            }

            // Инициализация локов после создания всех объектов
            foreach (var hidden in _hiddens)
                hidden.InitLocks(_lookup);

            // Обратный индекс: какой скрытый кого разблокирует
            foreach (var hidden in _hiddens) {
                if (hidden.Data.Locks == null) continue;
                foreach (var lck in hidden.Data.Locks) {
                    if (lck is HiddenLock hl) {
                        if (!_dependents.TryGetValue(hl.HiddenId, out var list)) {
                            list = new List<HiddenObject>();
                            _dependents[hl.HiddenId] = list;
                        }
                        list.Add(hidden);
                    }
                }
            }
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

            // DO: если будет тормозить, попробовать реализовать через SoA
            for (int hi = 0; hi < _hiddens.Count; hi++) {
                var hidden = _hiddens[hi];
                bool wasRevealed = hidden.Revealed;
                hidden.Update(dt, cursorPos, isFocus);

                if (hidden.Revealed && !wasRevealed)
                    NotifyDependents(hidden.Id);
            }

            // DO: сделать подписку вьюхи на стейт-ченжед
            foreach (var view in _views)
                view.UpdateManual();
        }

        private void NotifyDependents(int revealedId) {
            if (!_dependents.TryGetValue(revealedId, out var list))
                return;
            foreach (var dep in list)
                dep.RemoveLock(revealedId);
        }
    }
}
