using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using LH.Cheats;
using LH.Domain;
using LH.Save;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LH.Dev {
    public class DevPage_Gameplay : DevWindowPage {
        public override string Label => "Play";

        public override void Gui() {
            CheatsParams.NeedRunGameFromStart.Value = EditorGUILayout.ToggleLeft("Run Game From Start", CheatsParams.NeedRunGameFromStart);

            if (GUILayout.Button("Open save", GUILayout.Width(150))) {
                if (File.Exists(SaveSystem.SavePath)) {
                    InternalEditorUtility.OpenFileAtLineExternal(SaveSystem.SavePath, 0);
                } else {
                    var arguments = new List<string>();
                    arguments.Add(Path.GetDirectoryName(SaveSystem.SavePath)!.Replace("/", @"\"));
                    
                    Process.Start("explorer.exe", arguments.Join(","));
                }
            }
        }
    }
}