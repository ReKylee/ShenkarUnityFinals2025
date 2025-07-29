using Enemies.Interfaces;
using UnityEngine;

namespace Enemies.Behaviors
{
    // Trigger to check if the object is on the ground
    [RequireComponent(typeof(Collider2D))]
    public class GroundedTrigger : MonoBehaviour, ITrigger
    {
        [SerializeField] private LayerMask groundLayer;

        private bool _isGrounded;

        public bool IsTriggered => _isGrounded;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if ((1 << collision.gameObject.layer & groundLayer) != 0)
                _isGrounded = true;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if ((1 << collision.gameObject.layer & groundLayer) != 0)
                _isGrounded = false;
        }

        public void CheckTrigger()
        {
        }
    }
}
