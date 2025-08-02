using Core.Data;
using Player.Interfaces;
using Player.Services;
using Pooling;
using UnityEngine;
using VContainer;

namespace Collectables.Score
{
    public class PopupTextService : MonoBehaviour
    {
        [SerializeField] private GameObject scoreTextPrefab;
        [SerializeField] private GameObject oneUpTextPrefab;
        private IPoolService _scoreTextPool;
        private IPlayerLivesService _playerLivesService;

        private void OnEnable()
        {
            ScoreCollectable.OnScoreCollected += HandleScoreCollected;
            _playerLivesService.OnOneUpAwarded += HandleOneUpAwarded;
        }

        private void OnDisable()
        {
            ScoreCollectable.OnScoreCollected -= HandleScoreCollected;
            _playerLivesService.OnOneUpAwarded -= HandleOneUpAwarded;
        }

        [Inject]
        private void Configure(IPoolService poolService, IPlayerLivesService playerLivesService)
        {
            _scoreTextPool = poolService;
            _playerLivesService = playerLivesService;
        }

        private void HandleScoreCollected(int scoreAmount, Vector3 position)
        {
            ShowFloatingText(position, $"{scoreAmount}", scoreTextPrefab);
        }

        private void HandleOneUpAwarded(Vector3 position)
        {
            ShowFloatingText(position, "1UP", oneUpTextPrefab);
        }

        private void ShowFloatingText(Vector3 position, string text, GameObject prefab)
        {
            FloatingText floatingTextObj =
                _scoreTextPool?.Get<FloatingText>(prefab, position, Quaternion.identity);

            if (floatingTextObj)
            {
                floatingTextObj.Text = text;
                floatingTextObj.SetPoolingInfo(_scoreTextPool, prefab);
            }
        }
    }
}
