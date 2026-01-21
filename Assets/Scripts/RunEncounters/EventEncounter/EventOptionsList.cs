using System.Collections.Generic;
using UnityEngine;

namespace Game.RunEncounters
{
    [CreateAssetMenu(menuName = "RunEncounters/EventOptions/Options List", fileName = "OptionsList")]
    public class EventOptionsList : ScriptableObject
    {
        [SerializeField] private List<EventOptionActionBase> optionActionsList = new List<EventOptionActionBase>();
        [SerializeField] private string optionDescription = "";

        public string OptionDescription => optionDescription;

        public void Apply(EventContext context)
        {
            foreach (var action in optionActionsList)
            {
                action.Apply(context);
            }
        }
    }
}
