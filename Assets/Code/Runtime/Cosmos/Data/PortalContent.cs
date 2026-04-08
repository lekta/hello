using System;
using LH.Domain;

namespace LH.Cosmos {
    [Serializable]
    public class PortalContent : IHiddenContent {
        [ImprintSelect] public int ImprintId;

        private string GetImprintName() {
            // DO: 
            return ImprintId.ToString();
        }

        public override string ToString() => $"Portal ({GetImprintName()})";
    }
}
