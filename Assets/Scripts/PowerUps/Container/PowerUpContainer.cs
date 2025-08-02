using Health.Interfaces;
using UnityEngine;

namespace PowerUps.Container
{
    public class PowerUpContainer : MonoBehaviour, IDamageable
    {
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Sprite crackedSprite;
        [SerializeField] private GameObject powerUpPrefab;
        private SpriteRenderer _spriteRenderer;
        public int CurrentHp { get; private set; } = 2;
        public int MaxHp { get; } = 2;
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            // Player collision: damage on first hit
            if (other.gameObject.CompareTag("Player") && CurrentHp == MaxHp)
            {
                Damage(1);
            }
            // Ground collision: break open after hit
            else if ((1 << other.gameObject.layer & groundLayer) != 0 && CurrentHp < MaxHp)
            {
                BreakOpen();
            }
        }
        public void Damage(int amount, GameObject source = null)
        {
            CurrentHp -= 1;
            switch (CurrentHp)
            {
                case 1:
                    Crack();
                    break;
                case <= 0:
                    BreakOpen();
                    break;
            }
        }
        private void BreakOpen()
        {
            Instantiate(powerUpPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        private void Crack()
        {
            _spriteRenderer.sprite = crackedSprite;

        }

        #region DEBUG UI

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (powerUpPrefab)
            {
                // Success state - green wireframe
                Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.6f);

                // Draw icon
                DrawIcon(transform.position + Vector3.up * 0.7f);
                DrawLabel(transform.position + Vector3.down * 0.5f, powerUpPrefab.name, Color.white);
            }
            else
            {
                // Error state - red wireframe
                Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.8f);
                Gizmos.DrawWireCube(transform.position, Vector3.one * 0.6f);

                // Warning X
                Gizmos.color = new Color(1f, 0.2f, 0.2f, 1f);
                Vector3 pos = transform.position;
                Vector3 offset = Vector3.one * 0.2f;
                Gizmos.DrawLine(pos - offset, pos + offset);
                Gizmos.DrawLine(pos + new Vector3(-offset.x, offset.y, offset.z),
                    pos + new Vector3(offset.x, -offset.y, -offset.z));

            }
        }
        

        private void DrawIcon(Vector3 position)
        {
            if (!powerUpPrefab) return;

            SpriteRenderer sr = powerUpPrefab.GetComponent<SpriteRenderer>();
            if (!sr || !sr.sprite?.texture) return;

            Sprite sprite = sr.sprite;
            UnityEditor.Handles.BeginGUI();

            Vector2 guiPos = UnityEditor.HandleUtility.WorldToGUIPoint(position);
            const float size = 40f;
            Rect iconRect = new Rect(guiPos.x - size * 0.5f, guiPos.y - size * 0.5f, size, size);


            // Draw sprite
            Rect spriteRect = sprite.rect;
            Rect uvRect = new Rect(
                spriteRect.x / sprite.texture.width,
                spriteRect.y / sprite.texture.height,
                spriteRect.width / sprite.texture.width,
                spriteRect.height / sprite.texture.height
            );

            GUI.DrawTextureWithTexCoords(iconRect, sprite.texture, uvRect);
            GUI.color = Color.white;

            UnityEditor.Handles.EndGUI();
        }
        private void DrawLabel(Vector3 position, string text, Color textColor)
        {
            UnityEditor.Handles.BeginGUI();
    
            Vector2 guiPos = UnityEditor.HandleUtility.WorldToGUIPoint(position);
    
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = textColor }
            };
    
            Vector2 textSize = style.CalcSize(new GUIContent(text));
            const float padding = 8f;
    
            Rect labelRect = new Rect(
                guiPos.x - textSize.x * 0.5f - padding * 0.5f,
                guiPos.y - textSize.y * 0.5f - 2f,
                textSize.x + padding,
                textSize.y + 4f
            );
    
            // Draw background
            Color prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0f, 0f, 0f, 0.8f); 
            GUI.Box(labelRect, "");
            GUI.backgroundColor = prevBg;
    
            // Draw text
            GUI.Label(labelRect, text, style);
    
            UnityEditor.Handles.EndGUI();
        }
     
#endif

        #endregion

    }
}
