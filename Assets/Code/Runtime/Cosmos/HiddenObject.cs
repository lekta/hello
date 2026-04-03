using System.Collections.Generic;
using System.Linq;
using LH.Domain;
using LH.Save;
using UnityEngine;
using Random = System.Random;

namespace LH.Cosmos {
    public class HiddenObject {
        public const float REVEAL_FOCUS_TIME = 1f;

        private const float ANOMALY_TREMOR = .3f;
        private const float BLACKOUT_DURATION = .5f;
        private const float BLACKOUT_INTERVAL_MIN = 40f;
        private const float BLACKOUT_INTERVAL_MAX = 50f;

        public HiddenObjectData Data { get; }
        public int Id => Data.Index;
        public Vector2 Position => Data.Position;
        public float Radius => Data.Radius;

        public IReadOnlyDictionary<int, float> AffectedStars { get; private set; }

        private float _blackoutTimer;
        private float _blackoutDelay;

        public float BlackoutCoef { get; private set; }
        public float TremorCoef { get; private set; }

        public float FocusTime { get; private set; }

        private HiddenObjectSave _save;
        public bool Revealed { get => _save.Revealed; private set => _save.Revealed = value; }


        public HiddenObject(HiddenObjectData data) {
            Data = data;
        }

        public void Init(List<(int, float)> bestAffected, Random rng) {
            AffectedStars = bestAffected.ToDictionary(t => t.Item1, t => t.Item2);

            // DO: блэкаут не у всех, а только по типу палинга
            _blackoutDelay = BLACKOUT_INTERVAL_MIN + (float)rng.NextDouble() * (BLACKOUT_INTERVAL_MAX - BLACKOUT_INTERVAL_MIN);
            _blackoutTimer = (float)rng.NextDouble() * _blackoutDelay;

            TremorCoef = .1f + rng.NextFloat() * ANOMALY_TREMOR;

            _save = GameContext.Save.GetHiddenState(Id) ?? new HiddenObjectSave { Id = Id };
        }


        public void Update(float dt, Vector2 cursorPos, bool isFocus) {
            if (Revealed)
                return;

            UpdateBlackout(dt);
            UpdateReveal(dt, cursorPos, isFocus);
        }

        private void UpdateBlackout(float dt) {
            _blackoutTimer += dt;
            if (_blackoutTimer < _blackoutDelay)
                return;

            if (_blackoutTimer - _blackoutDelay > BLACKOUT_DURATION) {
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
                    Reveal();
                }
            } else if (FocusTime > 0) {
                FocusTime = Mathf.MoveTowards(FocusTime, 0f, dt * 2f);
            }
        }

        private void Reveal() {
            Revealed = true;
            GameContext.Save.SetHiddenState(Id, _save);
        }
    }
}