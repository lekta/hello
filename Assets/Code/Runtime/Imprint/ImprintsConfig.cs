using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LH.Imprint {
    [CreateAssetMenu(menuName = "LH/Imprints Config")]
    public class ImprintsConfig : ScriptableObject {
        public List<ImprintConfig> Imprints = new();

        private Dictionary<int, ImprintConfig> _cache;


        public ImprintConfig GetImprint(int imprintId) {
            if (_cache == null) {
                _cache = Imprints.ToDictionary(kv => kv.Id);
            }
            return _cache.GetValueOrDefault(imprintId);
        }

#if UNITY_EDITOR
        private void OnValidate() => _cache = null;
#endif
    }
}