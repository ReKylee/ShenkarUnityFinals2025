using UnityEngine;

namespace Collectables.Score
{
    public interface IPopupTextService
    {
        public void ShowFloatingText(Vector3 position, string text);
    }

}
