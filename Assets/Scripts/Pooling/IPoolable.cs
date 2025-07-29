using UnityEngine;

namespace Pooling
{
    /// <summary>
    ///     Interface for objects that can be returned to an object pool
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        ///     Sets the pool service and source prefab for this poolable object
        /// </summary>
        void SetPoolingInfo(IPoolService poolService, GameObject sourcePrefab);

        /// <summary>
        ///     Returns this object to its pool
        /// </summary>
        void ReturnToPool();
    }
}
