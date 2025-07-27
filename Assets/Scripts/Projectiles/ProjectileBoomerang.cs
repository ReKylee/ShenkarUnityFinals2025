﻿using System;
using Projectiles.Core;
using UnityEngine;
using Weapons;

namespace Projectiles
{
    public class ProjectileBoomerang : BaseProjectile
    {
        [SerializeField] private float totalFlightTime = 1f;
        [SerializeField] private AnimationCurve trajectoryXCurve = AnimationCurve.Linear(0, 0, 1, 0);
        [SerializeField] private AnimationCurve trajectoryYCurve = AnimationCurve.Linear(0, 0, 1, 0);

        private float _flightTimer;
        private bool _isFlying;
        private Vector3 _startPosition; // Fixed start position

        [NonSerialized] public float Direction;
        [NonSerialized] public Transform PlayerTransform;

        private void Update()
        {

            if (_isFlying && PlayerTransform)
            {
                _flightTimer += Time.deltaTime;
                float progress = _flightTimer / totalFlightTime;

                if (progress >= 1f)
                {
                    OnBoomerangReturned?.Invoke();
                    ReturnToPool();
                    return;
                }

                // Calculate curve offsets
                float xOffset = trajectoryXCurve.Evaluate(progress) * speed.x * Direction;
                float yOffset = trajectoryYCurve.Evaluate(progress) * speed.y;

                // Blend the base position from start to current player position
                Vector3 basePosition = Vector3.Lerp(_startPosition, PlayerTransform.position, progress);
                Vector3 curvePosition = basePosition + new Vector3(xOffset, yOffset, 0);

                transform.position = curvePosition;

                // Check if close to player (especially near the end)
                if (progress > 0.7f && Vector3.Distance(transform.position, PlayerTransform.position) < 1.5f)
                {
                    OnBoomerangReturned?.Invoke();
                    ReturnToPool();
                }
            }
        }

        private void OnEnable()
        {
            _isFlying = false;
            _flightTimer = 0f;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player") && _flightTimer > 0.5f)
            {
                OnBoomerangReturned?.Invoke();
                ReturnToPool();
            }
        }

        public event Action OnBoomerangReturned;

        protected override void Move()
        {
            _startPosition = transform.position;
            _flightTimer = 0f;
            _isFlying = true;
            transform.localScale = new Vector3(Direction, 1, 1);
        }
    }
}
