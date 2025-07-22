using TMPro;
using UnityEngine;

namespace Collectables.Score
{
    public class PopupTextService : MonoBehaviour, IPopupTextService
    {
        [SerializeField] private ScoreTextPool scoreTextPool;
        
        public void ShowFloatingText(Vector3 position, string text)
        {
            TextMeshProUGUI floatingText = scoreTextPool?.Get(text);
            if (floatingText)
            {
                floatingText.transform.position = position;
            }
        }
    }
}
