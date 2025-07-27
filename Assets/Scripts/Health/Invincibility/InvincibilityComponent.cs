using Health.Interfaces;
using UnityEngine;

namespace Health.Invincibility
{
    [DisallowMultipleComponent]
    public class InvincibilityComponent : MonoBehaviour, IInvincibility
    {
        public bool IsInvincible { get; private set; }
        public void SetInvincible(bool value)
        {
            IsInvincible = value;
        }
    }
}
