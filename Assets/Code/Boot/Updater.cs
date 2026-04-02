using System.Collections.Generic;
using LH.Api;
using UnityEngine;

namespace LH.Boot {
    public class Updater : MonoBehaviour {
        private List<IUpdatable> _updatables;


        public static void Run(List<IUpdatable> updatables) {
            var updaterGo = new GameObject("Updater");
            DontDestroyOnLoad(updaterGo);

            var updater = updaterGo.AddComponent<Updater>();
            updater._updatables = updatables;
        }

        private void Update() {
            float dt = Time.deltaTime;

            foreach (var updatable in _updatables) {
                updatable.Update(dt);
            }
        }
    }
}