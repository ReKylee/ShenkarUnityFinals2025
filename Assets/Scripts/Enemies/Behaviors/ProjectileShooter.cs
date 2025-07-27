using UnityEngine;
using Enemies.Interfaces;
using System.Threading.Tasks;
using Weapons.Models;

namespace Enemies.Behaviors
{
    // Shoots a projectile at intervals from a specified fire point
    public class ProjectileShooter : MonoBehaviour, IAttackBehavior
    {
        [SerializeField] private FireballWeapon fireballWeapon;
        [SerializeField] private float fireInterval = 2f;
        private bool _isFiring;

        private void OnEnable()
        {
            _isFiring = false;
        }

        public void Attack()
        {
            if (_isFiring || !fireballWeapon)
                return;
            _isFiring = true;
            _ = FireLoop();
        }

        private async Task FireLoop()
        {
            try
            {
                while (enabled && gameObject.activeInHierarchy)
                {
                    fireballWeapon.Shoot();
                    await Task.Delay((int)(fireInterval * 1000f));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ProjectileShooter FireLoop Exception: {ex}");
            }
            finally
            {
                _isFiring = false;
            }
        }
    }
}
