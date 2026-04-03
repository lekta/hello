using LH.Domain;
using UnityEngine;

namespace LH.Cosmos {
    public class CosmosController : MonoBehaviour {
        public CosmosConfig Config;
        public CameraConfig CameraConfig;

        public Camera Camera;
        public CursorView Cursor;
        public Transform Background;

        private readonly StarsManager _stars = new();
        private readonly HiddenObjectsManager _hiddens = new();
        private readonly CosmosCamera _cosmosCamera = new();
        private readonly CosmosCursor _cursor = new();
        private readonly CameraShake _shake = new();

        public Vector2 CursorWorldPos => _cursor.Position;
        public float CursorActivity => _cursor.Activity;
        public CursorState CursorState => _cursor.State;


        private void Awake() {
            float fieldRadius = Config.FieldRadius;

            _stars.Init(this, fieldRadius);
            _hiddens.Init(this, fieldRadius, _stars.Datas);
            _cursor.Init(this);
            _shake.Init(CameraConfig.Shake);
            _cosmosCamera.Init(Camera, Background, _cursor, fieldRadius, CameraConfig.Move, _shake);
        }

        private void Update() {
            _cursor.Update();

            if (GameContext.Input.ActionDown)
                _shake.AddTrauma(CameraConfig.Shake.FocusImpulse);

            float maxReveal = 0f;
            foreach (var h in _hiddens.Hiddens) {
                if (!h.Revealed && h.FocusTime > 0f)
                    maxReveal = Mathf.Max(maxReveal, h.FocusTime / HiddenObject.REVEAL_FOCUS_TIME);
            }
            _shake.SetSustained(maxReveal * CameraConfig.Shake.RevealShakeMax);

            _cosmosCamera.Update();
            _hiddens.Update();
            _stars.Update(_hiddens.Hiddens);
        }
    }
}
