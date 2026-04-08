using System.Collections.Generic;
using LH.Imprint;
using UnityEditor;
using UnityEngine;

namespace LH.Dev {
    [CustomEditor(typeof(ImprintConfig))]
    public class ImprintConfig_Editor : Editor {
        private ImprintConfig Config => (ImprintConfig)target;

        private List<ConfigIssue> _issues;
        private bool _validationDirty;


        private void OnEnable() {
            _validationDirty = true;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawDefaultInspector();

            if (GUI.changed) {
                serializedObject.ApplyModifiedProperties();
                _validationDirty = true;
            }

            DevGui.DrawInlineValidation(ref _issues, ref _validationDirty, () => ImprintUtils.ValidateImprint(Config), serializedObject);
        }
    }
}
