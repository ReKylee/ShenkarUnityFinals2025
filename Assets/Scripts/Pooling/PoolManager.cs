using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Pooling
{
    public class PoolManager : MonoBehaviour, IPoolService
    {
        [SerializeField] private int defaultCapacity = 10;
        [SerializeField] private int maxSize = 100;

        private readonly Dictionary<GameObject, IObjectPool<GameObject>> _pools = new();

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!_pools.TryGetValue(prefab, out var pool))
            {
                pool = CreatePool(prefab);
                _pools[prefab] = pool;
            }

            GameObject obj = pool.Get();
            obj.transform.SetPositionAndRotation(position, rotation);
            return obj;
        }

        public void Release(GameObject prefab, GameObject instance)
        {
            if (!instance)
            {
                Debug.LogWarning("Attempted to release a null instance.");
                return;
            }

            if (_pools.TryGetValue(prefab, out var pool))
            {
                pool.Release(instance);
            }
            else
            {
                Debug.LogWarning($"No pool found for prefab '{prefab.name}'. Ensure the prefab is registered correctly.");
                instance.SetActive(false); 
            }
        }

        public T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : MonoBehaviour
        {
            GameObject obj = Get(prefab, position, rotation);
            if (obj.TryGetComponent(out T component))
            {
                return component;
            }

            Debug.LogError($"The prefab {prefab.name} does not have a component of type {typeof(T).Name}.");
            return null;
        }

        public void Release<T>(GameObject prefab, T instance) where T : MonoBehaviour
        {
            Release(prefab, instance.gameObject);
        }

        private IObjectPool<GameObject> CreatePool(GameObject prefab)
        {
            return new ObjectPool<GameObject>(
                () =>
                {
                    GameObject obj = Instantiate(prefab, transform);
                    obj.SetActive(false);
                    return obj;
                },
                obj => obj?.SetActive(true),
                obj => obj?.SetActive(false),
                Destroy,
                false,
                defaultCapacity,
                maxSize
            );
        }
    }
}
