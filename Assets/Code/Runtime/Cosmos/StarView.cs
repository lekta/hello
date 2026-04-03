using LH.Domain;
using UnityEngine;

namespace LH.Cosmos {
    public class StarView : MonoBehaviour {
        [SerializeField] private SpriteRenderer _image;

        private StarData _data;
        private Vector2 _lastPosition;
        private float _lastScale;


        public void Setup(StarData data) {
            _data = data;
            _image.color = data.Color;
            gameObject.SetActive(true);
        }

        public void TurnOff() {
            gameObject.SetActive(false);
            _data = null;
        }

        public void ManualUpdate() {
            if (_lastPosition != _data.Position) {
                _lastPosition = _data.Position;
                transform.localPosition = _lastPosition;
            }
            if (_lastScale != _data.Scale) {
                _lastScale = _data.Scale;
                transform.localScale = Vector3.one * _lastScale;
            }

            _image.color = (_data.Color * _data.Brightness).WithAlpha(1f);
        }
    }
}