using UnityEngine;

namespace LH.Cosmos {
    public class CosmicBodyData {
        public int Index;

        public Vector2 AnchorPosition;
        public Vector2 Position;
        // ? public Vector2 Velocity;

        public float AnchorScale;
        public float Scale;

        // DO: надо ещё задать какую-то функцию "мерцания" и пару констант, чтобы чисто на них+время генерить микроотклонения
    }
}