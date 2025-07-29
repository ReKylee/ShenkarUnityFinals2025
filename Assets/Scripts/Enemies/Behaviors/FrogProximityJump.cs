using Enemies.Interfaces;
using Player.Components;
using UnityEngine;

namespace Enemies.Behaviors
{
    // Emits an event when the player is within a certain distance
    [RequireComponent(typeof(Rigidbody2D))]
    public class FrogProximityTrigger : MonoBehaviour, ITrigger
    {
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float triggerDistance = 3f;
        [SerializeField] private float jumpCooldown = 2f;
        [SerializeField] private int checkEveryNFrames = 1;

        private int _frameCounter;
        private bool _grounded;
        private float _lastTriggerTime;
        private Transform _player;

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

        public bool IsTriggered { get; private set; }

        public void CheckTrigger()
        {
            if (++_frameCounter % checkEveryNFrames != 0 || !_player) return;

            Vector2 toPlayer = _player.position - transform.position;
            float sqrDist = toPlayer.sqrMagnitude;
            float sqrTrigger = triggerDistance * triggerDistance;

            if (sqrDist < sqrTrigger && Time.time - _lastTriggerTime > jumpCooldown && _grounded)
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
