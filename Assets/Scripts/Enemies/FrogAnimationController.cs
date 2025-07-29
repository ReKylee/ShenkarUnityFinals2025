using GabrielBigardi.SpriteAnimator;
using UnityEngine;

namespace Enemies
{
    public class FrogAnimationController : MonoBehaviour
    {
        [SerializeField] private SpriteAnimator animator;
        private void OnCollisionEnter2D(Collision2D other)
        {
            if ((1 << other.gameObject.layer & LayerMask.GetMask("Ground")) != 0)
            {
                animator.PlayIfNotPlaying("Idle");
            }
        }
        private void OnCollisionExit2D(Collision2D other)
        {
            if ((1 << other.gameObject.layer & LayerMask.GetMask("Ground")) != 0)
            {
                animator.PlayIfNotPlaying("Jump");
            }
        }
    }
}
