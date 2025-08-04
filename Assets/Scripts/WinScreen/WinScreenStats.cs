using UnityEngine;
using UnityEngine.UI;
using Core;
using TMPro;

namespace WinScreen
{
    public class WinScreenStats : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI bestTimeText;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [SerializeField] private TextMeshProUGUI fruitsCollectedText;

        [Header("Level Name")]

        [SerializeField] private GameDataCoordinator gameDataCoordinator;

        private void Start()
        {
            if (!gameDataCoordinator)
            {
                bestTimeText.text = "Best Time: N/A";
                bestScoreText.text = "Best Score: N/A";
                fruitsCollectedText.text = "Fruits Collected: N/A";
                Debug.LogError("[WinScreenStats] GameDataCoordinator not found!");
                return;
            }

            float bestTime = gameDataCoordinator.GetBestTime();
            int bestScore = gameDataCoordinator.GetBestScore();
            int fruitsCollected = gameDataCoordinator.GetFruitCollectedCount();
            Debug.Log("Best Time: " + bestTime);
            bestTimeText.text = $"Best Time: {FormatTime(bestTime)}";
            bestScoreText.text = $"Best Score: {bestScore}";
            fruitsCollectedText.text = $"{fruitsCollected.ToString().PadLeft(2, '0')}";
        }
        public static string FormatTime(float timeInSeconds)
        {
            if (timeInSeconds is >= float.MaxValue or < 0)
                return "--:--.---";
        
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            int milliseconds = Mathf.FloorToInt((timeInSeconds - Mathf.Floor(timeInSeconds)) * 1000f);
    
            return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
        }
    }
}
