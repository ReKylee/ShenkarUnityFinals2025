using UnityEngine;
using Unity.Cinemachine;

namespace Unity.Cinemachine
{
    /// <summary>
    /// A Cinemachine extension to eliminate camera and UI jitter in 2D pixel art games.
    /// This script works by performing a two-step snapping process:
    /// 1. It makes the camera follow a "snapped" version of the target's position.
    /// 2. It then snaps the camera's own final position to the pixel grid.
    /// This ensures both the follow logic and the final camera position are pixel-aligned,
    /// which is crucial for perfectly stable UI and TextMeshPro elements on a Screen Space Canvas.
    ///
    /// </summary>
    [ExecuteInEditMode]
    [SaveDuringPlay]
    [AddComponentMenu("")] // Hide from menu, add via VCam extensions dropdown
    [DisallowMultipleComponent]
    public class CinemachinePixelPerfectFollow : CinemachineExtension
    {
        [Tooltip(
            "The number of pixels in one Unity unit. This MUST match the 'Assets Pixels Per Unit' setting in your Pixel Perfect Camera component.")]
        public float m_PixelsPerUnit = 100f;

        /// <summary>
        /// The core logic is executed here, after the 'Body' stage of the Cinemachine pipeline.
        /// </summary>
        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            // We only run this logic after the camera's position has been set by the Body component (e.g., Transposer).
            if (stage == CinemachineCore.Stage.Body)
            {
                // We need a Follow target to work with.
                if (vcam.Follow != null)
                {
                    // Get the camera's calculated position from the Transposer.
                    Vector3 cameraPos = state.RawPosition;

                    // --- STEP 1: Correct for Target's Sub-Pixel Movement ---
                    // This ensures the camera is aiming at a stable, pixel-aligned point.
                    Vector3 targetPos = vcam.Follow.position;
                    Vector3 snappedTargetPos = SnapToPixelGrid(targetPos);
                    Vector3 correction = snappedTargetPos - targetPos;
                    Vector3 correctedCameraPos = cameraPos + correction;

                    // --- STEP 2: Snap the Final Camera Position ---
                    // This is the crucial final step. It eliminates jitter caused by the camera's own
                    // smooth damping, ensuring the position sent to the renderer is always on the pixel grid.
                    state.RawPosition = SnapToPixelGrid(correctedCameraPos);
                }
            }
        }

        /// <summary>
        /// Snaps a given world position to the nearest pixel boundary based on the PPU setting.
        /// </summary>
        /// <param name="position">The world position to snap.</param>
        /// <returns>The snapped world position.</returns>
        private Vector3 SnapToPixelGrid(Vector3 position)
        {
            // Avoid division by zero. If PPU is 0, return the original position.
            if (m_PixelsPerUnit <= 0)
            {
                return position;
            }

            // Calculate the size of a single pixel in world units.
            float pixelSize = 1.0f / m_PixelsPerUnit;

            // Snap the X and Y coordinates to the nearest pixel boundary.
            float x = Mathf.Round(position.x / pixelSize) * pixelSize;
            float y = Mathf.Round(position.y / pixelSize) * pixelSize;

            // We don't snap the Z-axis in 2D games, so we preserve its original value.
            return new Vector3(x, y, position.z);
        }
    }
}
