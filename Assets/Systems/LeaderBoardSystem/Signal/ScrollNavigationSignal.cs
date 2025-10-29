using System;
using UnityEngine;

namespace Systems.LeaderBoardSystem.Signal
{
    [Serializable]
    public class ScrollNavigationSignal
    {
        public Vector2 scrollInput;

        public ScrollNavigationSignal(Vector2 scrollInput)
        {
            this.scrollInput = scrollInput;
        }
    }
}