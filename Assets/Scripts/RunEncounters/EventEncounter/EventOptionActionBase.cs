using UnityEngine;

namespace Game.RunEncounters
{
    public abstract class EventOptionActionBase : ScriptableObject
    {
        public abstract void Apply(EventContext context);
    }

}