using UnityEngine;

namespace LH.Cosmos {
    [CreateAssetMenu(menuName = "LH/Cosmos Config")]
    public class CosmosConfig : ScriptableObject {
        public CosmicBodyView CosmicBody;
        public HiddenObjectView HiddenObject;

        // DO: можно перенести в "уровни", и оставить геймплей чисто на поиске объектов
        public int Seed = 42;
        public int BodyCount = 600;
        public int HiddenObjectCount = 7;

        public float FieldRadius = 3000f;
    }
}