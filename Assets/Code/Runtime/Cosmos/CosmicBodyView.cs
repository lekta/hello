using UnityEngine;

namespace LH.Cosmos {
    public class CosmicBodyView : MonoBehaviour {
        [SerializeField] private SpriteRenderer _image;

        private CosmicBodyData _data;
        private Vector2 _lastPosition;
        private float _lastScale;


        public void Setup(CosmicBodyData data) {
            _data = data;

            gameObject.SetActive(true);
        }

        public void TurnOff() {
            gameObject.SetActive(false);
        }

        public void Apply() {
            if (_lastPosition != _data.Position) {
                _lastPosition = _data.Position;
                transform.localPosition = _lastPosition;
            }
            if (_lastScale != _data.Scale) {
                _lastScale = _data.Scale;
                transform.localScale = Vector3.one * _lastScale;
            }
        }
    }
}