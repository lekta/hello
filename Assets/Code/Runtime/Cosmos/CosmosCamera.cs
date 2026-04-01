using UnityEngine;

namespace LH.Cosmos {
    public class CosmosCamera {
        private const float DEAD_ZONE = 0.25f;      // 1/4 от центра до края
        private const float MAX_SPEED_ZONE = 0.67f; // 2/3 от центра — макс. скорость
        private const float SMOOTHING = 4f;
        private const float BOUNDARY_STIFFNESS = 10f;
        private const float BOUNDARY_OVERSHOOT = 0.5f;
        private const float PARALLAX = 0.1f;

        private Camera _camera;
        private Transform _background;
        private float _fieldRadius;
        private float _hardLimit;
        private float _maxSpeed;
        private Vector2 _velocity;


        public void Init(Camera camera, Transform background, float fieldRadius, float scrollSpeed) {
            _camera = camera;
            _background = background;
            _fieldRadius = fieldRadius;

            float screenWidth = _camera.orthographicSize * 2f * _camera.aspect;
            _hardLimit = fieldRadius + screenWidth * BOUNDARY_OVERSHOOT;
            _maxSpeed = screenWidth * scrollSpeed;
        }

        public void Update() {
            var camPos = UpdatePosition();

            // Параллакс фона
            if (_background != null)
                _background.position = new Vector3(camPos.x * PARALLAX, camPos.y * PARALLAX, _background.position.z);
        }

        private Vector2 UpdatePosition() {
            float dt = Time.deltaTime;
            Vector2 camPos = _camera.transform.position;

            UpdateVelocity();
            DumpVelocityByPos(camPos);

            camPos += _velocity * dt;

            // Жёсткий лимит
            if (camPos.magnitude > _hardLimit) {
                camPos = camPos.normalized * _hardLimit;
                float outward = Vector2.Dot(_velocity, camPos.normalized);
                if (outward > 0f)
                    _velocity -= camPos.normalized * outward;
            }

            _camera.transform.position = new Vector3(camPos.x, camPos.y, _camera.transform.position.z);

            return camPos;
        }

        private void UpdateVelocity() {
            float dt = Time.deltaTime;

            // Позиция мыши относительно центра экрана, нормализована к [-1, 1]
            Vector2 mouse = Input.mousePosition;
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 offset = (mouse - screenCenter) / screenCenter;

            if (offset.magnitude > 1f)
                offset = offset.normalized;

            // Мёртвая зона → скорость нарастает до макс. на 2/3 экрана
            float mag = offset.magnitude;
            Vector2 targetVel = Vector2.zero;
            if (mag > DEAD_ZONE) {
                float t = Mathf.Clamp01((mag - DEAD_ZONE) / (MAX_SPEED_ZONE - DEAD_ZONE));
                targetVel = offset.normalized * t * _maxSpeed;
            }

            _velocity = Vector2.Lerp(_velocity, targetVel, SMOOTHING * dt);
        }

        // Мягкая граница
        private void DumpVelocityByPos(Vector2 camPos) {
            float camDist = camPos.magnitude;
            if (camDist < _fieldRadius || camDist < .001f) {
                return;
            }
            float dt = Time.deltaTime;
            float halfWidth = _camera.orthographicSize * _camera.aspect;

            float overshoot = camDist - _fieldRadius;
            float maxOvershoot = _hardLimit - _fieldRadius;
            float norm = Mathf.Clamp01(overshoot / maxOvershoot);
            Vector2 pushBack = -camPos.normalized * norm * norm * BOUNDARY_STIFFNESS * halfWidth * 2f;

            _velocity += pushBack * dt;
            _velocity *= 1f - norm * 5f * dt;
        }
    }
}