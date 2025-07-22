using Collectables.Counter;
using TMPro;
using UnityEngine;

namespace Collectables.Score
{
    public class ScoreView : MonoBehaviour, ICounterView
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        public void UpdateCountDisplay(int count)
        {
            if (scoreText) scoreText.text = $"{count}".PadLeft(6, '0');

        }
    }
}
