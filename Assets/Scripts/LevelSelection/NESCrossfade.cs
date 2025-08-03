using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LevelSelection
{
    public class NesCrossfade : MonoBehaviour
    {
        [Header("Fade Settings")] public Image fadeImage;

        public float fadeDuration = 1f;
        public Color fadeColor = Color.black;
        public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("NES Style Effect")] public bool useNesEffect = true;

        public Color[] nesColors = { Color.black, new(0.2f, 0.2f, 0.3f), new(0.1f, 0.1f, 0.2f) };
        public float colorFlickerSpeed = 10f;

        private Action _onFadeComplete;

        /// <summary>
        ///     Check if currently fading
        /// </summary>
        public bool IsFading { get; private set; }

        private void Awake()
        {
            SetupFadeImage();
            // Start with image disabled (hidden)
            if (fadeImage)
            {
                fadeImage.enabled = false;
            }

            Debug.Log("[NESCrossfade] Initialized with hidden image");
        }

        private void SetupFadeImage()
        {
            if (fadeImage == null)
            {
                // Create fade image if not assigned
                GameObject fadeGO = new("FadeImage");
                fadeGO.transform.SetParent(transform, false);

                // Ensure we have a Canvas component
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas == null)
                {
                    canvas = gameObject.AddComponent<Canvas>();
                    canvas.sortingOrder = 1000;
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                    // Add CanvasScaler for proper scaling
                    CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);

                    // Add GraphicRaycaster
                    gameObject.AddComponent<GraphicRaycaster>();
                }

                fadeImage = fadeGO.AddComponent<Image>();
                RectTransform rect = fadeImage.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;

                // Set initial color with full alpha (but start disabled)
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
                fadeImage.enabled = false;
            }

            // Ensure the image covers the full screen
            if (fadeImage != null)
            {
                RectTransform rect = fadeImage.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;
            }
        }


        public void FadeOut(Action onComplete = null)
        {
            if (IsFading) return;

            _onFadeComplete = onComplete;
            StartCoroutine(FadeCoroutine(0f, 1f));
        }

        public void FadeIn(Action onComplete = null)
        {
            if (IsFading) return;

            _onFadeComplete = onComplete;
            StartCoroutine(FadeCoroutine(1f, 0f));
        }

        public void FadeOutAndIn(Action onMiddle = null, Action onComplete = null)
        {
            if (IsFading) return;

            StartCoroutine(FadeOutAndInCoroutine(onMiddle, onComplete));
        }

        private IEnumerator FadeCoroutine(float from, float to)
        {
            IsFading = true;

            // Enable image when starting fade
            if (fadeImage != null)
            {
                fadeImage.enabled = true;
            }

            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / fadeDuration;
                float curveValue = fadeCurve.Evaluate(progress);
                float alpha = Mathf.Lerp(from, to, curveValue);

                if (useNesEffect)
                {
                    SetNesStyleAlpha(alpha);
                }
                else
                {
                    SetAlpha(alpha);
                }

                yield return null;
            }

            if (useNesEffect)
            {
                SetNesStyleAlpha(to);
            }
            else
            {
                SetAlpha(to);
            }

            // Hide image if fade is complete (alpha = 0)
            if (to <= 0f && fadeImage != null)
            {
                fadeImage.enabled = false;
            }

            IsFading = false;
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

        private void SetNesStyleAlpha(float alpha)
        {
            if (fadeImage == null || nesColors == null || nesColors.Length == 0) return;

            // Create NES-style flickering effect
            int colorIndex = Mathf.FloorToInt(Time.time * colorFlickerSpeed) % nesColors.Length;
            Color nesColor = nesColors[colorIndex];
            nesColor.a = alpha;
            fadeImage.color = nesColor;
        }

        /// <summary>
        ///     Set fade color programmatically
        /// </summary>
        public void SetFadeColor(Color color)
        {
            fadeColor = color;
        }

        /// <summary>
        ///     Show the crossfade immediately (useful for scene start)
        /// </summary>
        public void Show()
        {
            if (fadeImage != null)
            {
                fadeImage.enabled = true;
                SetAlpha(1f);
            }
        }

        /// <summary>
        ///     Hide the crossfade immediately
        /// </summary>
        public void Hide()
        {
            if (fadeImage != null)
            {
                fadeImage.enabled = false;
                SetAlpha(0f);
            }
        }

        /// <summary>
        ///     Get current fade progress (0 = hidden, 1 = fully visible)
        /// </summary>
        public float GetFadeProgress() => fadeImage && fadeImage.enabled ? fadeImage.color.a : 0f;

        /// <summary>
        ///     Instantly set fade to specific alpha without animation
        /// </summary>
        public void SetInstantFade(float alpha)
        {
            if (useNesEffect)
            {
                SetNesStyleAlpha(alpha);
            }
            else
            {
                SetAlpha(alpha);
            }
        }
    }
}
