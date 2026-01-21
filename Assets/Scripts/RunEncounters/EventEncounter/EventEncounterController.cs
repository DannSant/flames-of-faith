using Game.Combat;
using Game.Currency;
using Game.Effects;
using Game.Overworld;
using Game.Progression;
using Game.Scene;
using Game.Waves;
using System;
using System.Collections;
using UnityEngine;

namespace Game.RunEncounters
{
    public class EventEncounterController : MonoBehaviour
    {
        [SerializeField] private EventEncounterData data;

        public event Action<EventEncounterData> OnEventPresented;
        public event Action OnEventResolved;

        private EventContext context;

        private void Start()
        {
            BuildContext();
           StartCoroutine(TriggerEventRoutine());
        }

        private IEnumerator TriggerEventRoutine()
        {
            yield return new WaitForSeconds(1.0f);
            TriggerEncounter();
        }

        public void TriggerEncounter()
        {
            OnEventPresented?.Invoke(data);
        }

        public void SelectOption(EventOptionsList option)
        {
            option.Apply(context);
            Resolve();
        }

        private void Resolve()
        {
            OnEventResolved?.Invoke();
            WaveSpawner.Instance.InvokeOnWaveComplete();
        }

        private void BuildContext()
        {
            context = new EventContext
            {
                playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>(),
                playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>(),
                playerCorruption = PlayerManager.Instance.GetPlayerComponent<PlayerCorruption>(),
                playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>(),
                playerWallet = PlayerManager.Instance.GetPlayerComponent<CurrencyWallet>(),
                playerEffectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>(),
                encounter = this
            };
        }
    }

}