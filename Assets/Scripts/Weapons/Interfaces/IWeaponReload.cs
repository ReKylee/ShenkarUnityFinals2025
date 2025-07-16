namespace Weapons.Interfaces
{
    /// <summary>
    ///     Interface for weapons that can be reloaded
    /// </summary>
    public interface IWeaponReload : IWeapon
    {
        void Reload();
    }
}
