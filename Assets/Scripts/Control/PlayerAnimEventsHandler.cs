using Game.Common;
using System;
using UnityEngine;

namespace Game.Control
{
    public class PlayerAnimEventsHandler : Singleton<PlayerAnimEventsHandler>
    {
        public event Action OnSpecialAttackEnded;
        public void OnSpecialAttackEndedAnimEvent() 
        {
            OnSpecialAttackEnded?.Invoke();
        }
    }
}
