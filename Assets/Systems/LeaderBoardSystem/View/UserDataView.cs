using System;
using Systems.InputSystem.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.LeaderBoardSystem.View
{
    [Serializable]
    public class UserDataView : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI rankText;
        public TextMeshProUGUI userNameText;
        public TextMeshProUGUI scoreText;
        public Image backgroundImage;
    
        [Header("Colors")]
        public Color normalColor = Color.white;
        public Color currentPlayerColor = Color.yellow;
        public Color stickyIndicatorColor = Color.cyan;
    
        private RectTransform rectTransform;
        public RectTransform RectTransform 
        {
            get
            {
                if (rectTransform == null)
                    rectTransform = GetComponent<RectTransform>();
                return rectTransform;
            }
        }
    
        public void UpdateElement(UserData userData, bool isStickyIndicator = false)
        {
            if (rankText != null)
                rankText.text = userData.rank.ToString();
        
            if (userNameText != null)
                userNameText.text = userData.userName;
            
            if (scoreText != null)
                scoreText.text = userData.score;
        
            // Set background color based on state
            if (backgroundImage != null)
            {
                if (isStickyIndicator)
                {
                    backgroundImage.color = stickyIndicatorColor;
                }
                else if (userData.isCurrentPlayer)
                {
                    backgroundImage.color = currentPlayerColor;
                }
                else
                {
                    backgroundImage.color = normalColor;
                }
            }
        }    }
}