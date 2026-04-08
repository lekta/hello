using System;

namespace LH.Cosmos {
    [Serializable]
    public class PortalContent : IHiddenContent {
        public int ImprintId;

        public override string ToString() => $"Portal → {ImprintId}";
    }
}
