using System;
using UnityEngine;

namespace Player.Components
{
    /// <summary>
    /// Provides a global reference to the player transform.
    /// </summary>
    public class PlayerLocator : MonoBehaviour
    {
        public static Transform PlayerTransform { get; private set; }
        private void Awake()
        {
            PlayerTransform = transform;
        }
    }
}
