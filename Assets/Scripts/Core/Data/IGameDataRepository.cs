namespace Core.Data
{
    public interface IGameDataRepository
    {
        GameData LoadData();
        void SaveData(GameData data);
        void DeleteData();
        string GetSaveFilePath();
    }

}
