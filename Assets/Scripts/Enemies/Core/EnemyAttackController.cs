using Enemies.Interfaces;
using UnityEngine;

namespace Enemies.Core
{
    // Handles all attack behaviors for an enemy
    public class EnemyAttackController : MonoBehaviour
    {
        private IAttackBehavior[] _attackBehaviors;

        private void Awake()
        {
            _attackBehaviors = GetComponents<IAttackBehavior>();
        }

        private void LateUpdate()
        {
            foreach (IAttackBehavior attack in _attackBehaviors)
                attack.Attack();
        }
    }
}
