using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace LH.Domain {
    public static class SceneExtension {
        
        public static T GetComponentInLoadedScene<T>() where T : class {
            foreach (var scene in GetLoadedScenes()) {
                var component = scene.GetComponentInRoot<T>();
                if (component != null) {
                    return component;
                }
            }
            return null;
        }

        public static IEnumerable<Scene> GetLoadedScenes() {
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded) {
                    yield return scene;
                }
            }
        }

        public static T GetComponentInRoot<T>(this Scene scene) => scene
            .GetComponentsInRoot<T>()
            .FirstOrDefault(comp => comp != null);

        public static IEnumerable<T> GetComponentsInRoot<T>(this Scene scene) => scene
            .GetRootGameObjects()
            .Select(o => o.GetComponent<T>());

    }
}