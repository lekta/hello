using LH.Api;
using UnityEngine;

namespace LH.Player {
    public class PlayerInput : IInput, IUpdatable {
        public Vector2 ScreenPosition { get; private set; }
        public Vector2 ScreenOffset { get; private set; }
        public bool ActionDown { get; private set; }
        public bool ActionUp { get; private set; }
        public bool ActionHeld { get; private set; }


        public void Update(float dt) {
            ScreenPosition = Input.mousePosition;

            Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            var offset = ((Vector2)Input.mousePosition - center) / center;
            ScreenOffset = offset.magnitude > 1f ? offset.normalized : offset;

            ActionDown = Input.GetMouseButtonDown(0);
            ActionUp = Input.GetMouseButtonUp(0);
            ActionHeld = Input.GetMouseButton(0);
        }
    }
}
