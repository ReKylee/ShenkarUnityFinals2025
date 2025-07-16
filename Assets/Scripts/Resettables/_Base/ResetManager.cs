using System.Collections.Generic;
using Interfaces.Resettable;
using UnityEngine;

namespace Managers
{

    public class ResetManager : MonoBehaviour
    {
        private readonly List<IResettable> _resettables = new();

        public static ResetManager Instance { get; private set; }

        private void Awake()
        {

            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

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
