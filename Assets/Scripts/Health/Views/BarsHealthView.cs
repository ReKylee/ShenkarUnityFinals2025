using Health.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Health
{
    public class BarsHealthView : MonoBehaviour, IHealthView
    {
        [SerializeField] private Image bar;

        public void UpdateDisplay(int currentHp, int maxHp)
        {
            // Truncate the fill amount to 3 decimal places
            bar.fillAmount = Mathf.Floor((float)currentHp / maxHp * 1000f) / 1000f;
        }
    }
}
