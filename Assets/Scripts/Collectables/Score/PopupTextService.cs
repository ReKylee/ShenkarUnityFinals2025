using System;
using Pooling;
using TMPro;
using UnityEngine;
using VContainer;

namespace Collectables.Score
{
    public class PopupTextService : MonoBehaviour
    {
        [SerializeField] private GameObject scoreTextPrefab;
        private IPoolService _scoreTextPool;
        
        [Inject]
        private void Configure(IPoolService poolService)
        {
            _scoreTextPool = poolService;
        }

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
            FloatingText floatingTextObj =
                _scoreTextPool?.Get<FloatingText>(scoreTextPrefab, position, Quaternion.identity);

            if (floatingTextObj)
            {
                floatingTextObj.Text = text;

                floatingTextObj.SetPoolingInfo(_scoreTextPool, scoreTextPrefab);
            }
        }
    }
}
