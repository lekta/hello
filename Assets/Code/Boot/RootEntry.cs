using System.IO;
using LH.Cheats;
using LH.Domain;
using LH.Player;
using LH.Save;
using UnityEngine;

namespace LH.Boot {
    public class RootEntry : MonoBehaviour {
        public RootConfig RootConfig;

        private void Awake() {
            Debug.Log("Hello Awaken..");

            Setup();
        }

        private void Setup() {
            CheatsPrepare();
            
            var input = new PlayerInput();
            var save = new SaveSystem();
            save.Init();

            GameContext.Setup(input, save);

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