using UnityEngine;
using Weapons.Models;

namespace Enemy
{
    public class EnemyWeaponHandler : MonoBehaviour
    {

        [SerializeField] private FireballWeapon fireballWeapon;
        [SerializeField] private float repeatRate;
        private void OnEnable()
        {
            fireballWeapon.Equip();
            InvokeRepeating(nameof(Shoot), 0f, repeatRate);
        }
        private void OnDisable()
        {
            CancelInvoke(nameof(Shoot));
        }
        private void Shoot()
        {
            fireballWeapon.Shoot();
        }
    }
}
