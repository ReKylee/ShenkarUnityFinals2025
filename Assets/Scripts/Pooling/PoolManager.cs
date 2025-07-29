using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Pool;

namespace Pooling
{
    public class PoolManager : MonoBehaviour, IPoolService
    {
        [SerializeField] private int defaultCapacity = 10;
        [SerializeField] private int maxSize = 128;
        [SerializeField] private int maxPools = 64;

        // Ultra-fast array-based mapping - fastest approach for Unity
        private PoolInfo[] _pools;
        private int _poolCount = 0;

        private struct PoolInfo
        {
            public IObjectPool<GameObject> Pool;
            public Transform Parent;
            public int PrefabId;
            public GameObject Prefab;
        }

        private void Awake()
        {
            _pools = new PoolInfo[maxPools];
        }

        private void OnDestroy()
        {
            ClearAllPools();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            int prefabId = prefab.GetInstanceID();
            int poolIndex = FindPoolIndex(prefabId);

            if (poolIndex >= 0)
            {
                // Verify that the prefabId we're using matches what's stored in the pool
                if (_pools[poolIndex].PrefabId != prefabId)
                {
                    Debug.LogError(
                        $"Pool ID mismatch! Expected ID: {_pools[poolIndex].PrefabId}, Actual ID: {prefabId}");
                }

                // Use the stored Prefab to verify we're getting the correct type
                if (_pools[poolIndex].Prefab != prefab)
                {
                    Debug.LogWarning(
                        $"Pool prefab mismatch! Requested: {prefab.name}, Pool contains: {_pools[poolIndex].Prefab.name}");
                }

                GameObject obj = _pools[poolIndex].Pool.Get();
                obj.transform.SetPositionAndRotation(position, rotation);
                return obj;
            }

            // Create new pool
            return CreatePoolAndGet(prefab, prefabId, position, rotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(GameObject prefab, GameObject instance)
        {
            if (!instance) return;

            // Get the pool index first
            int prefabId = prefab.GetInstanceID();
            int poolIndex = FindPoolIndex(prefabId);

            // Fast active state check - only warn if not already in pool
            if (!instance.activeInHierarchy)
            {
                // If object is already inactive, we can assume it's already pooled
                // or in the process of being released
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (poolIndex < 0)
                {
                    Debug.LogWarning($"Attempting to release inactive object: {instance.name}");
                }
#endif
                return;
            }

            if (poolIndex >= 0)
            {
                _pools[poolIndex].Pool.Release(instance);
            }
            else
            {
                instance.SetActive(false);
            }
        }

        // Linear search is faster than Dictionary for small collections (<50 items)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindPoolIndex(int prefabId)
        {
            for (int i = 0; i < _poolCount; i++)
                if (_pools[i].PrefabId == prefabId)
                    return i;

            return -1;
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

        private GameObject CreatePoolAndGet(GameObject prefab, int prefabId, Vector3 position, Quaternion rotation)
        {
            if (_poolCount >= maxPools)
            {
                Debug.LogError("Maximum pool count exceeded!");
                return Instantiate(prefab, position, rotation);
            }

            // Create organized parent
            Transform poolParent = new GameObject($"Pool_{prefab.name}").transform;
            poolParent.SetParent(transform);

            // Create Unity's optimized ObjectPool
            var pool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(prefab, poolParent),
                actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj => obj.SetActive(false),
                actionOnDestroy: obj =>
                {
                    if (obj) Destroy(obj);
                },
                collectionCheck: false, // Disable for performance
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );

            int poolIndex = _poolCount;
            _pools[poolIndex] = new PoolInfo
            {
                Pool = pool,
                Parent = poolParent,
                PrefabId = prefabId,
                Prefab = prefab
            };

            _poolCount++;

            // Pre-populate pool
            WarmPool(prefab, Mathf.Min(8, defaultCapacity));

            // Get first object
            GameObject obj = pool.Get();
            obj.transform.SetPositionAndRotation(position, rotation);
            return obj;
        }

        // Utility methods
        public void WarmPool(GameObject prefab, int count)
        {
            int prefabId = prefab.GetInstanceID();
            int poolIndex = FindPoolIndex(prefabId);

            if (poolIndex < 0)
            {
                GameObject tempObj = Get(prefab, Vector3.zero, Quaternion.identity);
                Release(prefab, tempObj);
                poolIndex = FindPoolIndex(prefabId);
            }

            if (poolIndex >= 0)
            {
                var tempObjects = new GameObject[count];
                for (int i = 0; i < count; i++)
                {
                    tempObjects[i] = _pools[poolIndex].Pool.Get();
                }

                for (int i = 0; i < count; i++)
                {
                    _pools[poolIndex].Pool.Release(tempObjects[i]);
                }
            }
        }

        public void ClearPool(GameObject prefab)
        {
            int prefabId = prefab.GetInstanceID();
            int poolIndex = FindPoolIndex(prefabId);

            if (poolIndex >= 0)
            {
                DisposePool(poolIndex);

                // Compact array - move last element to removed position
                if (poolIndex < _poolCount - 1)
                {
                    _pools[poolIndex] = _pools[_poolCount - 1];
                }

                _poolCount--;

                // Clear last slot
                _pools[_poolCount] = default;
            }
        }

        public void ClearAllPools()
        {
            for (int i = 0; i < _poolCount; i++)
            {
                DisposePool(i);
            }

            Array.Clear(_pools, 0, _poolCount);
            _poolCount = 0;
        }

        private void DisposePool(int poolIndex)
        {

            if (_pools[poolIndex].Pool is IDisposable disposablePool)
            {
                disposablePool.Dispose();
            }

            if (_pools[poolIndex].Parent)
            {
                Destroy(_pools[poolIndex].Parent.gameObject);
            }
        }

        public int GetPooledCount(GameObject prefab)
        {
            int prefabId = prefab.GetInstanceID();
            int poolIndex = FindPoolIndex(prefabId);

            if (poolIndex >= 0 && _pools[poolIndex].Pool is ObjectPool<GameObject> objectPool)
            {
                return objectPool.CountAll - objectPool.CountActive;
            }

            return 0;
        }

        // Debug method 
        public void LogPoolStatus()
        {
            Debug.Log($"===== Pool Manager Status: {_poolCount}/{maxPools} pools =====");

            for (int i = 0; i < _poolCount; i++)
            {
                PoolInfo info = _pools[i];
                string prefabName = info.Prefab ? info.Prefab.name : "<missing>";
                string poolStatus = "";

                if (info.Pool is ObjectPool<GameObject> objPool)
                {
                    poolStatus =
                        $"Total: {objPool.CountAll}, Active: {objPool.CountActive}, Inactive: {objPool.CountInactive}";
                }

                Debug.Log($"Pool #{i}: PrefabId={info.PrefabId}, Prefab={prefabName}, {poolStatus}");
            }
        }


    }

}
