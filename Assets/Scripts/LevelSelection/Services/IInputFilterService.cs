using UnityEngine;

namespace LevelSelection.Services
{
    /// <summary>
    ///     Service responsible for input processing and filtering
    /// </summary>
    public interface IInputFilterService
    {
        void Initialize();
        void SetEnabled(bool enabled);
        bool ProcessNavigationInput(Vector2 direction, out Vector2 filteredDirection);
    }

    /// <summary>
    ///     Handles input filtering and cooldowns (Single Responsibility)
    /// </summary>
    public class InputFilterService : IInputFilterService
    {
        private const float InputCooldownTime = 0.2f; // Prevent input spam
        private const float InputDeadzone = 0.5f; // Input threshold

        private bool _isEnabled = true;
        private Vector2 _lastInputDirection;
        private float _lastInputTime;

        public void Initialize()
        {
        }

        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }

        public bool ProcessNavigationInput(Vector2 direction, out Vector2 filteredDirection)
        {
            filteredDirection = Vector2.zero;

            if (!_isEnabled) return false;

            // Apply deadzone filtering
            if (direction.magnitude < InputDeadzone) return false;

            // Apply input cooldown to prevent spam
            if (Time.time - _lastInputTime < InputCooldownTime) return false;

            // Normalize direction for consistent behavior
            direction = direction.normalized;

            // Check if this is the same direction as last input (prevent repeats)
            if (Vector2.Dot(direction, _lastInputDirection) > 0.8f &&
                Time.time - _lastInputTime < InputCooldownTime * 2f) return false;

            _lastInputDirection = direction;
            _lastInputTime = Time.time;

            filteredDirection = direction;
            return true;
        }
    }
}
