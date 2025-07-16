using Health.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Health
{
    public class SliderHealthView : MonoBehaviour, IHealthView
    {
        private Slider healthSlider;

        public void UpdateDisplay(int currentHp, int maxHp)
        {
            healthSlider.value = (float)currentHp / maxHp;
            healthSlider.maxValue = 1f;

        }
    }
}
