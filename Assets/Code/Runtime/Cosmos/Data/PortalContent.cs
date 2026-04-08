using System;
using LH.Domain;
using LH.Imprint;

namespace LH.Cosmos {
    [Serializable]
    public class PortalContent : IHiddenContent {
        [ImprintSelect] public int ImprintId;

        public override string ToString() => $"Portal ({DebugInfoProvider.GetImprintName(ImprintId)})";
    }
}
