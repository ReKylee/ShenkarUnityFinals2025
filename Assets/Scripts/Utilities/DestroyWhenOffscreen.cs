using System;
using UnityEngine;

namespace Utilities
{
    public class DestroyWhenOffscreen : MonoBehaviour
    {
        private void OnBecameInvisible()
        {
            Destroy(gameObject);
        }
    }
}
