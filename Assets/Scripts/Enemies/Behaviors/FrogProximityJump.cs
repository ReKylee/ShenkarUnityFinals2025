using UnityEngine;
using Enemies.Interfaces;
using Player.Components;

namespace Enemies.Behaviors
{
    // Makes the enemy jump when the player is within a certain distance
    [RequireComponent(typeof(Rigidbody2D))]
    public class FrogProximityJump : MonoBehaviour, ITriggerBehavior
    {
        [SerializeField] private float triggerDistance = 3f;
        [SerializeField] private float jumpForceX = 4f;
        [SerializeField] private float jumpForceY = 8f;
        [SerializeField] private float jumpCooldown = 2f;
        [SerializeField] private int checkEveryNFrames = 1; 
        [SerializeField] private LayerMask groundLayer;
        private Rigidbody2D _rb;
        private float _lastJumpTime;
        private Transform _player;
        private int _frameCounter;
        private bool _isGrounded;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _player = PlayerLocator.PlayerTransform;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (((1 << collision.gameObject.layer) & groundLayer) != 0)
                _isGrounded = true;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (((1 << collision.gameObject.layer) & groundLayer) != 0)
                _isGrounded = false;
        }

        public void CheckTrigger()
        {
            _frameCounter++;
            if (_frameCounter % checkEveryNFrames != 0) return;
            if (!_player) return;
            float sqrDist = (transform.position - _player.position).sqrMagnitude;
            float sqrTrigger = triggerDistance * triggerDistance;
            if (sqrDist < sqrTrigger && Time.time - _lastJumpTime > jumpCooldown && _isGrounded)
            {
                _rb.linearVelocity = new Vector2(jumpForceX, jumpForceY);
                _lastJumpTime = Time.time;
            }
        }
    }
}
