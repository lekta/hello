using LH.Story;
using UnityEngine;

namespace LH {
    public class RootConfigs : ScriptableObject {
        public static RootConfigs Instance { get; private set; }

        public StoriesConfig Stories;

        public void Init() {
            Instance = this;
        }
    }
}