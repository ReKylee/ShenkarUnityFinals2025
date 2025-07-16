using Interfaces.Resettable;
using Managers;
using UnityEngine;

namespace Resettables
{

    public class ActiveResetter : MonoBehaviour, IResettable
    {
        [SerializeField] private bool activeOnReset = true;

        private void Start()
        {
            ResetManager.Instance?.Register(this);
        }

        private void OnDestroy()
        {
            ResetManager.Instance?.Unregister(this);
        }

        public void ResetState()
        {
            gameObject.SetActive(activeOnReset);
        }
    }
}
