using UnityEngine;

namespace LH.Cosmos {
    public class CameraShake {
        private CameraShakeParams _params;

        private float _trauma;
        private float _sustainedTrauma;
        private float _seed;

        public void Init(CameraShakeParams shakeParams) {
            _params = shakeParams;
            _seed = Random.Range(0f, 1000f);
        }

        public void AddTrauma(float amount) {
            _trauma = Mathf.Min(_trauma + amount, 1f);
        }

        public void SetSustained(float trauma) {
            _sustainedTrauma = Mathf.Clamp01(trauma);
        }

        public Vector2 GetOffset(float dt) {
            _trauma = Mathf.Max(_trauma, _sustainedTrauma);

            if (_trauma < 0.001f)
                return Vector2.zero;

            float shake = _trauma * _trauma;
            float time = Time.time * _params.Frequency;

            float ox = (Mathf.PerlinNoise(_seed, time) - 0.5f) * 2f;
            float oy = (Mathf.PerlinNoise(_seed + 100f, time) - 0.5f) * 2f;
            Vector2 offset = new Vector2(ox, oy) * shake * _params.MaxOffset;

            _trauma = Mathf.MoveTowards(_trauma, _sustainedTrauma, _params.Decay * dt);

            return offset;
        }
    }
}
