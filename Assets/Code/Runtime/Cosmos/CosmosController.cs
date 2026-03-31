using UnityEngine;

namespace LH.Cosmos {
    public class CosmosController : MonoBehaviour {
        public CosmosConfig Config;

        private readonly CosmicBodiesManager _bodies = new();


        private void Awake() {
            _bodies.Init(this);
        }

        private void Update() {
            _bodies.Update();
        }
    }
}