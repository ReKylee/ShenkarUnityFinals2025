using System.Collections.Generic;
using Interfaces.Resettable;
using Managers.Interfaces;
using UnityEngine;

namespace Managers
{
    public class ResetManager : MonoBehaviour, IResetManager
    {
        private readonly List<IResettable> _resettables = new();

        public void Register(IResettable resettable)
        {
            if (!_resettables.Contains(resettable))
                _resettables.Add(resettable);
        }

        public void Unregister(IResettable resettable)
        {
            if (_resettables.Contains(resettable))
                _resettables.Remove(resettable);
        }

        public void ResetAll()
        {
            foreach (IResettable r in _resettables)
            {
                r.ResetState();
            }
        }
    }
}
