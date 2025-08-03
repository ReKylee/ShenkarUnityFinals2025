using Enemies.Interfaces;
using UnityEngine;

namespace Enemies.Behaviors
{
    // Trigger that activates at a fixed interval
    public class IntervalTrigger : MonoBehaviour, ITrigger
    {
        [SerializeField] private float interval = 1f;
        private float _lastTriggerTime;
        public bool IsTriggered { get; private set; }

        public void CheckTrigger()
        {
            if (Time.time - _lastTriggerTime >= interval)
            {
                IsTriggered = true;
                _lastTriggerTime = Time.time;
            }
            else
            {
                IsTriggered = false;
            }
        }
    }
}
