using UnityEngine;

namespace LH.Boot {
    public class Entry : MonoBehaviour {
        public RootConfig RootConfig;

        private void Awake() {
            Debug.Log("Hello..");
        }
    }
}