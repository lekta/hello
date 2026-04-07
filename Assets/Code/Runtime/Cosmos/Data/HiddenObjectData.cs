using System;
using System.Collections.Generic;
using System.Text;
using LH.Domain;
using UnityEngine;

namespace LH.Cosmos {
    [Serializable]
    public class HiddenObjectData {
        [ReadOnly] public int Id;

        public Vector2 Position;
        public float Radius;

        [SerializeReference, SubtypeSelect] public List<IHiddenBehavior> Behaviors;
        [SerializeReference, SubtypeSelect] public IHiddenContent Content;

        public List<int> Dependencies;


        public void ResetToDefaults() {
            Position = Vector2.zero;
            Radius = 100;

            Behaviors = new List<IHiddenBehavior>();
            Content = null;
            Dependencies = new List<int>();
        }

        public string GetShortInfo() {
            var sb = new StringBuilder("#");
            sb.Append(Id);

            sb.Append(", r=");
            sb.Append((int)Radius);
            sb.Append(", ");
            sb.Append(Position.ToIntString());
            sb.Append(", beh: ");

            if (Behaviors.IsEmpty()) {
                sb.Append("none");
            } else if (Behaviors.Count == 1) {
                sb.Append(Behaviors[0]);
            } else {
                sb.Append("{");
                sb.Append(string.Join(", ", Behaviors));
                sb.Append("}");
            }

            sb.Append(", ");
            sb.Append(Content);

            return sb.ToString();
        }
    }
}