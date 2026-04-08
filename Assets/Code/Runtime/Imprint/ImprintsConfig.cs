using System.Collections.Generic;
using UnityEngine;

namespace LH.Imprint {
    [CreateAssetMenu(menuName = "LH/Imprints Config")]
    public class ImprintsConfig : ScriptableObject {
        public List<ImprintConfig> Imprints = new();

        private Dictionary<int, ImprintConfig> _cache;

        public ImprintConfig GetImprint(int imprintId) {
            if (_cache == null) {
                _cache = new Dictionary<int, ImprintConfig>(Imprints.Count);
                foreach (var s in Imprints)
                    _cache[s.Id] = s;
            }
            return _cache.TryGetValue(imprintId, out var imprint) ? imprint : null;
        }
    }
}
