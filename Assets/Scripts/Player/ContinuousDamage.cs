using System.Collections;
using Core;
using GameEvents;
using GameEvents.Interfaces;
using UnityEngine;
using VContainer;

namespace Player
{
    public class ContinuousDamage : MonoBehaviour
    {
        [SerializeField] private PlayerHealthController healthController;
        [SerializeField] private float damageInterval = 3f;
        private Coroutine _damageCoroutine;
        private GameManager _gameManager;
        private IEventBus _eventBus;

        #region VContainer Injection
        [Inject]
        public void Construct(GameManager gameManager, IEventBus eventBus)
        {
            _gameManager = gameManager;
            _eventBus = eventBus;
        }
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // Subscribe to game state changes to automatically start/stop damage
            _eventBus?.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            
            // Start damage if game is already playing
            if (_gameManager?.IsPlaying == true)
            {
                StartContinuousDamage();
            }
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            StopContinuousDamage();
        }
        #endregion

        #region Event Handlers
        private void OnGameStateChanged(GameStateChangedEvent gameStateEvent)
        {
            switch (gameStateEvent.NewState)
            {
                case GameState.Playing:
                    StartContinuousDamage();
                    break;
                case GameState.Paused:
                case GameState.GameOver:
                case GameState.Victory:
                case GameState.MainMenu:
                    StopContinuousDamage();
                    break;
            }
        }
        #endregion

        #region Public API
        public void StartContinuousDamage()
        {
            if (_gameManager?.IsPlaying != true) return;
            _damageCoroutine ??= StartCoroutine(DamageLoop());
        }

        public void StopContinuousDamage()
        {
            if (_damageCoroutine == null) return;
            StopCoroutine(_damageCoroutine);
            _damageCoroutine = null;
        }
        #endregion

        #region Private Methods
        private IEnumerator DamageLoop()
        {
            while (healthController.CurrentHp > 0)
            {
                // Only apply damage when game is actively playing
                if (_gameManager?.IsPlaying != true)
                {
                    yield return null;
                    continue;
                }
                
                healthController.Damage(1);
                yield return new WaitForSeconds(damageInterval);
            }
            _damageCoroutine = null;
        }
        #endregion
    }
}
