using System.Collections;
using Health.Damage;
using UnityEngine;
using Weapons.Interfaces;

namespace Weapons.Models
{
    public class MeleeWeapon : MonoBehaviour, IUseableWeapon
    {
        [SerializeField] private WeaponType weaponType = WeaponType.Melee;
        [SerializeField] private GameObject meleeCollider;
        [SerializeField] private float activeTime = 1.33f;
        [SerializeField] private TakeDamageOnCollision takeDamageOnCollision;
        private bool IsEquipped { get; set; }
        public WeaponType WeaponType => weaponType;

        public void Shoot()
        {
            if (!IsEquipped || !meleeCollider)
                return;

            StartCoroutine(ActivateCollider());
        }

        public void Equip()
        {
            IsEquipped = true;
        }

        public void UnEquip()
        {
            IsEquipped = false;
            meleeCollider?.SetActive(false);
        }

        private IEnumerator ActivateCollider()
        {
            meleeCollider?.SetActive(true);
            takeDamageOnCollision.SetActive(false);
            yield return new WaitForSeconds(activeTime);
            meleeCollider?.SetActive(false);
            takeDamageOnCollision.SetActive(true);
        }
    }
}
