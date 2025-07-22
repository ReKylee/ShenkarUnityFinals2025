using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using TMPro;

namespace Collectables.Score
{
    [RequireComponent(typeof(TextMeshPro))]
    public class FloatingText : MonoBehaviour
    {
        public IObjectPool<TextMeshPro> Pool { get; set; }
        [SerializeField] private float ppu = 100f; 
        [SerializeField] private float floatDistance = 1f;
        [SerializeField] private float duration = 1f;

        private TextMeshPro _tmp;

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
            Pool?.Release(_tmp);
        }
    }
}
