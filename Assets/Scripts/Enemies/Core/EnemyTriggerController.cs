using UnityEngine;
using Enemies.Interfaces;

namespace Enemies.Core
{
    // Handles all trigger behaviors for an enemy
    public class EnemyTriggerController : MonoBehaviour
    {
        private ITriggerBehavior[] _triggerBehaviors;

        private void Awake()
        {
            _triggerBehaviors = GetComponents<ITriggerBehavior>();
        }

        private void Update()
        {
            foreach (ITriggerBehavior trigger in _triggerBehaviors)
                trigger.CheckTrigger();
        }
    }
}

