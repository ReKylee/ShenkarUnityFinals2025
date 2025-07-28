using System.Linq;
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
        [SerializeField] private WeaponType[] allowedWeaponTypesArray = { WeaponType.Boomerang, WeaponType.Spark };
        private bool _allowAllWeaponTypes;
        private int _allowedWeaponMask;

        private void Awake()
        {
            _allowAllWeaponTypes = allowedWeaponTypesArray.Length == 0;
            _allowedWeaponMask = _allowAllWeaponTypes
                ? 0
                : allowedWeaponTypesArray
                    .Aggregate(0,
                        (mask, wt) => mask | 1 << (int)wt
                    );
        }

        public bool CanBeDamagedBy(GameObject damager)
        {
            if ((1 << damager.layer & projectileLayers) == 0)
                return false;

            if (_allowAllWeaponTypes)
                return true;

            if (!damager.TryGetComponent(out IWeaponTypeProvider weaponTypeProvider))
                return false;

            return (_allowedWeaponMask & 1 << (int)weaponTypeProvider.WeaponType) != 0;
        }
    }
}
