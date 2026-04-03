using LH.Domain;
using UnityEngine;

namespace LH.Cosmos {
    public class StarView : MonoBehaviour {
        [SerializeField] private SpriteRenderer _image;

        private Star _star;

        public Vector2 LastPosition { get; private set; }
        private float _lastScale;
        private Color _baseColor;
        private float _lastBrightness = -1f;


        public void Setup(Star star) {
            _star = star;

            var color = star.Data.Color;
            _baseColor = color;
            _image.color = color;

            Apply();
            gameObject.SetActive(true);
        }

        public void TurnOff() {
            gameObject.SetActive(false);
        }

        public void Apply() {
            var position = _star.Position;
            if (LastPosition != position) {
                LastPosition = position;
                transform.localPosition = LastPosition;
            }

            var scale = _star.Scale;
            if (_lastScale != scale) {
                _lastScale = scale;
                transform.localScale = Vector3.one * _lastScale;
            }

            var brightness = _star.Brightness;
            if (_lastBrightness != brightness) {
                _lastBrightness = brightness;
                _image.color = (_baseColor * brightness).WithAlpha(1f);
            }
        }
    }
}