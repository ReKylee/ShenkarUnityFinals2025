namespace Weapons.Interfaces
{
    /// <summary>
    ///     Base interface for all weapons
    /// </summary>
    public interface IWeapon
    {
        /// <summary>
        ///     Type of this weapon.
        /// </summary>
        WeaponType WeaponType { get; }

        void Shoot();
    }
}
