using System;
using Collectables._Base;
using LocksAndKeys;
using UnityEngine;

namespace Collectables.Keys
{
    public class KeyCollectable : CollectibleBase
    {
        private IKey _myKey;
        private void Awake()
        {
            _myKey = GetComponent<IKey>();
        }

        public override void OnCollect(GameObject collector)
        {
            if (_myKey == null) return;

            OnKeyCollected?.Invoke(_myKey);
        }
        public static event Action<IKey> OnKeyCollected;
    }
}
