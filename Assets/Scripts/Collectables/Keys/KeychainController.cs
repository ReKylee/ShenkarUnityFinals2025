using Interfaces.Resettable;
using LocksAndKeys;
using Managers.Interfaces;
using UnityEngine;
using VContainer;

namespace Collectables.Keys
{
    public class KeychainController : MonoBehaviour, IResettable
    {
        public Keychain Keychain { get; private set; }
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
            Keychain = new Keychain();
        }

        private void Start()
        {
            _resetManager?.Register(this);
        }

        private void OnEnable()
        {
            KeyCollectable.OnKeyCollected += CollectKey;
        }

        private void OnDisable()
        {
            KeyCollectable.OnKeyCollected -= CollectKey;
        }

        private void OnDestroy()
        {
            _resetManager?.Unregister(this);
        }

        public void ResetState() => Keychain.Reset();

        private void CollectKey(IKey key)
        {
            Keychain?.PickUpKey(key);
        }
    }
}
