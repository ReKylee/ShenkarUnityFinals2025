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

}
