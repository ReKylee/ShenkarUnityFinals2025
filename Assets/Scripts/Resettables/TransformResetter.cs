using Interfaces.Resettable;
using UnityEngine;

namespace Resettables
{
    public class TransformResetter : BaseResettable
    {
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;

        protected override void Start()
        {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            base.Start(); // This calls the BaseResettable registration
        }

        public override void ResetState()
        {
            transform.position = _initialPosition;
            transform.rotation = _initialRotation;
        }
    }
}
