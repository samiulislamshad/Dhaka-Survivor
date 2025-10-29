using System.Collections.Generic;
using Systems.InputSystem.Model;
using UnityEngine;

namespace Systems.LeaderBoardSystem.Scriptable
{
    [CreateAssetMenu(fileName = "LeaderBoardScriptable", menuName = "Scriptable/LeaderBoardScriptable")]
    public class LeaderBoardScriptable : ScriptableObject
    {
        public List<UserData> leaderBoardUsers;
    }
}