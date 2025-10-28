using System;
using Systems.InputSystem.Model;
using TMPro;
using UnityEngine;

namespace Systems.LeaderBoardSystem.View
{
    [Serializable]
    public class UserDataView : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI rankText;
        public TextMeshProUGUI userNameText;
        public TextMeshProUGUI scoreText;
    
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
    
        public void UpdateElement(UserData userData)
        {
            if (rankText != null)
                rankText.text = userData.rank.ToString();
        
            if (userNameText != null)
                userNameText.text = userData.userName;
            
            if (scoreText != null)
                scoreText.text = userData.score;
        }
    }
}