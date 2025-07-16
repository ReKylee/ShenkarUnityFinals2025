using System.Collections;
using Managers;
using Player;
using TMPro;
using UnityEngine;

namespace GameEvents
{
    public class EndGameManager : MonoBehaviour
    {
        [SerializeField] private PlayerDeath playerDeath;
        [SerializeField] private TextMeshProUGUI endGameText;
        private bool _gameEnded;
        private void OnEnable()
        {
            playerDeath.onDeath.AddListener(PlayerDied);
        }
        private void OnDisable()
        {
            playerDeath.onDeath.RemoveListener(PlayerDied);
        }
        public void EndGame()
        {
            if (_gameEnded) return;
            StartCoroutine(EndGameRoutine());
            _gameEnded = true;

        }
        private void PlayerDied()
        {
            endGameText.text = "You Died!";
            endGameText.gameObject.SetActive(true);
            EndGame();
        }
        public void PlayerWon()
        {
            endGameText.text = "You Won!";
            endGameText.gameObject.SetActive(true);
            EndGame();
        }
        private IEnumerator EndGameRoutine()
        {
            Debug.Log("Game Ended! Closing in 5 seconds.");
            yield return new WaitForSeconds(5f);
            ResetManager.Instance?.ResetAll();
            _gameEnded = false;
        }
    }
}
