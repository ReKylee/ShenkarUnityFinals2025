using Core;
using UnityEngine;
using VContainer;

namespace Collectables.Coin
{
    public class CoinController : MonoBehaviour
    {
        [SerializeField] private CoinView view;
        private PersistentDataManager _persistentData;

        #region VContainer Injection
        [Inject]
        public void Construct(PersistentDataManager persistentData)
        {
            _persistentData = persistentData;
        }
        #endregion

        private void Start()
        {
            // Update view with current coin count from persistent data
            if (view && _persistentData is not null)
            {
                view.UpdateCountDisplay(_persistentData.Data.coins);
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
            // Add coin to persistent data
            _persistentData?.AddCoins(1);
            
            // Update view
            if (view is not null && _persistentData is not null)
            {
                view.UpdateCountDisplay(_persistentData.Data.coins);
            }
        }
    }
}
