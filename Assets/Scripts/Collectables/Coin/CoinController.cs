using Core.Data;
using UnityEngine;
using VContainer;

namespace Collectables.Coin
{
    public class CoinController : MonoBehaviour
    {
        [SerializeField] private CoinView view;
        private IGameDataService _gameDataService;

        #region VContainer Injection
        [Inject]
        public void Construct(IGameDataService gameDataService)
        {
            _gameDataService = gameDataService;
        }
        #endregion

        private void Start()
        {
            // Update view with current coin count from game data service
            if (view && _gameDataService != null)
            {
                view.UpdateCountDisplay(_gameDataService.CurrentData.coins);
            }
        }

        private void OnEnable()
        {
            CoinCollectable.OnCoinCollected += HandleCoinCollected;
        }

        private void OnDisable()
        {
            CoinCollectable.OnCoinCollected -= HandleCoinCollected;
        }

        private void HandleCoinCollected()
        {
            // Add coin through game data service
            if (_gameDataService != null)
            {
                int newCoinCount = _gameDataService.CurrentData.coins + 1;
                _gameDataService.UpdateCoins(newCoinCount);
                
                // Update view
                view?.UpdateCountDisplay(newCoinCount);
            }
        }
    }
}
