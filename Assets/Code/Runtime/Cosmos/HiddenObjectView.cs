using UnityEngine;

namespace LH.Cosmos {
    public class HiddenObjectView : MonoBehaviour {
        [SerializeField] private ParticleSystem _teaserParticles;
        [SerializeField] private SpriteRenderer _image;

        public HiddenObject Hidden { get; private set; }
        public HiddenObjectData Data => Hidden?.Data;

        private bool _wasRevealed;
        private bool _wasActive;


        public void Setup(HiddenObject hidden) {
            Hidden = hidden;
            _wasRevealed = hidden.Revealed;
            _wasActive = hidden.Active;

            transform.localPosition = Data.Position;

            var teaserShape = _teaserParticles.shape;
            teaserShape.radius = Data.Radius;

            RefreshState();
        }

        public void UpdateManual() {
            if (!_wasActive && Hidden.Active) {
                _wasActive = true;
                RefreshState();
            }

            if (Hidden.Revealed && !_wasRevealed) {
                _wasRevealed = true;
                RefreshState();
            }

            // DO: добавить вращение наверно, и пусть его тоже трясёт нахуй
        }

        private void RefreshState() {
            if (_wasRevealed) {
                _teaserParticles.Stop();
                _image.gameObject.SetActive(true);
            } else if (_wasActive) {
                _teaserParticles.Play();
                _image.gameObject.SetActive(false);
            } else {
                _teaserParticles.Stop();
                _image.gameObject.SetActive(false);
            }
        }
    }
}