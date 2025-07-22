using UnityEngine;
using UnityEngine.Pool;
using TMPro;

namespace Collectables.Score
{
    public class ScoreTextPool : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreTextPrefab;

        private IObjectPool<TextMeshProUGUI> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<TextMeshProUGUI>(
                CreateNewTextForPool,
                OnTakeFromPool,
                OnReturnToPool,
                OnDestroyPoolObject,
                true, 10, 100);
        }
        private void OnTakeFromPool(TextMeshProUGUI tmp)
        {
            tmp.gameObject.SetActive(true);
        }

        private void OnReturnToPool(TextMeshProUGUI tmp)
        {
            tmp.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(TextMeshProUGUI tmp)
        {
            if (!tmp) return;
            Destroy(tmp.gameObject);
        }
        private TextMeshProUGUI CreateNewTextForPool()
        {
            TextMeshProUGUI tmp = Instantiate(scoreTextPrefab, transform);
            tmp.gameObject.SetActive(false);
            FloatingText floatComp = tmp.GetComponent<FloatingText>();
            floatComp.Pool = _pool;
            return tmp;
        }
        public TextMeshProUGUI Get(string text)
        {
            TextMeshProUGUI tmp = _pool.Get();
            tmp.text = text;
            return tmp;
        }
    }
}
