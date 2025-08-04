using System;
using Core;
using InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace StartGame
{
    public class StartGameListener : MonoBehaviour
    {
        [SerializeField] private GameFlowManager gameFlowManager;
        private InputSystem_Actions _submitAction;
        public void Start()
        {
            _submitAction = new InputSystem_Actions();
            _submitAction.UI.Submit.performed += StartGame;
            _submitAction.UI.Submit.Enable();
        }

        private void StartGame(InputAction.CallbackContext callbackContext) => gameFlowManager?.NavigateToLevelSelection();
    }
}
