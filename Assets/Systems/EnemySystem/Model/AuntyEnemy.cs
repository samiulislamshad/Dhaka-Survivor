using Cysharp.Threading.Tasks;
using Systems.EnemySystem.Enum;
using Systems.PlayerSystem.Signals.GameSignals;
using UnityEngine;

namespace Systems.EnemySystem.Model
{
    public class AuntyEnemy : Enemy
    {
        public override EnemyType Type => EnemyType.Aunty;

        protected override void OnStart()
        {
            
        }

        protected override void OnFixedUpdate()
        {
            var finalSpeedX = GetCalculatedSpeedX();
            var finalSpeedY = GetCalculatedSpeedY();
        
            var movement = new Vector2(-finalSpeedX, finalSpeedY) * (Time.fixedDeltaTime * Config.enemySpeedMultiplier);
            rb.MovePosition(rb.position + movement);

            ShowSpeechBubble();
        }

        #region Speech Bubble

        private void ShowSpeechBubble()
        {
            if (!(transform.position.x <= 30)) return;
            if (!canShowSpeechBubble) return;
            if (HasShownSpeechBubble) return;
            speechBubbleAnimator.Play("SpeechBubbleOn");
            HasShownSpeechBubble = true;
            canShowSpeechBubble = false;
            HideSpeechBubble().Forget();
        }

        private async UniTaskVoid HideSpeechBubble()
        {
            await UniTask.WaitForSeconds(SpeechBubbleOffTime);
            speechBubbleAnimator.Play("SpeechBubbleOff");
        }

        #endregion

        public override string GetEnemyName() => "Aunty";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Limit"))
            {
                Debug.Log($"Triggered {other.gameObject.name}");
                IsDespawning = true;
            }
            
            if(other.gameObject.CompareTag("Player") && !IsDead)
                SignalBus.Fire<ContactWithEnemySignal>();
        }
    }

}