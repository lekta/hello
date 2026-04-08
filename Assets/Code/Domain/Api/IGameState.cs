namespace LH.Api {
    public interface IGameState {
        void EnterStory(int storyId);
        void ExitStory();
    }
}