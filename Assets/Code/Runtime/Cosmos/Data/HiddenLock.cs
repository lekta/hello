using System;

namespace LH.Cosmos {
    [Serializable]
    public class HiddenLock : ILockCondition {
        public int HiddenId;

        public override string ToString() => $"Hidden #{HiddenId}";
    }
}
