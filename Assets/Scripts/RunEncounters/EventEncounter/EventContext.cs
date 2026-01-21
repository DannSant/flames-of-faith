using Game.Combat;
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
        public PlayerCorruption playerCorruption;
        public PlayerGrace playerGrace;
        public CurrencyWallet playerWallet;
        public EffectStore playerEffectStore;

        public EventEncounterController encounter;
    }

}