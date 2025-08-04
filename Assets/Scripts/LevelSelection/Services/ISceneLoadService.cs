namespace LevelSelection.Services
{
    /// <summary>
    ///     Service responsible for scene loading and transitions
    /// </summary>
    public interface ISceneLoadService
    {
        void LoadLevel(string sceneName);
        string GetSceneNameForLevel(LevelData levelData);
    }

}
