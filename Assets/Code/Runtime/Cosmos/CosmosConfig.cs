using LH.Cosmos;
using UnityEngine;

namespace LH {
    public class CosmosConfig : ScriptableObject {
        public CosmicBodyView CosmicBody;

        // перенести в отдельный класс
        public int Seed = 42;
        public int BodyCount = 600;
        public float RadiusInScreens = 2f;
    }
}