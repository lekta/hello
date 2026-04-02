using UnityEngine;

namespace LH.Cosmos {
    public class HiddenObjectView : MonoBehaviour {
        [SerializeField] private GameObject _teaser;
        [SerializeField] private SpriteRenderer _image;

        public HiddenObject Hidden { get; private set; }
        public HiddenObjectData Data => Hidden?.Data;

        private bool _wasRevealed;


        public void Setup(HiddenObject hidden) {
            Hidden = hidden;

            transform.position = Data.Position;
            _image.gameObject.SetActive(false);
            _wasRevealed = false;
        }

        public void UpdateManual() {
            if (Hidden.Revealed && !_wasRevealed) {
                _wasRevealed = true;
                _image.gameObject.SetActive(true);
            }

            // DO: добавить вращение наверно, и пусть его тоже трясёт нахуй
        }
    }
}