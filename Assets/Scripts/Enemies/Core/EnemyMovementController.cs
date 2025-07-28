using Enemies.Interfaces;
using UnityEngine;

namespace Enemies.Core
{
    // Handles all movement behaviors for an enemy
    public class EnemyMovementController : MonoBehaviour
    {
        private IMovementBehavior[] _movementBehaviors;

        private void Awake()
        {
            _movementBehaviors = GetComponents<IMovementBehavior>();
        }

        private void FixedUpdate()
        {
            foreach (IMovementBehavior move in _movementBehaviors)
                move.Move();
        }
    }
}
