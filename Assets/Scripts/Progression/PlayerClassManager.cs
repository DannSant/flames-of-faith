using Game.Control;
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
                abilityIcons.SetIcons(classData.abilityIcons[0], classData.abilityIcons[1]);
            }else
            {
                Debug.LogError("PlayerClassManager: IconCooldownDisplay not found in the scene.");
            }
        }
    }
}
