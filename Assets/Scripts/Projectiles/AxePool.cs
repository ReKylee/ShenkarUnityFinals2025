using Projectiles.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace Projectiles
{
    public class AxePool : MonoBehaviour
    {
        [SerializeField] private GameObject axePrefab;

        private IObjectPool<GameObject> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<GameObject>(
                CreateNewAxeForPool,
                OnTakeFromPool,
                OnReturnToPool,
                OnDestroyPoolObject,
                true, 10, 100);
        }
        private void OnTakeFromPool(GameObject laser)
        {
            laser.SetActive(true);
        }

        private void OnReturnToPool(GameObject laser)
        {
            laser.SetActive(false);
        }

        private void OnDestroyPoolObject(GameObject laser)
        {
            DestroyImmediate(laser);
        }
        private GameObject CreateNewAxeForPool()
        {
            GameObject newLaser = Instantiate(axePrefab, transform);
            // Give the projectile a reference to the pool so it can return itself.
            if (newLaser.TryGetComponent(out BaseProjectile projectile))
            {
                projectile.Pool = _pool;

            }

            return newLaser;
        }
        public GameObject Get() => _pool.Get();
    }
}
