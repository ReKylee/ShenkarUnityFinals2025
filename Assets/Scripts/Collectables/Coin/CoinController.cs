using Collectables.Counter;
using Interfaces.Resettable;
using Managers.Interfaces;
using UnityEngine;
using VContainer;

namespace Collectables.Coin
{
    public class CoinController : MonoBehaviour, IResettable
    {
        [SerializeField] private CoinView view;

        private ICounterModel _model;
        private IResetManager _resetManager;

        #region VContainer Injection
        [Inject]
        public void Construct(IResetManager resetManager)
        {
            _resetManager = resetManager;
        }
        #endregion

        private void Awake()
        {
            _model = new CounterModel();
        }

        private void Start()
        {
            _resetManager?.Register(this);
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
            _resetManager?.Unregister(this);
        }

        public void ResetState() => _model.Reset();
    }
}
