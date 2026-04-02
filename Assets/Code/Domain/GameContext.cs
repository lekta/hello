using LH.Api;
using UnityEngine;

namespace LH.Domain {
    public class GameContext {
        public static IInput Input { get; private set; }
        private static bool _isSettled;

        public static void Setup(IInput input) {
            if (_isSettled) {
                Debug.LogError($"Game Context already settled!");
            }
            _isSettled = true;
            
            Input = input;
        }

        // DO: в тестах сбрасывать
        public static void Reset() {
            Input = null;
        }
    }
}