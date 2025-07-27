using System.Collections;
using Health.Interfaces;
using Health.Invincibility;
using UnityEngine;

namespace PowerUps.Invincibility
{

    public class InvincibilityEffect : MonoBehaviour
    {
        [SerializeField] private GameObject effectObject;
        private IInvincibility _invincibility;

        private void Awake()
        {
            _invincibility = GetComponent<IInvincibility>();
        }

        public void Activate(float duration)
        {
            if (_invincibility == null) return;
            StartCoroutine(RunEffect(duration));
        }

        private IEnumerator RunEffect(float duration)
        {
            _invincibility.SetInvincible(true);

            effectObject?.SetActive(true);

            yield return new WaitForSeconds(duration);

            _invincibility.SetInvincible(false);

            effectObject?.SetActive(false);
        }
    }
}
