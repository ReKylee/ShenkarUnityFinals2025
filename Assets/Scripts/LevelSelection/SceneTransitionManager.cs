using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core.Events;

namespace LevelSelection
{
    /// <summary>
    /// Standalone scene transition manager that works in any scene
    /// Automatically creates crossfade and handles all scene transitions
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        [Header("Transition Settings")]
        [SerializeField] private bool useNESEffect = true;
        [SerializeField] private bool fadeInOnSceneStart = true;
        [SerializeField] private float fadeInDelay = 0.1f;
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private Color fadeColor = Color.black;
        [SerializeField] private Color[] nesColors = { Color.black, new(0.2f, 0.2f, 0.3f), new(0.1f, 0.1f, 0.2f) };
        
        private static SceneTransitionManager _instance;
        private NESCrossfade _activeCrossfade;
        private bool _isTransitioning;
        
        public static SceneTransitionManager Instance => _instance;
        public bool IsTransitioning => _isTransitioning;
        
        private void Awake()
        {
            // Implement singleton pattern that works across scenes
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                CreateStandaloneCrossfade();
                SubscribeToSceneEvents();
                Debug.Log("[SceneTransitionManager] Initialized as standalone singleton");
            }
            else if (_instance != this)
            {
                Debug.Log("[SceneTransitionManager] Duplicate instance destroyed");
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            // Fade in when scene starts (if enabled and crossfade is visible)
            if (fadeInOnSceneStart && _activeCrossfade != null)
            {
                StartCoroutine(FadeInAfterDelay());
            }
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
            {
                UnsubscribeFromSceneEvents();
                _instance = null;
            }
        }

        private void CreateStandaloneCrossfade()
        {
            // Create a completely standalone crossfade system
            GameObject crossfadeGO = new GameObject("SceneTransitionCrossfade");
            crossfadeGO.transform.SetParent(transform);
            
            // Add Canvas for overlay rendering
            Canvas canvas = crossfadeGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // Highest priority
            
            // Add CanvasScaler for proper scaling
            UnityEngine.UI.CanvasScaler scaler = crossfadeGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add GraphicRaycaster
            crossfadeGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Create fade image
            GameObject imageGO = new GameObject("FadeImage");
            imageGO.transform.SetParent(crossfadeGO.transform, false);
            
            UnityEngine.UI.Image fadeImage = imageGO.AddComponent<UnityEngine.UI.Image>();
            RectTransform rect = fadeImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            
            // Set initial color and disable
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
            fadeImage.enabled = false;
            
            // Add and configure NESCrossfade component
            _activeCrossfade = crossfadeGO.AddComponent<NESCrossfade>();
            _activeCrossfade.fadeImage = fadeImage;
            _activeCrossfade.fadeDuration = fadeDuration;
            _activeCrossfade.fadeColor = fadeColor;
            _activeCrossfade.useNESEffect = useNESEffect;
            _activeCrossfade.nesColors = nesColors;
            
            Debug.Log("[SceneTransitionManager] Standalone crossfade created");
        }

        private void SubscribeToSceneEvents()
        {
            // Listen for scene load events to handle automatic transitions
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void UnsubscribeFromSceneEvents()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[SceneTransitionManager] Scene loaded: {scene.name}");
            
            // Auto fade in after scene load
            if (fadeInOnSceneStart && _activeCrossfade != null)
            {
                StartCoroutine(FadeInAfterDelay());
            }
        }
        
        private IEnumerator FadeInAfterDelay()
        {
            if (_activeCrossfade == null) yield break;
            
            yield return new WaitForSeconds(fadeInDelay);
            
            // Only fade in if crossfade is currently visible
            if (_activeCrossfade.GetFadeProgress() > 0f)
            {
                Debug.Log("[SceneTransitionManager] Auto fading in on scene start");
                _activeCrossfade.FadeIn(() => Debug.Log("[SceneTransitionManager] Scene fade-in complete"));
            }
        }
        
        /// <summary>
        /// Transition to a new scene with crossfade effect
        /// </summary>
        public void TransitionToScene(string sceneName)
        {
            if (!_isTransitioning)
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
            
            _isTransitioning = true;
            
            Debug.Log($"[SceneTransitionManager] Starting transition to: {sceneName}");
            
            // Fade out
            bool fadeOutComplete = false;
            _activeCrossfade.FadeOut(() => fadeOutComplete = true);
            
            // Wait for fade out to complete
            yield return new WaitUntil(() => fadeOutComplete);
            
            // Load the new scene
            Debug.Log($"[SceneTransitionManager] Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
            
            // Reset transitioning flag after scene loads
            _isTransitioning = false;
            
            Debug.Log($"[SceneTransitionManager] Transition to {sceneName} complete");
        }
        
        /// <summary>
        /// Manually trigger fade out
        /// </summary>
        public void FadeOut(Action onComplete = null)
        {
            _activeCrossfade?.FadeOut(onComplete);
        }
        
        /// <summary>
        /// Manually trigger fade in
        /// </summary>
        public void FadeIn(Action onComplete = null)
        {
            _activeCrossfade?.FadeIn(onComplete);
        }
        
        /// <summary>
        /// Check if crossfade is currently visible
        /// </summary>
        public bool IsCrossfadeVisible()
        {
            return _activeCrossfade != null && _activeCrossfade.GetFadeProgress() > 0f;
        }
        
        /// <summary>
        /// Create a standalone SceneTransitionManager if one doesn't exist
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void EnsureInstance()
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SceneTransitionManager");
                go.AddComponent<SceneTransitionManager>();
                Debug.Log("[SceneTransitionManager] Auto-created standalone instance");
            }
        }
        
        /// <summary>
        /// Static method to transition scenes from anywhere
        /// </summary>
        public static void TransitionTo(string sceneName)
        {
            if (_instance != null)
            {
                _instance.TransitionToScene(sceneName);
            }
            else
            {
                Debug.LogWarning("[SceneTransitionManager] No instance available, loading scene directly");
                SceneManager.LoadScene(sceneName);
            }
        }
    }
}
