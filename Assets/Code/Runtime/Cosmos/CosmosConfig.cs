using System;
using LH.Cosmos;
using UnityEngine;

namespace LH {
    [Serializable]
    public class CosmosConfig : ScriptableObject {
        public CosmicBodyView CosmicBody;

        public int Seed = 42;
        public int BodyCount = 600;
        [Tooltip("Радиус поля звёзд в ширинах экрана")]
        public float RadiusInScreens = 2f;
    }
}