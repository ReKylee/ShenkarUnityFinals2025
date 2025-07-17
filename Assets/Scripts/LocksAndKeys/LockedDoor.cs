using Collectables.Keys;
using UnityEngine;
using UnityEngine.Events;

namespace LocksAndKeys
{
    public class LockedDoor : BaseLock
    {
        [SerializeField] private UnityEvent doorOpened;

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.CompareTag("Player"))
                return;

            KeychainController keychain = col.GetComponent<KeychainController>();
            if (!keychain) return;

            IKey usedKey = keychain.Keychain.TryUnlock(this);
            if (usedKey == null)
            {
                Debug.Log($"No key was found for {gameObject.name} door!");
                return;
            }

            keychain.Keychain.RemoveKey(usedKey);
        }

        protected override void OnUnlocked()
        {
            Debug.Log($"Unlocked: {gameObject.name}");
            doorOpened?.Invoke();
        }
    }
}
