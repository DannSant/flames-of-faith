using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Game.RunEncounters
{
    [CreateAssetMenu(menuName = "RunEncounters/EventOptions/Options List", fileName = "OptionsList")]
    public class EventOptionsList : ScriptableObject
    {
        [SerializeField] private List<EventOptionActionBase> optionActionsList = new List<EventOptionActionBase>();
        [SerializeField] private string optionDescription = "";

        public List<EventOptionActionBase> OptionActionsList => optionActionsList;

        public string OptionDescription => optionDescription;

        public string Apply(EventContext context)
        {
            var sb = new StringBuilder();
            foreach (var action in optionActionsList)
            {
                if (action == null)
                {
                    continue;
                }

                string result = action.Apply(context);
                if (string.IsNullOrEmpty(result))
                {
                    continue;
                }

                sb.AppendLine(result);
            }

            return sb.ToString();
        }
    }
}
