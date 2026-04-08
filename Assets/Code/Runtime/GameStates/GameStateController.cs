using Cysharp.Threading.Tasks;
using LH.Api;
using LH.Cosmos;
using LH.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LH.GameStates {
    public class GameStateController : IGameState {
        private CosmosController _cosmos;

        private bool _inTransition;
        
        // DO: Scene?
        private string _loadedImprintScene;


        public void Init() {
            // DO: тут нужно инициализировать и "где мы находимся"
            // если стартовали дебаг сразу с импринта, космоса не будет
            _cosmos = SceneExtension.GetComponentInLoadedScene<CosmosController>();
        }

        public void EnterImprint(int imprintId) {
            if (_inTransition)
                return;
            EnterImprintAsync(imprintId).Forget();
        }

        public void ExitImprint() {
            if (_inTransition || _loadedImprintScene == null)
                return;
            ExitImprintAsync().Forget();
        }

        private async UniTaskVoid EnterImprintAsync(int imprintId) {
            _inTransition = true;
            var imprint = RootConfigs.Instance.Imprints.GetImprint(imprintId);

            if (imprint == null) {
                Debug.LogError($"Imprint #{imprintId} not found in config");
                _inTransition = false;
                return;
            }

            await AnimateCosmosHide();

            _cosmos.gameObject.SetActive(false);
            await SceneManager.LoadSceneAsync(imprint.SceneName, LoadSceneMode.Additive);
            _loadedImprintScene = imprint.SceneName;

            _inTransition = false;
        }

        private async UniTaskVoid ExitImprintAsync() {
            _inTransition = true;

            await SceneManager.UnloadSceneAsync(_loadedImprintScene);
            _loadedImprintScene = null;

            _cosmos.gameObject.SetActive(true);
            await AnimateCosmosShow();

            _inTransition = false;
        }

        // DO: анимация скрытия космоса (разлетание звёзд и т.п.)
        private async UniTask AnimateCosmosHide() {
            await UniTask.Yield();
        }

        // DO: анимация появления космоса
        private async UniTask AnimateCosmosShow() {
            await UniTask.Yield();
        }
    }
}