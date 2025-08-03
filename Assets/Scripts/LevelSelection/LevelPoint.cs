using UnityEngine;

namespace LevelSelection
{
    public class LevelPoint : MonoBehaviour
    {
        [Header("Level Configuration")]
        public string levelName;
        public string displayName;
        public string sceneName;
        public Sprite levelIcon;
        
        [Header("Visual Configuration")]
        public SpriteRenderer iconRenderer;
        public SpriteRenderer lockRenderer;
        public Color unlockedColor = Color.white;
        public Color lockedColor = Color.gray;
        
        [Header("Animation")]
        public float pulseSpeed = 2f;
        public float pulseScale = 1.1f;
        
        private bool _isUnlocked;
        private bool _isSelected;
        private Vector3 _originalScale;

        private void Awake()
        {
            _originalScale = transform.localScale;
            
            if (iconRenderer && levelIcon)
            {
                iconRenderer.sprite = levelIcon;
            }
        }

        public void SetUnlocked(bool unlocked)
        {
            _isUnlocked = unlocked;
            UpdateVisuals();
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (iconRenderer)
            {
                iconRenderer.color = _isUnlocked ? unlockedColor : lockedColor;
            }
            
            if (lockRenderer)
            {
                lockRenderer.gameObject.SetActive(!_isUnlocked);
            }
        }

        private void Update()
        {
            if (_isSelected && _isUnlocked)
            {
                // Pulse animation for selected level
                float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * (pulseScale - 1f) * 0.5f;
                transform.localScale = _originalScale * scale;
            }
            else
            {
                transform.localScale = _originalScale;
            }
        }

        public bool IsUnlocked => _isUnlocked;
        public bool IsSelected => _isSelected;
    }
}
