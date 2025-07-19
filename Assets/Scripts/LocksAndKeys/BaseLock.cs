using UnityEngine;

namespace LocksAndKeys
{
    public abstract class BaseLock : MonoBehaviour, ILock
    {
        [SerializeField] private string requiredKeyId;
        [SerializeField] private bool isUnlocked;

        public bool IsUnlocked => isUnlocked;

        public bool TryUnlock(IKey key)
        {
            if (isUnlocked)
                return true;

            if (!IsCorrectKey(key))
                return false;

            isUnlocked = true;
            OnUnlocked();
            return true;
        }
        public void SetUnlocked(bool unlocked)
        {
            isUnlocked = unlocked;
        }

        protected virtual bool IsCorrectKey(IKey key) => key.KeyId == requiredKeyId;

        protected virtual void OnLocked()
        {
        }

        protected abstract void OnUnlocked();
    }

}
