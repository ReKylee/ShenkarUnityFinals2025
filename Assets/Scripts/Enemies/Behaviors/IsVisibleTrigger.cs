using System;
using Enemies.Interfaces;
using UnityEngine;

namespace Enemies.Behaviors
{
    // Trigger that activates when the GameObject becomes visible to any camera
    public class IsVisibleTrigger : MonoBehaviour, ITrigger
    {
        public bool IsTriggered { get; private set; }
        private void OnBecameVisible()
        {
            IsTriggered = true;
        }
        private void OnBecameInvisible()
        {
            IsTriggered = false;
        }
        public void CheckTrigger()
        {
            
        }
    }
}
