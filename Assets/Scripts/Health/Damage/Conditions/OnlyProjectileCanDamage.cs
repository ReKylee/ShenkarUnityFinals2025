using System.Collections.Generic;
using Health.Interfaces;
using UnityEngine;
using Weapons;
using Weapons.Interfaces;

namespace Health.Damage.Conditions
{
    [DisallowMultipleComponent]
    public class OnlyProjectileCanDamage : MonoBehaviour, IDamageCondition
    {
        [SerializeField] private LayerMask projectileLayers = ~0;
        [SerializeField] private bool requireWeaponType = true;
        [SerializeField] private List<WeaponType> allowedWeaponType = new() { WeaponType.Boomerang, WeaponType.Spark };
        public bool CanBeDamagedBy(GameObject damager)
        {
            if (((1 << damager.layer) & projectileLayers) == 0)
                return false;

            if (!requireWeaponType)
                return true;

            IWeaponTypeProvider weaponTypeProvider = damager.GetComponent<IWeaponTypeProvider>();
            return allowedWeaponType?.Contains(weaponTypeProvider.WeaponType) ?? false;
        }
    }
}
