using UnityEngine;

namespace Collectables._Base
{
    public interface ICollectable
    {

        void OnCollect(GameObject collector);
    }
}
