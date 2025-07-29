using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Pooling
{
    public class PoolManager : MonoBehaviour, IPoolService
    {
        [SerializeField] private int defaultCapacity = 10;
        [SerializeField] private int maxSize = 128;

        // Fast array-based pools for performance
        private readonly Dictionary<int, PoolData> _poolMap = new();
        private readonly Transform[] _poolParents = new Transform[64];
        private readonly Stack<GameObject>[] _pools = new Stack<GameObject>[64];
        private readonly GameObject[] _prefabLookup = new GameObject[64];
        private int _poolCount;

        private void Awake()
        {
            // Pre-initialize pools
            for (int i = 0; i < _pools.Length; i++)
            {
                _pools[i] = new Stack<GameObject>(defaultCapacity);
            }
        }

        private void OnDestroy()
        {
            ClearAllPools();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            int prefabId = prefab.GetInstanceID();

            if (_poolMap.TryGetValue(prefabId, out PoolData poolData))
            {
                return GetFromPool(poolData.PoolIndex, position, rotation);
            }

            // First time - create pool
            return CreatePoolAndGet(prefab, prefabId, position, rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(GameObject prefab, GameObject instance)
        {
            if (!instance) return;

            int prefabId = prefab.GetInstanceID();
            if (_poolMap.TryGetValue(prefabId, out PoolData poolData))
            {
                ReleaseToPool(poolData.PoolIndex, instance);
            }
            else
            {
                instance.SetActive(false);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : MonoBehaviour
        {
            GameObject obj = Get(prefab, position, rotation);
            return obj ? obj.GetComponent<T>() : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release<T>(GameObject prefab, T instance) where T : MonoBehaviour
        {
            if (instance)
                Release(prefab, instance.gameObject);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GameObject GetFromPool(int poolIndex, Vector3 position, Quaternion rotation)
        {
            var pool = _pools[poolIndex];

            while (pool.Count > 0)
            {
                GameObject obj = pool.Pop();
                if (obj && !obj.activeInHierarchy)
                {
                    obj.transform.SetPositionAndRotation(position, rotation);
                    obj.SetActive(true);
                    return obj;
                }
            }

            // Pool empty or all objects still active - create new instance
            return CreateNewInstance(poolIndex, position, rotation);
        }

        private GameObject CreatePoolAndGet(GameObject prefab, int prefabId, Vector3 position, Quaternion rotation)
        {
            if (_poolCount >= _pools.Length)
            {
                Debug.LogError("Maximum pool count exceeded!");
                return Instantiate(prefab, position, rotation);
            }

            int poolIndex = _poolCount++;

            // Create organized parent
            Transform poolParent = new GameObject($"Pool_{prefab.name}").transform;
            poolParent.SetParent(transform);

            PoolData poolData = new()
            {
                PoolIndex = poolIndex,
                Prefab = prefab
            };

            _poolMap[prefabId] = poolData;
            _prefabLookup[poolIndex] = prefab;
            _poolParents[poolIndex] = poolParent;

            // Pre-populate with a few instances
            PrePopulatePool(poolIndex, prefab, poolParent);

            return GetFromPool(poolIndex, position, rotation);
        }

        private void PrePopulatePool(int poolIndex, GameObject prefab, Transform parent)
        {
            var pool = _pools[poolIndex];
            int initialCount = Mathf.Min(8, defaultCapacity); // Start with 8 instances

            for (int i = 0; i < initialCount; i++)
            {
                GameObject obj = Instantiate(prefab, parent);
                obj.SetActive(false);
                pool.Push(obj);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private GameObject CreateNewInstance(int poolIndex, Vector3 position, Quaternion rotation)
        {
            GameObject prefab = _prefabLookup[poolIndex];
            Transform parent = _poolParents[poolIndex];

            GameObject obj = Instantiate(prefab, parent);
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReleaseToPool(int poolIndex, GameObject instance)
        {
            // Only release if object is actually active
            if (!instance.activeInHierarchy)
            {
                Debug.LogWarning($"Attempting to release an already inactive object: {instance.name}");
                return;
            }

            instance.SetActive(false);

            var pool = _pools[poolIndex];
            if (pool.Count < maxSize)
            {
                pool.Push(instance);
            }
            else
            {
                // Pool full - destroy excess
                Destroy(instance);
            }
        }

        // Utility methods
        public void WarmPool(GameObject prefab, int count)
        {
            int prefabId = prefab.GetInstanceID();
            if (!_poolMap.TryGetValue(prefabId, out PoolData poolData))
            {
                // Create pool if it doesn't exist
                CreatePoolAndGet(prefab, prefabId, Vector3.zero, Quaternion.identity);
                Release(prefab, GetFromPool(_poolCount - 1, Vector3.zero, Quaternion.identity));
                poolData = _poolMap[prefabId];
            }

            var pool = _pools[poolData.PoolIndex];
            Transform parent = _poolParents[poolData.PoolIndex];

            for (int i = 0; i < count && pool.Count < maxSize; i++)
            {
                GameObject obj = Instantiate(prefab, parent);
                obj.SetActive(false);
                pool.Push(obj);
            }
        }

        public void ClearPool(GameObject prefab)
        {
            int prefabId = prefab.GetInstanceID();
            if (_poolMap.TryGetValue(prefabId, out PoolData poolData))
            {
                var pool = _pools[poolData.PoolIndex];

                while (pool.Count > 0)
                {
                    GameObject obj = pool.Pop();
                    if (obj) Destroy(obj);
                }

                if (_poolParents[poolData.PoolIndex])
                    Destroy(_poolParents[poolData.PoolIndex].gameObject);

                _poolMap.Remove(prefabId);
                _prefabLookup[poolData.PoolIndex] = null;
                _poolParents[poolData.PoolIndex] = null;
            }
        }

        public void ClearAllPools()
        {
            for (int i = 0; i < _poolCount; i++)
            {
                var pool = _pools[i];
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Pop();
                    if (obj) Destroy(obj);
                }

                if (_poolParents[i])
                    Destroy(_poolParents[i].gameObject);
            }

            _poolMap.Clear();
            Array.Clear(_prefabLookup, 0, _poolCount);
            Array.Clear(_poolParents, 0, _poolCount);
            _poolCount = 0;
        }

        // Simple stats
        public int GetPooledCount(GameObject prefab)
        {
            int prefabId = prefab.GetInstanceID();
            if (_poolMap.TryGetValue(prefabId, out PoolData poolData))
            {
                return _pools[poolData.PoolIndex].Count;
            }

            return 0;
        }

        public bool HasPool(GameObject prefab) => _poolMap.ContainsKey(prefab.GetInstanceID());

        private struct PoolData
        {
            public int PoolIndex;
            public GameObject Prefab;
        }
    }
}
