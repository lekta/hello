using UnityEngine;

namespace LH.Api {
    public interface IInput {
        Vector2 ScreenPosition { get; }
        Vector2 ScreenOffset { get; }
        bool ActionDown { get; }
        bool ActionUp { get; }
        bool ActionHeld { get; }
    }
}