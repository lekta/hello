using LH.Domain;
using UnityEngine;

namespace LH.Cosmos {
    public class CursorView : MonoBehaviour {
        private const float IDLE_EMISSION = 10f;
        private const float IDLE_RADIUS_THICKNESS = 0.1f;

        [SerializeField] private SpriteRenderer _image;

        // первые частицы - локальные, следующие за курсором; вторые - распыляющиеся, остаются в мировых координатах
        [SerializeField] private ParticleSystem _linkParticles;
        [SerializeField] private ParticleSystem _worldParticles;

        private CosmosCursor _cursor;
        private CursorState _prevState;
        private float _baseEmissionRate;
        private float _baseRadius;
        private float _baseRadiusThickness;
        private Color _baseParticleColor;
        private float _activeRadiusThickness;
        private ParticleSystem.Particle[] _particleBuffer;


        public void Setup(CosmosCursor cursor) {
            _cursor = cursor;

            var emission = _linkParticles.emission;
            _baseEmissionRate = emission.rateOverTime.constant;

            var shape = _linkParticles.shape;
            _baseRadius = shape.radius;
            _baseRadiusThickness = shape.radiusThickness;

            var main = _linkParticles.main;
            _baseParticleColor = main.startColor.color;
        }

        private void LateUpdate() {
            transform.SetPositionXY(_cursor.Position);
            UpdateParticles();
        }

        private void UpdateParticles() {
            if (_cursor.State != _prevState) {
                ApplyState(_cursor.State);
                _prevState = _cursor.State;
            }

            float activity = _cursor.Activity;

            var emission = _linkParticles.emission;
            emission.rateOverTime = Mathf.Lerp(IDLE_EMISSION, _baseEmissionRate, activity);

            var shape = _linkParticles.shape;
            shape.radiusThickness = Mathf.Lerp(IDLE_RADIUS_THICKNESS, _activeRadiusThickness, activity);
        }

        private void ApplyState(CursorState state) {
            bool focus = state == CursorState.Focus;
            Color color = focus ? Color.red : _baseParticleColor;

            RecolorExistingParticles(color);

            var shape = _linkParticles.shape;
            shape.radius = focus ? _baseRadius * 1.5f : _baseRadius;
            _activeRadiusThickness = focus ? _baseRadiusThickness * 0.5f : _baseRadiusThickness;

            var main = _linkParticles.main;
            main.startColor = color;
        }

        private void RecolorExistingParticles(Color color) {
            int count = _linkParticles.particleCount;
            if (count == 0) return;

            if (_particleBuffer == null || _particleBuffer.Length < count)
                _particleBuffer = new ParticleSystem.Particle[count];

            _linkParticles.GetParticles(_particleBuffer, count);
            Color32 c32 = color;
            for (int i = 0; i < count; i++)
                _particleBuffer[i].startColor = c32;
            _linkParticles.SetParticles(_particleBuffer, count);
        }
    }
}