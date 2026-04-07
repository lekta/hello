using System;

namespace LH.Cosmos {
    [Serializable]
    public class KeyContent : IHiddenContent {
        public string KeyId;

        public override string ToString() => $"Key ({KeyId})";
    }
}
