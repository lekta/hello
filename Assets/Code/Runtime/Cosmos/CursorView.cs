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
        private float _baseEmissionRate;
        private float _baseRadiusThickness;
        private Color _baseParticleColor;


        public void Setup(CosmosCursor cursor) {
            _cursor = cursor;

            var emission = _linkParticles.emission;
            _baseEmissionRate = emission.rateOverTime.constant;

            var shape = _linkParticles.shape;
            _baseRadiusThickness = shape.radiusThickness;

            var main = _linkParticles.main;
            _baseParticleColor = main.startColor.color;
        }

        private void LateUpdate() {
            transform.SetPositionXY(_cursor.Position);
            UpdateParticles();
        }

        private void UpdateParticles() {
            float activity = _cursor.Activity;

            var emission = _linkParticles.emission;
            emission.rateOverTime = Mathf.Lerp(IDLE_EMISSION, _baseEmissionRate, activity);

            var shape = _linkParticles.shape;
            shape.radiusThickness = Mathf.Lerp(IDLE_RADIUS_THICKNESS, _baseRadiusThickness, activity);

            var main = _linkParticles.main;
            main.startColor = _cursor.State == CursorState.Focus ? Color.red : _baseParticleColor;
        }
    }
}