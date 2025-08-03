using UnityEngine;

namespace LevelSelection
{
    public class LevelPoint : MonoBehaviour
    {
        [Header("Level Configuration")] public string levelName;

        public string displayName;
        public string sceneName;
        public Sprite levelIcon;

        [Header("Visual Configuration")] public SpriteRenderer iconRenderer;

        public SpriteRenderer lockRenderer;
        public Color unlockedColor = Color.white;
        public Color lockedColor = Color.gray;

        [Header("Animation")] public float pulseSpeed = 2f;

        public float pulseScale = 1.1f;

        private Vector3 _originalScale;

        public bool IsUnlocked { get; private set; }

        public bool IsSelected { get; private set; }

        private void Awake()
        {
            _originalScale = transform.localScale;

            if (iconRenderer && levelIcon)
            {
                iconRenderer.sprite = levelIcon;
            }
        }

        private void Update()
        {
            if (IsSelected && IsUnlocked)
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

        public void SetUnlocked(bool unlocked)
        {
            IsUnlocked = unlocked;
            UpdateVisuals();
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (iconRenderer)
            {
                iconRenderer.color = IsUnlocked ? unlockedColor : lockedColor;
            }

            if (lockRenderer)
            {
                lockRenderer.gameObject.SetActive(!IsUnlocked);
            }
        }
    }
}
