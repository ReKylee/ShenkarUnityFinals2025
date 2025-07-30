using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Extensions
{
    public static class VContainerExtensions
    {
        /// <summary>
        /// Finds and registers all components of type T in the scene.
        /// Using FindObjectsByType with None sort mode for performance.
        /// </summary>
        public static void RegisterComponentsInHierarchy<T>(
            this IContainerBuilder builder
        ) where T : Component
        {
            var components = Object.FindObjectsByType<T>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

            foreach (T comp in components)
            {
                builder.RegisterInstance(comp);
            }
        }

    }
}
