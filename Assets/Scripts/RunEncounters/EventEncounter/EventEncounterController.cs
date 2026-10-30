using Game.Combat;
using Game.Currency;
using Game.Effects;
using Game.Overworld;
using Game.Progression;
using Game.Scene;
using Game.Waves;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.RunEncounters
{
    public class EventEncounterController : MonoBehaviour
    {
        [SerializeField] private List<EventEncounterData> encountersList;

        public event Action<EventEncounterData> OnEventPresented;
        public event Action<EventResolution> OnEventResolved;

        private EventContext context;
        private EventEncounterData data;

        private void Start()
        {
            if (encountersList.Count == 0)
            {
                Debug.LogWarning("No encounters assigned to EventEncounterController.");
                return;
            }
            data = encountersList[UnityEngine.Random.Range(0, encountersList.Count)];
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
            // Old option assets have no result text: the Results window shows only the summary line
            var resolution = new EventResolution();
            resolution.parts.Add(new EventNarration { results = option.Apply(context) });
            Resolve(resolution);
        }

        private void Resolve(EventResolution resolution)
        {
            OnEventResolved?.Invoke(resolution);
            //StartCoroutine(DelayedWaveComplete());
        }

        private IEnumerator DelayedWaveComplete()
        {
            yield return new WaitForSeconds(5f);
            WaveSpawner.Instance.InvokeOnWaveComplete();
        }

        private void BuildContext()
        {
            context = new EventContext
            {
                playerHealth = PlayerManager.Instance.GetPlayerComponent<PlayerHealth>(),
                playerProgression = PlayerManager.Instance.GetPlayerComponent<PlayerProgression>(),
                playerGrace = PlayerManager.Instance.GetPlayerComponent<PlayerGrace>(),
                playerWallet = PlayerManager.Instance.GetPlayerComponent<CurrencyWallet>(),
                playerEffectStore = PlayerManager.Instance.GetPlayerComponent<EffectStore>(),
                classData = PlayerManager.Instance.GetPlayerComponent<PlayerClassManager>()?.ClassData,
                encounter = this
            };
        }
    }

}