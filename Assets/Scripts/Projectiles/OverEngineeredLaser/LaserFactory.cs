using UnityEngine;

namespace Projectiles.OverEngineeredLaser
{
    public class LaserFactory
    {
        private readonly LaserPoolManager _poolManager;
        public LaserFactory(LaserPoolManager poolManager)
        {
            _poolManager = poolManager;
        }
        public GameObject Create()
        {
            Debug.Log("LaserFactory: Received a request to create a laser.");
            return _poolManager.Get();
        }
    }
}
