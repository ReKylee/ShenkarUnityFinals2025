using System;
using Collectables._Base;
using UnityEngine;

namespace Collectables.Coin
{
    public class CoinCollectable : CollectibleBase
    {

        public override void OnCollect(GameObject collector)
        {
            OnCoinCollected?.Invoke();
        }
        public static event Action OnCoinCollected;
    }
}
