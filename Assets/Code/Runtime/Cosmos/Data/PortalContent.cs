using System;

namespace LH.Cosmos {
    [Serializable]
    public class PortalContent : IHiddenContent {
        public string LocationId;

        public override string ToString() => $"Portal → {LocationId}";
    }
}
