using System;
using System.IO;
using UnityEngine;

namespace Core.Data
{
    public interface IGameDataRepository
    {
        GameData LoadData();
        void SaveData(GameData data);
        void DeleteData();
        string GetSaveFilePath();
    }

    public class JsonGameDataRepository : IGameDataRepository
    {
        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "gamedata.json");

        public GameData LoadData()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    var data = JsonUtility.FromJson<GameData>(json);
                    Debug.Log($"Game data loaded from: {SaveFilePath}");
                    return data;
                }
                else
                {
                    Debug.Log("No save file found, creating default data");
                    return CreateDefaultData();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game data: {e.Message}");
                return CreateDefaultData();
            }
        }

        public void SaveData(GameData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"Game data saved to: {SaveFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game data: {e.Message}");
            }
        }

        public void DeleteData()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                    Debug.Log("Save file deleted");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save file: {e.Message}");
            }
        }

        public string GetSaveFilePath() => SaveFilePath;

        private GameData CreateDefaultData()
        {
            return new GameData
            {
                lives = 3,
                score = 0,
                coins = 0,
                currentLevel = "Level_01",
                bestTime = float.MaxValue,
                hasFireball = false,
                hasAxe = false,
                musicVolume = 1.0f,
                sfxVolume = 1.0f
            };
        }
    }
}
