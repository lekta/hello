using LH.Api;
using LH.Domain;
using UnityEngine;

namespace LH.Cosmos {
    public class CosmosCursor {
        private const float IDLE_DELAY = 2f;
        private const float IDLE_FADE = 3f;
        private const float RECOVER_TIME = 1f;

        private static IInput Input => GameContext.Input;

        private Camera _camera;
        private CursorView _view;

        private Vector2 _prevPosition;
        private float _idleTime;

        public Vector2 Position;
        public float Activity;
        public CursorState State;


        public void Init(CosmosController cosmos) {
            _camera = cosmos.Camera;

            _view = cosmos.Cursor;
            _view.Setup(this);

            Cursor.visible = false;
        }

        public void Update() {
            Position = _camera.ScreenToWorldPoint(Input.ScreenPosition);
            UpdateState();
            UpdateActivity();
            _prevPosition = Position;
        }

        private void UpdateState() {
            if (Input.ActionDown)
                State = CursorState.Focus;
            else if (Input.ActionUp)
                State = CursorState.Idle;
        }

        private void UpdateActivity() {
            float dt = Time.deltaTime;
            bool moving = (Position - _prevPosition).sqrMagnitude > 0.01f;

            if (moving) {
                _idleTime = 0f;
                Activity = Mathf.MoveTowards(Activity, 1f, dt / RECOVER_TIME);
            } else {
                _idleTime += dt;
                if (_idleTime > IDLE_DELAY)
                    Activity = Mathf.MoveTowards(Activity, 0f, dt / IDLE_FADE);
            }
        }
    }

    public enum CursorState {
        Idle = 0,
        Focus = 1,
        Aim = 2,
        Scatter = 3
    }
}