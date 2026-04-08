using System.IO;
using LH.Cheats;
using LH.Cosmos;
using LH.Domain;
using LH.GameStates;
using LH.Player;
using LH.Save;
using UnityEngine;

namespace LH.Boot {
    public class RootEntry : MonoBehaviour {
        public RootConfigs RootConfigs;
        public CosmosController Cosmos;

        private void Awake() {
            Debug.Log("Hello Awaken..");

            Setup();
        }

        private void Setup() {
            CheatsPrepare();

            RootConfigs.Init();

            var input = new PlayerInput();
            var save = new SaveSystem();
            save.Init();

            var gameState = new GameStateController();
            gameState.Init(Cosmos);

            GameContext.Setup(input, save, gameState);

            Updater.Run(new() { input, save });
        }

        private void CheatsPrepare() {
#if UNITY_EDITOR
            if (CheatsParams.NeedRunGameFromStart) {
                File.Delete(SaveSystem.SavePath);
            }
#endif
        }
    }
}