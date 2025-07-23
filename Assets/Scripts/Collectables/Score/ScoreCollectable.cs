using Collectables._Base;
using UnityEngine;
using System;

namespace Collectables.Score
{
    public class ScoreCollectable : CollectibleBase
    {
        [SerializeField] private int scoreAmount = 1;

        public static Action<int, Vector3> OnScoreCollected;

        public override void OnCollect(GameObject collector)
        {
            Debug.Log("Score collected: " + gameObject.name);

            OnScoreCollected?.Invoke(scoreAmount, transform.position);
        }
    }
}
