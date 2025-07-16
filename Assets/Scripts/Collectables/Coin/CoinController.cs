using Collectables.Counter;
using Interfaces.Resettable;
using Managers;
using UnityEngine;

namespace Collectables.Coin
{
    public class CoinController : MonoBehaviour, IResettable
    {
        [SerializeField] private CoinView view;

        private ICounterModel _model;

        private void Awake()
        {
            _model = new CounterModel();
        }
        private void Start()
        {
            ResetManager.Instance?.Register(this);
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
        private void OnDestroy()
        {
            ResetManager.Instance?.Unregister(this);
        }

        public void ResetState() => _model.Reset();
    }
}
