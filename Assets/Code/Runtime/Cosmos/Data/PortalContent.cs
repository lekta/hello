using System;

namespace LH.Cosmos {
    [Serializable]
    public class PortalContent : IHiddenContent {
        public int StoryId;

        public override string ToString() => $"Portal → {StoryId}";
    }
}
