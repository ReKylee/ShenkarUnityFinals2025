using UnityEngine;

namespace Pooling
{
    public interface IPoolService
    {
        GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation);
        void Release(GameObject prefab, GameObject instance);
        T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : MonoBehaviour;
        void Release<T>(GameObject prefab, T instance) where T : MonoBehaviour;
    }
}
