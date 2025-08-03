using TMPro;
using UnityEngine;

namespace Collectables.Counter
{
    public class PaddedTextView : MonoBehaviour, ICounterView
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private int totalWidth = 2;

        public void UpdateCountDisplay(int count)
        {
            if (text)
            {
                text.text = $"{count}".PadLeft(totalWidth, '0');
            }

        }
    }

}
