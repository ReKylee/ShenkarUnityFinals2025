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
            bar.fillAmount = Mathf.Clamp01((float)currentHp / maxHp);
        }
    }
}
