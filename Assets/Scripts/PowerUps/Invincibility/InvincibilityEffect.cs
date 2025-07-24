using System.Collections;
using Health.Components;
using Health.Interfaces;
using UnityEngine;

namespace PowerUps.Invincibility
{

    public class InvincibilityEffect : MonoBehaviour, IInvincibilityDealer
    {
        [SerializeField] private GameObject effectObject;
        private PlayerDamageController _dc;

        private void Awake()
        {
            _dc = GetComponent<PlayerDamageController>();
        }
        public int GetDamageAmount() => _dc.IsInvulnerable ? 10 : 0;
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
