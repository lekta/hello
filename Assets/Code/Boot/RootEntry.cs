using LH.Domain;
using LH.Player;
using UnityEngine;

namespace LH.Boot {
    public class RootEntry : MonoBehaviour {
        public RootConfig RootConfig;

        private void Awake() {
            Debug.Log("Hello Awaken..");

            Setup();
        }

        private void Setup() {
            var input = new PlayerInput();

            GameContext.Setup(input);

            Updater.Run(new() { input });
        }
    }
}