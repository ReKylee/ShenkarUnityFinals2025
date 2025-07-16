using Interfaces.Resettable;
using LocksAndKeys;
using Managers;
using UnityEngine;

namespace Collectables.Keys
{
    public class KeychainController : MonoBehaviour, IResettable
    {
        public Keychain Keychain { get; private set; }

        private void Awake()
        {
            Keychain = new Keychain();
        }
        private void Start()
        {
            ResetManager.Instance?.Register(this);
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
            ResetManager.Instance?.Unregister(this);
        }
        public void ResetState() => Keychain.Reset();
        private void CollectKey(IKey key)
        {
            Keychain?.PickUpKey(key);
        }
    }
}
