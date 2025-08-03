using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LevelSelection
{
    public class NESCrossfade : MonoBehaviour
    {
        [Header("Crossfade Configuration")] public Image fadeImage;

        public float fadeDuration = 1f;
        public Color fadeColor = Color.black;
        public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("NES Style Effect")] public bool useNESEffect = true;

        public Color[] nesColors = { Color.black, new(0.2f, 0.2f, 0.3f), new(0.1f, 0.1f, 0.2f) };
        public float colorFlickerSpeed = 10f;
        private LevelSelectionConfig _config;

        private bool _isFading;
        private Action _onFadeComplete;

        private void Awake()
        {
            if (fadeImage == null)
            {
                // Create fade image if not assigned
                GameObject fadeGO = new("FadeImage");
                fadeGO.transform.SetParent(transform, false);

                Canvas canvas = GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = gameObject.AddComponent<Canvas>();
                    canvas.sortingOrder = 1000;
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }

                fadeImage = fadeGO.AddComponent<Image>();
                RectTransform rect = fadeImage.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;
            }

            // Start with transparent
            SetAlpha(0f);
        }

        public void SetConfig(LevelSelectionConfig config)
        {
            _config = config;

            // Update fade settings from config
            if (_config != null)
            {
                fadeDuration = _config.transitionDuration;
                nesColors = _config.nesTransitionColors;
            }
        }

        public void FadeOut(Action onComplete = null)
        {
            if (_isFading) return;

            _onFadeComplete = onComplete;
            StartCoroutine(FadeCoroutine(0f, 1f));
        }

        public void FadeIn(Action onComplete = null)
        {
            if (_isFading) return;

            _onFadeComplete = onComplete;
            StartCoroutine(FadeCoroutine(1f, 0f));
        }

        public void FadeOutAndIn(Action onMiddle = null, Action onComplete = null)
        {
            if (_isFading) return;

            StartCoroutine(FadeOutAndInCoroutine(onMiddle, onComplete));
        }

        private IEnumerator FadeCoroutine(float from, float to)
        {
            _isFading = true;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fadeDuration;
                float curveValue = fadeCurve.Evaluate(progress);
                float alpha = Mathf.Lerp(from, to, curveValue);

                if (useNESEffect)
                {
                    SetNESStyleAlpha(alpha);
                }
                else
                {
                    SetAlpha(alpha);
                }

                yield return null;
            }

            if (useNESEffect)
            {
                SetNESStyleAlpha(to);
            }
            else
            {
                SetAlpha(to);
            }

            _isFading = false;
            _onFadeComplete?.Invoke();
            _onFadeComplete = null;
        }

        private IEnumerator FadeOutAndInCoroutine(Action onMiddle, Action onComplete)
        {
            // Fade out
            yield return StartCoroutine(FadeCoroutine(0f, 1f));

            // Middle action (like loading scene)
            onMiddle?.Invoke();

            // Small delay to ensure scene is loaded
            yield return new WaitForSeconds(0.1f);

            // Fade in
            yield return StartCoroutine(FadeCoroutine(1f, 0f));

            onComplete?.Invoke();
        }

        private void SetAlpha(float alpha)
        {
            if (fadeImage != null)
            {
                Color color = fadeColor;
                color.a = alpha;
                fadeImage.color = color;
            }
        }

        private void SetNESStyleAlpha(float alpha)
        {
            if (fadeImage == null || nesColors == null || nesColors.Length == 0) return;

            // Create NES-style flickering effect
            int colorIndex = Mathf.FloorToInt(Time.time * colorFlickerSpeed) % nesColors.Length;
            Color nesColor = nesColors[colorIndex];
            nesColor.a = alpha;
            fadeImage.color = nesColor;
        }

        /// <summary>
        /// Set fade color programmatically
        /// </summary>
        public void SetFadeColor(Color color)
        {
            fadeColor = color;
        }

        /// <summary>
        /// Get current fade progress (0 = transparent, 1 = opaque)
        /// </summary>
        public float GetFadeProgress()
        {
            return fadeImage ? fadeImage.color.a : 0f;
        }

        /// <summary>
        /// Instantly set fade to specific alpha without animation
        /// </summary>
        public void SetInstantFade(float alpha)
        {
            if (useNESEffect)
            {
                SetNESStyleAlpha(alpha);
            }
            else
            {
                SetAlpha(alpha);
            }
        }

        /// <summary>
        /// Check if currently fading
        /// </summary>
        public bool IsFading => _isFading;
    }
}
