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
        [SerializeField] private WeaponType allowedWeaponType = WeaponType.Boomerang;
        public bool CanBeDamagedBy(GameObject damager)
        {
            if (((1 << damager.layer) & projectileLayers) == 0)
                return false;
            if (!requireWeaponType)
                return true;
            
            IWeaponTypeProvider weaponTypeProvider = damager.GetComponent<IWeaponTypeProvider>();
            if (weaponTypeProvider != null)
            {
                return weaponTypeProvider.WeaponType == allowedWeaponType;
            }
            return false;
        }
    }
}
