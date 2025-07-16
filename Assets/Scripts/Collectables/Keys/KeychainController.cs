using LocksAndKeys;
using UnityEngine;

namespace Collectables.Keys
{
    public class KeychainController : MonoBehaviour
    {
        public Keychain Keychain { get; private set; }

        private void Awake()
        {
            Keychain = new Keychain();
        }

        private void OnEnable()
        {
            KeyCollectable.OnKeyCollected += CollectKey;
        }

        private void OnDisable()
        {
            KeyCollectable.OnKeyCollected -= CollectKey;
        }

        private void CollectKey(IKey key)
        {
            Keychain?.PickUpKey(key);
        }
    }
}
