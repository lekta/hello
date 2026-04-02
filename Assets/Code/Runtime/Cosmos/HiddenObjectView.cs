using UnityEngine;

namespace LH.Cosmos {
    public class HiddenObjectView : MonoBehaviour {
        // DO: объект-тизер - то, что будет тизерить, пока скрытое не раскрыто
        [SerializeField] private SpriteRenderer _image;

        public HiddenObjectData Data { get; private set; }

        private bool _wasRevealed;


        public void Setup(HiddenObjectData data) {
            Data = data;

            transform.position = new Vector3(data.Position.x, data.Position.y, 0f);
            _image.gameObject.SetActive(false);
            _wasRevealed = false;
        }

        public void UpdateManual() {
            if (Data.Revealed && !_wasRevealed) {
                _wasRevealed = true;
                _image.gameObject.SetActive(true);
            }
            
            // DO: добавить вращение наверно, и пусть его тоже трясёт нахуй
        }
    }
}