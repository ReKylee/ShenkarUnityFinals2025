using System.Collections;
using UnityEngine;

namespace Hazards
{
    public class DisappearingFloor : MonoBehaviour
    {
        [SerializeField] private float disappearDelay = 2f;
        [SerializeField] private float reappearDelay = 2f;
        private Collider2D _platformCollider;

        private SpriteRenderer _platformRenderer;

        private void Awake()
        {
            _platformRenderer = GetComponent<SpriteRenderer>();
            _platformCollider = GetComponent<Collider2D>();
        }

        private void Start()
        {
            StartCoroutine(DisappearReappearCycle());
        }

        private IEnumerator DisappearReappearCycle()
        {
            while (true)
            {
                yield return new WaitForSeconds(disappearDelay);
                _platformRenderer.enabled = false;
                _platformCollider.enabled = false;
                yield return new WaitForSeconds(reappearDelay);
                _platformRenderer.enabled = true;
                _platformCollider.enabled = true;
            }
        }
    }
}
