using UnityEngine;

namespace LH.Cosmos {
    public class CosmosCursor {
        private Camera _camera;
        private CursorView _view;

        public Vector2 Position;
        public CursorState State;


        public void Init(CosmosController cosmos) {
            _camera = cosmos.Camera;

            _view = cosmos.Cursor;
            _view.Setup(this);

            Cursor.visible = false;
        }

        public void Update() {
            // DO: 
            Position = _camera.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    public enum CursorState {
        Idle = 0,
        Focus = 1,
        Aim = 2,
        Scatter = 3
    }
}