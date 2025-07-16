using Projectiles.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace Projectiles.OverEngineeredLaser
{

    public class LaserPoolManager : MonoBehaviour
    {

        [SerializeField] private GameObject laserPrefab;
        private IObjectPool<GameObject> _pool;

        private void Awake()
        {

            _pool = new ObjectPool<GameObject>(
                CreateNewLaserForPool,
                OnTakeFromPool,
                OnReturnToPool,
                OnDestroyPoolObject,
                true, 10, 100);
        }


        private GameObject CreateNewLaserForPool()
        {
            Debug.Log("Pool is creating a new laser using the Director/Builder.");
            LaserDirector director = new(laserPrefab);
            GameObject newLaser = director.Construct();
            newLaser.transform.parent = transform;
            // Give the projectile a reference to the pool so it can return itself.
            if (newLaser.TryGetComponent(out BaseProjectile projectile))
            {
                projectile.Pool = _pool;
            }

            return newLaser;
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


        public GameObject Get() => _pool.Get();
    }
}
