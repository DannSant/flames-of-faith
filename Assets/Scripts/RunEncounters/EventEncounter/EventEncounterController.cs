using Game.Combat;
using Game.Currency;
using Game.Effects;
using Game.Overworld;
using Game.Progression;
using Game.Scene;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.RunEncounters
{
    /// <summary>
    /// Runs the event of an event node: picks it, then shows each screen (the intro, then any step its options lead
    /// to) and pushes the final resolution to the Results window. The UI only gets plain screens and a callback.
    /// </summary>
    public class EventEncounterController : MonoBehaviour
    {
        [SerializeField] private List<EventEncounterData> encountersList;

        public event Action<EventScreen> OnEventPresented;
        public event Action<EventResolution> OnEventResolved;

        private EventContext context;
        private EventEncounterData data;
        private EventRunner runner;

        private void Start()
        {
            if (encountersList.Count == 0)
            {
                Debug.LogWarning("No encounters assigned to EventEncounterController.");
                return;
            }
            data = PickEvent();
            if (data == null)
            {
                Debug.LogWarning("[EventEncounter] No event could be picked.");
                return;
            }
            if (GameSession.Instance != null) GameSession.Instance.MarkEventSeen(EventPool.IdOf(data));
            BuildContext();
            var settings = EventCheckSettings.Instance;
            runner = new EventRunner(data, context.CharacterClass,
                check => EventCheckRoller.Roll(check, context, settings),
                actions => EventActions.ApplyAll(actions, context));
            StartCoroutine(TriggerEventRoutine());
        }

        /// <summary>
        /// An event not seen yet in this Pilgrimage, chosen from the run seed and this node, so leaving the level and
        /// coming back (Save &amp; Exit, Continue) offers the same one. A single-event list (the Wayside Shrine) always
        /// gives its event.
        /// </summary>
        private EventEncounterData PickEvent()
        {
            var map = MapRunController.Instance;
            bool onMap = map != null && map.IsInitialized;
            int seed = onMap ? map.RunSeed : UnityEngine.Random.Range(1, int.MaxValue);
            string nodeId = onMap ? map.CurrentNode.id : string.Empty;
            var seen = GameSession.Instance != null ? GameSession.Instance.SeenEvents : null;
            return EventPool.Pick(encountersList, seen != null ? new HashSet<string>(seen) : null, seed, nodeId);
        }

        private IEnumerator TriggerEventRoutine()
        {
            yield return new WaitForSeconds(1.0f);
            TriggerEncounter();
        }

        public void TriggerEncounter()
        {
            Present();
        }

        private void Present()
        {
            var screen = runner.BuildScreen();
            screen.choose = Choose;
            OnEventPresented?.Invoke(screen);
        }

        private void Choose(int index)
        {
            if (runner == null || !runner.Choose(index)) return;

            if (runner.IsFinished)
            {
                OnEventResolved?.Invoke(runner.Resolution);
            }
            else
            {
                Present();
            }
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
