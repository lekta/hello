using LH.Api;
using UnityEngine;

namespace LH.Domain {
    public class GameContext {
        public static IInput Input { get; private set; }
        public static ISaveSystem Save { get; private set; }
        public static IGameState GameState { get; private set; }
        private static bool _isSettled;


        public static void Setup(IInput input, ISaveSystem save, IGameState gameState) {
            if (_isSettled) {
                Debug.LogError("Game Context already settled!");
            }
            _isSettled = true;

            Input = input;
            Save = save;
            GameState = gameState;
        }

        // DO: в тестах сбрасывать
        public static void Reset() {
            Input = null;
            Save = null;
            GameState = null;
        }
    }
}