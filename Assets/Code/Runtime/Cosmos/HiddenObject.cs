using System.Collections.Generic;
using System.Linq;
using LH.Api;
using LH.Domain;
using LH.Save;
using UnityEngine;
using Random = System.Random;

namespace LH.Cosmos {
    public class HiddenObject {
        public const float REVEAL_FOCUS_TIME = 1f;
        public const float UNLOCK_WARMUP = .5f;

        private const float DEFAULT_TREMOR = .3f;
        private const float DEFAULT_BLACKOUT_DURATION = .5f;
        private const float DEFAULT_BLACKOUT_INTERVAL_MIN = 40f;
        private const float DEFAULT_BLACKOUT_INTERVAL_MAX = 50f;

        private static ISaveSystem Save => GameContext.Save;
        private static IGameState GameState => GameContext.GameState;

        public HiddenObjectData Data { get; }
        public int Id => Data.Id;
        public Vector2 Position => Data.Position;
        public float Radius => Data.Radius;

        public IReadOnlyDictionary<int, float> AffectedStars { get; private set; }

        private float _blackoutTimer;
        private float _blackoutDelay;
        private bool _hasBlackout;
        private float _blackoutDuration;

        public float BlackoutCoef { get; private set; }
        public float TremorCoef { get; private set; }

        public float FocusTime { get; private set; }

        private HiddenObjectSave _save;
        public bool Revealed { get => _save.Revealed; private set => _save.Revealed = value; }

        public bool Locked => _pendingLocks != null && _pendingLocks.Count > 0;
        public bool Active { get; private set; }
        private List<HiddenLock> _pendingLocks;
        private float _warmupTimer;


        public HiddenObject(HiddenObjectData data) {
            Data = data;
        }

        public void Init(List<(int, float)> bestAffected, Random rng) {
            AffectedStars = bestAffected.ToDictionary(t => t.Item1, t => t.Item2);

            InitBehaviors(rng);

            _save = Save.GetHiddenState(Id) ?? new HiddenObjectSave { Id = Id };
        }

        public void InitLocks(IReadOnlyDictionary<int, HiddenObject> lookup) {
            if (Data.Locks == null || Data.Locks.Count == 0) {
                Active = true;
                return;
            }

            _pendingLocks = new List<HiddenLock>();
            foreach (var lck in Data.Locks) {
                if (lck is HiddenLock hl && lookup.TryGetValue(hl.HiddenId, out var dep) && !dep.Revealed)
                    _pendingLocks.Add(hl);
            }
            Active = !Locked;
        }

        private void InitBehaviors(Random rng) {
            TremorCoef = 0f;
            _hasBlackout = false;

            foreach (var behavior in Data.Behaviors) {
                switch (behavior) {
                    case TremorBehavior tremor:
                        TremorCoef = .1f + rng.NextFloat() * tremor.Magnitude;
                        break;
                    case BlackoutBehavior blackout:
                        _hasBlackout = true;
                        _blackoutDuration = blackout.Duration;
                        _blackoutDelay = blackout.IntervalMin + (float)rng.NextDouble() * (blackout.IntervalMax - blackout.IntervalMin);
                        _blackoutTimer = (float)rng.NextDouble() * _blackoutDelay;
                        break;
                }
            }
        }


        public void Update(float dt, Vector2 cursorPos, bool isFocus) {
            if (Revealed)
                return;

            if (Locked)
                return;

            if (!Active) {
                _warmupTimer += dt;
                if (_warmupTimer >= UNLOCK_WARMUP)
                    Active = true;
                return;
            }

            UpdateBlackout(dt);
            UpdateReveal(dt, cursorPos, isFocus);
        }

        public void RemoveLock(int hiddenId) {
            if (_pendingLocks == null) return;
            for (int i = _pendingLocks.Count - 1; i >= 0; i--) {
                if (_pendingLocks[i].HiddenId == hiddenId) {
                    _pendingLocks.RemoveAt(i);
                    break;
                }
            }
        }

        private void UpdateBlackout(float dt) {
            if (!_hasBlackout)
                return;

            _blackoutTimer += dt;
            if (_blackoutTimer < _blackoutDelay)
                return;

            if (_blackoutTimer - _blackoutDelay > _blackoutDuration) {
                _blackoutTimer = 0f;

                BlackoutCoef = 0f;
            } else {
                // DO: не тупо выкл, а должно быть мерцание, как при помехах электричества
                BlackoutCoef = 1f;
            }
        }

        private void UpdateReveal(float dt, Vector2 cursorPos, bool isFocus) {
            bool isRevealing = false;

            float dist = (cursorPos - Position).magnitude;
            if (dist < Radius * .5f) {
                if (isFocus) {
                    isRevealing = true;
                }

                // DO: добавить логику анхайда - когда ещё не раскрыт фокусом, но под лупой уже "виднеется"
            }

            if (isRevealing) {
                FocusTime += dt;

                if (FocusTime >= REVEAL_FOCUS_TIME) {
                    SetRevealed();
                }
            } else if (FocusTime > 0) {
                FocusTime = Mathf.MoveTowards(FocusTime, 0f, dt * 2f);
            }
        }

        private void SetRevealed() {
            Revealed = true;
            Save.SetHiddenState(Id, _save);

            ApplyContents();
        }

        private void ApplyContents() {
            if (Data.Content is PortalContent portal)
                GameState.EnterImprint(portal.ImprintId);
        }
    }
}
