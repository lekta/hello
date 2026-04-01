using UnityEngine;

namespace LH.Cosmos {
    public class CosmosCamera {
        private CameraMoveParams _params;
        private Camera _camera;
        private Transform _background;

        private float _fieldRadius;
        private Vector2 _velocity;
        private Vector2 _bgHalfSize;


        public void Init(Camera camera, Transform background, float fieldRadius, CameraMoveParams moveParams) {
            _camera = camera;
            _background = background;
            _fieldRadius = fieldRadius;
            _params = moveParams;

            var sr = background.GetComponent<SpriteRenderer>();
            if (sr != null)
                _bgHalfSize = sr.bounds.extents;
        }

        public void Update() {
            var camPos = UpdatePosition();
            UpdateParallax(camPos);
        }

        private Vector2 UpdatePosition() {
            float dt = Time.deltaTime;
            float screenWidth = _camera.orthographicSize * 2f * _camera.aspect;
            float hardLimit = _fieldRadius + screenWidth * _params.BoundaryOvershoot;

            Vector2 camPos = _camera.transform.position;

            UpdateVelocity(screenWidth);
            DumpVelocityByPos(camPos, hardLimit);

            camPos += _velocity * dt;

            // Жёсткий лимит
            if (camPos.magnitude > hardLimit) {
                camPos = camPos.normalized * hardLimit;
                float outward = Vector2.Dot(_velocity, camPos.normalized);
                if (outward > 0f)
                    _velocity -= camPos.normalized * outward;
            }

            _camera.transform.position = new Vector3(camPos.x, camPos.y, _camera.transform.position.z);

            return camPos;
        }

        private void UpdateVelocity(float screenWidth) {
            float dt = Time.deltaTime;
            float maxSpeed = screenWidth * _params.ScrollSpeed;

            // Позиция мыши относительно центра экрана, нормализована к [-1, 1]
            Vector2 mouse = Input.mousePosition;
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 offset = (mouse - screenCenter) / screenCenter;

            if (offset.magnitude > 1f)
                offset = offset.normalized;

            // Мёртвая зона → скорость нарастает до макс. на заданной зоне
            float mag = offset.magnitude;
            Vector2 targetVel = Vector2.zero;
            if (mag > _params.DeadZone) {
                float t = Mathf.Clamp01((mag - _params.DeadZone) / (_params.MaxSpeedZone - _params.DeadZone));
                targetVel = offset.normalized * t * maxSpeed;
            }

            _velocity = Vector2.Lerp(_velocity, targetVel, _params.Smoothing * dt);
        }

        // Мягкая граница
        private void DumpVelocityByPos(Vector2 camPos, float hardLimit) {
            float camDist = camPos.magnitude;
            if (camDist < _fieldRadius || camDist < .001f) {
                return;
            }
            float dt = Time.deltaTime;
            float halfWidth = _camera.orthographicSize * _camera.aspect;

            float overshoot = camDist - _fieldRadius;
            float maxOvershoot = hardLimit - _fieldRadius;
            float norm = Mathf.Clamp01(overshoot / maxOvershoot);
            Vector2 pushBack = -camPos.normalized * norm * norm * _params.BoundaryStiffness * halfWidth * 2f;

            _velocity += pushBack * dt;
            _velocity *= 1f - norm * 5f * dt;
        }

        private void UpdateParallax(Vector2 camPos) {
            float parallax = _params.Parallax;
            float pixelSize = _camera.orthographicSize * 2f / Screen.height;
            float margin = pixelSize * 2f;
            float maxShiftX = _bgHalfSize.x - _camera.orthographicSize * _camera.aspect - margin;
            float maxShiftY = _bgHalfSize.y - _camera.orthographicSize - margin;

            float offsetX = SmoothClamp(camPos.x * (parallax - 1f), maxShiftX);
            float offsetY = SmoothClamp(camPos.y * (parallax - 1f), maxShiftY);

            _background.position = new Vector3(camPos.x + offsetX, camPos.y + offsetY, _background.position.z);
        }

        private static float SmoothClamp(float value, float limit) {
            if (limit <= 0f)
                return 0f;
            float t = Mathf.Clamp(value / limit, -10f, 10f);
            float e = Mathf.Exp(2f * t);
            return limit * (e - 1f) / (e + 1f);
        }
    }
}