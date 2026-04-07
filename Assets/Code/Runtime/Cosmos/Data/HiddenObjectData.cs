using System;
using System.Collections.Generic;
using UnityEngine;

namespace LH.Cosmos {
    [Serializable]
    public class HiddenObjectData {
        // DO: read only
        public int Id;

        public Vector2 Position;
        public float Radius = 150f;

        [SerializeReference] public List<IHiddenBehavior> Behaviors = new();
        [SerializeReference] public IHiddenContent Content;

        public List<int> Dependencies = new List<int>();
    }
}
