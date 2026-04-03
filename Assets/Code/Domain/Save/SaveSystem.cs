using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LH.Api;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LH.Save {
    public class SaveSystem : ISaveSystem, IUpdatable {
        private const float AUTOSAVE_DURATION = .5f;
        public static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");

        private GameSave _data;

        private float _autosaveTimer;
        private bool _isModified;


        public void Init() {
            LoadOrCreate();

            _data.SessionsCount++;
            _isModified = true;

            Application.quitting += OnQuit;
        }

        void ISaveSystem.SetHiddenState(int id, HiddenObjectSave save) {
            _isModified = true;

            var current = _data.HiddenObjects.FirstOrDefault(s => s.Id == id);
            if (current == save) {
                return;
            }
            if (current != null) {
                _data.HiddenObjects.Remove(current);
            }
            _data.HiddenObjects.Add(save);
        }

        HiddenObjectSave ISaveSystem.GetHiddenState(int id) => _data.HiddenObjects.FirstOrDefault(s => s.Id == id);


        public void Update(float dt) {
            _autosaveTimer += dt;

            if (_autosaveTimer < AUTOSAVE_DURATION)
                return;
            _autosaveTimer = 0;

            WriteIfChanged();
        }

        private void OnQuit() => WriteIfChanged();


        private void LoadOrCreate() {
            if (File.Exists(SavePath)) {
                Load();
            }
            _data ??= new GameSave();
        }

        private void Load() {
            try {
                string json = File.ReadAllText(SavePath);
                _data = JsonUtility.FromJson<GameSave>(json);

                Debug.Log($"Loaded {_data}");
            } catch (Exception ex) {
                Debug.LogError($"Can't read save from {SavePath}; {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void WriteIfChanged() {
            if (!_isModified)
                return;
            _isModified = false;

            _data.SavesCount++;
            Write();
        }

        private void Write() {
            var sw = Stopwatch.StartNew();

            try {
                string json = JsonUtility.ToJson(_data, true);
                File.WriteAllText(SavePath, json);

                Debug.Log($"Save written in {sw.ElapsedMilliseconds} ms");
            } catch (Exception ex) {
                Debug.LogError($"Can't write save; {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}