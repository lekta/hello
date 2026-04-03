#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LH.Domain {
    public class EditorPrefInt {
#pragma warning disable 67, 414, 649

        private readonly string _prefsKey;

        private readonly int _defEditorValue;
        private readonly int _defPlayerValue;

        private int? _value;
#pragma warning restore 67, 414, 649

        public int Value {
#if UNITY_EDITOR
            get {
                if (_value == null) {
                    _value = EditorPrefs.GetInt(_prefsKey, _defEditorValue);
                }
                return _value.Value;
            }
            set {
                if (_value != value) {
                    _value = value;

                    EditorPrefs.SetInt(_prefsKey, value);
                }
            }
#else
            // this is default behaviour in player for insurance; BUT better to watch to NOT use it in production

            get { return _defPlayerValue; }
            set { }
#endif
        }


        public EditorPrefInt(string prefsKey, int defaultValue = 0, int? defaultPlayerValue = null) {
            _prefsKey = prefsKey;
            _defEditorValue = defaultValue;
            _defPlayerValue = defaultPlayerValue ?? defaultValue;
        }

        public static implicit operator int(EditorPrefInt prefInt) {
            return prefInt.Value;
        }
    }
}