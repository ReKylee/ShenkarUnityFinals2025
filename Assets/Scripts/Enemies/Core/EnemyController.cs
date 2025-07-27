using UnityEngine;
using Enemies.Interfaces;

namespace Enemies.Core
{
    // Wires up modular behaviors. Attach this to any enemy prefab.
    public class EnemyController : MonoBehaviour
    {
        private IMovementBehavior[] _movementBehaviors;
        private IAttackBehavior[] _attackBehaviors;
        private ITriggerBehavior[] _triggerBehaviors;

        private void Awake()
        {
            _movementBehaviors = GetComponents<IMovementBehavior>();
            _attackBehaviors = GetComponents<IAttackBehavior>();
            _triggerBehaviors = GetComponents<ITriggerBehavior>();
        }

        private void Update()
        {
            foreach (ITriggerBehavior trigger in _triggerBehaviors) trigger.CheckTrigger();
            foreach (IMovementBehavior move in _movementBehaviors) move.Move();
            foreach (IAttackBehavior attack in _attackBehaviors) attack.Attack();
        }
    }
}
