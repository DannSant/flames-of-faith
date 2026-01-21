using TMPro;
using UnityEngine;

namespace Game.UI.RunEncounters
{
    public class EventEncounterOptionUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI optionText;

        public void Initialize(string text)
        {
            optionText.text = text;
        }
    }
}