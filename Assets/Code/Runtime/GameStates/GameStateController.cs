using Cysharp.Threading.Tasks;
using LH.Api;
using LH.Cosmos;
using LH.Story;
using UnityEngine.SceneManagement;

namespace LH.GameStates {
    public class GameStateController : IGameState {
        private CosmosController _cosmos;

        private bool _inTransition;
        private string _loadedStoryScene;


        public void Init(CosmosController cosmos) {
            _cosmos = cosmos;
        }

        public void EnterStory(int storyId) {
            if (_inTransition)
                return;
            EnterStoryAsync(storyId).Forget();
        }

        public void ExitStory() {
            if (_inTransition || _loadedStoryScene == null)
                return;
            ExitStoryAsync().Forget();
        }

        private async UniTaskVoid EnterStoryAsync(int storyId) {
            _inTransition = true;
            var story = RootConfigs.Instance.Stories.GetStory(storyId);

            await AnimateCosmosHide();

            _cosmos.gameObject.SetActive(false);
            await SceneManager.LoadSceneAsync(story.SceneName, LoadSceneMode.Additive);
            _loadedStoryScene = story.SceneName;

            _inTransition = false;
        }

        private async UniTaskVoid ExitStoryAsync() {
            _inTransition = true;

            await SceneManager.UnloadSceneAsync(_loadedStoryScene);
            _loadedStoryScene = null;

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