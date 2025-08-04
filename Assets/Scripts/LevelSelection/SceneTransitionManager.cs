using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LevelSelection
{
    public class SceneTransitionManager : MonoBehaviour
    {
        private static bool _shouldFadeIn;

        [Header("Transition Settings")] [SerializeField]
        private float fadeDuration = 1.2f; // Slightly longer for NES feel

        [SerializeField] private Color fadeColor = Color.black;
        [SerializeField] private bool fadeInOnStart = true;
        [SerializeField] private bool useNesEffect = true;
        [SerializeField] private float sceneLoadDelay = 0.1f; // Small delay before loading

        private NesCrossfade _crossfade;

        public static SceneTransitionManager Instance { get; private set; }
        public bool IsTransitioning { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetupCrossfade();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (fadeInOnStart && _shouldFadeIn)
            {
                StartCoroutine(FadeInOnStart());
            }
            else if (fadeInOnStart)
            {
                // First scene load, start hidden
                _crossfade?.Hide();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void SetupCrossfade()
        {
            // Create canvas with NES-appropriate settings
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000; // Ensure it's always on top

            // Add canvas scaler with NES resolution
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(256, 240); // Authentic NES resolution
            scaler.matchWidthOrHeight = 0.5f;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            // Add GraphicRaycaster
            gameObject.AddComponent<GraphicRaycaster>();

            // Create fade image
            GameObject imageGo = new("NES_TransitionFade");
            imageGo.transform.SetParent(transform, false);

            Image fadeImage = imageGo.AddComponent<Image>();
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

            // Make it cover the entire screen
            RectTransform rect = fadeImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            // Set up NES crossfade component
            _crossfade = gameObject.AddComponent<NesCrossfade>();
            _crossfade.fadeImage = fadeImage;
            _crossfade.fadeDuration = fadeDuration;
            _crossfade.fadeColor = fadeColor;
            _crossfade.useNesEffect = useNesEffect;

            // Start hidden
            _crossfade.Hide();

            Debug.Log("[SceneTransitionManager] Setup complete with NES crossfade");
        }

        private IEnumerator FadeInOnStart()
        {
            // Small delay to ensure everything is loaded
            yield return new WaitForSeconds(0.05f);

            if (_crossfade != null)
            {
                Debug.Log("[SceneTransitionManager] Fading in on scene start");
                _crossfade.Show(); // Start visible
                _crossfade.FadeIn(() => { Debug.Log("[SceneTransitionManager] Fade in complete"); });
            }

            _shouldFadeIn = false;
        }

        public static void TransitionTo(string sceneName)
        {
            if (Instance != null)
            {
                Instance.StartCoroutine(Instance.TransitionCoroutine(sceneName));
            }
            else
            {
                Debug.LogWarning("[SceneTransitionManager] No instance found, loading scene directly");
                SceneManager.LoadScene(sceneName);
            }
        }

        public static void TransitionToWithDelay(string sceneName, float delay = 0f)
        {
            if (Instance != null)
            {
                Instance.StartCoroutine(Instance.DelayedTransition(sceneName, delay));
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        private IEnumerator DelayedTransition(string sceneName, float delay)
        {
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            yield return StartCoroutine(TransitionCoroutine(sceneName));
        }

        private IEnumerator TransitionCoroutine(string sceneName)
        {
            if (IsTransitioning)
            {
                Debug.LogWarning("[SceneTransitionManager] Already transitioning, ignoring request");
                yield break;
            }

            IsTransitioning = true;
            Debug.Log($"[SceneTransitionManager] Starting NES transition to: {sceneName}");

            // Fade out with NES effect
            bool fadeOutComplete = false;
            _crossfade.FadeOut(() =>
            {
                fadeOutComplete = true;
                Debug.Log("[SceneTransitionManager] Fade out complete");
            });

            // Wait for fade out to complete
            yield return new WaitUntil(() => fadeOutComplete);

            // Small delay for authentic NES timing
            yield return new WaitForSeconds(sceneLoadDelay);

            // Mark that we should fade in on the next scene
            _shouldFadeIn = true;

            // Load scene (this will destroy current scene but preserve this manager)
            Debug.Log($"[SceneTransitionManager] Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);

            // Reset transition state (will be set again if needed)
            IsTransitioning = false;
        }

        // Public methods for external control
        public void FadeOut(Action onComplete = null)
        {
            if (_crossfade != null && !IsTransitioning)
            {
                _crossfade.FadeOut(onComplete);
            }
        }

        public void FadeIn(Action onComplete = null)
        {
            if (_crossfade != null && !IsTransitioning)
            {
                _crossfade.FadeIn(onComplete);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureInstance()
        {
            if (Instance == null)
            {
                GameObject go = new("NES_SceneTransitionManager");
                go.AddComponent<SceneTransitionManager>();
                Debug.Log("[SceneTransitionManager] Auto-created instance");
            }
        }
    }
}
