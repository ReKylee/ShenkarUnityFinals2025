using Interfaces.Resettable;
using Managers.Interfaces;
using Weapons.Interfaces;

namespace Resettables
{
    public class UsableWeaponResetter : IResettable
    {
        private readonly IUseableWeapon _usableWeapon;
        private readonly IResetManager _resetManager;

        public UsableWeaponResetter(IUseableWeapon weapon, IResetManager resetManager)
        {
            _usableWeapon = weapon;
            _resetManager = resetManager;
            _resetManager?.Register(this);
        }

        public void ResetState()
        {
            _usableWeapon?.UnEquip();
        }

        public void Dispose()
        {
            _resetManager?.Unregister(this);
        }
    }
}
