using System;
using Collectables._Base;
using UnityEngine;

namespace Collectables.Score
{
    public class ScoreCollectable : CollectibleBase
    {

        public static Action<int, Vector3> OnScoreCollected;
        [SerializeField] private int scoreAmount = 1;
        public override void OnCollect(GameObject collector)
        {
            Debug.Log("Score collected: " + gameObject.name);
            OnScoreCollected?.Invoke(scoreAmount, transform.position);
        }
    }
}
