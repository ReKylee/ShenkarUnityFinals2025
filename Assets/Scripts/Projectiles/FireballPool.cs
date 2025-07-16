using Projectiles.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace Projectiles
{
    public class FireballPool : MonoBehaviour
    {
        [SerializeField] private GameObject fireballPrefab;

        private IObjectPool<GameObject> _pool;

        private void Awake()
        {

            _pool = new ObjectPool<GameObject>(
                CreateNewFireballForPool,
                OnTakeFromPool,
                OnReturnToPool,
                OnDestroyPoolObject,
                true, 10, 100);
        }
        private void OnTakeFromPool(GameObject laser)
        {
            Debug.Log("Laser taken from pool.");
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
        private GameObject CreateNewFireballForPool()
        {
            Debug.Log("Pool is creating a new laser using the Director/Builder.");
            GameObject newLaser = Instantiate(fireballPrefab, transform);
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
