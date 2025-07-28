using Pooling;
using TMPro;
using UnityEngine;
using VContainer;

namespace Collectables.Score
{
    public class PopupTextService : MonoBehaviour
    {
        [SerializeField] private GameObject scoreTextPrefab;
        [Inject] private IPoolService _scoreTextPool;


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

        private void ShowFloatingText(Vector3 position, string text)
        {
            TextMeshPro floatingTextObj =
                _scoreTextPool?.Get<TextMeshPro>(scoreTextPrefab, position, Quaternion.identity);

            if (floatingTextObj)
                floatingTextObj.text = text;
        }
    }
}
