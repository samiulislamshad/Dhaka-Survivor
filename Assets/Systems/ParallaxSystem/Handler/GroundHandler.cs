using Systems.GameSystem.Config;
using UnityEngine;
using Zenject;

namespace Systems.ParallaxSystem.Handler
{
    public class GroundHandler : MonoBehaviour
    {
        private GameConfig _gameConfig;
        
        [SerializeField] private Vector2 startPoint;
        [SerializeField] private Vector2 respawnPoint;
        
        [SerializeField] private float despawnPointX;
        [SerializeField] private float gameSpeed;
        
        [SerializeField] private Rigidbody2D rb;
        
        [Inject]
        private void InitializeDiReference(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }
        
        private void Start()
        {
            transform.position = startPoint;
        }

        private void FixedUpdate()
        {
            if(!_gameConfig.hasGameStarted.Value) return;
            
            var movement = new Vector2(-_gameConfig.gameSpeed.Value, 0) * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
            
            var pos = transform.position;
            if (pos.x <= despawnPointX)
            {
                transform.position = respawnPoint;
            }
        }
    }
}