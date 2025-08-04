using InputSystem;
using UnityEngine;

namespace ModularCharacterController.Core
{
    /// <summary>
    ///     Input handler for Kirby's controls with separate contexts for Update and FixedUpdate
    /// </summary>
    public class InputHandler : MonoBehaviour
    {

        // Main input context that gets populated by events
        private InputContext _currentInput;

        // Copy specifically for FixedUpdate to use - completely separate
        private InputContext _fixedUpdateInput;

        private InputSystem_Actions _inputActions;
        public InputContext CurrentInput => _currentInput;

        public InputContext FixedInput => _fixedUpdateInput;
        
        #if UNITY_EDITOR
        public bool debugMode = false;
        #endif
        private void Awake()
        {
            _inputActions = new InputSystem_Actions();
            _currentInput = new InputContext();
            _fixedUpdateInput = new InputContext();
            SetupEvents();
        }

        // Copy the current input state to the fixed input state
        // This guarantees FixedUpdate always has access to inputs, even if they were reset in Update
        private void FixedUpdate()
        {
            _fixedUpdateInput.WalkInput = _currentInput.WalkInput;
            _fixedUpdateInput.JumpPressed = _currentInput.JumpPressed;
            _fixedUpdateInput.JumpReleased = _currentInput.JumpReleased;
            _fixedUpdateInput.JumpHeld = _currentInput.JumpHeld;
            _fixedUpdateInput.AttackPressed = _currentInput.AttackPressed;
            _fixedUpdateInput.AttackReleased = _currentInput.AttackReleased;
            _fixedUpdateInput.AttackHeld = _currentInput.AttackHeld;
            ResetButtons();
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();
        }

        #region DEBUG

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!debugMode) return;
            string[] labels =
            {
                $"Walk Input: {_currentInput.WalkInput}",
                $"Jump Pressed: {_currentInput.JumpPressed}",
                $"Jump Released: {_currentInput.JumpReleased}",
                $"Jump Held: {_currentInput.JumpHeld}",
                $"Attack Pressed: {_currentInput.AttackPressed}",
                $"Attack Released: {_currentInput.AttackReleased}",
                $"Attack Held: {_currentInput.AttackHeld}"
            };

            GUIStyle labelStyle = new(GUI.skin.label);
            labelStyle.fontSize = 30;
            labelStyle.normal.background = Texture2D.grayTexture;
            labelStyle.alignment = TextAnchor.MiddleCenter;

            const float offsetX = 20;
            const float labelWidth = 500;
            const float labelHeight = 50;
            const float spacing = 20;

            float screenHeight = Screen.height;
            float totalHeight = labels.Length * labelHeight + (labels.Length - 1) * spacing;
            float startY = (screenHeight - totalHeight) / 2;

            for (int i = 0; i < labels.Length; i++)
            {
                float currentY = startY + i * (labelHeight + spacing);
                GUI.Label(new Rect(offsetX, currentY, labelWidth, labelHeight), labels[i], labelStyle);
            }
        }

#endif

        #endregion

        // Reset button press/release states but leave held states
        // Only affects the Update input context, never the FixedUpdate context
        private void ResetButtons()
        {
            _currentInput.JumpPressed = false;
            _currentInput.JumpReleased = false;
            _currentInput.AttackPressed = false;
            _currentInput.AttackReleased = false;
        }

        private void SetupEvents()
        {
            _inputActions.Player.Walk.performed += ctx => _currentInput.WalkInput = ctx.ReadValue<float>();
            _inputActions.Player.Walk.canceled += _ => _currentInput.WalkInput = 0f;


            _inputActions.Player.Jump.performed += _ =>
            {
                _currentInput.JumpPressed = true;
                _currentInput.JumpHeld = true;
            };

            _inputActions.Player.Jump.canceled += _ =>
            {
                _currentInput.JumpReleased = true;
                _currentInput.JumpHeld = false;
            };

            _inputActions.Player.Attack.performed += _ =>
            {
                _currentInput.AttackPressed = true;
                _currentInput.AttackHeld = true;
            };

            _inputActions.Player.Attack.canceled += _ =>
            {
                _currentInput.AttackReleased = true;
                _currentInput.AttackHeld = false;
            };


        }
    }
}
