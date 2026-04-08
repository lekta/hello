using UnityEngine;

namespace LH.Story {
    [CreateAssetMenu(menuName = "LH/Story Config")]
    public class StoryConfig : ScriptableObject {
        public int Id;
        public string SceneName;
    }
}
