namespace Core.Flow
{
    public interface IGameFlowController
    {
        void HandlePlayerDeath();
        void HandleLevelCompletion(float completionTime);
        void HandleGameOver();
    }

}



