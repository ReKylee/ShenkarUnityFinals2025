using Health.Views;
using Player.Services;
using UnityEngine;
using VContainer;

namespace Player.UI
{
    public class PlayerLivesUIController : MonoBehaviour
    {
        [SerializeField] private TextHealthView livesView;
        
        private IPlayerLivesService _livesService;

        [Inject]
        public void Construct(IPlayerLivesService livesService)
        {
            _livesService = livesService;
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

        private void UpdateDisplay(int lives)
        {
            livesView?.UpdateDisplay(lives, _livesService.MaxLives);
        }
    }
}
