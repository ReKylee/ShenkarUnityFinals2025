using Collectables.Counter;
using UnityEngine;

namespace Collectables.Coin
{
    public class CoinController : MonoBehaviour
    {
        [SerializeField] private CoinView view;
        private ICounterModel _model;

        private void Awake()
        {
            _model = new CounterModel();
        }

        private void OnEnable()
        {
            _model.OnCountChanged += view.UpdateCountDisplay;
            CoinCollectable.OnCoinCollected += _model.Increment;
        }

        private void OnDisable()
        {
            _model.OnCountChanged -= view.UpdateCountDisplay;
            CoinCollectable.OnCoinCollected -= _model.Increment;
        }
    }
}
