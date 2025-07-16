using Interfaces.Resettable;
using Managers;
using Weapons.Interfaces;

namespace Resettables
{
    public class AmmoResetter : IResettable
    {
        private readonly IAmmoWeapon _ammoWeapon;
        private readonly int _initialAmmo;
        public AmmoResetter(IAmmoWeapon weapon)
        {
            _ammoWeapon = weapon;
            _initialAmmo = _ammoWeapon?.CurrentAmmo ?? 0;
            ResetManager.Instance?.Register(this);
        }
        public void ResetState()
        {
            _ammoWeapon?.SetAmmo(_initialAmmo);
        }
        public void Dispose()
        {
            ResetManager.Instance?.Unregister(this);
        }
    }
}
