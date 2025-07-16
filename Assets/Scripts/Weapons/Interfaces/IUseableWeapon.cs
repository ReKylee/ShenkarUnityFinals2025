namespace Weapons.Interfaces
{
    /// <summary>
    ///     Interface for weapons that can be equipped and unequipped
    /// </summary>
    public interface IUseableWeapon : IWeapon
    {
        void Equip();
        void UnEquip();
    }
}
