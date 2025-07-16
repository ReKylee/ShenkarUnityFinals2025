using Interfaces.Resettable;
using Managers.Interfaces;
using Weapons.Interfaces;

namespace Resettables
{
    public class AmmoResetter : IResettable
    {
        private readonly IAmmoWeapon _ammoWeapon;
        private readonly int _initialAmmo;
        private readonly IResetManager _resetManager;

        public AmmoResetter(IAmmoWeapon weapon, IResetManager resetManager)
        {
            _ammoWeapon = weapon;
            _resetManager = resetManager;
            _initialAmmo = _ammoWeapon?.CurrentAmmo ?? 0;
            _resetManager?.Register(this);
        }

        public void ResetState()
        {
            _ammoWeapon?.SetAmmo(_initialAmmo);
        }

        public void Dispose()
        {
            _resetManager?.Unregister(this);
        }
    }
}
