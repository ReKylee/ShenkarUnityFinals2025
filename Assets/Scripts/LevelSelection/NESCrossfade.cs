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

        [Header("NES Style Effect")] public bool useNesEffect = true;

        [SerializeField] private float noiseIntensity = 0.1f;
        [SerializeField] private int frameSkip = 3; // Skip frames for authentic NES feel

        // Authentic NES palette colors for fade effect
        private readonly Color[] _nesBlackPalette =
        {
            new(0.0f, 0.0f, 0.0f, 1f), // Pure black
            new(0.2f, 0.2f, 0.2f, 1f), // Dark gray
            new(0.1f, 0.1f, 0.2f, 1f), // Dark blue-gray
            new(0.15f, 0.1f, 0.25f, 1f) // Purple-gray
        };

        private Coroutine _currentFade;
        private int _frameCounter;

        private Action _onFadeComplete;

        public bool IsFading { get; private set; }

        private void Awake()
        {
            if (fadeImage == null)
            {
                SetupFadeImage();
            }
        }

        private void SetupFadeImage()
        {
            // Create fade image if not assigned
            GameObject fadeGO = new("NES_FadeImage");
            fadeGO.transform.SetParent(transform, false);

            // Ensure we have a Canvas component
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.sortingOrder = 10000; // Very high to ensure it's on top
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                // Add CanvasScaler for proper scaling
                CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(256, 240); // NES resolution!
                scaler.matchWidthOrHeight = 0.5f;

                // Add GraphicRaycaster
                gameObject.AddComponent<GraphicRaycaster>();
            }

            fadeImage = fadeGO.AddComponent<Image>();
            RectTransform rect = fadeImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            // Set initial state
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            fadeImage.enabled = false;

            Debug.Log("[NesCrossfade] Fade image setup complete");
        }

        public void FadeOut(Action onComplete = null)
        {
            if (_currentFade != null)
            {
                StopCoroutine(_currentFade);
            }

            _onFadeComplete = onComplete;
            _currentFade = StartCoroutine(FadeCoroutine(0f, 1f));
        }

        public void FadeIn(Action onComplete = null)
        {
            if (_currentFade != null)
            {
                StopCoroutine(_currentFade);
            }

            _onFadeComplete = onComplete;
            _currentFade = StartCoroutine(FadeCoroutine(1f, 0f));
        }

        private IEnumerator FadeCoroutine(float from, float to)
        {
            IsFading = true;
            _frameCounter = 0;

            // Enable image when starting fade
            if (fadeImage != null)
            {
                fadeImage.enabled = true;
            }

            float elapsed = 0f;
            float lastAlpha = from;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Use unscaled time for reliable fades
                float progress = elapsed / fadeDuration;

                // Use stepped animation for NES authenticity
                if (useNesEffect)
                {
                    progress = StepProgress(progress, 16); // 16 steps like NES
                }

                float targetAlpha = Mathf.Lerp(from, to, progress);

                // Only update on certain frames for NES effect
                _frameCounter++;
                if (!useNesEffect || _frameCounter >= frameSkip)
                {
                    _frameCounter = 0;

                    if (useNesEffect)
                    {
                        SetNesStyleAlpha(targetAlpha);
                    }
                    else
                    {
                        SetAlpha(targetAlpha);
                    }

                    lastAlpha = targetAlpha;
                }

                yield return null;
            }

            // Ensure final value is set
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
            _currentFade = null;
            _onFadeComplete?.Invoke();
            _onFadeComplete = null;
        }

        private float StepProgress(float progress, int steps) => Mathf.Floor(progress * steps) / steps;

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
            if (fadeImage == null) return;

            // Use the NES black palette for authentic fade
            Color baseColor;

            if (alpha <= 0.25f)
            {
                baseColor = Color.Lerp(Color.clear, _nesBlackPalette[0], alpha * 4f);
            }
            else if (alpha <= 0.5f)
            {
                baseColor = Color.Lerp(_nesBlackPalette[0], _nesBlackPalette[1], (alpha - 0.25f) * 4f);
            }
            else if (alpha <= 0.75f)
            {
                baseColor = Color.Lerp(_nesBlackPalette[1], _nesBlackPalette[2], (alpha - 0.5f) * 4f);
            }
            else
            {
                baseColor = Color.Lerp(_nesBlackPalette[2], _nesBlackPalette[3], (alpha - 0.75f) * 4f);
            }

            // Add subtle noise for CRT effect
            if (alpha > 0f && alpha < 1f)
            {
                float noise = (Mathf.PerlinNoise(Time.time * 10f, 0f) - 0.5f) * noiseIntensity;
                baseColor.r = Mathf.Clamp01(baseColor.r + noise);
                baseColor.g = Mathf.Clamp01(baseColor.g + noise);
                baseColor.b = Mathf.Clamp01(baseColor.b + noise);
            }

            fadeImage.color = baseColor;
        }

        public void Show()
        {
            if (fadeImage != null)
            {
                fadeImage.enabled = true;
                if (useNesEffect)
                {
                    SetNesStyleAlpha(1f);
                }
                else
                {
                    SetAlpha(1f);
                }
            }
        }

        public void Hide()
        {
            if (fadeImage != null)
            {
                fadeImage.enabled = false;
                SetAlpha(0f);
            }
        }

        public float GetFadeProgress() => fadeImage && fadeImage.enabled ? fadeImage.color.a : 0f;

        public void SetInstantFade(float alpha)
        {
            if (fadeImage != null)
            {
                fadeImage.enabled = alpha > 0f;

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

        public void SetFadeColor(Color color)
        {
            fadeColor = color;
        }
    }
}
