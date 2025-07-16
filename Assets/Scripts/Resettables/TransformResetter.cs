using Interfaces.Resettable;
using Managers;
using UnityEngine;

namespace Resettables
{
    public class TransformResetter : MonoBehaviour, IResettable
    {
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;

        private void Start()
        {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            ResetManager.Instance?.Register(this);
        }
        public void OnDestroy()
        {
            ResetManager.Instance?.Unregister(this);
        }

        public void ResetState()
        {
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;
        }
    }
}
