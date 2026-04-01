using UnityEngine;

namespace LH.Cosmos {
    public class CosmosController : MonoBehaviour {
        public CosmosConfig Config;
        public CursorConfig CursorConfig;

        public Camera Camera;
        public CursorView Cursor;
        public Transform Background;

        private readonly CosmicBodiesManager _bodies = new();
        private readonly CosmosCamera _cosmosCamera = new();
        private readonly CosmosCursor _cursor = new();


        private void Awake() {
            float screenWidth = Camera.orthographicSize * 2f * Camera.aspect;
            float fieldRadius = screenWidth * Config.RadiusInScreens;

            _bodies.Init(this, fieldRadius);
            _cosmosCamera.Init(Camera, Background, fieldRadius, CursorConfig.CameraScrollSpeed);
            _cursor.Init(this);
        }

        private void Update() {
            _cosmosCamera.Update();
            _cursor.Update();
            _bodies.Update();
        }
    }
}