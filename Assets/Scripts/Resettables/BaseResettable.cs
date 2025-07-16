using Interfaces.Resettable;
using Managers.Interfaces;
using UnityEngine;
using VContainer;

namespace Resettables
{
    public abstract class BaseResettable : MonoBehaviour, IResettable
    {
        private IResetManager _resetManager;

        [Inject]
        public void Construct(IResetManager resetManager)
        {
            _resetManager = resetManager;
        }

        protected virtual void Start()
        {
            _resetManager?.Register(this);
        }

        protected virtual void OnDestroy()
        {
            _resetManager?.Unregister(this);
        }

        public abstract void ResetState();
    }
}
