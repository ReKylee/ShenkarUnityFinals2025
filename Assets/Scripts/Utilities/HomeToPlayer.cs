using Player.Components;
using UnityEngine;

namespace Utilities
{
    public class HomeToPlayer : MonoBehaviour
    {
        [Tooltip("Speed of the homing movement.")] [SerializeField]
        private float speed = 5f;

        private Transform _target;

        private void Awake()
        {
            _target = PlayerLocator.PlayerTransform;
        }

        private void Update()
        {
            if (!_target) return;
            transform.position = Vector2.MoveTowards(transform.position, _target.position, speed * Time.deltaTime);
        }
    }
}
