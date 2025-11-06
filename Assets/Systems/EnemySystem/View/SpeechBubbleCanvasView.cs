using UnityEngine;

namespace Systems.EnemySystem.View
{
    public class SpeechBubbleCanvasView : MonoBehaviour
    {
        [SerializeField] private RuntimeAnimatorController auntySpeechBubbleAnimator;
        [SerializeField] private RuntimeAnimatorController officeBossSpeechBubbleAnimator;
        [SerializeField] private RuntimeAnimatorController chesra2SpeechBubbleAnimator;
        [SerializeField] private RuntimeAnimatorController chesra3SpeechBubbleAnimator;

        [SerializeField] private Animator animator;

        public void ShowSpeechBubble(string enemyName)
        {
            gameObject.SetActive(true);
            animator.runtimeAnimatorController = enemyName switch
            {
                "Aunty" => auntySpeechBubbleAnimator,
                "OfficeBoss" => officeBossSpeechBubbleAnimator,
                "Chesra2" => chesra2SpeechBubbleAnimator,
                "Chesra3" => chesra3SpeechBubbleAnimator,
                _ => animator.runtimeAnimatorController
            };

            animator.Play($"Animate");
        }
    }
}