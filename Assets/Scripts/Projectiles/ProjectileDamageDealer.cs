using Health.Interfaces;
using UnityEngine;

namespace Projectiles
{
    public class ProjectileDamageDealer : MonoBehaviour, IDamageDealer
    {
        [SerializeField] private int damage = 1;
        public int GetDamageAmount() => damage;
    }
}
