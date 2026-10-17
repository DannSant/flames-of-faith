using Game.Control;
using Game.Effects;
using Game.Effects.Abilities;
using Game.UI;
using Game.Utils;
using UnityEngine;

namespace Game.Progression
{
    /**
     * Sets data depending on the player class
     * */
    public class PlayerClassManager : MonoBehaviour, IInitializeAfterStateReady
    {
        [SerializeField] private CharacterClassData classData;

        private CharacterVisual characterVisual;

        public CharacterClass CharacterClass => classData != null ? classData.characterClass : CharacterClass.None;

        private void Awake()
        {
            if (classData == null)
            {
                Debug.LogError("PlayerClassManager: classData is not assigned in the inspector.");
                return;
            }

            characterVisual = GetComponentInChildren<CharacterVisual>();
            if (characterVisual != null)
            {
                characterVisual.Initialize(classData);
            }
        }

        public void InitializeAfterStateReady()
        {
            var abilityIcons = FindAnyObjectByType<IconCooldownDisplay>();
            if (abilityIcons!=null)
            {
                // The class's Glyph items drive the icons and the dash tooltip; the class data is
                // the fallback for a kit without them.
                var effectStore = GetComponent<EffectStore>();
                Effect dashItem = null;
                effectStore?.TryGetAbilityItem(AbilitySlot.Dash, out dashItem);

                abilityIcons.SetIcons(
                    GetAbilityIcon(effectStore, AbilitySlot.Primary, 0),
                    GetAbilityIcon(effectStore, AbilitySlot.Special, 1),
                    GetAbilityIcon(effectStore, AbilitySlot.Dash, 2));
                abilityIcons.SetDashTooltip(
                    dashItem != null ? dashItem.EffectName : classData.dashName,
                    dashItem != null ? dashItem.Description : classData.dashDescription);
            }
        }

        private Sprite GetAbilityIcon(EffectStore effectStore, AbilitySlot slot, int fallbackIndex)
        {
            if (effectStore != null && effectStore.TryGetAbilityItem(slot, out Effect item) && item.EffectIcon != null)
            {
                return item.EffectIcon;
            }
            return classData.abilityIcons != null && fallbackIndex < classData.abilityIcons.Count
                ? classData.abilityIcons[fallbackIndex]
                : null;
        }
    }
}
