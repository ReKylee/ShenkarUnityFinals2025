namespace LocksAndKeys
{
    public interface ILock
    {
        bool IsUnlocked { get; }
        bool TryUnlock(IKey key);
        void SetUnlocked(bool unlocked);
    }
}
