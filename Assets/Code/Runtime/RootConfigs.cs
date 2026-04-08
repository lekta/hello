using LH.Imprint;
using UnityEngine;

namespace LH {
    public class RootConfigs : ScriptableObject {
        public static RootConfigs Instance { get; private set; }

        public ImprintsConfig Imprints;

        public void Init() {
            Instance = this;
        }
    }
}