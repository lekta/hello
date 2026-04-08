using LH.Domain;
using UnityEngine;

namespace LH.Imprint {
    [CreateAssetMenu(menuName = "LH/Imprint Config")]
    public class ImprintConfig : ScriptableObject {
        [ReadOnly] public int Id;
        [ImprintSceneSelect] public string SceneName;
    }
}
