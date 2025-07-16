using UnityEngine;

namespace Projectiles.OverEngineeredLaser
{

    public class LaserBuilder
    {
        private readonly GameObject _projectileInstance;
        private readonly ProjectileLaser _projectileLaserComponent;

        public LaserBuilder(GameObject prefab)
        {
            _projectileInstance = Object.Instantiate(prefab);
            _projectileLaserComponent = _projectileInstance.GetComponent<ProjectileLaser>();
        }


        public LaserBuilder SetSpeed(Vector2 speed)
        {
            _projectileLaserComponent?.SetSpeed(speed);
            return this;
        }


        public GameObject Build() => _projectileInstance;
    }
}
