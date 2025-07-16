using UnityEngine;

namespace Projectiles.OverEngineeredLaser
{

    public class LaserDirector
    {
        private readonly LaserBuilder _builder;

        public LaserDirector(GameObject prefab)
        {
            _builder = new LaserBuilder(prefab);
        }


        public GameObject Construct() =>
            _builder
                .SetSpeed(new Vector2(0, 12f))
                .Build();


        public GameObject ConstructFastLaser() =>
            _builder
                .SetSpeed(new Vector2(0, 30f))
                .Build();
    }
}
