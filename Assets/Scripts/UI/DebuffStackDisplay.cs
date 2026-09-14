using TMPro;
using UnityEngine;
using Game.Combat.Elemental;

namespace Game.UI
{
    public class DebuffStackDisplay : MonoBehaviour
    {
        [SerializeField] private DebuffHandler debuffHandler;
        [SerializeField] private TextMeshProUGUI stacksText;
        [SerializeField] private ElementalType trackedType = ElementalType.Bleed;

        private void Awake()
        {
            UpdateDisplay(0);
        }

        private void OnEnable()
        {
            if (debuffHandler == null) return;

            debuffHandler.onStackApplied += HandleStackApplied;
            debuffHandler.onStackRemoved += HandleStackRemoved;

            // Sync to the true current state rather than assuming zero, in case a debuff of
            // the tracked type is already active when this display enables.
            UpdateDisplay(debuffHandler.GetStacks(trackedType));
        }

        private void OnDisable()
        {
            if (debuffHandler == null) return;

            debuffHandler.onStackApplied -= HandleStackApplied;
            debuffHandler.onStackRemoved -= HandleStackRemoved;
        }

        private void HandleStackApplied(ElementalType type, int stacks)
        {
            if (type != trackedType) return;
            UpdateDisplay(stacks);
        }

        private void HandleStackRemoved(ElementalType type)
        {
            if (type != trackedType) return;
            UpdateDisplay(0);
        }

        private void UpdateDisplay(int stacks)
        {
            if (stacksText == null) return;

            bool show = stacks > 0;
            stacksText.gameObject.SetActive(show);
            if (show)
            {
                stacksText.SetText(stacks.ToString());
            }
        }
    }
}
