using UnityEngine;

namespace Systems.EnemySystem.View
{
    public class SpeechBubbleView : MonoBehaviour
    {
        [SerializeField] private RuntimeAnimatorController auntySpeechBubbleAnimator;
        [SerializeField] private RuntimeAnimatorController officeBossSpeechBubbleAnimator;
        [SerializeField] private RuntimeAnimatorController chesra1SpeechBubbleAnimator;
        [SerializeField] private RuntimeAnimatorController chesra2SpeechBubbleAnimator;
        [SerializeField] private RuntimeAnimatorController chesra3SpeechBubbleAnimator;

        [SerializeField] private Animator animator;

        public void ShowSpeechBubble(string enemyName)
        {
            gameObject.SetActive(true);
            switch (enemyName)
            {
                case "Aunty":
                    animator.runtimeAnimatorController = auntySpeechBubbleAnimator;
                    animator.Play($"Animate");
                    break;
                case "OfficeBoss":
                    animator.runtimeAnimatorController = officeBossSpeechBubbleAnimator;
                    animator.Play($"Animate");
                    break;
                case "Chesra1":
                    animator.runtimeAnimatorController = chesra1SpeechBubbleAnimator;
                    animator.Play($"Animate");
                    break;
                case "Chesra2":
                    animator.runtimeAnimatorController = chesra2SpeechBubbleAnimator;
                    animator.Play($"Animate");
                    break;
                case "Chesra3":
                    animator.runtimeAnimatorController = chesra3SpeechBubbleAnimator;
                    animator.Play($"Animate");
                    break;
            }
        }
    }
}