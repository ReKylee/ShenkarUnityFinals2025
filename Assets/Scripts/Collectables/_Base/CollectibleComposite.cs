using UnityEngine;

namespace Collectables._Base
{
    /// <summary>
    ///     Composite manager for multiple ICollectable components on a single GameObject.
    ///     Attach this only to objects that need multiple collectibles.
    /// </summary>
    public class CollectibleComposite : MonoBehaviour
    {
        private ICollectable[] _collectables;

        private void Awake()
        {
            _collectables = GetComponents<ICollectable>();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
            {
                foreach (ICollectable collectable in _collectables)
                {
                    collectable.OnCollect(col.gameObject);
                }

                gameObject.SetActive(false);
            }
        }
    }
}
