using UnityEngine;

namespace LH.Imprint {
    [CreateAssetMenu(menuName = "LH/Imprint Config")]
    public class ImprintConfig : ScriptableObject {
        public int Id;
        public string SceneName;
    }
}
