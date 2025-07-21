using System.Collections;
using Health.Components;
using UnityEngine;

namespace PowerUps.Invincibility
{

    public class InvincibilityEffect : MonoBehaviour
    {
        [SerializeField] private GameObject effectObject;
        private DamageController _dc;

        private void Awake()
        {
            _dc = GetComponent<DamageController>();
        }
        public void Activate(float duration)
        {
            if (!_dc) return;
            StartCoroutine(RunEffect(duration));
        }

        private IEnumerator RunEffect(float duration)
        {
            _dc.IsInvulnerable = true;
            effectObject?.SetActive(true);

            yield return new WaitForSeconds(duration);

            _dc.IsInvulnerable = false;
            effectObject?.SetActive(false);
        }
    }
}
