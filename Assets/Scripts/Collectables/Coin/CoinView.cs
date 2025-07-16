using Collectables.Counter;
using TMPro;
using UnityEngine;

namespace Collectables.Coin
{
    public class CoinView : MonoBehaviour, ICounterView
    {
        [SerializeField] private TextMeshProUGUI coinText;

        public void UpdateCountDisplay(int count)
        {
            if (coinText) coinText.text = $"Coins: {count}";
        }
    }
}
