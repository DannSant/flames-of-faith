using Game.Combat;
using Game.Control;
using Game.Currency;
using Game.Effects;
using Game.Progression;
using UnityEngine;

namespace Game.RunEncounters
{
    public class EventContext
    {
        public PlayerHealth playerHealth;
        public PlayerProgression playerProgression;
        public PlayerGrace playerGrace;
        public CurrencyWallet playerWallet;
        public EffectStore playerEffectStore;

        /// <summary>The chosen pilgrim's class: its class content and its Key stat for checks.</summary>
        public CharacterClassData classData;

        public EventEncounterController encounter;

        public CharacterClass CharacterClass => classData != null ? classData.characterClass : CharacterClass.None;
    }

}
