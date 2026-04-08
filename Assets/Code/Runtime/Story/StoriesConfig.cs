using System.Collections.Generic;
using UnityEngine;

namespace LH.Story {
    [CreateAssetMenu(menuName = "LH/Stories Config")]
    public class StoriesConfig : ScriptableObject {
        public List<StoryConfig> Stories = new();

        private Dictionary<int, StoryConfig> _cache;

        public StoryConfig GetStory(int storyId) {
            if (_cache == null) {
                _cache = new Dictionary<int, StoryConfig>(Stories.Count);
                foreach (var s in Stories)
                    _cache[s.Id] = s;
            }
            return _cache.TryGetValue(storyId, out var story) ? story : null;
        }
    }
}
