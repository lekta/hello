using UnityEngine;

namespace LH.Cosmos {
    public class HiddenObjectView : MonoBehaviour {
        [SerializeField] private ParticleSystem _teaserParticles;
        [SerializeField] private SpriteRenderer _image;

        public HiddenObject Hidden { get; private set; }
        public HiddenObjectData Data => Hidden?.Data;

        private bool _wasRevealed;


        public void Setup(HiddenObject hidden) {
            Hidden = hidden;
            _wasRevealed = false;

            transform.position = Data.Position;

            var teaserShape = _teaserParticles.shape;
            teaserShape.radius = Data.Radius;

            _image.gameObject.SetActive(false);
        }

        public void UpdateManual() {
            if (Hidden.Revealed && !_wasRevealed) {
                _wasRevealed = true;

                _teaserParticles.Stop();
                _image.gameObject.SetActive(true);
            }

            // DO: добавить вращение наверно, и пусть его тоже трясёт нахуй
        }
    }
}