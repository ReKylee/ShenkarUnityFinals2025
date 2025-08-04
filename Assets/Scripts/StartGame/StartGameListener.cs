using Audio.Interfaces;
using Audio.Services;
using InputSystem;
using LevelSelection.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace StartGame
{
    public class StartGameListener : MonoBehaviour
    {
        private ISceneLoadService _sceneLoadService;
        private InputSystem_Actions _submitAction;

        public void Start()
        {
            _submitAction = new InputSystem_Actions();
            _submitAction.UI.Submit.performed += StartGame;
            _submitAction.UI.Submit.Enable();
        }

        private void OnDestroy()
        {
            if (_submitAction != null)
            {
                _submitAction.UI.Submit.performed -= StartGame;
                _submitAction.UI.Submit.Disable();
                _submitAction.Dispose();
            }
        }

        [Inject]
        public void Construct(ISceneLoadService sceneLoadService)
        {
            _sceneLoadService = sceneLoadService;
        }

        private void StartGame(InputAction.CallbackContext callbackContext)
        {
            _sceneLoadService?.LoadLevel("Level Select");
        }
    }
}
