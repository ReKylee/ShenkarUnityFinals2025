using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LevelSelection
{
    /// <summary>
    ///     Standalone scene transition manager that works in any scene.
    ///     Automatically creates crossfade and handles all scene transitions.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {

        #region Private Fields

        private NESCrossfade _activeCrossfade;

        #endregion

        #region Serialized Fields

        [Header("Transition Settings")] [SerializeField]
        private bool useNesEffect = true;

        [SerializeField] private bool fadeInOnSceneStart = true;
        [SerializeField] private float fadeInDelay = 0.1f;
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private Color fadeColor = Color.black;

        [SerializeField] private Color[] nesColors =
        {
            Color.black,
            new(0.2f, 0.2f, 0.3f),
            new(0.1f, 0.1f, 0.2f)
        };

        #endregion

        #region Properties

        public static SceneTransitionManager Instance { get; private set; }
        public bool IsTransitioning { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeSingleton();
        }

        private void Start()
        {
            if (fadeInOnSceneStart && _activeCrossfade != null)
            {
                StartCoroutine(FadeInAfterDelay());
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                UnsubscribeFromSceneEvents();
                Instance = null;
            }
        }

        #endregion

        #region Initialization

        private void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetupCrossfade();
                SubscribeToSceneEvents();
                Debug.Log("[SceneTransitionManager] Initialized as singleton");
            }
            else if (Instance != this)
            {
                Debug.Log("[SceneTransitionManager] Duplicate instance destroyed");
                Destroy(gameObject);
            }
        }

        private void SetupCrossfade()
        {
            SetupCanvas();
            CreateFadeImage();
            ConfigureCrossfadeComponent();
            Debug.Log("[SceneTransitionManager] Crossfade setup complete");
        }

        private void SetupCanvas()
        {
            // Ensure Canvas exists
            if (GetComponent<Canvas>() == null)
            {
                Canvas canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 9999;
            }

            // Ensure CanvasScaler exists
            if (GetComponent<CanvasScaler>() == null)
            {
                CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(256, 240);
            }

            // Ensure GraphicRaycaster exists
            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private void CreateFadeImage()
        {
            GameObject imageObject = new("FadeImage");
            imageObject.transform.SetParent(transform, false);

            Image fadeImage = imageObject.AddComponent<Image>();
            RectTransform rect = fadeImage.rectTransform;

            // Center the image and use reference resolution size
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = GetComponent<CanvasScaler>().referenceResolution;
            rect.anchoredPosition = Vector2.zero;

            // Configure initial state
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
            fadeImage.enabled = false;

            // Store reference for crossfade component
            _activeCrossfade = gameObject.AddComponent<NESCrossfade>();
            _activeCrossfade.fadeImage = fadeImage;
        }

        private void ConfigureCrossfadeComponent()
        {
            _activeCrossfade.fadeDuration = fadeDuration;
            _activeCrossfade.fadeColor = fadeColor;
            _activeCrossfade.useNESEffect = useNesEffect;
            _activeCrossfade.nesColors = nesColors;
        }

        #endregion

        #region Scene Events

        private void SubscribeToSceneEvents()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void UnsubscribeFromSceneEvents()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[SceneTransitionManager] Scene loaded: {scene.name}");

            if (fadeInOnSceneStart && _activeCrossfade != null)
            {
                StartCoroutine(FadeInAfterDelay());
            }
        }

        #endregion

        #region Fade Effects

        private IEnumerator FadeInAfterDelay()
        {
            if (_activeCrossfade == null) yield break;

            yield return new WaitForSeconds(fadeInDelay);

            if (_activeCrossfade.GetFadeProgress() > 0f)
            {
                Debug.Log("[SceneTransitionManager] Auto fading in on scene start");
                _activeCrossfade.FadeIn(() => Debug.Log("[SceneTransitionManager] Scene fade-in complete"));
            }
        }

        public void FadeOut(Action onComplete = null)
        {
            _activeCrossfade?.FadeOut(onComplete);
        }

        public void FadeIn(Action onComplete = null)
        {
            _activeCrossfade?.FadeIn(onComplete);
        }

        public bool IsCrossfadeVisible() =>
            _activeCrossfade != null && _activeCrossfade.GetFadeProgress() > 0f;

        #endregion

        #region Scene Transitions

        public void TransitionToScene(string sceneName)
        {
            if (!IsTransitioning)
            {
                StartCoroutine(TransitionToSceneCoroutine(sceneName));
            }
        }

        private IEnumerator TransitionToSceneCoroutine(string sceneName)
        {
            if (_activeCrossfade == null)
            {
                Debug.LogError("[SceneTransitionManager] No crossfade available! Loading scene directly.");
                SceneManager.LoadScene(sceneName);
                yield break;
            }

            IsTransitioning = true;
            Debug.Log($"[SceneTransitionManager] Starting transition to: {sceneName}");

            // Fade out
            bool fadeOutComplete = false;
            _activeCrossfade.FadeOut(() => fadeOutComplete = true);
            yield return new WaitUntil(() => fadeOutComplete);

            // Load new scene
            Debug.Log($"[SceneTransitionManager] Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);

            IsTransitioning = false;
            Debug.Log($"[SceneTransitionManager] Transition to {sceneName} complete");
        }

        #endregion

        #region Static Methods

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void EnsureInstance()
        {
            if (Instance == null)
            {
                GameObject go = new("SceneTransitionManager");
                go.AddComponent<SceneTransitionManager>();
                Debug.Log("[SceneTransitionManager] Auto-created instance");
            }
        }

        public static void TransitionTo(string sceneName)
        {
            if (Instance != null)
            {
                Instance.TransitionToScene(sceneName);
            }
            else
            {
                Debug.LogWarning("[SceneTransitionManager] No instance available, loading scene directly");
                SceneManager.LoadScene(sceneName);
            }
        }

        #endregion

    }
}
