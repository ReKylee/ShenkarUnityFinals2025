using Enemies.Interfaces;
using Player.Components;
using UnityEngine;

namespace Enemies.Behaviors
{
    // Makes the enemy jump when the player is within a certain distance
    [RequireComponent(typeof(Rigidbody2D))]
    public class FrogProximityJump : MonoBehaviour, ITriggerBehavior
    {
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float triggerDistance = 3f;
        [SerializeField] private float jumpForceX = 4f;
        [SerializeField] private float jumpForceY = 8f;
        [SerializeField] private float jumpCooldown = 2f;
        [SerializeField] private int checkEveryNFrames = 1;
        private int _frameCounter;
        private bool _grounded;
        private float _lastJumpTime;
        private Transform _player;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }
        private void Start()
        {
            _player = PlayerLocator.PlayerTransform;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if ((1 << collision.gameObject.layer & groundLayer) != 0)
                _grounded = true;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if ((1 << collision.gameObject.layer & groundLayer) != 0)
                _grounded = false;
        }

        public void CheckTrigger()
        {
            _frameCounter++;
            if (_frameCounter % checkEveryNFrames != 0) return;
            if (!_player) return;
            Vector2 toPlayer = _player.position - transform.position;
            float sqrDist = toPlayer.sqrMagnitude;
            float sqrTrigger = triggerDistance * triggerDistance;
            if (sqrDist < sqrTrigger && Time.time - _lastJumpTime > jumpCooldown && _grounded)
            {
                // Flip to face the player
                Vector3 scale = transform.localScale;
                scale.x = toPlayer.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;

                Vector2 jumpDir = new(transform.localScale.x > 0 ? jumpForceX : -jumpForceX, jumpForceY);
                _rb.linearVelocity = jumpDir;
                _lastJumpTime = Time.time;
            }
        }
    }
}
