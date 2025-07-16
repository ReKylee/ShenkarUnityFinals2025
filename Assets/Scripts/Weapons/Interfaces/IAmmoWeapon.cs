namespace Weapons.Interfaces
{
    /// <summary>
    ///     Interface for weapons that use ammunition
    /// </summary>
    public interface IAmmoWeapon : IWeaponReload
    {
        int CurrentAmmo { get; }
        int MaxAmmo { get; }
        bool HasAmmo { get; }
        void SetAmmo(int ammo);
    }
}
