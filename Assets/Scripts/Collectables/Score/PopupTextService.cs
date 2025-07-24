using TMPro;
using UnityEngine;

namespace Collectables.Score
{
    public class PopupTextService : MonoBehaviour
    {
        [SerializeField] private ScoreTextPool scoreTextPool;


        private void OnEnable()
        {
            ScoreCollectable.OnScoreCollected += HandleScoreCollected;
        }

        private void OnDisable()
        {
            ScoreCollectable.OnScoreCollected -= HandleScoreCollected;
        }

        private void HandleScoreCollected(int scoreAmount, Vector3 position)
        {
            ShowFloatingText(position, $"{scoreAmount}");
        }

        public void ShowFloatingText(Vector3 position, string text)
        {
            TextMeshPro floatingText = scoreTextPool?.Get(text);
            if (floatingText)
            {
                floatingText.transform.position = position;
            }
        }
    }
}
