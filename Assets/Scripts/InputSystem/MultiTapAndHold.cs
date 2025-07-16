using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

// Required for InitializeOnLoad

#if UNITY_EDITOR
namespace InputSystem
{
    [InitializeOnLoad]
#endif
    public class MultiTapAndHold : IInputInteraction
    {

        [Tooltip("Time in seconds to complete the entire multi-tap sequence (from the first press).")]
        public readonly float Duration = 0.5f;

        [Tooltip("The control actuation threshold (0 to 1) to register a 'press'. Input value must exceed this.")]
        public readonly float PressPoint = 0.4f;

        // Internal state variables
        private int _currentTapCount;

        // True if we've had a release and are waiting for the next qualifying press.
        private bool _isWaitingForNextPress;

        [Tooltip(
            "The control actuation threshold (0 to 1) to register a 'release'. Input value must go below this. Must be less than Press Point.")]
        public float ReleasePoint = 0.2f;

        [Tooltip("Number of taps required (e.g., 2 for double-tap). Must be 1 or greater for sensible behavior.")]
        public int TapCount = 2;

        // Static constructor for editor registration
        static MultiTapAndHold()
        {
            UnityEngine.InputSystem.InputSystem.RegisterInteraction<MultiTapAndHold>();
        }

        public void Process(ref InputInteractionContext context)
        {
            // Ensure parameters are sensible, especially if changed at runtime or misconfigured.
            // (Consider doing this once if parameters are guaranteed not to change after init)
            if (TapCount <= 0) TapCount = 1; // Default to 1 if invalid
            if (ReleasePoint >= PressPoint) ReleasePoint = PressPoint * 0.5f; // Ensure release is below press

            if (context.timerHasExpired)
            {
                context.Canceled();
                return;
            }

            bool isActuatedPastPressPoint = context.ControlIsActuated(PressPoint);
            bool isReleasedBelowReleasePoint = !context.ControlIsActuated(ReleasePoint);

            switch (context.phase)
            {
                case InputActionPhase.Waiting:
                    if (isActuatedPastPressPoint)
                    {
                        _currentTapCount = 1;
                        _isWaitingForNextPress = false; // After the first press, we're waiting for its release.

                        if (_currentTapCount >= TapCount) // Handles tapCount == 1 case
                        {
                            // Required taps met on the first press (e.g. tapCount is 1)
                            context.PerformedAndStayPerformed(); // Start "hold" part immediately
                        }
                        else
                        {
                            // More taps are needed
                            context.Started();
                            context.SetTimeout(Duration); // Set timeout for the entire multi-tap sequence
                        }
                    }

                    break;

                case InputActionPhase.Started: // In this phase, _currentTapCount < tapCount
                    if (_isWaitingForNextPress) // True: we are waiting for the press of the next tap
                    {
                        if (isActuatedPastPressPoint)
                        {
                            _currentTapCount++;
                            _isWaitingForNextPress = false; // Press occurred, now wait for its release.

                            if (_currentTapCount >= TapCount)
                            {
                                // All taps completed, transition to "hold" state.
                                context.PerformedAndStayPerformed();
                            }
                            // If more taps still needed, stay in Started. Timeout is still active.
                        }
                        // If no press yet, and timer hasn't expired, do nothing; wait for press or timeout.
                    }
                    else // False: we are waiting for a release from the current/previous tap
                    {
                        if (isReleasedBelowReleasePoint)
                        {
                            // Release occurred. Now we are waiting for the next tap's press.
                            _isWaitingForNextPress = true;
                        }
                        // If still held, or not yet fully released, do nothing; wait for release or timeout.
                    }

                    break;

                case InputActionPhase.Performed: // In "hold" state
                    if (isReleasedBelowReleasePoint) // If control is released while holding
                    {
                        context.Canceled();
                    }

                    break;

                // No explicit case for InputActionPhase.Canceled needed here,
                // as Reset() will be called by the system, and Process() won't be called again for this attempt.
            }
        }

        public void Reset()
        {
            _currentTapCount = 0;
            _isWaitingForNextPress = false; // Reset to initial state for next interaction attempt
        }

        // This method is necessary for `InitializeOnLoad` to find and register.
        // If you don't have specific logic for it, it can be empty.
        // The original file had it, so keeping it.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // Static registration is handled by the static constructor.
            // This method can be used for other runtime initializations if needed.
        }
    }
#if UNITY_EDITOR
} // Close namespace
#endif
