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
        private Rigidbody2D _rb;
        private float _lastJumpTime;
        private Transform _player;
        private int _frameCounter;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _player = PlayerLocator.PlayerTransform;
        }

        public void CheckTrigger()
        {
            _frameCounter++;
            if (_frameCounter % checkEveryNFrames != 0) return;
            if (!_player) return;
            float sqrDist = (transform.position - _player.position).sqrMagnitude;
            float sqrTrigger = triggerDistance * triggerDistance;
            if (sqrDist < sqrTrigger && Time.time - _lastJumpTime > jumpCooldown && IsGrounded())
            {
                _rb.linearVelocity = new Vector2(jumpForceX, jumpForceY);
                _lastJumpTime = Time.time;
            }
        }

        private bool IsGrounded()
        {
            return Physics2D.Raycast(transform.position, Vector2.down, 0.1f);
        }
    }
}
