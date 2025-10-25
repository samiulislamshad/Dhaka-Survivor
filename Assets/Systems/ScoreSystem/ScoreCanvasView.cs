using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.ScoreSystem
{
    public class ScoreCanvasView : MonoBehaviour
    {
        public Animator animator;
        
        public GameObject runEndScorePanel;
        public GameObject runStartScorePanel;
        
        [Header("Run End Score Panel")]
        public TMP_Text userName;
        public TMP_Text score;
        public Button okayButton;

        [Header("Run Start Score Panel")] public TMP_Text playerScore;

    }
}