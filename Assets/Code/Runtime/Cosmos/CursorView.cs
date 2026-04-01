using LH.Domain;
using UnityEngine;

namespace LH.Cosmos {
    public class CursorView : MonoBehaviour {
        [SerializeField] private SpriteRenderer _image;

        // первые частицы - локальные, следующие за курсором; вторые - распыляющиеся, остаются в мировых координатах
        [SerializeField] private ParticleSystem _linkParticles;
        [SerializeField] private ParticleSystem _worldParticles;

        private CosmosCursor _cursor;


        public void Setup(CosmosCursor cursor) {
            _cursor = cursor;
        }

        private void LateUpdate() {
            transform.SetPositionXY(_cursor.Position);
        }
    }
}