#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LH.Domain {
    public class EditorPrefBool {
#pragma warning disable 67, 414, 649
        private readonly string _prefsKey;

        private readonly bool _defEditorValue;
        private readonly bool _defPlayerValue;

        private bool? _value;
#pragma warning restore 67, 414, 649

        public bool Value {
#if UNITY_EDITOR
            get {
                if (_value == null) {
                    _value = EditorPrefs.GetBool(_prefsKey, _defEditorValue);
                }
                return _value.Value;
            }
            set {
                if (_value != value) {
                    _value = value;

                    EditorPrefs.SetBool(_prefsKey, value);
                }
            }
#else
            // this is default behaviour in player for insurance; BUT better to watch to NOT use it in production

            get { return _defPlayerValue; }
            set { }
#endif
        }

        public void Toggle() => Value = !Value;


        public EditorPrefBool(string prefsKey, bool defaultValue = false, bool? defaultPlayerValue = null) {
            _prefsKey = prefsKey;
            _defEditorValue = defaultValue;
            _defPlayerValue = defaultPlayerValue ?? defaultValue;
        }

        public static implicit operator bool(EditorPrefBool prefBool) {
            return prefBool.Value;
        }
    }
}