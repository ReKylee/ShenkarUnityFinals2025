using Interfaces.Resettable;

namespace Managers.Interfaces
{
    public interface IResetManager
    {
        void Register(IResettable resettable);
        void Unregister(IResettable resettable);
        void ResetAll();
    }
}
