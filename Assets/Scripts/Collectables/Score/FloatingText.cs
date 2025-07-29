using System;
using System.Collections;
using Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace Collectables.Score
{
    [RequireComponent(typeof(TextMeshPro))]
    public class FloatingText : MonoBehaviour, IPoolable
    {
        [SerializeField] private float ppu = 16f;
        [SerializeField] private float floatDistance = 1f;
        [SerializeField] private float duration = 1f;

        private TextMeshPro _tmp;
        private IPoolService _poolService;
        private GameObject _sourcePrefab;

        public string Text
        {
            get => _tmp.text;
            set => _tmp.text = value;
        }

        private void Awake()
        {
            _tmp = GetComponent<TextMeshPro>();
        }

        private void OnEnable()
        {
            StartCoroutine(FloatAndRelease());
        }

        private IEnumerator FloatAndRelease()
        {
            // I've got to wait a frame to ensure the text is fully initialized
            yield return null;

            Vector3 startPos = transform.localPosition;
            float worldUnitsUp = floatDistance / ppu; // Convert pixel distance to world units
            Vector3 endPos = startPos + Vector3.up * worldUnitsUp;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                Vector3 nextPos = Vector3.Lerp(startPos, endPos, t);

                transform.localPosition = nextPos;

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = endPos;
            // Return to pool instead of invoking event
            ReturnToPool();
        }

        public void SetPoolingInfo(IPoolService poolService, GameObject sourcePrefab)
        {
            _poolService = poolService;
            _sourcePrefab = sourcePrefab;
        }

        public void ReturnToPool()
        {
            if (_poolService != null && _sourcePrefab && gameObject.activeInHierarchy)
            {
                _poolService.Release(_sourcePrefab, gameObject);
            }
        }
    }
}
