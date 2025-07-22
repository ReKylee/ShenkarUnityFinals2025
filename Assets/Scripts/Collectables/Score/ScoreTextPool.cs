using UnityEngine;
using UnityEngine.Pool;
using TMPro;

namespace Collectables.Score
{
    public class ScoreTextPool : MonoBehaviour
    {
        [SerializeField] private TextMeshPro scoreTextPrefab;

        private IObjectPool<TextMeshPro> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<TextMeshPro>(
                CreateNewTextForPool,
                OnTakeFromPool,
                OnReturnToPool,
                OnDestroyPoolObject,
                true, 10, 100);
        }
        private void OnTakeFromPool(TextMeshPro tmp)
        {
            tmp.gameObject.SetActive(true);
        }

        private void OnReturnToPool(TextMeshPro tmp)
        {
            tmp.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(TextMeshPro tmp)
        {
            if (!tmp) return;
            Destroy(tmp.gameObject);
        }
        private TextMeshPro CreateNewTextForPool()
        {
            TextMeshPro tmp = Instantiate(scoreTextPrefab, transform);
            tmp.gameObject.SetActive(false);
            FloatingText floatComp = tmp.GetComponent<FloatingText>();
            floatComp.Pool = _pool;
            return tmp;
        }
        public TextMeshPro Get(string text)
        {
            TextMeshPro tmp = _pool.Get();
            tmp.text = text;
            return tmp;
        }
    }
}
