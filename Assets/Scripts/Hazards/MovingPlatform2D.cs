using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hazards
{
    /// <summary>
    ///     Controls a 2D platform that moves between two points, carrying a player object on top.
    ///     The platform uses transform.position for movement and parents the player to move them.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingPlatform2D : MonoBehaviour
    {

        private const float PlatformReachedThreshold = 0.01f;
        private const float PlayerOnTopNormalYThreshold = -0.5f;

        [Header("Platform Movement Settings")] [SerializeField]
        private float speed = 2f;

        [SerializeField] private float waitTime = 1f;

        [Header("Waypoint Positions")] [Tooltip("The starting position of the platform.")] [SerializeField]
        private Vector2 startPosition;

        [Tooltip("The ending position of the platform.")] [SerializeField]
        private Vector2 endPosition;

        [Header("Player Interaction Settings")]
        [Tooltip("The tag of the player GameObject to interact with.")]
        [SerializeField]
        private string playerTag = "Player";

        // Player tracking
        private Transform _currentPlayerTransform;
        private Vector2 _currentTargetPosition;
        private float _currentWaitTimer;
        private bool _isMovingToEndPoint = true;

        // Internal state
        private Rigidbody2D _platformRigidbody;
        private Transform _playerOriginalParent;

        private void Awake()
        {
            _platformRigidbody = GetComponent<Rigidbody2D>();
            if (!_platformRigidbody)
            {
                Debug.LogError("MovingPlatform2D requires a Rigidbody2D component. Disabling script.", this);
                enabled = false;
                return;
            }

            _platformRigidbody.bodyType = RigidbodyType2D.Kinematic;
        }

        private void Start()
        {
            // Initialize platform position and target
            transform.position = new Vector3(startPosition.x, startPosition.y, transform.position.z);
            _currentTargetPosition = endPosition;
            _isMovingToEndPoint = true;
        }

        private void FixedUpdate()
        {
            HandlePlatformMovement();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag(playerTag)) return;
            if (_currentPlayerTransform) return;

            // Check if the player is on top of the platform
            bool playerIsOnTop = collision.contacts.Any(contact => contact.normal.y < PlayerOnTopNormalYThreshold);

            if (!playerIsOnTop) return;

            // Parent the player to the platform
            Transform playerTransform = collision.transform;
            if (playerTransform)
            {
                _currentPlayerTransform = playerTransform;
                _playerOriginalParent = _currentPlayerTransform.parent;
                _currentPlayerTransform.SetParent(transform);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (!gameObject.activeInHierarchy) return;
            if (!collision.gameObject.CompareTag(playerTag)) return;

            // Unparent the player if it's the one currently being carried
            if (_currentPlayerTransform && collision.transform == _currentPlayerTransform)
            {
                _currentPlayerTransform.SetParent(_playerOriginalParent);
                _currentPlayerTransform = null;
                _playerOriginalParent = null;
            }
        }


        private void OnValidate()
        {
            // If this is a new component, initialize the positions to the current transform position
            if (startPosition == Vector2.zero && endPosition == Vector2.zero)
            {
                startPosition = transform.position;
                endPosition = transform.position + new Vector3(3f, 0f, 0f); // Default 3 units to the right
            }
        }


        private void HandlePlatformMovement()
        {
            if (_currentWaitTimer > 0)
            {
                _currentWaitTimer -= Time.fixedDeltaTime;
                return;
            }

            Vector2 currentPosition = transform.position;
            Vector2 newPosition =
                Vector2.MoveTowards(currentPosition, _currentTargetPosition, speed * Time.fixedDeltaTime);

            // Apply the new position, keeping the original z value
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);

            // Check if the platform has reached the target position
            if (Vector2.Distance(transform.position, _currentTargetPosition) < PlatformReachedThreshold)
            {
                // Switch target and set wait timer
                _isMovingToEndPoint = !_isMovingToEndPoint;
                _currentTargetPosition = _isMovingToEndPoint ? endPosition : startPosition;
                _currentWaitTimer = waitTime;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Always show positions even when not selected
            Vector3 start3D = new(startPosition.x, startPosition.y, transform.position.z);
            Vector3 end3D = new(endPosition.x, endPosition.y, transform.position.z);

            // Draw platform shape at start position
            Gizmos.color = new Color(0, 0.7f, 0, 0.5f);
            DrawPlatformGizmo(start3D);

            // Draw platform shape at end position
            Gizmos.color = new Color(0.7f, 0, 0, 0.5f);
            DrawPlatformGizmo(end3D);

            // Draw path line
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            Gizmos.DrawLine(start3D, end3D);
        }

        /// <summary>
        ///     Draw a gizmo representing the platform's shape at the specified position
        ///     Uses SpriteRenderer bounds for accurate representation including auto-tiling
        /// </summary>
        private void DrawPlatformGizmo(Vector3 position)
        {
            // Store original position and temporarily move for accurate bounds calculation
            Vector3 originalPos = transform.position;
            transform.position = position;

            // Get sprite renderer
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer && spriteRenderer.sprite)
            {
                // Use sprite bounds for accurate representation, including auto-tiling
                Bounds spriteBounds = spriteRenderer.bounds;
                Vector3 size = new(spriteBounds.size.x, spriteBounds.size.y, 0.1f);
                Vector3 center = spriteBounds.center - transform.position;

                Matrix4x4 originalGizmosMatrix = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(center, size);
                Gizmos.matrix = originalGizmosMatrix;
            }
            else
            {
                // Fallback if no sprite is found
                Gizmos.DrawSphere(transform.position, 0.2f);
            }

            // Restore the platform's original position
            transform.position = originalPos;
        }

        // Add editor functionality for handles
        [CustomEditor(typeof(MovingPlatform2D))]
        public class MovingPlatform2DEditor : Editor
        {
            private SerializedProperty _endPositionProp;
            private SerializedProperty _startPositionProp;

            private void OnEnable()
            {
                _startPositionProp = serializedObject.FindProperty("startPosition");
                _endPositionProp = serializedObject.FindProperty("endPosition");
            }

            private void OnSceneGUI()
            {
                MovingPlatform2D platform = (MovingPlatform2D)target;
                Transform transform = platform.transform;

                // Convert to world positions for handles
                Vector3 startPos = new(_startPositionProp.vector2Value.x, _startPositionProp.vector2Value.y,
                    transform.position.z);

                Vector3 endPos = new(_endPositionProp.vector2Value.x, _endPositionProp.vector2Value.y,
                    transform.position.z);

                // Draw position handles
                EditorGUI.BeginChangeCheck();

                // Start position handle
                Vector3 newStartPos = Handles.PositionHandle(startPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Change Start Position");
                    _startPositionProp.vector2Value = new Vector2(newStartPos.x, newStartPos.y);
                    serializedObject.ApplyModifiedProperties();
                }

                // End position handle
                EditorGUI.BeginChangeCheck();
                Vector3 newEndPos = Handles.PositionHandle(endPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Change End Position");
                    _endPositionProp.vector2Value = new Vector2(newEndPos.x, newEndPos.y);
                    serializedObject.ApplyModifiedProperties();
                }

                // Draw labels with better contrast
                DrawHighContrastLabel(startPos + Vector3.up * 0.5f, "Start", Color.green);
                DrawHighContrastLabel(endPos + Vector3.up * 0.5f, "End", Color.red);
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                DrawDefaultInspector();
                serializedObject.ApplyModifiedProperties();
            }

            /// <summary>
            ///     Draw a label with a background for better visibility against any scene color
            /// </summary>
            private void DrawHighContrastLabel(Vector3 position, string text, Color color)
            {
                GUIStyle style = new(EditorStyles.boldLabel)
                {
                    normal = { textColor = color },
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };

                // Calculate size of the text
                Vector2 textSize = style.CalcSize(new GUIContent(text));

                // Draw background
                Handles.BeginGUI();

                // Convert world position to screen position
                Vector2 screenPos = HandleUtility.WorldToGUIPoint(position);

                // Create a rect for the background with padding
                Rect bgRect = new(screenPos.x - textSize.x / 2 - 5, screenPos.y - textSize.y / 2 - 2,
                    textSize.x + 10, textSize.y + 4);

                // Draw the background
                EditorGUI.DrawRect(bgRect, new Color(0, 0, 0, 0.6f));

                // Draw text with adjusted position for centering
                GUI.Label(bgRect, text, style);

                Handles.EndGUI();
            }
        }
#endif
    }
}
