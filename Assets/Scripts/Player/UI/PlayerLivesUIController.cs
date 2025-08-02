using System;
using Collectables.Counter;
using Health.Views;
using Player.Interfaces;
using UnityEngine;
using VContainer;

namespace Player.UI
{
    public class PlayerLivesUIController : MonoBehaviour
    {
        [SerializeField] private PaddedTextView livesHealthView;
        private ICounterView _livesHealthView;
        private IPlayerLivesService _livesService;
        private void Awake()
        {
            _livesHealthView = livesHealthView;
        }
        private void Start()
        {
            if (_livesService != null)
            {
                _livesService.OnLivesChanged += UpdateDisplay;
                UpdateDisplay(_livesService.CurrentLives);
            }
        }

        private void OnDestroy()
        {
            if (_livesService != null)
            {
                _livesService.OnLivesChanged -= UpdateDisplay;
            }
        }

        [Inject]
        public void Construct(IPlayerLivesService livesService)
        {
            _livesService = livesService;
        }

        private void UpdateDisplay(int lives)
        {
            _livesHealthView?.UpdateCountDisplay(lives);
        }
    }
}
