using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using TMPro;

namespace Collectables.Score
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class FloatingText : MonoBehaviour
    {
        public IObjectPool<TextMeshProUGUI> Pool { get; set; }

        [SerializeField] private float floatDistance = 50f;
        [SerializeField] private float duration = 1f;

        private RectTransform rect;
        private TextMeshProUGUI tmp;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            tmp = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            StartCoroutine(FloatAndRelease());
        }

        private IEnumerator FloatAndRelease()
        {
            Vector2 start = rect.anchoredPosition;
            Vector2 end = start + Vector2.up * floatDistance;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rect.anchoredPosition = Vector2.Lerp(start, end, t);
                yield return null;
            }

            // return to pool
            Pool?.Release(tmp);
        }
    }
}
