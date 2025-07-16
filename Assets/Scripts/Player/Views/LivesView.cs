using Player.Interfaces;
using TMPro;
using UnityEngine;

namespace Player.Views
{
    /// <summary>
    ///     Specialized view for displaying player lives
    /// </summary>
    public class LivesView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI livesText;
        [SerializeField] private string format = "Lives: {0}";

        private ILivesSystem _livesSystem;

        private void OnDestroy()
        {
            if (_livesSystem != null)
            {
                _livesSystem.OnLivesChanged -= UpdateDisplay;
            }
        }

        public void Initialize(ILivesSystem livesSystem)
        {
            _livesSystem = livesSystem;

            if (_livesSystem != null)
            {
                _livesSystem.OnLivesChanged += UpdateDisplay;
                UpdateDisplay(_livesSystem.CurrentLives, _livesSystem.MaxLives);
            }
        }

        public void UpdateDisplay(int currentLives, int maxLives)
        {
            if (livesText != null)
            {
                livesText.text = string.Format(format, currentLives);
            }
        }
    }
}
