using Interfaces.Resettable;
using UnityEngine;

namespace Resettables
{

    public class ActiveResetter : BaseResettable
    {
        [SerializeField] private bool activeOnReset = true;

        public override void ResetState()
        {
            gameObject.SetActive(activeOnReset);
        }
    }
}
