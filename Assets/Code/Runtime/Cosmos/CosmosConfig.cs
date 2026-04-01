using LH.Cosmos;
using UnityEngine;

namespace LH {
    [CreateAssetMenu(menuName = "LH/Cosmos Config")]
    public class CosmosConfig : ScriptableObject {
        public CosmicBodyView CosmicBody;

        public int Seed = 42;
        public int BodyCount = 600;

        public float FieldRadius = 3000f;
    }
}