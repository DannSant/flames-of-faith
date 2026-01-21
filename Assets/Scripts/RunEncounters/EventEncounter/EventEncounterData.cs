using System.Collections.Generic;
using UnityEngine;

namespace Game.RunEncounters
{
    [CreateAssetMenu(menuName = "RunEncounters/Event Encounter", fileName = "EventEncounter")]
    public class EventEncounterData : ScriptableObject
    {
        public string eventName;
        public string title;

        [TextArea(5, 10)]
        public string fullText;

        public List<EventOptionsList> options;
                
        public Sprite backgroundArt;
    }
}
